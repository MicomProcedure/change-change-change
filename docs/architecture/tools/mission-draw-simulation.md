# Mission抽選シミュレーション設計

> [ドキュメント一覧](../../README.md) / 区分: クラス設計

> **対象範囲**: 本番と同じCatalog準備と本実装版Mission抽選StrategyをEditor上で繰り返し実行し、抽選TraceをCSVへ出力する開発ツール

---

## 1. 目的

手動プレイでは集めにくい量の抽選データを、本番と同じロジックで生成すること。抽選ロジックやパラメータ調整の判断材料とする。

| 方式 | 答える問い | 抽選Contextの決め方 |
| --- | --- | --- |
| ① 流れ（`SequentialFlow`） | プレイヤーが実際にどの順序で問題に出会うか | 各SessionをSection 1の初回抽選から始め、毎回正解したものとしてSectionを1ずつ進める。次回抽選の現在式は直前Missionを満たす式から一様に選び、直前Missionには直前の選出候補を使う |
| ② 固定条件（`FixedCondition`） | 特定の状態で、各段階にどの候補が残り、最終的に何がどの割合で選ばれるか | Section、抽選の種類（初回／次回）、現在式、直前Missionを固定し、指定回数だけ独立に抽選する |

---

## 2. 設計方針

- **本番ロジックを再利用する**: Catalogは `MissionCatalogPreparer.Prepare`、抽選は `FullMissionDrawStrategy` をそのまま使うこと。シミュレーション専用の抽選ロジックや確率計算を実装しないこと。
- **出力形式を共有する**: 抽選Traceは `IDiagnosticsReporter<MissionDrawTrace>` へ渡し、プレイログと同じ `MissionDrawTraceCsvFormatter` と `CsvDiagnosticsReporter` でCSVへ出力すること。
- **再現性**: 1つのseedから抽選用乱数（seedそのまま）とシナリオ用乱数（seedと固定値の排他的論理和）を作り、同じseedと入力から同じ結果を再現すること。
- **純C#とEditorの分離**: シミュレーション本体はUnity APIに依存しない `Ccc.Simulation` に置き、設定Asset・EditorWindow・Asset変換は `Ccc.Editor.Simulation` に置くこと。

---

## 3. アセンブリと名前空間

| 型名 | アセンブリ / 名前空間 | 役割 |
| --- | --- | --- |
| `SimulationRandom` | `Ccc.Simulation` | seed付きの `IRandomSource`。`NextIndex(count)` で一様な整数選択も提供 |
| `MissionConditionSpec` | `Ccc.Simulation` | Kind・パラメータA/B・否定の有無で条件を指定し、Catalog候補を特定する |
| `IMissionDrawScenario` / `MissionDrawScenarioEnvironment` | `Ccc.Simulation` | 次の抽選Contextを決めるシナリオと、準備済みCatalog・計算結果が定義される候補式・シナリオ用乱数 |
| `SequentialFlowScenario` | `Ccc.Simulation` | ① 流れ |
| `FixedConditionScenario` / `FixedConditionDrawKind` / `ScenarioValueSource` | `Ccc.Simulation` | ② 固定条件 |
| `MissionDrawSimulation` / `MissionDrawSimulationSummary` | `Ccc.Simulation` | Catalog準備、シナリオ生成、抽選の繰り返しと集計 |
| `MissionDrawSimulationSettingsAsset` / `MissionDrawSimulationMode` | `Ccc.Editor.Simulation` | 対象Config、出力先、seed、方式別パラメータを保持するScriptableObject |
| `MissionDrawSimulationExecutor` | `Ccc.Editor.Simulation` | 設定からConfigとCatalog準備設定を作り、CSV Reporterを組み立てて実行する |
| `MissionDrawSimulationWindow` | `Ccc.Editor.Simulation` | `CCC/Mission Draw Simulation` メニューから開くEditorWindow |

---

## 4. シナリオ仕様

### 4.1 SequentialFlowScenario

- コンストラクタは環境null、Session数1未満、最大Section 1未満を拒否すること（`ArgumentNullException` / `ArgumentOutOfRangeException`）。
- 直前の抽選が成功し、直前Sectionが最大Section未満なら、Sectionを1進めた次回抽選Contextを返すこと。
- それ以外（最初の呼び出し、抽選失敗、最大Section到達）は、未開始のSessionがあればSection 1の初回抽選Contextを返し、なければfalseを返すこと。
- 次回抽選の現在式は、直前の選出候補の条件を満たす定義済み候補式から一様に選ぶこと。満たす式がない場合は定義済み候補式全体から選ぶこと。

### 4.2 FixedConditionScenario

- コンストラクタは環境null、Section 1未満、抽選回数1未満を拒否すること。
- 抽選の種類が `Initial` の場合、現在式と直前Missionを持たない初回抽選Contextを返すこと。次回抽選では現在式と直前Missionがどちらも必須のため、「直前Missionなし」は初回抽選として扱うこと。
- 抽選の種類が `Next` の場合、現在式と直前Missionをそれぞれ次のいずれかで決めること。
  - `Fixed`: 指定値を全抽選で使う。現在式が定義済み候補式にない場合、直前MissionがCatalogにない場合は `ArgumentException`。
  - `RandomPerDraw`: 抽選ごとに、定義済み候補式／SectionのCurrent Tier以下で解放済みのCatalog候補から一様に選ぶ。実プレイで到達しない未解放Tierの候補を直前Missionにしないこと。
- 抽選回数に達したらfalseを返すこと。

### 4.3 MissionDrawSimulation.Run

- Catalog準備に失敗した場合は `InvalidOperationException` とすること。
- シナリオがfalseを返すまで抽選を繰り返し、抽選回数と失敗理由ごとの失敗回数を返すこと。
- 抽選の失敗はシナリオへ渡し、①では次Sessionへ移ること。

---

## 5. 設定Assetと実行

`[CreateAssetMenu(menuName = "CCC/Mission Draw Simulation Settings")]` の `MissionDrawSimulationSettingsAsset` で設定すること。

| 区画 | 項目 | 既定値 |
| --- | --- | --- |
| Target | `configEntry` / `logSettings` | 未設定（Window直接入力時は最初に見つかったAssetを自動設定） |
| Output | `outputDirectoryName` / `csvCandidateCount` | `Simulation` / 10 |
| Run | `seed` / `mode` | 0 / `SequentialFlow` |
| Sequential Flow | `sessionCount` / `maximumSection` | 100 / 20 |
| Fixed Condition | `section` / `drawCount` / `drawKind` | 1 / 1000 / `Next` |
| Fixed Condition | `expressionSource` / `expressionLeft` / `expressionOperator` / `expressionRight` | `RandomPerDraw` / 1 / `Add` / 1 |
| Fixed Condition | `previousMissionSource` / `previousMissionKind` / `previousMissionParameterA` / `previousMissionParameterB` / `isPreviousMissionNegated` | `RandomPerDraw` / `Even` / 0 / 0 / false |

`MissionDrawSimulationWindow` の動作:

- 設定Assetを指定した場合はそのAssetを編集・実行すること。
- 指定しない場合はWindow内の一時設定を使い、`Save As Asset` で設定Assetとして保存できること。
- `Run` は `MissionDrawSimulationExecutor.Execute` を呼び、抽選回数・失敗回数・出力先をWindowとConsoleへ表示すること。例外はWindowへメッセージを表示し、Consoleへ例外を出力すること。

`MissionDrawSimulationExecutor.Execute` の動作:

- 設定Asset、ConfigEntry、GameplayConfigAsset、LogSettingsAssetを検証し、エラーがあれば `InvalidOperationException` とすること。
- `GameplayConfigAsset.CreateRuntimeConfig()` と `GameplayConfigEntry.CreateCatalogPreparationSettings(config)` で本番と同じ入力を作ること。
- SessionIdは `sim_{日時}_seed{seed}` とし、1回の実行を `Logs/Ccc/{outputDirectoryName}/{SessionId}_draws.csv` と `_candidates.csv` の1組へ出力すること。①のSession境界は `IsInitialDraw=True` の行で判別すること。

---

## 6. 受け入れ条件

- 同じseedと入力から同じ選出列を再現すること。
- ①が各SessionをSection 1の初回抽選から始め、Sectionを最大まで1ずつ進めること。
- ①の次回抽選の直前Missionが直前の選出候補であり、現在式がその条件を満たすこと。
- ①が抽選失敗後に次Sessionの初回抽選へ移ること。
- ②の固定値が全抽選で使われ、毎回選ぶ値が定義済み候補式／解放済みCatalog候補から選ばれること。
- ②が候補式にない現在式と、Catalogにない直前Missionを拒否すること。
- 全抽選のTraceが診断Reporterへ報告されること。

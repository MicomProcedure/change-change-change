# Mission抽選シミュレーション解析Notebook

> [ドキュメント一覧](../../README.md) / 区分: ツール設計

> **対象範囲**: [Mission抽選シミュレーション](./mission-draw-simulation.md) が出力した1回分のCSVを、Google Colaboratoryで解析するNotebook

---

## 1. 目的

シミュレーション結果から、抽選ロジックとパラメータ調整の判断材料を得ること。

| 観点 | 答える問い |
| --- | --- |
| Mission単位の選出割合 | 特定のMissionが出過ぎていないか。狙ったMissionが出ていないか |
| Kind・Tierの出現分布 | Section（Tier解放の進行）に応じてKindとTierの構成がどう変わるか |
| パラメータ・適合率の質 | Sectionごとの目標式適合割合の範囲に対して、選出Missionの適合率がその抽選の目標値からどれだけ離れているか。Pool内順位とKind内パラメータに偏りがないか |
| 連続出題の偏り | Session内で同じKind・Missionが続いていないか |

---

## 2. 配置と実行環境

- Notebookは `Tools/Analysis/mission_draw_analysis.ipynb` に置き、出力なしの状態でコミットすること。
- Google Colaboratoryで実行すること。依存はColab標準搭載の `pandas`・`numpy`・`plotly` に限ること。
- 入力は `Logs/CCC/Simulation/` の `{SessionId}_catalog.csv`・`_draws.csv`・`_candidates.csv` の3ファイルとし、Google Driveのフォルダへアップロードして読み込むこと。
- 入力フォルダとSessionIdは設定セルの定数で指定し、環境変数 `CCC_SIMULATION_DIRECTORY`・`CCC_SIMULATION_SESSION_ID` で上書きできること。SessionIdが空の場合は、フォルダ内でdraws表の更新日時が最も新しい実行を使うこと。
- 入力フォルダが `/content/drive` 配下かつColab上で実行する場合だけDriveをマウントすること。

---

## 3. 入力の検証

次のいずれかに当たる場合は例外で停止すること。別の実行のCSVが混ざった状態で集計しないためである。

- 3ファイルのいずれかが存在しない、または必須列が欠けている。
- draws表の選出候補（SelectedKind・SelectedParameterA・SelectedParameterB・SelectedPolarity）がcatalog表に存在しない。
- 各抽選の `EnabledCount`・`UnlockedCount` が、catalog表と `NegatedMissionSelectionWeight`・`CurrentTier` から再計算した値と一致しない。

---

## 4. 解析仕様

### 4.1 Mission単位の選出割合

- Missionの識別は Kind・ParameterA・ParameterB・極性の組とし、表示は `AtLeast(8)`・`NOT Between(3,5)` 形式の構造ラベルとすること。本番の表示文言は再現しないこと。
- Missionごとに次の値を求めること。

| 値 | 定義 |
| --- | --- |
| `Selected` / `Share` | 選出回数と、全抽選に占める割合 |
| `ExposedDraws` | そのMissionが抽選対象（有効かつTierが `CurrentTier` 以下）だった抽選回数 |
| `UniformExpected` | 抽選対象だった各抽選で `1 / UnlockedCount` を合計した値（解放済み候補から一様に選んだ場合の期待選出回数） |
| `Lift` | `Selected / UniformExpected` |
| `StaticPoolRank` | 同じTier・極性・Kindの候補を、目標式適合割合の上限 `TargetExpressionMatchRatio`（Section 1の値）との差の昇順・Catalog順で並べた順位（全候補が残り、目標値が上限の場合のPool内順位） |

- 偏りの指標として、上位 `CONCENTRATION_TOP_FRACTION` のMissionが占める選出割合、実効Mission数（1/Σp²）、ジニ係数を示すこと。
- 選出回数と `Lift` の上位、抽選対象だったのに未選出のMission、Kind×極性とTierごとの網羅率を表で示すこと。

### 4.2 Kind・Tierの出現分布

- Kind別の選出回数（極性で積み上げ）、Section区間ごとの選出Tier構成比、Section区間×Kindの選出割合を示すこと。Section区間の幅は `SECTION_BIN_WIDTH` とすること。

### 4.3 パラメータ・適合率の質

- 目標式適合割合は抽選ごとに異なるため、目標値との差はdraws表の `TargetRatio`（その抽選の目標値）に対して求めること。
- Section別の選出Missionの適合率分布を、その抽選の目標式適合割合の範囲（`TargetRatioLowerBound`・`TargetRatioUpperBound`）とともに示すこと。
- 選出Missionの適合率から `TargetRatio` を引いた差の分布、Kind別の `TargetRatio` との差、PoolSize別の `SelectedRank` 構成比、Kind別のPoolが設定値未満だった抽選の割合を示すこと。
- パラメータを持つKindについて、抽選対象だった全Missionのパラメータ別選出回数を示すこと。1パラメータのKindは極性別の棒グラフ、2パラメータのKindは極性ごとのA×Bヒートマップとし、Catalogにない組を選出0回と区別すること。

### 4.4 連続出題の偏り

- draws表の先頭が初回抽選で、Session内のSectionがすべて1ずつ進む実行（`SequentialFlow`）でのみ計算し、それ以外は省略すること。Session境界は `IsInitialDraw=True` の行とすること。
- 直前と同じKind・極性・Missionの割合、直前 `REPEAT_WINDOW_DRAWS` 回以内に同じMissionが出ていた割合、Sessionごとの同一Kind最長連続回数、直前Kind→次Kindの遷移確率を示すこと。
- 同じKind・極性の割合は、全体の構成比から求めた独立抽選の一致確率（Σp²）と並べること。

---

## 5. グラフ表記

- グラフ内の表記は英語とKind名などの識別子とし、説明はMarkdownセルに日本語で書くこと。
- 極性は分類色（Positive: 青、Negated: 橙）、Tierは1色の明度段階、割合・回数のヒートマップは1色の連続階調、目標値は無彩色の破線とすること。

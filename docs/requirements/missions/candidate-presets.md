# Missionパラメータ候補プリセット

> [ドキュメント一覧](../../README.md) / 区分: 設定仕様

## ドキュメントの目的

簡易版（初期実装フェーズ）において評価対象とする Mission パラメータ候補群を定義する。
本ドキュメントに記載された候補データは、Gameplay 開始時に全評価され、成立条件を満たしたもののみが `MissionCatalog` に登録される。簡易版ではパラメータの自動生成は行わないこと。

## 候補データの定義形式

表の1行が1つの `MissionParameterCandidate` オブジェクトを表現する。

| 項目名 | 定義内容 |
| --- | --- |
| `Kind` | ミッションの種別 |
| `ParameterA` | Kindごとの第1パラメータ |
| `ParameterB` | Kindごとの第2パラメータ |
| `IntendedTier` | 候補を評価対象とする適用 Tier |
| `Note` | 表示または選定上の補足・注記 |

- パラメータを必要としない Kind の場合、`ParameterA` および `ParameterB` は空欄とすること。
- 候補表に存在しないパラメーター数値の組み合わせは、簡易版では生成しないこと。
- パラメータ A と B の集合から機械的な直積（組み合わせ）を生成するのではなく、許可する特定のペアを1行ずつ明示すること。
- 同一の `Kind`・`ParameterA`・`ParameterB`・`IntendedTier` を持つ完全な重複行は配置不可とする。

## Kindごとのパラメータ構造

| Kind | ParameterA | ParameterB | 概要説明 |
| --- | --- | --- | --- |
| **Even** | （使用しない） | （使用しない） | 偶数 |
| **Odd** | （使用しない） | （使用しない） | 奇数 |
| **Integer** | （使用しない） | （使用しない） | 整数 |
| **Decimal** | （使用しない） | （使用しない） | 小数（非整数） |
| **Positive** | （使用しない） | （使用しない） | 正数（> 0） |
| **Negative** | （使用しない） | （使用しない） | 負数（< 0） |
| **Prime** | （使用しない） | （使用しない） | 素数 |
| **Zero** | （使用しない） | （使用しない） | ゼロ（0） |
| **Square** | （使用しない） | （使用しない） | 平方数（平方根が整数） |
| **Composite** | （使用しない） | （使用しない） | 合成数 |
| **AtLeast** | 下限値 N | （使用しない） | N 以上 |
| **AtMost** | 上限値 N | （使用しない） | N 以下 |
| **Exact** | 目標値 N | （使用しない） | N と一致 |
| **Multiple** | 基準値 N | （使用しない） | N の倍数 |
| **Between** | 下限値 N | 範囲幅 W | N 以上 (N+W) 以下 |
| **Absolute** | 絶対値 N | （使用しない） | 絶対値が N と一致 |
| **OnesDigit** | 一の位 N | （使用しない） | 一の位が N と一致 |
| **Modulo** | 法 N | 剰余 M | N で割った余りが M |
| **DecimalFirstDigit** | 小数第1位 N | （使用しない） | 小数第1位が N と一致 |

- `Between` の上限値は `N + W` として導出すること（`W >= 1`）。
- `Modulo` は `N > 1` かつ `0 <= M < N` の範囲とすること。

## 初期実装版における除外仕様

- `NegatedMission` のパラメータ候補は用意しないこと。
- 複合ミッション（`CompoundMission`）のパラメータ候補は用意しないこと。
- 到達可能な整数計算結果からの動的なパラメータ候補自動生成は行わないこと。

## パラメータ候補一覧表

※プロトタイプ実装において最終採用する具象パラメータ値は、Normal 難易度の `ExpressionCandidate` に対する評価テスト結果を確認して設定すること。
※「（未決定）」と書かれた行は登録用テンプレートであり、未決定のままでは Config データとしてロードされないこと。具体的な整数値を入力して使用すること。

| Kind | ParameterA | ParameterB | IntendedTier | Note（備考） |
| --- | --- | --- | --- | --- |
| **Even** | | | Tier1 | パラメータなし・確定 |
| **Odd** | | | Tier1 | パラメータなし・確定 |
| **Integer** | | | Tier1 | パラメータなし・確定 |
| **Decimal** | | | Tier1 | パラメータなし・確定 |
| **Positive** | | | Tier1 | パラメータなし・確定 |
| **Negative** | | | Tier1 | パラメータなし・確定 |
| **Prime** | | | Tier2 | パラメータなし・確定 |
| **Zero** | | | Tier2 | パラメータなし・確定 |
| **Square** | | | Tier2 | パラメータなし・確定 |
| **Composite** | | | Tier3 | パラメータなし・確定 |
| **AtLeast** | （未決定） | | Tier1 | 下限N。必要な組み合わせ分行を複製 |
| **AtMost** | （未決定） | | Tier1 | 上限N。必要な組み合わせ分行を複製 |
| **Exact** | （未決定） | | Tier2 | 目標値N。必要な組み合わせ分行を複製 |
| **Multiple** | （未決定） | | Tier2 | 基準N。必要な組み合わせ分行を複製 |
| **Between** | （未決定） | （未決定） | Tier2 | 幅広候補（下限N, 幅Wのペア） |
| **Between** | （未決定） | （未決定） | Tier3 | 幅狭候補（下限N, 幅Wのペア） |
| **Absolute** | （未決定） | | Tier3 | 絶対値N。必要な組み合わせ分行を複製 |
| **OnesDigit** | （未決定） | | Tier3 | 一の位N。必要な組み合わせ分行を複製 |
| **Modulo** | （未決定） | （未決定） | Tier4 | 法Nと剰余Mのペア |
| **DecimalFirstDigit** | （未決定） | | Tier4 | 小数第1位N。必要な組み合わせ分行を複製 |

具体的なパラメーターが入力された各行は、[CCCテスト仕様](../testing/acceptance-specification.md)のパラメータ制約および `MissionCatalog` 成立要件を満たすことを必須とする。

---
- ミッション種別仕様: [生成するMission](./mission-types.md)
- カタログ・抽選仕様: [Missionカタログ・抽選内部仕様](./catalog-and-draw.md)
- ゲームモード・難易度設定: [ゲームモード・難易度](../gameplay/game-modes-and-difficulty.md)
- テスト要件: [CCCテスト仕様](../testing/acceptance-specification.md)

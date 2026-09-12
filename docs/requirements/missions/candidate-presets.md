# Missionパラメータ候補プリセット

> [ドキュメント一覧](../../README.md) / 区分: 設定仕様
>
> 正本: [Cosense「Missionパラメータ候補プリセット」](https://scrapbox.io/proto-TeamL/Mission%E3%83%91%E3%83%A9%E3%83%A1%E3%83%BC%E3%82%BF%E5%80%99%E8%A3%9C%E3%83%97%E3%83%AA%E3%82%BB%E3%83%83%E3%83%88)
> 取得時点の原文更新日時: 2026-08-26T05:51:20.000Z


## このページの目的

簡易版で評価対象とするMissionパラメータ候補を定義する。
ここに記載した候補をGameplay開始時に評価し、成立条件を通過したものだけをMissionCatalogへ登録する。
候補の自動生成は行わない。

## 記載単位

1行を1つのMissionParameterCandidateとする。

| 項目 | 意味 |
| --- | --- |
| Kind | Missionの種類 |
| ParameterA | Kindごとの第1パラメータ |
| ParameterB | Kindごとの第2パラメータ |
| IntendedTier | 候補を評価するTier |
| Note | 表示または選定上の注記 |

パラメータを持たないKindはParameterAとParameterBを空欄とする。
候補表にない値は簡易版では生成しない。
ParameterAとParameterBの候補集合から直積を作らず、許可する組を1行ずつ明示する。
同じKind、同じParameterA、同じParameterB、同じIntendedTierの重複行を置かない。

## Kindごとの意味

| Kind | ParameterA | ParameterB |
| --- | --- | --- |
| Even | 使用しない | 使用しない |
| Odd | 使用しない | 使用しない |
| Integer | 使用しない | 使用しない |
| Decimal | 使用しない | 使用しない |
| Positive | 使用しない | 使用しない |
| Negative | 使用しない | 使用しない |
| Prime | 使用しない | 使用しない |
| Zero | 使用しない | 使用しない |
| Square | 使用しない | 使用しない |
| Composite | 使用しない | 使用しない |
| AtLeast | 下限N | 使用しない |
| AtMost | 上限N | 使用しない |
| Exact | 目標値N | 使用しない |
| Multiple | 基準N | 使用しない |
| Between | 下限N | 幅W |
| Absolute | 絶対値N | 使用しない |
| OnesDigit | 一の位N | 使用しない |
| Modulo | 法N | 剰余M |
| DecimalFirstDigit | 小数第1位N | 使用しない |

Betweenの上限は N + W として求める。
BetweenのWは1以上とする。
ModuloはNを1より大きくし、Mを0以上N未満とする。

## 初期実装版の対象外

- NegatedMissionの候補は用意しない。
- 複合Missionの候補は用意しない。
- 到達可能な整数結果からの候補自動生成は行わない。

## 候補表

実際に採用する候補値は、Normal難易度のExpressionCandidateを用いた評価結果を確認して別途確定する。
パラメータなしの行は、そのまま有効な候補として扱う。
「（未決定）」を含む行は入力用の雛形であり、MissionParameterCandidateとして読み込まない。
具体値を決定したら「（未決定）」を整数へ置き換える。
同じKindに複数の具体値を用意する場合は、そのKindの行を複製し、許可するパラメータの組を1行ずつ入力する。
未決定行を残したまま実装用Configとして確定しない。

| Kind | ParameterA | ParameterB | IntendedTier | Note |
| --- | --- | --- | --- | --- |
| Even |  |  | Tier1 | パラメータなし・確定 |
| Odd |  |  | Tier1 | パラメータなし・確定 |
| Integer |  |  | Tier1 | パラメータなし・確定 |
| Decimal |  |  | Tier1 | パラメータなし・確定 |
| Positive |  |  | Tier1 | パラメータなし・確定 |
| Negative |  |  | Tier1 | パラメータなし・確定 |
| Prime |  |  | Tier2 | パラメータなし・確定 |
| Zero |  |  | Tier2 | パラメータなし・確定 |
| Square |  |  | Tier2 | パラメータなし・確定 |
| Composite |  |  | Tier3 | パラメータなし・確定 |
| AtLeast | （未決定） |  | Tier1 | 下限N。必要数だけ行を複製 |
| AtMost | （未決定） |  | Tier1 | 上限N。必要数だけ行を複製 |
| Exact | （未決定） |  | Tier2 | 目標値N。必要数だけ行を複製 |
| Multiple | （未決定） |  | Tier2 | 基準N。必要数だけ行を複製 |
| Between | （未決定） | （未決定） | Tier2 | 幅広候補。下限Nと幅Wの組 |
| Between | （未決定） | （未決定） | Tier3 | 幅狭候補。下限Nと幅Wの組 |
| Absolute | （未決定） |  | Tier3 | 絶対値N。必要数だけ行を複製 |
| OnesDigit | （未決定） |  | Tier3 | 一の位N。必要数だけ行を複製 |
| Modulo | （未決定） | （未決定） | Tier4 | 法Nと剰余Mの組 |
| DecimalFirstDigit | （未決定） |  | Tier4 | 小数第1位N。必要数だけ行を複製 |

具体値を入力した各行は、[CCCテスト仕様](../testing/acceptance-specification.md)のパラメータ制約とMissionCatalog成立条件をすべて満たすことを確認する。

## 関連仕様

- [生成するMission](./mission-types.md)
- [Missionカタログ・抽選内部仕様](./catalog-and-draw.md)
- [ゲームモード・難易度](../gameplay/game-modes-and-difficulty.md)
- [CCCテスト仕様](../testing/acceptance-specification.md)

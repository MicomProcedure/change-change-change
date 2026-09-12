# 生成するMission

> [ドキュメント一覧](../../README.md) / 区分: 要件
>
> 正本: [Cosense「生成するMission」](https://scrapbox.io/proto-TeamL/%E7%94%9F%E6%88%90%E3%81%99%E3%82%8BMission)
> 取得時点の原文更新日時: 2026-08-27T06:43:05.000Z


[CCC要件書](../overview.md)から分離した、Missionの種類、判定、候補成立条件の仕様。
具体的なクラス構成は定義しない。
簡易版と本実装版で候補の取得方法とSession中の抽選方法が異なるが、MissionConditionの判定規則は共通とする。

追加したいMission案
N（m進数表記）を10進数で表せ

## 通常Mission

| Tier | 説明 | 判定 | Key | 表示 |
| --- | --- | --- | --- | --- |
| 1 | 偶数 | 整数かつ絶対値を2で割った余りが0。0を含む | Even | 偶数にしろ |
| 1 | 奇数 | 整数かつ絶対値を2で割った余りが1 | Odd | 奇数にしろ |
| 1 | 整数 | 計算結果に小数部分がない | Integer | 整数にしろ |
| 1 | 小数 | 計算結果に小数部分がある | Decimal | 小数にしろ |
| 1 | 正の数 | 0より大きい有理数 | Positive | 正の数にしろ |
| 1 | 負の数 | 0より小さい有理数 | Negative | 負の数にしろ |
| 1 | N以上 | Nを含む | AtLeast | N以上にしろ |
| 1 | N以下 | Nを含む | AtMost | N以下にしろ |
| 2 | N | 整数かつ値がN | Exact | Nにしろ |
| 2 | 素数 | 2以上の正の整数かつ約数が1と自身のみ | Prime | 素数にしろ |
| 2 | Nの倍数 | 整数のみ。負の倍数と0を含む | Multiple | Nの倍数にしろ |
| 2 | 0 | 整数かつ値が0 | Zero | 0にしろ |
| 2 | 平方数 | 0以上の整数平方。0と1を含む | Square | 平方数にしろ |
| 2 | N以上M以下（幅広い） | NとMを両方含む | Between | N以上M以下にしろ |
| 3 | 絶対値N | 整数結果のみ | Absolute | 絶対値をNにしろ |
| 3 | 一の位N | 整数結果の符号を除いた一の位 | OnesDigit | 一の位をNにしろ |
| 3 | 合成数 | 1より大きく素数ではない整数 | Composite | 合成数にしろ |
| 3 | N以上M以下（幅狭い） | NとMを両方含む | Between | N以上M以下にしろ |
| 4 | 法NでM | 整数結果のみ。ユークリッド剰余を使用 | Modulo | Nで割った時のあまりをMにしろ |
| 4 | 小数第1位N | 非整数結果の符号を除いた小数第1位 | DecimalFirstDigit | 小数第1位をNにしろ |

小数桁のお題は第1位だけを使用する。第2位、第3位のお題は出題しない。

Betweenの表示上の上限Mは、内部パラメータの下限Nと幅Wから N + W として求める。Wは1以上とする。

簡易版では許可するNとWの組を[Missionパラメータ候補プリセット](./candidate-presets.md)へ明示し、プリセットに記載したIntendedTierを使用する。
本実装版ではExpressionMatchRatioがBetweenWideRatioThreshold以上なら幅広いMissionとしてTier2、未満なら幅狭いMissionとしてTier3にできる。
Normalの本実装版閾値は0.25とし、同値はTier2とする。
ここでいう幅広・幅狭は数値上のWだけではなく、到達可能なExpressionCandidateに対する式適合割合を基準とする。

## NegatedMission（本実装版）

NegatedMissionは簡易版では生成も抽選もしない。本実装版でのみ次の規則を使用する。

| 元のMission | 生成可否 | NegatedMission | デフォルト実効Tier | 理由 |
| --- | --- | --- | --- | --- |
| 偶数 | 可 | 偶数にするな | Tier2 | 否定形を瞬時に解釈する負荷によって難易度を上げる |
| 奇数 | 可 | 奇数にするな | Tier2 | 否定形を瞬時に解釈する負荷によって難易度を上げる |
| 整数 | 可 | 整数にするな | Tier2 | 否定形を瞬時に解釈する負荷によって難易度を上げる |
| 小数 | 可 | 小数にするな | Tier2 | 否定形を瞬時に解釈する負荷によって難易度を上げる |
| Nにしろ | 不可 | 生成しない | - | N以外のほぼ全結果が正解となり、Nの設定価値が低い |
| 正の数 | 可 | 正の数にするな | Tier2 | 0以下を要求する条件になる |
| 負の数 | 可 | 負の数にするな | Tier2 | 0以上を要求する条件になる |
| N以上 | 可 | N以上にするな | Tier3 | N未満を要求する条件になる |
| N以下 | 可 | N以下にするな | Tier3 | Nより大きい値を要求する条件になる |
| 素数 | 不可 | 生成しない | - | 非整数や負数まで正解となり、正解範囲が広すぎる |
| Nの倍数 | 不可 | 生成しない | - | Nが大きいほど正解範囲が広くなりすぎる |
| 0 | 不可 | 生成しない | - | 0以外の全結果が正解となる |
| 平方数 | 不可 | 生成しない | - | 非整数を含む正解範囲が広すぎる |
| 小数第1位をN | 不可 | 生成しない | - | 10通り中9通りが許容されやすい |
| 絶対値をN | 不可 | 生成しない | - | Nと-N以外のほぼ全結果が正解となり、Nの設定価値が低い |
| 一の位をN | 不可 | 生成しない | - | 10通り中9通りが許容されやすい |
| 合成数 | 不可 | 生成しない | - | 非整数や1以下まで正解となり、正解範囲が広すぎる |
| N以上M以下 | 可 | N以上M以下にするな | Tier4 | 指定範囲の外側を要求する条件になる |
| 法NでM | 不可 | 生成しない | - | 多くの剰余が許容され、Mの設定価値が低い |

## 共通：MissionCandidateの評価

Gameplay開始時に、正常なExpressionCandidateを基にMissionParameterCandidateを評価する。
この段階では実際に出題するMissionを選ばない。
各候補について次を求める。

```text
ExpressionMatchCount:
   Missionに適合する正常なExpressionCandidateの数
```

- NonMatchingExpressionCount:
    - Missionに適合しない正常なExpressionCandidateの数

- ExpressionMatchRatio:
    - ExpressionMatchCount
    - ÷
    - 正常なExpressionCandidateの総数

0除算は正常なExpressionCandidateに含めず、件数と式適合割合の計算対象にしない。
MissionCatalogには次をすべて満たす候補だけを登録する。

- 正常な適合式が1件以上存在する。
- 正常な非適合式が1件以上存在する。
- ExpressionMatchRatioがMinExpressionMatchRatio以上である。
- ExpressionMatchRatioがMaxExpressionMatchRatio以下である。
- ZeroとPrime以外はExpressionMatchCountが2以上である。
- ZeroとPrimeはExpressionMatchCountが1以上である。
- 失敗時に表示できる正常なAnswerExampleを1件以上持つ。
- Kind固有のパラメータ条件を満たす。

Normalの初期値はMinExpressionMatchRatio 0.10、MaxExpressionMatchRatio 0.50とする。
TargetExpressionMatchRatio 0.25は本実装版のSession中抽選に使用し、MissionCatalogから候補を削除する条件には使用しない。
これらの値はGameDifficultyごとのConfig対象とする。

## 簡易版：用意済み候補の評価

簡易版ではMissionパラメータを自動生成しない。
[Missionパラメータ候補プリセット](./candidate-presets.md)に用意された有限個の候補を読み込み、共通の評価規則を適用する。
評価を通過した候補だけをMissionCatalogへ登録する。
パラメータを持たないKindも候補表に明示する。
NegatedMissionと複合Missionの候補は用意しない。

Betweenは下限Nと幅Wの許可する組を1行ずつ明示する。
Nの集合とWの集合から直積を作らない。
上限はN + Wから求める。

## 本実装版：候補の自動生成

本実装版では正常なExpressionCandidateから到達可能な整数結果IntegerResultsを求める。
AtLeast、AtMost、Exact、Multiple、Absolute、OnesDigit、Moduloなどの候補は、Kind固有の制約を適用して生成する。
境界値の生成元はThresholdMinからThresholdMaxまでの全整数ではなく、IntegerResultsまたは正常な結果のfloorとceilを重複除去したIntegerBoundaryResultsに限定する。
Betweenも無制限なNとMの組合せを作らず、到達可能な結果を基準に候補を生成する。
生成後の候補には簡易版と同じ共通評価規則を適用する。

## Session中の抽選との分離

MissionCatalogの準備ではCurrentExpression、現在Section、直前のMissionを使わない。
簡易版のSession中抽選ではTierを0.60／0.40で選び、Kindとパラメータを一様抽選する。
本実装版のSession中抽選ではCurrentExpression未達成条件、NegatedMission相対重み、連続出題回避、TargetExpressionMatchRatioを使用する。
詳細は[Missionカタログ・抽選内部仕様](./catalog-and-draw.md)を参照する。

## 複合Mission

簡易版では複合Missionを生成、評価、抽選、表示しない。
将来の成立条件は[複合Mission](./compound-missions.md)に定義する。

## 関連仕様

- [Missionカタログ・抽選内部仕様](./catalog-and-draw.md)
- [Missionパラメータ候補プリセット](./candidate-presets.md)
- [ゲームモード・難易度](../gameplay/game-modes-and-difficulty.md)
- [CCCテスト仕様](../testing/acceptance-specification.md)

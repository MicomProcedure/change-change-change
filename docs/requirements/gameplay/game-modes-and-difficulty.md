# ゲームモード・難易度

> [ドキュメント一覧](../../README.md) / 区分: 要件
>
> 正本: [Cosense「ゲームモード・難易度」](https://scrapbox.io/proto-TeamL/%E3%82%B2%E3%83%BC%E3%83%A0%E3%83%A2%E3%83%BC%E3%83%89%E3%83%BB%E9%9B%A3%E6%98%93%E5%BA%A6)
> 取得時点の原文更新日時: 2026-08-28T14:16:27.000Z


[CCC要件書](../overview.md)から分離した、ゲームモード、難易度プリセット、Mission実装段階の仕様。

## GameDifficulty

Session開始時にゲームモードとGameDifficultyを`GameplayStartRequest`で確定する。
完成対象のMainMenuはゲームモードに`LifeEndless`、GameDifficultyに`Normal`を設定してGameplayを開始する。

| GameDifficulty | 計算結果表示 | 数字範囲 | 実装優先度 |
| --- | --- | --- | --- |
| Easy／かんたん | 表示する | 1〜5 | 追加実装 |
| Normal／ふつう | 表示しない | 1〜5 | プロトスプリントリーグ必須範囲 |
| Hard／むずかしい | 表示しない | -5〜5 | 追加実装 |

完成対象はNormalとする。

## Mission実装段階

Missionの実装段階とGameDifficultyは別の概念とする。
GameDifficultyはプレイヤーが選ぶルールプリセットであり、実装段階は開発中に使用するMission生成・抽選方式である。

| 項目 | 簡易版 | 本実装版 |
| --- | --- | --- |
| パラメータ候補 | 用意済み候補 | 到達可能な結果から自動生成可能 |
| CurrentExpression考慮 | なし | 未達成Missionだけを抽選 |
| NegatedMission | なし | 相対重みで抽選 |
| 連続出題回避 | なし | あり |
| 複合Mission | なし | 将来拡張 |
| Tier抽選 | 現在0.60／下位0.40 | 現在0.60／下位0.40 |

最初の完成対象は簡易版とする。
本実装版への置換条件と処理差分は[Missionカタログ・抽選内部仕様](../missions/catalog-and-draw.md)に定義する。

## ライフ制エンドレスモード

- 固定の最終Sectionやクリア条件を設けない。
- Mission失敗時にLifeを1減らす。
- Lifeが残っている限りGameplayを継続する。
- Lifeが0になるとSessionを終了し、結果画面へ移る。
- 初期Lifeは3とする。
- 結果画面には正解した問題数と最終到達Sectionを表示する。
- 正解した問題数は max(0, Section - 1) として導出する。

## Config対象

次の値はプレイヤーが個別入力する設定ではなく、ゲームモードまたはGameDifficultyのプリセットとして定義する。
Session開始時に確定し、同じSession中は変更しない。

- 数字範囲
- 初期式
- タイマーの初期値、短縮量、下限
- 初期Life
- SectionとTierの対応
- ShowCurrentAnswer
- CurrentTierSelectionWeight
- LowerTierSelectionWeight
- MinExpressionMatchRatio
- MaxExpressionMatchRatio
- TargetExpressionMatchRatio
- MinimumExpressionMatchCount
- ZeroおよびPrimeのMinimumExpressionMatchCount
- NegatedMissionSelectionWeight
- CompoundMissionSelectionWeight
- 本実装版の目標式適合割合に対する抽選プール数
- 本実装版のBetween実効Tier判定閾値

TargetExpressionMatchRatioは難易度ごとに変更できる。
簡易版ではパラメータ候補を一様抽選するため使用しない。
本実装版では同じKindの候補を優先順位付けするときに使用する。

## Normal用プリセット

| 項目 | 値 |
| --- | --- |
| 数字範囲 | 1〜5 |
| タイマー | 初期10秒、Sectionごとに0.5秒短縮、下限3秒 |
| 初期Life | 3 |
| Tier進行 | [ゲーム進行](./game-flow.md)を参照 |
| ShowCurrentAnswer | false |
| CurrentTierSelectionWeight | 0.60 |
| LowerTierSelectionWeight | 0.40 |
| MinExpressionMatchRatio | 0.10 |
| MaxExpressionMatchRatio | 0.60 |
| TargetExpressionMatchRatio | 0.20 |
| MinimumExpressionMatchCount | 2 |
| Zero／PrimeのMinimumExpressionMatchCount | 1 |
| NegatedMissionSelectionWeight | 0.60 |
| CompoundMissionSelectionWeight | 0 |
| 本実装版のBetween実効Tier判定閾値 | 0.25 |
| 本実装版のMissionパラメータ抽選プール数 | 3 |

NegatedMissionSelectionWeightは肯定Missionの基準重み1.00に対する相対重みである。
0.60の場合、候補数など他条件が同じならNegated対肯定の期待出題比率は6対10となる。
1.00でもNegatedMissionだけにはならず、期待比率は1対1となる。

式適合割合の最小値と最大値はMissionCatalogへの採用条件である。
目標値は本実装版のSession中抽選にだけ使用する。
各値は候補分布を確認して調整する。

## 関連仕様

- [CCC内部仕様](../internals/game-invariants.md)
- [Missionカタログ・抽選内部仕様](../missions/catalog-and-draw.md)
- [Missionパラメータ候補プリセット](../missions/candidate-presets.md)
- [CCCテスト仕様](../testing/acceptance-specification.md)

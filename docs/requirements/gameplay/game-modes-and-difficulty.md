# ゲームモード・難易度

> [ドキュメント一覧](../../README.md) / 区分: 要件

[CCC要件書](../overview.md)から分離した、ゲームモード、難易度プリセット（`GameDifficulty`）、およびミッション（Mission）の実装段階に関する仕様を定義する。

## GameDifficulty

Session 開始時に、ゲームモードおよび難易度設定（`GameDifficulty`）を `GameplayStartRequest` 経由で確定すること。
標準実装対象となるメインメニュー（`MainMenu`）では、ゲームモードに `LifeEndless`、難易度に `Normal` を設定して Gameplay を開始すること。

| GameDifficulty | 計算結果表示 (`ShowCurrentAnswer`) | 数字範囲 | 実装優先度 |
| --- | --- | --- | --- |
| **Easy** / かんたん | 表示する (`true`) | 1〜5 | 追加実装 |
| **Normal** / ふつう | 表示しない (`false`) | 1〜5 | プロトスプリントリーグ必須範囲（基本ターゲット） |
| **Hard** / むずかしい | 表示しない (`false`) | -5〜5 | 追加実装 |

※デフォルトの完成ターゲットは **Normal** とする。

## Mission実装段階の区分

ミッションの実装段階と `GameDifficulty` は独立した別の概念とする。 `GameDifficulty` がプレイヤーの選択するルールプリセットであるのに対し、実装段階は開発プロセスにおいて適用するミッション生成・抽選アルゴリズムのフェーズを表す。

| 項目 | 簡易版 | 本実装版 |
| --- | --- | --- |
| **パラメータ候補** | 定義済みプリセット候補のみ | 到達可能な結果から自動生成可能 |
| **CurrentExpression考慮** | なし（現在の式を評価しない） | 初回は考慮せず、2問目以降は達成可能候補群をConfig確率で選択 |
| **NegatedMission** | なし | 相対重みづけによる抽選 |
| **連続出題回避** | なし | あり（直前と同種パラメータを除外） |
| **複合Mission** | なし | 将来拡張 |
| **Tier抽選重み** | 現在Tier `0.60` / 下位Tier `0.40` | 現在Tier `0.60` / 下位Tier `0.40` |

※開発の最初のターゲットは「簡易版」とし、本実装版への置き換え条件および挙動の差分については [Missionカタログ・抽選内部仕様](../missions/catalog-and-draw.md) にて定義すること。

## ライフ制エンドレスモード

- 固定の最終 Section や明示的なクリア条件（エンディング）は設けないこと。
- ミッションのクリア失敗時に Life を 1 減算すること。
- Life が残っている限り Gameplay を無制限に継続すること。
- Life が 0 に達した時点で Session を終了し、リザルト（結果）画面へ遷移すること。
- 初期 Life は `3` とする。
- リザルト画面には、正解した問題数および最終到達 Section を表示すること。正解問題数は `max(0, Section - 1)` により算定すること。

## Config（設定）対象パラメーター

以下のパラメーター群は、プレイヤーが個別に設定変更するものではなく、ゲームモードまたは `GameDifficulty` のプリセット値として保持すること。これらは Session 開始時に確定され、同一 Session 中に変更されないこと。

- 数字選択範囲（`NumberRange`）
- 初期式（`InitialExpression`）
- タイマー初期値・短縮量・下限制限時間
- 初期ライフ数（`InitialLife`）
- Section と Tier の対応マップ
- `ShowCurrentAnswer`（現在式の計算結果表示フラグ）
- `CurrentTierSelectionWeight`（現在Tier抽選重み）
- `LowerTierSelectionWeight`（下位Tier抽選重み）
- `MinExpressionMatchRatio` / `MaxExpressionMatchRatio`
- `CurrentExpressionSatisfiedMissionSelectionRatio`（現在式で達成できるMission候補群の選択確率）
- `TargetExpressionMatchRatio`（目標式適合率）
- `MinimumExpressionMatchCount`（最小適合式数）
- `Zero` / `Prime` における `MinimumExpressionMatchCount`
- `NegatedMissionSelectionWeight`（反転ミッション抽選重み）
- `CompoundMissionSelectionWeight`（複合ミッション抽選重み）
- 本実装版における目標式適合率に対する抽選プールサイズ
- 本実装版における `Between` の実効 Tier 判定閾値

※`TargetExpressionMatchRatio` は難易度ごとに設定可能とする。簡易版では候補を一律ランダム抽選するため使用されず、本実装版において同一 Kind 内の候補を優位順にソート・フィルタリングする際に利用すること。

## Normalモード用プリセット設定値

| パラメーター項目 | 設定値 |
| --- | --- |
| **数字範囲** | `1〜5` |
| **タイマー** | 初期 `10秒`、Sectionごとに `0.5秒` 短縮、下限 `3秒` |
| **初期Life** | `3` |
| **Tier進行** | [ゲーム進行](./game-flow.md) を参照 |
| **ShowCurrentAnswer** | `false` |
| **CurrentTierSelectionWeight** | `0.60` |
| **LowerTierSelectionWeight** | `0.40` |
| **MinExpressionMatchRatio** | `0.10` |
| **MaxExpressionMatchRatio** | `0.60` |
| **CurrentExpressionSatisfiedMissionSelectionRatio** | `0.10` |
| **TargetExpressionMatchRatio** | `0.20` |
| **MinimumExpressionMatchCount** | `2` |
| **Zero / Prime MinimumExpressionMatchCount** | `1` |
| **NegatedMissionSelectionWeight** | `0.60` |
| **CompoundMissionSelectionWeight** | `0` |
| **本実装版 Between実効Tier判定閾値** | `0.25` |
| **本実装版 Missionパラメータ抽選プール数** | `3` |

> [!NOTE]
> - `NegatedMissionSelectionWeight` は、肯定ミッションの基準重み `1.00` に対する相対重みとする。`0.60` に設定した場合、他条件が同等であれば Negated と肯定の期待出題比率は `6 : 10` となる（`1.00` の場合は `1 : 1`）。
> - `CurrentExpressionSatisfiedMissionSelectionRatio` は候補件数に対する重みではなく、達成できる候補群を選択する直接確率とする。両候補群が存在するとき、`0.10`なら達成できる候補群を10%、達成できない候補群を90%で選ぶこと。
> - 式適合割合の最小値（`Min`）と最大値（`Max`）は、`MissionCatalog` への登録採用条件として使用すること。目標値（`Target`）は本実装版の Session 内抽選でのみ適用すること。

---
- 内部仕様: [CCC内部仕様](../internals/game-invariants.md)
- カタログ・抽選詳細: [Missionカタログ・抽選内部仕様](../missions/catalog-and-draw.md)
- プリセット定義: [Missionパラメータ候補プリセット](../missions/candidate-presets.md)
- テスト仕様: [CCCテスト仕様](../testing/acceptance-specification.md)

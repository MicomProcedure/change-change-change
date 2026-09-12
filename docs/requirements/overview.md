# CCC要件書

> [ドキュメント一覧](../README.md) / 区分: 要件
>
> 正本: [Cosense「CCC要件書」](https://scrapbox.io/proto-TeamL/CCC%E8%A6%81%E4%BB%B6%E6%9B%B8)
> 取得時点の原文更新日時: 2026-08-28T14:16:44.000Z


## 関連ドキュメント

- [CCC内部仕様](./internals/game-invariants.md)
- [Missionカタログ・抽選内部仕様](./missions/catalog-and-draw.md)
- [Missionパラメータ候補プリセット](./missions/candidate-presets.md)
- [CCCテスト仕様](./testing/acceptance-specification.md)

## 詳細仕様

| カード | 内容 |
| --- | --- |
| [ゲームモード・難易度](./gameplay/game-modes-and-difficulty.md) | 難易度、数字範囲、LifeEndless、Normalプリセット |
| [ゲーム進行](./gameplay/game-flow.md) | Session、Section、初期式、Tier、タイマー、Submit、ゲームオーバー |
| [式と数値計算](./gameplay/expression-and-calculation.md) | 式の構造、0除算、正確な計算、小数表示、剰余 |
| [生成するMission](./missions/mission-types.md) | 通常Mission、判定、Tier、NegatedMission、生成・成立保証 |
| [複合Mission](./missions/compound-missions.md) | 2条件Missionの生成・判定・フォールバック。なお、実装としては後回し |
| [入力仕様](./gameplay/input.md) | 数字・演算子の変更、タッチ領域、マルチタッチ |
| [画面・UI仕様](./presentation/screens-and-ui.md) | 画面ごとの表示内容、画面遷移、スマートフォンレイアウト |
| [フィードバック・演出](./presentation/feedback-and-presentation.md) | 入力、成功、失敗、回答例の演出 |
| [CCC内部仕様](./internals/game-invariants.md) | 内部状態、計算規則、進行上の不変条件 |
| [Missionカタログ・抽選内部仕様](./missions/catalog-and-draw.md) | カタログ準備、簡易版と本実装版の抽選規則 |
| [Missionパラメータ候補プリセット](./missions/candidate-presets.md) | 簡易版で評価する候補データの形式 |
| [CCCテスト仕様](./testing/acceptance-specification.md) | 仕様を一文ずつ確認するTDDチェックリスト |

---

## ゲーム概要

Change Change Change（略称CCC）は、2人が同じ画面と端末を使って遊ぶローカル協力型の計算ゲームである。
PLAYER Aは左の数字、PLAYER Bは右の数字を担当する。中央の演算子は共有されており、どちらのプレイヤーからも変更できる。
プレイヤーは制限時間内に次の式を操作し、画面に提示されたミッションを満たすことを目指す。

```text
PLAYER Aの数字  [共有演算子]  PLAYER Bの数字
```

ゲームはセクション制で進行し、成功すると次のセクションへ進む。失敗するとライフを失い、ライフが0になるとゲームオーバーとなる。

### 対象プレイ形式

- プレイヤー数：2人固定
- 協力／対戦：協力
- プレイ環境：同一端末
- 対象端末：マルチタッチ対応スマートフォン（Android想定）
- 入力方式：タッチのみ
- オンライン通信：使用しない
- プレイヤーごとの個別画面：使用しない

---

## 用語

| 用語 | 意味 |
| --- | --- |
| CurrentExpression | 現在プレイヤーが操作している式。左の数字、演算子、右の数字を持つ |
| MissionCondition | 「偶数」「5以上」などの個別条件 |
| Mission | 今回提示されるお題全体。1個または2個のMissionConditionを持つ |
| MissionId | 新しいMissionを表示するたびに採番する実行時識別子。古いタイマー通知やSubmitを無視するために使用する |
| NegatedMission | MissionConditionの判定を反転し、「条件を満たすな」と要求する出題 |
| ExpressionMatchRatio | 式適合割合。正常なExpressionCandidateのうちMissionに適合する最終式の割合 |
| Gameplay | タイトルと結果画面を除く、プレイヤーがMissionと式を操作するゲームのメインコンテンツ部分 |
| Session | Gameplayの開始からゲームオーバー、またはMainMenuへ戻るまでの1回のプレイ全体 |
| SessionResult | Session終了時に結果画面へ渡す不変スナップショット。最終到達Section、最後のMission、確定した式、最後の判定結果を持つ |
| Section | Session内の進行番号。1から始まり、Mission成功時に1増える。失敗時は同じSectionのまま新しいMissionを出題する |
| CorrectAnswerCount | 正解した問題数。max(0, Section - 1)としてSectionから導出し、独立した進行状態として保持しない |
| LifeEndless | Lifeが0になるまでSectionを繰り返すライフ制エンドレスモード |
| GameDifficulty | Session全体のルールプリセット。Easy、Normal、Hardの3種類 |
| Tier | Missionの難易度段階 |
| Submit | プレイヤーが現在の式を確定し、Mission判定を要求する操作 |

`CurrentExpression`の「Expression」は式を意味し、ゲームの進行状態を意味するStateではない。

`Mission`は画面に表示される今回のお題全体であり、単一条件と複合条件を同じ形式で扱う。

1回のSessionには複数のSectionが含まれる。MainMenuからGameplayへ移るとき、または結果画面で再挑戦するときに新しいSessionを開始し、Section、Life、CurrentExpressionはそのSession内だけで保持する。正解した問題数は`max(0, Section - 1)`としてSectionから導出し、独立した進行状態として保持しない。

---

## プロトスプリントリーグの必須範囲に含まれないもの

将来拡張の参照先：[Missionカタログ・抽選内部仕様](./missions/catalog-and-draw.md)「本実装版：Missionパラメータ候補の自動生成」

次は今回のプロトスプリントリーグの必須範囲には含めない。追加する場合は別途仕様化する。

- EasyとHardのGameDifficulty実装
- ゲームモード・難易度選択画面
- オンラインマルチプレイ
- CPUプレイヤー
- プレイヤー別端末
- ポーズ
- セーブデータ
- ハイスコア永続保存
- ResultFeatureIndex、BitSet、逆像を用いたMissionパラメータの直接生成
- 現在式からの入力回数を用いたMission難易度評価
- Submitした式に最も近い回答例の選択
- ランキング
- 実績
- 音声、BGM、振動
- チュートリアル専用ステージ
- アクセシビリティ設定
- ローカライズ

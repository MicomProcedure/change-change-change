# CCC 要件書

> [ドキュメント一覧](../README.md) / 区分: 要件

---

## 関連ドキュメント・内部仕様

- [CCC内部仕様](./internals/game-invariants.md)
- [Missionカタログ・抽選内部仕様](./missions/catalog-and-draw.md)
- [Missionパラメータ候補プリセット](./missions/candidate-presets.md)
- [CCCテスト仕様](./testing/acceptance-specification.md)

---

## 仕様ドキュメント一覧

| ドキュメント | 主な記載内容 |
| --- | --- |
| [ゲームモード・難易度](./gameplay/game-modes-and-difficulty.md) | 難易度設定、数値範囲、LifeEndlessモード、Normalモードのプリセット |
| [ゲーム進行](./gameplay/game-flow.md) | セッション、セクション、初期式、Tier、タイマー、Submit、ゲームオーバー条件 |
| [式と数値計算](./gameplay/expression-and-calculation.md) | 数式の構造、ゼロ除算の扱い、正確な有理数計算、小数表示、剰余計算 |
| [生成するMission](./missions/mission-types.md) | 通常Missionの仕様、クリア判定、Tier分類、NegatedMission |
| [複合Mission](./missions/compound-missions.md) | 2条件Missionの生成・判定（※現時点では後回し項目） |
| [入力仕様](./gameplay/input.md) | 数字・演算子の変更操作、タッチ領域、マルチタッチ制御 |
| [画面・UI仕様](./presentation/screens-and-ui.md) | 各画面の表示要素、画面遷移、スマートフォン向けUIレイアウト |
| [フィードバック・演出](./presentation/feedback-and-presentation.md) | 操作入力、成功・失敗、回答例提示の演出仕様 |
| [CCC内部仕様](./internals/game-invariants.md) | 内部状態管理、計算規則、ゲーム進行上の不変条件（Invariants） |
| [Missionカタログ・抽選内部仕様](./missions/catalog-and-draw.md) | 自動生成、カタログ構築、本実装版の出題抽選規則 |
| [Missionパラメータ候補PresetDebug](./missions/candidate-presets.md) | DebugMode限定候補データの仕様 |
| [CCCテスト仕様](./testing/acceptance-specification.md) | 仕様項目の検証用 TDD チェックリスト |

---

## ゲーム概要

**Change Change Change（略称: CCC）** は、2人のプレイヤーが1台の端末（同じ画面）を使用してプレイする、ローカル協力型の計算パズルゲームとする。

- **PLAYER A**: 画面左側の数字を担当する。
- **PLAYER B**: 画面右側の数字を担当する。
- **中央の演算子**: 左右両方のプレイヤーから自由に共有して変更可能とする。

プレイヤー同士で協力し、制限時間内に以下の数式を操作して、提示された Mission（クリア条件）を満たす解の構築を目指す。

```
[PLAYER Aの数字]  [共有演算子]  [PLAYER Bの数字]
```

ゲームは**セクション制**で進行する。Mission に成功すると次のセクションへ進み、失敗するとライフを失う。ライフが 0 になるとゲームオーバーとする。

### 対象プレイ形式・動作条件

- **プレイヤー数**: 2人固定とする。
- **プレイスタイル**: 協力プレイとする。
- **プレイ環境**: 同一端末によるローカルマルチとする。
- **対象端末**: マルチタッチ対応スマートフォン（Android想定）とする。
- **入力方式**: タッチ操作のみとする。
- **オンライン通信**: 使用しないこと。
- **プレイヤー別画面**: 使用しない（単一画面）こと。

---

## 主要用語の定義

| 用語 | 定義・役割 |
| --- | --- |
| **CurrentExpression** | プレイヤーが現在操作中の数式（左数字、演算子、右数字の組み合わせ） |
| **MissionCondition** | 「偶数」「5以上」などの個別の判定条件 |
| **Mission** | 画面に提示される今回のお題全体（1つまたは複数の MissionCondition で構成） |
| **MissionId** | 新しい Mission が提示されるたびに採番される連番識別子 |
| **NegatedMission** | 条件判定を反転させ、「条件を満たさないこと」を要求する出題 |
| **ExpressionMatchRatio** | 式適合割合（全候補式のうち、該当 Mission 条件に合致する式の割合） |
| **Gameplay** | タイトル画面や結果画面を除く、プレイヤーが式を操作してゲームを遊ぶメインコンテンツ部分 |
| **Session** | Gameplay の開始からゲームオーバー、またはメインメニューへ戻るまでの1回のプレイ全体 |
| **SessionResult** | セッション終了時に結果画面へ引き渡される不変スナップショット |
| **Section** | セッション内の進行番号（1から開始し、成功時に1増加。失敗時は同じSectionのまま新しいMissionが出題される） |
| **CorrectAnswerCount** | 正解した問題数（`max(0, Section - 1)` としてセクション数から動的に導出される） |
| **LifeEndless** | ライフが 0 になるまでセクションを更新し続けるエンドレスモード |
| **GameDifficulty** | セッション全体の難易度プリセット（Easy / Normal / Hard） |
| **Tier** | Mission の難易度段階 |
| **Submit** | プレイヤーが現在の式を確定し、判定を依頼する提出操作 |

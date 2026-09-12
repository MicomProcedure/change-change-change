# CCCドキュメント

> 仕様・要件の正本は[Cosense「proto-TeamL」](https://scrapbox.io/proto-TeamL/)です。
> このディレクトリは、実装時に参照しやすいようCosenseの対象ページをMarkdownへ変換したスナップショットです。内容が競合する場合はCosenseを優先してください。

## 読み方

1. ゲームの目的と対象範囲は[CCC要件書](./requirements/overview.md)から確認します。
2. ゲーム進行とMission規則は`requirements/`以下を参照します。
3. 実装上の責務と依存方向は[CCC Unityクラス設計](./architecture/unity-class-design.md)を起点に確認します。
4. `設計解説`は理解を助ける資料であり、規範文書と競合する場合は規範文書を優先します。
5. `将来要件`は現在の必須実装範囲とは分離して扱います。

## 更新運用

- 仕様、要件、設計判断を変更するときは、先にCosenseの対応ページを更新します。
- `docs/`は自動同期されません。Cosense更新後に該当文書を再転記し、各文書の原文更新日時も更新します。
- 転記後は、`docs/README.md`からの索引リンク、文書間の相対リンク、コードブロック、表を確認します。
- 内容上の競合をローカル文書だけで解消しません。競合はCosense側で解消してから再転記します。

## 要件・仕様

ゲームの外部仕様、内部不変条件、Mission仕様、受け入れ仕様です。

- [CCC要件書](./requirements/overview.md) — 要件 / 原文更新: 2026-08-28T14:16:44.000Z
- [ゲームモード・難易度](./requirements/gameplay/game-modes-and-difficulty.md) — 要件 / 原文更新: 2026-08-28T14:16:27.000Z
- [ゲーム進行](./requirements/gameplay/game-flow.md) — 要件 / 原文更新: 2026-08-26T05:28:04.000Z
- [式と数値計算](./requirements/gameplay/expression-and-calculation.md) — 要件 / 原文更新: 2026-08-26T05:26:17.000Z
- [入力仕様](./requirements/gameplay/input.md) — 要件 / 原文更新: 2026-08-26T05:26:09.000Z
- [画面・UI仕様](./requirements/presentation/screens-and-ui.md) — 要件 / 原文更新: 2026-08-29T22:39:27.000Z
- [フィードバック・演出](./requirements/presentation/feedback-and-presentation.md) — 要件 / 原文更新: 2026-08-26T05:26:21.000Z
- [生成するMission](./requirements/missions/mission-types.md) — 要件 / 原文更新: 2026-08-27T06:43:05.000Z
- [複合Mission](./requirements/missions/compound-missions.md) — 将来要件 / 原文更新: 2026-08-26T05:39:16.000Z
- [CCC内部仕様](./requirements/internals/game-invariants.md) — 内部仕様 / 原文更新: 2026-08-26T05:38:04.000Z
- [Missionカタログ・抽選内部仕様](./requirements/missions/catalog-and-draw.md) — 内部仕様 / 原文更新: 2026-08-26T05:45:49.000Z
- [Missionパラメータ候補プリセット](./requirements/missions/candidate-presets.md) — 設定仕様 / 原文更新: 2026-08-26T05:51:20.000Z
- [CCCテスト仕様](./requirements/testing/acceptance-specification.md) — テスト仕様 / 原文更新: 2026-08-26T05:31:24.000Z

## アーキテクチャ・クラス設計

依存方向、クラス境界、画面遷移、UI部品、Audio実装境界です。

- [CCC Unityクラス設計](./architecture/unity-class-design.md) — クラス設計 / 原文更新: 2026-08-29T22:02:58.000Z
- [CCC実装における設計原則](./architecture/design-principles.md) — 設計原則 / 原文更新: 2026-08-28T23:45:16.000Z
- [CCC_asmdef](./architecture/assembly-boundaries.md) — Assembly設計 / 原文更新: 2026-08-28T23:12:15.000Z
- [CCC Unity中核ロジック関係図](./architecture/core/core-logic-overview.md) — 設計解説 / 原文更新: 2026-08-28T09:48:31.000Z
- [CCC Unity画面遷移クラス設計](./architecture/presentation/scene-navigation.md) — クラス設計 / 原文更新: 2026-08-29T22:33:15.000Z
- [UIコンポーネントクラス設計](./architecture/presentation/ui-components.md) — クラス設計 / 原文更新: 2026-08-29T04:53:55.000Z
- [CCC UnityAudioクラス設計](./architecture/infrastructure/audio.md) — クラス設計 / 原文更新: 2026-08-28T23:41:54.000Z

## プロジェクト規約

ファイル配置と命名に関する開発者向け規約です。

- [Unityプロジェクトファイル構成](./project/project-layout.md) — プロジェクト構成 / 原文更新: 2026-08-28T15:06:29.000Z
- [命名規則](./project/naming-conventions.md) — 開発規約 / 原文更新: 2026-08-28T12:46:11.000Z

## 移行対象外

デザイン素材、グラフィック案、個人メモ、日程計画、旧ページは移行していません。`CCC（ゲーム概要）`と`やらないこと・作らないこと`の要件上有効な内容は、正本の`CCC要件書`に既に含まれているため独立ファイルを作成していません。

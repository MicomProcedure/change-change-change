# ChangeChangeChange ドキュメント

---

## ドキュメントの閲覧・参照順序

1. **ゲーム概要・対象範囲**: [CCC要件書](./requirements/overview.md) を参照すること。
2. **ゲーム進行・Mission仕様**: [`requirements/`](./requirements/) ディレクトリ以下の各仕様書を参照すること。
3. **アーキテクチャ・設計**: [CCC Unityクラス設計](./architecture/unity-class-design.md) を起点として、実装上の責務やモジュール間の依存方向を確認すること。
4. **設計解説資料**: `設計解説` と記載されたドキュメントは理解を補助するための解説資料とする。規範文書（要件書・クラス設計等）と内容が競合する場合は、規範文書を優先すること。
5. **将来要件の扱い**: `将来要件` と記載された項目は、現時点での必須実装範囲とは区別して管理すること。

## ディレクトリ・索引一覧

### 要件・仕様
ゲームの外部仕様、内部の不変条件、Mission仕様、および受け入れ仕様に関するドキュメント一覧。

- [CCC要件書](./requirements/overview.md) — 要件
- [ゲームモード・難易度](./requirements/gameplay/game-modes-and-difficulty.md) — 要件
- [ゲーム進行](./requirements/gameplay/game-flow.md) — 要件
- [式と数値計算](./requirements/gameplay/expression-and-calculation.md) — 要件
- [入力仕様](./requirements/gameplay/input.md) — 要件
- [画面・UI仕様](./requirements/presentation/screens-and-ui.md) — 要件
- [フィードバック・演出](./requirements/presentation/feedback-and-presentation.md) — 要件
- [生成するMission](./requirements/missions/mission-types.md) — 要件
- [複合Mission](./requirements/missions/compound-missions.md) — 将来要件
- [CCC内部仕様](./requirements/internals/game-invariants.md) — 内部仕様
- [Missionカタログ・抽選内部仕様](./requirements/missions/catalog-and-draw.md) — 内部仕様
- [Missionパラメータ候補プリセット](./requirements/missions/candidate-presets.md) — 設定仕様
- [CCCテスト仕様](./requirements/testing/acceptance-specification.md) — テスト仕様

### アーキテクチャ・クラス設計
モジュール間の依存方向、クラスの境界、画面遷移、UIコンポーネント、Audio実装境界、およびLog・診断出力に関する設計書。

- [CCC Unityクラス設計](./architecture/unity-class-design.md) — クラス設計
- [CCC実装における設計原則](./architecture/design-principles.md) — 設計原則
- [CCC_asmdef](./architecture/assembly-boundaries.md) — Assembly設計
- [CCC Unity中核ロジック関係図](./architecture/core/core-logic-overview.md) — 設計解説
- [CCC Unity画面遷移クラス設計](./architecture/presentation/scene-navigation.md) — クラス設計
- [UIコンポーネントクラス設計](./architecture/presentation/ui-components.md) — クラス設計
- [CCC UnityAudioクラス設計](./architecture/infrastructure/audio.md) — クラス設計
- [CCC Log・診断出力クラス設計](./architecture/infrastructure/logging.md) — クラス設計

### プロジェクト規約
ディレクトリ構造、ファイル配置、および命名規則に関する開発者向けガイドライン。

- [Unityプロジェクトファイル構成](./project/project-layout.md) — プロジェクト構成
- [命名規則](./project/naming-conventions.md) — 開発規約

---

## 移行対象外のリソースについて

デザイン素材、グラフィック案、個人用メモ、スケジュール計画、および旧バージョンのページはMarkdown化の対象外とする。「CCC（ゲーム概要）」や「やらないこと・作らないこと」などの要件に含まれる主要事項は、[CCC要件書](./requirements/overview.md) 内に集約すること。

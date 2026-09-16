# Unityプロジェクトファイル構成

> [ドキュメント一覧](../README.md) / 区分: プロジェクト構成

---

## 1. Unityプロジェクト直下のディレクトリ構成

```text
CCC-UnityProject/
├─ Assets/
│  └─ CCC/
├─ Packages/
│  ├─ manifest.json
│  └─ packages-lock.json
├─ ProjectSettings/
└─ UserSettings/                 # Git管理対象外
```

- **Unity管理ディレクトリ**: `Packages` および `ProjectSettings` はUnityが自動管理する設定領域とする。
- **Git排除対象**: `Library`、`Temp`、`Logs`、`obj`、`Build`、`Builds` などの自動生成フォルダはGit管理に含めないこと。
- **ビルド出力先**: Androidビルドなどの出力ファイルは、プロジェクト直下から外れたディレクトリ、またはGit管理対象外となる `Builds/Android` 配下に配置すること。

---

## 2. Assets/CCC 全体構成

```text
Assets/
└─ CCC/
   ├─ Scenes/
   │  ├─ TitleScene.unity
   │  └─ GameScene.unity
   │
   ├─ Scripts/
   │  ├─ Core/
   │  │  ├─ Model/
   │  │  ├─ Logic/
   │  │  ├─ UseCases/
   │  │  └─ Ccc.Core.asmdef
   │  ├─ Presentation/
   │  │  ├─ Gameplay/
   │  │  ├─ Navigation/
   │  │  ├─ Formatting/
   │  │  └─ Ccc.Presentation.asmdef
   │  ├─ Infrastructure/
   │  │  ├─ UI/
   │  │  │  ├─ Common/
   │  │  │  ├─ Title/
   │  │  │  ├─ Gameplay/
   │  │  │  └─ Result/
   │  │  ├─ Input/
   │  │  ├─ Feedback/
   │  │  ├─ Timer/
   │  │  ├─ Config/
   │  │  ├─ Navigation/
   │  │  └─ Ccc.Infrastructure.asmdef
   │  └─ Bootstrap/
   │     ├─ Application/
   │     ├─ Title/
   │     ├─ Gameplay/
   │     └─ Ccc.Bootstrap.asmdef
   │
   ├─ Prefabs/
   │  ├─ Application/
   │  └─ UI/
   │     ├─ Common/
   │     ├─ Title/
   │     ├─ Gameplay/
   │     └─ Result/
   │
   ├─ Data/
   │  ├─ GameplayConfig/
   │  ├─ MissionParameterGenerationProfile/
   │  ├─ MissionPreset/          # PresetDebug限定
   │  ├─ MissionText/
   │  └─ Feedback/
   │
   ├─ Fonts/
   │  ├─ Source/
   │  ├─ TextMeshPro/
   │  └─ Licenses/
   │
   ├─ Settings/
   │  ├─ Input/
   │  └─ Build/
   │
   └─ Tests/
      └─ Editor/
         ├─ Core/
         ├─ Presentation/
         ├─ Infrastructure/
         └─ Ccc.Editor.Tests.asmdef
```

- **ディレクトリ作成ポリシー**: 必要なファイルが発生した段階でディレクトリを作成すること。
- **ダミーファイルの禁止**: 空ディレクトリを維持するためだけのプレースホルダーファイル（.gitkeep等）は配置しないこと。

---

## 3. Scriptsとアセンブリ（Assembly）の配置規約

### 3.1 Core
`Assets/CCC/Scripts/Core` には、Unityエンジンに依存しない純粋なC#クラス（Model、同期ロジック、UseCases）を配置する。

- **Model**: 不変データ、値オブジェクト、`GameplayConfig`、`GameplayState`、`SessionResult`
- **Logic**: 式評価、Mission判定、Catalog構築、Selector、Reducer
- **UseCases**: Controller、`ExpressionInput`、タイマー・乱数のポート定義、Mission抽選Strategy

> **依存方向と参照制約**
> - Core内部での依存方向は `UseCases` → `Logic` → `Model` とすること。
> - `Model` および `Logic` は、`UnityEngine`、`UniTask`、`DOTween`、`VContainer`、`Input System` を参照しないこと。
> - リアクティブライブラリ（`R3`）は `UseCases` 層でのみ使用可能とする。

### 3.2 Presentation
`Assets/CCC/Scripts/Presentation` には、Presenter、View / Navigationインターフェース契約、表示用DTO、Formatterを配置する。

- `Gameplay`、`Navigation`、`Formatting` のように責務・機能単位でディレクトリを分けること。画面ごとの `MonoBehaviour` はここには配置しない。
- DTOクラスの型名は必ず `Dto` の接尾辞で終わるようにすること。

### 3.3 Infrastructure
`Assets/CCC/Scripts/Infrastructure` には、Unityエンジンやサードパーティライブラリに依存する具体的な実装を配置する。

- **UI**: シーン上のView、子View、画面表示処理
- **Input**: PointerDown入力処理および Input System との接続
- **Feedback**: DOTweenを用いた演出制御
- **Timer**: UniTaskを使用した Section Timer 実装
- **Config**: ScriptableObjectと純粋C#型とのデータ相互変換
- **Navigation**: `SceneNavigator` およびシーン内の Screen Model / View

※ SceneやPrefabから直接参照される `MonoBehaviour` クラスは、対応する機能サブフォルダ配下に配置すること。

### 3.4 Bootstrap
`Assets/CCC/Scripts/Bootstrap` には、VContainerの `LifetimeScope`、依存関係の登録（DI）、および `EntryPoint` のみを配置する。

- ライフサイクル単位（`Application`、`Title`、`Gameplay`）ごとにフォルダを分けること。
- ゲームルールや具体的表示制御などのロジックを Bootstrap 内に記述することは厳禁とする。

### 3.5 Tests
Editorテスト用コードは `Assets/CCC/Tests/Editor` に配置し、製品コードと物理的に分離すること。

- **Coreテスト対象**: `Rational`（有理数計算）、各種数値計算、Mission、Catalog、Selector、Reducer
- **PlayModeテスト**: 原則として必須としない。作成が必要となった場合のみ `Tests/PlayMode` および `Ccc.PlayMode.Tests.asmdef` を定義すること。

---

## 4. Scene（シーン）構成

### 4.1 TitleScene

```text
TitleScene
├─ TitleLifetimeScope
├─ EventSystem
│  └─ InputSystemUIInputModule
└─ TitleCanvas
   └─ TitleScreenView
      ├─ TitleScreen
      ├─ MainMenuScreen
      ├─ OptionMenuScreen
      ├─ CreditScreen
      ├─ SceneLoadingBlocker
      └─ ErrorMessageArea
```

- **画面初期表示**: 通常起動時は `TitleScreen` のみを表示すること。`GameScene` から戻ってきた場合は `MainMenuScreen` から開始すること。
- **画面切り替え**: `TitleScreenView` は `TitleScreenModel` の状態に応じて、4つのサブ画面のうち該当する1つのみを表示制御すること。
- **ロード遮断制御**: シーンロード中は `SceneLoadingBlocker` をアクティブにし、ボタン連打等の誤操作を防止すること。
- **エラー表示**: `ErrorMessageArea` は `GameScene` の起動失敗時などにエラーメッセージを表示するために使用すること。

### 4.2 GameScene

```text
GameScene
├─ GameplayLifetimeScope
├─ EventSystem
│  └─ InputSystemUIInputModule
└─ GameCanvas
   └─ GameScreenView
      ├─ GameplayScreen
      │  └─ GameplayView
      │     ├─ StatusArea
      │     │  ├─ Section
      │     │  ├─ Tier
      │     │  ├─ Life
      │     │  └─ Timer
      │     ├─ MissionArea
      │     ├─ ExpressionArea
      │     │  ├─ PlayerAInputArea
      │     │  ├─ SharedOperatorArea
      │     │  └─ PlayerBInputArea
      │     ├─ SubmitArea
      │     ├─ MainMenuButton
      │     └─ FeedbackLayer
      ├─ ResultScreen
      │  ├─ ResultSummary
      │  ├─ RetryButton
      │  ├─ MainMenuButton
      │  └─ ErrorMessageArea
      └─ SceneLoadingBlocker
```

- **表示切り替え**: `GameScreenView` は `GameplayScreen` または `ResultScreen` のいずれか一方のみを表示制御すること。
- **親Viewの責務**: `GameplayView` は子View群をまとめる親コンポーネントであり、ライフやセクションの更新判定などの判断ロジックは一切持たないこと。
- **レイヤー重なり順**: `FeedbackLayer` は通常画面表示より前面に、`SceneLoadingBlocker` はすべての操作を受け付けない最前面に配置すること。
- **リトライ失敗時**: リトライ処理に失敗した場合は `ResultScreen` の表示を維持し、`RetryButton` を無効化（Interactable=false）、`MainMenuButton` を有効化すること。

### 4.3 EventSystemの設定

- 各Sceneには `EventSystem` を必ず1つだけ配置すること。
- 入力モジュールには `InputSystemUIInputModule` を使用すること。
- Gameplay中の各種操作は `Button.onClick` と重複させず、`PointerDown` イベント通知に一本化すること。
- 左右同時入力や同一領域への2本指入力を阻害する透明パネルや不要な Raycast Target の配置は避けること。

---

## 5. Dataと設定アセット（Asset）

### 5.1 ScriptableObject

| 配置パス例 | 用途 | 生成される実行時データ |
| --- | --- | --- |
| Data/GameplayConfig/NormalGameplayConfig.asset | Normal、LifeEndlessモードの設定 | `GameplayConfig` |
| Data/MissionParameterGenerationProfile/Normal.asset | 自動生成ドメイン・上限・Kind別規則 | `MissionParameterGenerationSettings` / `MissionCatalogEvaluationPolicy` |
| Data/MissionPreset/NormalMissionPreset.asset | PresetDebug用Tier未確定候補 | `MissionParameterCandidate` 配列 |
| Data/MissionText/JapaneseMissionText.asset | Mission表示用テキスト | `IMissionTextSource` として参照 |
| Data/Feedback/DefaultFeedbackSettings.asset | 成功・失敗演出の再生時間設定 | 任意（Viewへ直接設定する場合は作成不要） |

> **運用上の注意**
> - ScriptableObject アセットを `Core` 層に直接渡さないこと。Gameplay開始時に検証を行った上で、純粋なC#型へ変換して渡すこと。
> - バリデーションエラー検出時は一覧として画面/ログに表示し、Gameplayの開始を中断すること。
> - 製品用Entryは`Automatic`を選択し、`MissionPreset`を候補供給元にしないこと。
> - Normal Profileの具体的なドメインと候補上限は、入力後に生成件数とCatalog成立を確認すること。

### 5.2 Input設定
Input System の設定アセットを独自に作成・カスタマイズする場合は `Settings/Input` 配下に配置すること。
標準のUI操作アセットで事足りる場合は、不要な Gameplay Action Map を新設しないこと。`TitleScene` と `GameScene` で同一の設定を参照するように統一すること。

### 5.3 ビルド（Build）設定
- Build Profile または Build Settings には `TitleScene`、`GameScene` の順序で登録すること。
- アプリケーションの起動シーンは必ず `TitleScene` とすること。
- Android向け設定では「画面向き: 横固定（Landscape）」に設定し、Input Handling は Input System に設定すること。
- スモークテスト用ビルドの出力生成物は、Git管理対象外の場所に配置すること。

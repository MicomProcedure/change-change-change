# Unityプロジェクトファイル構成

> [ドキュメント一覧](../README.md) / 区分: プロジェクト構成
>
> 正本: [Cosense「Unityプロジェクトファイル構成」](https://scrapbox.io/proto-TeamL/Unity%E3%83%97%E3%83%AD%E3%82%B8%E3%82%A7%E3%82%AF%E3%83%88%E3%83%95%E3%82%A1%E3%82%A4%E3%83%AB%E6%A7%8B%E6%88%90)
> 取得時点の原文更新日時: 2026-08-28T15:06:29.000Z


## 1. Unityプロジェクト直下

```text
CCC-UnityProject/
├─ Assets/
│  └─ CCC/
├─ Packages/
│  ├─ manifest.json
│  └─ packages-lock.json
├─ ProjectSettings/
└─ UserSettings/                 # Git共有対象外
```

PackagesとProjectSettingsはUnityが管理する。
Library、Temp、Logs、obj、Build、BuildsはGit管理へ含めない。
Androidの出力先はプロジェクト外、またはGit管理外のBuilds/Androidとする。

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
   │  ├─ MissionPreset/
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

フォルダは必要になった時点で作る。
空フォルダを維持するためだけのダミーファイルは置かない。

## 3. Scriptsとassemblyの配置

### 3.1 Core

Assets/CCC/Scripts/Coreへ、Unity非依存のModel、同期ロジック、UseCasesを置く。

- Model：不変データ、値オブジェクト、GameplayConfig、GameplayState、SessionResult
- Logic：式評価、Mission判定、Catalog構築、Selector、Reducer
- UseCases：Controller、ExpressionInput、Timer・乱数のポート、Mission抽選Strategy

Core内の参照方向はUseCases → Logic → Modelとする。
ModelとLogicはUnityEngine、UniTask、DOTween、VContainer、Input Systemを参照しない。
R3はUseCasesだけで使用する。

### 3.2 Presentation

Assets/CCC/Scripts/Presentationへ、Presenter、View／Navigation契約、表示DTO、Formatterを置く。
Gameplay、Navigation、Formattingのように責務単位で章分けし、画面別のMonoBehaviourは置かない。
DTOの型名はDtoで終える。

### 3.3 Infrastructure

Assets/CCC/Scripts/Infrastructureへ、Unity依存の実装を置く。

- UI：Scene上のView、子View、画面表示
- Input：PointerDown入力とInput System接続
- Feedback：DOTween演出
- Timer：UniTaskを使うSection Timer
- Config：ScriptableObjectと純C#型の変換
- Navigation：SceneNavigator、Scene内Screen Model／View

SceneやPrefabから参照されるMonoBehaviourは、機能に対応するサブフォルダへ置く。

### 3.4 Bootstrap

Assets/CCC/Scripts/Bootstrapへ、VContainerのLifetimeScope、依存登録、EntryPointだけを置く。
Application、Title、GameplayでScope単位に分ける。
ゲームルールや画面表示処理はBootstrapへ書かない。

### 3.5 Tests

EditorテストはAssets/CCC/Tests/Editorへ置き、製品コードと物理的に分離する。

- Core：Rational、計算、Mission、Catalog、Selector、Reducer

PlayModeテストは必須ではない。
追加する場合だけTests/PlayModeとCcc.PlayMode.Tests.asmdefを作る。

## 4. Scene構成

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

通常起動ではTitleScreenだけを表示する。
GameSceneから戻った場合はMainMenuScreenから開始する。
TitleScreenViewはTitleScreenModelの状態に応じて、4つのScreenのうち1つだけを表示する。
Sceneロード中はSceneLoadingBlockerを表示し、開始ボタンの連打を受け付けない。
ErrorMessageAreaはGameScene開始失敗を表示する。

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

GameScreenViewはGameplayScreenとResultScreenのどちらか一方だけを表示する。
GameplayViewは子Viewを束ねる親であり、LifeやSectionの更新判断を持たない。
FeedbackLayerは通常表示より前面、SceneLoadingBlockerはすべての操作より前面に置く。
Retry失敗時はResultScreenを維持し、RetryButtonを無効、MainMenuButtonを有効にする。

### 4.3 EventSystem

各SceneにEventSystemを1つだけ置く。
入力moduleはInputSystemUIInputModuleを使用する。
Gameplay操作はButton.onClickと重複させず、PointerDown通知へ統一する。
左右同時入力と同一領域への2本指入力を妨げる透明PanelやRaycast Targetを置かない。

## 6. Dataと設定Asset

### 6.1 ScriptableObject

| 配置例 | 用途 | 生成する実行時データ |
| --- | --- | --- |
| Data/GameplayConfig/NormalGameplayConfig.asset | Normal、LifeEndlessの設定 | GameplayConfig |
| Data/MissionPreset/NormalMissionPreset.asset | 簡易版Mission候補 | MissionParameterCandidate列 |
| Data/MissionText/JapaneseMissionText.asset | Mission表示文言 | IMissionTextSourceとして参照 |
| Data/Feedback/DefaultFeedbackSettings.asset | 成功・失敗演出時間 | 任意。Viewへ直接設定する場合は作らない |

ScriptableObjectをCoreへ直接渡さない。
Gameplay開始時に検証し、純C#型へ変換する。
Validationエラーは一覧で表示し、Gameplayを開始しない。
MissionPresetの「未決定」値は実行用Assetへ入力しない。

### 6.2 Input設定

Input Systemの設定Assetを独自作成する場合はSettings/Inputへ置く。
UI操作だけで既定のUI Actionを使用できる場合は、不要なGameplay Action Mapを作らない。
EventSystemとInputSystemUIInputModuleの参照はTitleSceneとGameSceneで同じ設定にする。

### 6.3 Build設定

Build ProfileまたはBuild SettingsにはTitleScene、GameSceneの順で登録する。
起動SceneはTitleSceneとする。
Androidは横向き固定、Input HandlingはInput Systemを使用する。
smoke buildの出力はGit管理外へ置く。

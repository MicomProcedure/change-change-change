# CCC_asmdef

> [ドキュメント一覧](../README.md) / 区分: Assembly設計
>
> 正本: [Cosense「CCC_asmdef」](https://scrapbox.io/proto-TeamL/CCC_asmdef)
> 取得時点の原文更新日時: 2026-08-28T23:12:15.000Z


### 3.1 Ccc.Core

`Ccc.Core`はModel、同期ロジック、UseCasesを含む単一asmdefである。R3は`Ccc.Core.UseCases`名前空間で使用する。参照packageはR3である。

名前空間ごとの責務：

- `Ccc.Core.Model`：標準C#で構成する不変データと値オブジェクト。`Rational`、`Expression`、`MissionCondition`、`MissionCatalog`、`GameplayConfig`、`GameplayState`、`SessionResult`など。
- `Ccc.Core.Logic`：`Ccc.Core.Model`を扱う同期・決定的な処理。`ExpressionCalculator`、`MissionConditionMatcher`、`MissionCatalogBuilder`、`TierResolver`、Selector、Session遷移用`GameplayReducer`など。
- `Ccc.Core.UseCases`：状態保持と副作用の調停。`ExpressionInput`、`GameplayController`、Mission抽選Strategy、`ISectionTimer`、`SectionTimerSnapshot`、`IRandomSource`、開始Result・Failureなど。状態配信にはR3を使用する。

単一asmdef内の`UseCases -> Logic -> Model`という参照方向は、名前空間規約、コードレビュー、EditModeテストで検証する。

### 3.2 Ccc.Presentation

`Ccc.Core`、R3、UniTaskを参照する。MVPのPresenter、View／Navigation契約、DTO、Formatterを配置する。

主な型：

- `GameplayPresenter`
- `IGameplayView`
- `IGameplayNavigation`
- `GameplayInput`
- `GameplayViewStateDto`
- `FeedbackViewDataDto`
- `RationalDecimalFormatter`
- `IMissionTextSource`
- `MissionTextFormatter`

Presentationの外部package参照はR3とUniTaskである。

### 3.3 Ccc.Infrastructure

`Ccc.Core`と`Ccc.Presentation`を参照し、Presentation／`Ccc.Core.UseCases`のポートをUnityで実装する。UnityEngine、Input System、DOTween、UniTask、R3、VContainerを利用できる。

主な型：

- `GameplayView : MonoBehaviour, IGameplayView`
- `PointerDownInputView : MonoBehaviour, IPointerDownHandler`
- `ExpressionView`、`MissionView`、`TimerView`、`LifeView`
- `DotweenFeedbackView`
- `UniTaskSectionTimer : ISectionTimer`
- `UnityRandomSource : IRandomSource`
- `GameplayConfigAsset : ScriptableObject`
- `MissionPresetAsset : ScriptableObject`
- `MissionTextAsset : ScriptableObject`
- `SceneNavigator`
- `TitleScreenModel`、`GameScreenModel : IGameplayNavigation`
- `TitleScreenView : MonoBehaviour`、`GameScreenView : MonoBehaviour`

Infrastructureの外部package参照はR3、UniTask、Input System、DOTween、VContainer、Unity modulesである。VContainerはMonoBehaviourの`[Inject]`メソッドとScene Scopeへの開始Request登録に使用する。

### 3.4 Ccc.Bootstrap

全assemblyとVContainerを参照するComposition Rootである。

主な型：

- `GameplayLifetimeScope : LifetimeScope`
- `GameplayEntryPoint`

Bootstrapの責務は依存登録と起動処理である。

### 3.5 asmdef参照表

| asmdef | プロジェクト内参照 | 外部package参照 |
| --- | --- | --- |
| `Ccc.Core` | なし | R3 |
| `Ccc.Presentation` | `Ccc.Core` | R3、UniTask |
| `Ccc.Infrastructure` | `Ccc.Core`、`Ccc.Presentation` | R3、UniTask、Input System、DOTween、VContainer、Unity modules |
| `Ccc.Bootstrap` | `Ccc.Core`、`Ccc.Presentation`、`Ccc.Infrastructure` | VContainer、必要なUnity modules |

`Ccc.Infrastructure`から`Ccc.Core`への参照用途は、Assetから純C# Modelへの変換とTimer／Randomポート実装である。`Ccc.Bootstrap`は依存グラフの最外層に置く。

EditModeテストは単一の`Ccc.Editor.Tests`へ配置する。`includePlatforms`を`Editor`とし、`Ccc.Core`、`Ccc.Presentation`、`Ccc.Infrastructure`、Test Assembliesを参照する。テストの分類には`Ccc.Editor.Tests.Core`、`Ccc.Editor.Tests.Presentation`、`Ccc.Editor.Tests.Infrastructure`のnamespaceとフォルダを使用する。

PlayModeテストは`Ccc.PlayMode.Tests`へ配置し、対象となる製品assemblyとTest Assembliesを参照する。テスト用asmdefは製品コード4 asmdefの数に含めない。

### 3.6 配置とasmdef共通設定

```text
Assets/CCC/Scripts/
├── Core/
│   └── Ccc.Core.asmdef
├── Presentation/
│   └── Ccc.Presentation.asmdef
├── Infrastructure/
│   └── Ccc.Infrastructure.asmdef
└── Bootstrap/
   └── Ccc.Bootstrap.asmdef
```

各asmdefの`name`と`rootNamespace`はファイル名と同じにする。共通値：

- `autoReferenced: true`
- `overrideReferences: false`
- `allowUnsafeCode: false`
- `includePlatforms: []`
- `excludePlatforms: []`
- `defineConstraints: []`
- `precompiledReferences: []`

CoreとPresentationは`noEngineReferences: true`、InfrastructureとBootstrapはfalse。Core／PresentationへUnityEngine型を書いた時点でcompile errorにして依存違反を検出する。

asmdefのGUID直書きはUnity Editorが生成した値を使う。別projectのmeta GUIDを文書やAI出力からコピーしない。

### 3.7 参照の入力手順

Unity InspectorのAssembly Definition Referencesへ、次の順で登録する。

| asmdef | 製品assembly参照 | package assembly参照 |
| --- | --- | --- |
| `Ccc.Core` | なし | 導入済みR3のCore assembly |
| `Ccc.Presentation` | `Ccc.Core` | R3、UniTask |
| `Ccc.Infrastructure` | `Ccc.Core`、`Ccc.Presentation` | R3、UniTask、Unity.InputSystem、DOTween、VContainerのUnity連携assembly |
| `Ccc.Bootstrap` | `Ccc.Core`、`Ccc.Presentation`、`Ccc.Infrastructure` | VContainerのCore／Unity連携assembly |

package assemblyのraw nameは、Package Managerで実際に導入された版のasmdefをInspectorのObject Pickerから選ぶ。package名から文字列を推測してJSONへ手入力しない。特にDOTween、UniTask、VContainerは導入方法でassembly分割が異なり得るため、導入済みpackageのasmdefを正本とする。

InfrastructureでDOTweenのawait拡張がcompileされない場合は、導入版が要求するVersion Definesまたは`UNITASK_DOTWEEN_SUPPORT`をInfrastructureだけへ設定する。Core／Presentation／BootstrapへDOTween用defineを追加しない。

### 3.8 Assemblyごとの禁止参照

| assembly | 禁止 |
| --- | --- |
| `Ccc.Core` | UnityEngine、UniTask、DOTween、Input System、VContainer、`Ccc.Presentation`、`Ccc.Infrastructure`、`Ccc.Bootstrap` |
| `Ccc.Presentation` | UnityEngine、DOTween、Input System、VContainer、Infrastructure、Bootstrap |
| `Ccc.Infrastructure` | Bootstrap |
| `Ccc.Bootstrap` | ゲームルールの実装、表示ロジックの実装 |

同じ`Ccc.Core`内のModel／Logic／UseCases方向はcompilerでは強制できないため、namespaceとfolderを一致させる。ModelからLogic／UseCases、LogicからUseCasesを参照しない。

### 3.9 新規ファイルの振り分け

判断順：

- 1. Unity型なしの不変データ／値ObjectならCore.Model。
- 2. 同期・決定的な計算ならCore.Logic。
- 3. 状態保持、R3配信、Timer／Random portならCore.UseCases。
- 4. DTO、Formatter、View／Navigation／Audio portならPresentation。
- 5. MonoBehaviour、ScriptableObject、Unity port実装ならInfrastructure。
- 6. DI登録とEntryPointだけならBootstrap。

迷った場合、利用するpackageではなく責務と依存先で決める。例えばR3を使うControllerはCore.UseCases、AudioClipを使うCatalogはInfrastructure、AudioKeyとIAudioPlayerはPresentation。

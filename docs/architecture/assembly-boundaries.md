# CCC アセンブリ境界設計（CCC_asmdef）

> [ドキュメント一覧](../README.md) / 区分: Assembly設計

---

## 1. アセンブリ（asmdef）定義と各層の責務

### 1.1 Ccc.Core
`Ccc.Core` は Model、同期ロジック、および UseCases を包含する単一のアセンブリ（asmdef）とする。外部パッケージ参照は R3 と `Microsoft.Extensions.Logging.Abstractions` に限定し、いずれも `Ccc.Core.UseCases` 名前空間以下でのみ使用すること。ログの出力先と出力レベルは Bootstrap で構成すること。1行で完結するメッセージ（Trace・警告・エラー等）は `ILogger<T>` へ直接書き込み、整形規則や件数設定を伴う構造化診断は `Ccc.Core.UseCases` の `IDiagnosticsReporter<TTrace>` ポートへ診断Traceとして渡すこと。Core は診断の整形・件数・出力先を知らないこと。詳細は [CCC Log・診断出力クラス設計](./infrastructure/logging.md) を参照すること。

> **名前空間ごとの責務分類**
> - **`Ccc.Core.Model`**: 標準C#のみで構成される不変データおよび値オブジェクト。  
>   （例: `Rational`、`Expression`、`MissionCondition`、`MissionCatalog`、`GameplayConfig`、`GameplayState`、`SessionResult` など）
> - **`Ccc.Core.Logic`**: `Ccc.Core.Model` を扱う同期・決定的なドメイン計算処理。  
>   （例: `ExpressionCalculator`、`MissionConditionMatcher`、`MissionCatalogBuilder`、`TierResolver`、Selector、セッション遷移用 `GameplayReducer` など）
> - **`Ccc.Core.UseCases`**: 状態保持および副作用の調停層。  
>   （例: `ExpressionInput`、`GameplayController`、Mission抽選Strategy、`ISectionTimer`、`SectionTimerSnapshot`、`IRandomSource` など。状態の非同期配信には R3 を使用）

※ 単一アセンブリ内における `UseCases` → `Logic` → `Model` という依存方向の遵守は、名前空間規約、コードレビュー、および EditMode テストによって検証・維持すること。

### 1.2 Ccc.Presentation
`Ccc.Core`、R3、および UniTask を参照すること。MVPパターンの Presenter、View / Navigation のインターフェース契約、表示用DTO、Formatter などを配置する。

> **主要な型**
> - `GameplayPresenter`
> - `IGameplayView` / `IGameplayNavigation`
> - `GameplayInput`
> - `GameplayViewStateDto` / `FeedbackViewDataDto`
> - `RationalDecimalFormatter` / `MissionTextFormatter`
> - `IMissionTextSource`

※ Presentation層が参照する外部パッケージは R3 と UniTask のみに限定すること。

### 1.3 Ccc.Infrastructure
`Ccc.Core` および `Ccc.Presentation` を参照し、Presentation層や `Ccc.Core.UseCases` で定義されたポートの Unity具体的な実装を提供すること。`UnityEngine`、`Input System`、`DOTween`、`UniTask`、`R3`、`VContainer`、`Microsoft.Extensions.Logging.Abstractions` を利用可能とする。

> **主要な型**
> - `GameplayView : MonoBehaviour, IGameplayView`
> - `PointerDownInputView : MonoBehaviour, IPointerDownHandler`
> - `ExpressionView`、`MissionView`、`TimerView`、`LifeView`
> - `DotweenFeedbackView`
> - `UniTaskSectionTimer : ISectionTimer`
> - `UnityRandomSource : IRandomSource`
> - `GameplayConfigAsset` / `MissionParameterGenerationProfileAsset` / `MissionPresetAsset` / `MissionTextAsset` / `LogSettingsAsset`（いずれも `ScriptableObject`）
> - 診断Reporter群（`CompositeDiagnosticsReporter<TTrace>` / `SafeDiagnosticsReporter<TTrace>` / `LoggerDiagnosticsReporter<TTrace>` / `CsvDiagnosticsReporter<TTrace>` : `IDiagnosticsReporter<TTrace>`）
> - `SceneNavigator`
> - `TitleScreenModel` / `GameScreenModel : IGameplayNavigation`
> - `TitleScreenView` / `GameScreenView`（`MonoBehaviour`）

※ VContainer は `MonoBehaviour` への `[Inject]` メソッドや、Scene Scope への開始リクエスト登録に使用すること。

### 1.4 Ccc.Bootstrap
プロジェクト内の全アセンブリおよび VContainer を参照するコンポジションルート（Composition Root）とする。

> **主要な型**
> - `GameplayLifetimeScope : LifetimeScope`
> - `GameplayEntryPoint`

※ Bootstrapの責務は、アプリ全体の依存関係の登録（DI）と起動ライフサイクルの管理に限定すること。

`GameplayEntryPoint`は選択Sourceに必要なAssetだけを検証し、Profile/Presetを純C#の`MissionCatalogPreparationSettings`へ変換すること。Profile、候補Generator、Catalog Builder、Mission候補Tier Resolverは状態を持たないためDI登録しないこと。`MissionPresetAsset`を直接DI登録しないこと。

---

## 2. アセンブリ参照構造一覧

### 2.1 asmdef参照サマリー

| asmdef | プロジェクト内参照 | 外部パッケージ参照 |
| --- | --- | --- |
| `Ccc.Core` | （なし） | R3、Microsoft.Extensions.Logging.Abstractions |
| `Ccc.Presentation` | `Ccc.Core` | R3、UniTask |
| `Ccc.Infrastructure` | `Ccc.Core`、`Ccc.Presentation` | R3、UniTask、Input System、DOTween、VContainer、Microsoft.Extensions.Logging.Abstractions、Unity modules |
| `Ccc.Bootstrap` | `Ccc.Core`、`Ccc.Presentation`、`Ccc.Infrastructure` | VContainer、Microsoft.Extensions.Logging、ZLogger、ZLogger.Unity、必要なUnity modules |

- `Ccc.Infrastructure` が `Ccc.Core` を参照する主な目的は、アセットから純粋C# Modelへの変換処理、および Timer / Random などのポート実装とする。
- `Ccc.Bootstrap` は依存関係グラフの最外層に配置すること。

### 2.2 テスト用アセンブリの構成
- **EditModeテスト**: 単一のアセンブリ `Ccc.Editor.Tests` に配置すること。`includePlatforms` を `Editor` に設定し、`Ccc.Core`、`Ccc.Presentation`、`Ccc.Infrastructure` および Test Assemblies を参照すること。内部フォルダ/名前空間は `Ccc.Editor.Tests.Core`、`Ccc.Editor.Tests.Presentation`、`Ccc.Editor.Tests.Infrastructure` に分けて整理すること。
- **PlayModeテスト**: 必要に応じて `Ccc.PlayMode.Tests` を作成し、テスト対象の製品アセンブリおよび Test Assemblies を参照すること。

---

## 3. アセンブリの設定・入力規約

### 3.1 ディレクトリ配置と基本パラメータ

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

各 asmdef ファイルの `name` および `rootNamespace` は、ファイル名と完全に一致させること。

> **共通設定値**
> - `autoReferenced`: `true`
> - `overrideReferences`: `false`
> - `allowUnsafeCode`: `false`
> - `includePlatforms`: `[]` / `excludePlatforms`: `[]`
> - `defineConstraints`: `[]` / `precompiledReferences`: `[]`

> **Engine参照フラグ（`noEngineReferences`）**
> - `Ccc.Core` および `Ccc.Presentation`: **`true`**（`UnityEngine` 型の誤参照をコンパイルエラーとして即座に検知するため）
> - `Ccc.Infrastructure` および `Ccc.Bootstrap`: **`false`**

※ asmdef の GUID は Unity Editor が自動生成した値をそのまま使用すること。

### 3.2 Inspectorでの参照入力手順

Unity Inspector の **Assembly Definition References** には以下の順で登録を行うこと。

| asmdef | 製品アセンブリ参照 | パッケージアセンブリ参照 |
| --- | --- | --- |
| `Ccc.Core` | （なし） | 導入済み R3 の Core アセンブリ、Microsoft.Extensions.Logging.Abstractions（自動参照） |
| `Ccc.Presentation` | `Ccc.Core` | R3、UniTask |
| `Ccc.Infrastructure` | `Ccc.Core`、`Ccc.Presentation` | R3、UniTask、Unity.InputSystem、DOTween、VContainerのUnity連携アセンブリ、Microsoft.Extensions.Logging.Abstractions（自動参照） |
| `Ccc.Bootstrap` | `Ccc.Core`、`Ccc.Presentation`、`Ccc.Infrastructure` | VContainerのCore / Unity連携アセンブリ、ZLogger.Unity、Microsoft.Extensions.Logging / ZLogger（自動参照） |

※ パッケージアセンブリ参照は、Object Picker を使用して実際にプロジェクトへ導入されたアセンブリを選択すること。手入力は避けること。  
※ NuGet for Unity 導入の `Microsoft.Extensions.Logging` と ZLogger の DLL は自動参照されるため、製品用 asmdef では Assembly Definition References に個別登録しないこと。`ZLogger.Unity` は UPM の asmdef なので Bootstrap から明示参照すること。
※ Infrastructure で DOTween の async/await 拡張をコンパイルする場合は、Infrastructure アセンブリのみに `UNITASK_DOTWEEN_SUPPORT` などの Define を設定すること。Core / Presentation / Bootstrap へ追加しないこと。

---

## 4. 参照制約とクラス分類基準

### 4.1 アセンブリごとの禁止参照ルール

| アセンブリ | 参照禁止対象 |
| --- | --- |
| `Ccc.Core` | `UnityEngine`、UniTask、DOTween、Input System、VContainer、`Ccc.Presentation`、`Ccc.Infrastructure`、`Ccc.Bootstrap` |
| `Ccc.Presentation` | `UnityEngine`、DOTween、Input System、VContainer、`Ccc.Infrastructure`、`Ccc.Bootstrap` |
| `Ccc.Infrastructure` | `Ccc.Bootstrap` |
| `Ccc.Bootstrap` | ゲームルールの直接実装、画面表示ロジックの直接実装 |

※ `Ccc.Core` 内部での `Model` / `Logic` / `UseCases` 間の参照規則（ModelからLogic/UseCasesの参照禁止、LogicからUseCasesの参照禁止）は名前空間およびフォルダ分離によって厳密に維持すること。

### 4.2 新規ファイルの配置判断フロー

新しいクラスを作成する際は、以下の順序で配置先アセンブリを判断すること。

1. **Unity非依存の不変データ / 値オブジェクト** → `Ccc.Core.Model`
2. **同期・決定的な計算ロジック** → `Ccc.Core.Logic`
3. **状態保持、R3イベント配信、タイマー/乱数ポート** → `Ccc.Core.UseCases`
4. **DTO、Formatter、View / Navigation / Audio のポート定義** → `Ccc.Presentation`
5. **MonoBehaviour、ScriptableObject、Unityポートの具象実装** → `Ccc.Infrastructure`
6. **DI登録およびEntryPoint設定のみ** → `Ccc.Bootstrap`

迷った場合は「**そのクラスが担う責務と依存先**」で判断すること。

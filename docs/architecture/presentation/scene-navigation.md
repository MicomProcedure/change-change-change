# CCC Unity 画面遷移クラス設計

> [ドキュメント一覧](../../README.md) / 区分: クラス設計

> **対象範囲**: TitleScene / GameScene 間の遷移、Scene内 Screen 切り替え、Gameplay 開始、Result 表示、Retry 制御

---

## 1. 目的

2つの Unity Scene（TitleScene / GameScene）および各 Scene 内部の画面（Screen）切り替えを、各 Scene 専用の Model と View を組み合わせて制御すること。

- **Scene間遷移**: Unity Scene を跨ぐ非同期ロード・遷移処理は `SceneNavigator` が一元管理すること。
- **Scene内Screen遷移**: シーン内部の表示画面切り替えおよび操作状態管理は、Scene ごとの Model（`TitleScreenModel` / `GameScreenModel`）が担当すること。
- **Viewの役割**: View コンポーネントは Model の公開状態（ReactiveProperty等）を購読し、対応する Screen GameObject の表示/非表示に反映すること。
- **ゲーム進行の委譲**: Gameplay の進行管理およびセッション終了判定は `IGameplayFlow` および `GameplayPresenter` へ委譲すること。
- **Retry処理**: リトライ実行時は同一の GameScene を破棄せず、新規セッションを開始すること。

---

## 2. 画面構成と遷移仕様

| Unity Scene | Scene内の画面（Screen）構成 |
| --- | --- |
| `TitleScene` | `TitleScreen`、`MainMenu`、`Stage`、`Tutorial`、`OptionMenu`、`Credit` |
| `GameScene` | `GameplayScreen`、`ResultScreen` |

```mermaid
stateDiagram-v2
   [*] --> TitleScreen
   TitleScreen --> MainMenu

   MainMenu --> Stage
   Stage --> MainMenu
   Stage --> Tutorial
   Tutorial --> Stage
   MainMenu --> OptionMenu
   OptionMenu --> MainMenu
   MainMenu --> Credit
   Credit --> MainMenu

   Stage --> GameplayScreen: GameSceneをロード
   GameplayScreen --> ResultScreen: Session終了
   GameplayScreen --> MainMenu: TitleSceneをロード
   ResultScreen --> GameplayScreen: Retry
   ResultScreen --> MainMenu: TitleSceneをロード
```

- アプリケーションの通常起動時は `TitleScreen` から開始すること。
- GameScene から TitleScene へ戻った場合は `MainMenu` から開始すること。
- Stage のゲーム開始操作時は、ゲームモードを `LifeEndless`、難易度を Stage で選択中の値と指定した `GameplayStartRequest` を生成すること。
- TitleScene 内の遷移は方向（`TitleScreenTransitionDirection`）を持つ。`Open*` / `EnterMainMenu` による遷移は `Forward`、`Close*` による遷移は `Backward` とすること。

---

## 3. アセンブリと名前空間

| 型名 | 配置アセンブリ / 名前空間 | 役割概要 |
| --- | --- | --- |
| `TitleScreenState` / `GameScreenState` | `Ccc.Presentation.Navigation` | Unity API に依存しない純粋な画面状態 enum |
| `TitleScreenTransitionDirection` | `Ccc.Presentation.Navigation` | TitleScene 内で受け付けた画面遷移の方向 enum |
| `OptionMenuViewStateDto` / `OptionSettingsSnapshot` | `Ccc.Presentation.Navigation` | OptionMenu 表示データおよび Gameplay 開始時の不変設定スナップショット |
| `IGameplayNavigation` | `Ccc.Presentation` | GameplayPresenter から画面切り替えを依頼するためのポート |
| `SceneNavigator` | `Ccc.Infrastructure.Navigation` | Unity Scene アシンクロナスロードおよびリクエスト引き渡しの具象クラス |
| `TitleScreenModel` / `GameScreenModel` / `OptionMenuModel` | `Ccc.Infrastructure.Navigation` | シーン内の Screen 遷移状態および画面操作ロジック |
| `TitleScreenView` / `GameScreenView` | `Ccc.Infrastructure.Navigation` | 各 Scene 内の Screen 表示制御と入力イベンド検知 |
| `TitleScreenTransitionSoundPlayer` | `Ccc.Infrastructure.Navigation` | TitleScene 内の画面遷移方向に対応する SE の再生 |
| `OptionVolumeAudioSynchronizer` | `Ccc.Infrastructure.Navigation` | `OptionMenuModel` の音量レベルを Audio へ継続反映 |
| `CycleSelectorView` | `Ccc.Infrastructure.UI` | Gameplay と OptionMenu で共有する循環選択 UI コンポーネント |
| `ApplicationLifetimeScope` | `Ccc.Bootstrap` | アプリケーション共通のシングルトンコンポーネント登録 |
| `TitleLifetimeScope` / `GameplayLifetimeScope` | `Ccc.Bootstrap` | 各シーンの DI 依存関係登録 |
| `TitleEntryPoint` | `Ccc.Bootstrap` | TitleScene 開始時の初期処理（購読開始と Title BGM 再生） |

---

## 4. 全体のクラス関係図

```mermaid
classDiagram
direction LR
class ApplicationLifetimeScope
class SceneNavigator
class TitleLifetimeScope
class GameplayLifetimeScope
class TitleScreenModel
class OptionMenuModel
class TitleScreenView
class TitleEntryPoint
class TitleScreenTransitionSoundPlayer
class OptionVolumeAudioSynchronizer
class IAudioPlayer
class IAudioVolumeControl
class CycleSelectorView
class GameScreenModel
class GameScreenView
class GameplayPresenter
class IGameplayNavigation
class IGameplayFlow
class GameplayStartRequest
class OptionSettingsSnapshot
class SessionResult

ApplicationLifetimeScope o-- SceneNavigator : singleton
TitleLifetimeScope o-- TitleScreenModel
TitleLifetimeScope o-- OptionMenuModel
TitleLifetimeScope o-- TitleScreenView
TitleLifetimeScope o-- TitleScreenTransitionSoundPlayer
TitleLifetimeScope o-- OptionVolumeAudioSynchronizer
TitleLifetimeScope o-- TitleEntryPoint
GameplayLifetimeScope o-- GameScreenModel
GameplayLifetimeScope o-- GameScreenView
GameplayLifetimeScope o-- GameplayPresenter
GameplayLifetimeScope o-- IGameplayFlow
TitleScreenView --> TitleScreenModel
TitleScreenView --> OptionMenuModel
TitleScreenView o-- CycleSelectorView
TitleScreenModel --> OptionMenuModel : settings snapshot
TitleScreenModel --> SceneNavigator
TitleEntryPoint --> TitleScreenTransitionSoundPlayer : StartSubscription
TitleEntryPoint --> OptionVolumeAudioSynchronizer : StartSubscription
TitleEntryPoint --> IAudioPlayer : PlayBgm
TitleScreenTransitionSoundPlayer --> TitleScreenModel : ScreenTransitioned
TitleScreenTransitionSoundPlayer --> IAudioPlayer : PlaySE
OptionVolumeAudioSynchronizer --> OptionMenuModel : volume levels
OptionVolumeAudioSynchronizer --> IAudioVolumeControl
GameplayPresenter --> IGameplayNavigation
GameScreenModel ..|> IGameplayNavigation
GameScreenModel --> SceneNavigator
GameScreenModel --> IGameplayFlow
SceneNavigator ..> GameplayStartRequest
GameplayStartRequest *-- OptionSettingsSnapshot
GameScreenModel ..> SessionResult
```

---

## 5. 画面状態と遷移リクエスト（Request）

### 5.1 画面状態 (State Enum)

```csharp
public enum TitleScreenState
{
    TitleScreen = 0,
    MainMenu,
    Stage,
    OptionMenu,
    Credit,
    Tutorial
}

public enum TitleScreenTransitionDirection
{
    Forward = 0,
    Backward
}

public enum GameScreenState
{
    Gameplay = 0,
    Result
}
```

### 5.2 シーン開始リクエスト（Request）

```csharp
public sealed class GameplayStartRequest
{
    public GameMode GameMode { get; }
    public GameDifficulty Difficulty { get; }
    public OptionSettingsSnapshot OptionSettings { get; }

    public GameplayStartRequest(
        GameMode gameMode,
        GameDifficulty difficulty,
        OptionSettingsSnapshot optionSettings);
}

public sealed class TitleStartRequest
{
    public TitleScreenState InitialScreen { get; }
    public TitleStartRequest(TitleScreenState initialScreen);
}
```

- `GameplayStartRequest` は生成後の変更が不可能な不変オブジェクトとする。
- `GameMode` と `GameDifficulty` は `Ccc.Core.Model` の型を使用し、Presentation 層に同名の型を定義しないこと。
- MainMenu でのゲーム開始時は `OptionMenuModel.CreateSnapshot()` により設定スナップショットを生成し、`GameplayStartRequest` を構築してシーンロードを開始すること。
- リトライ実行時は、GameScene の LifetimeScope に保持された同一の `GameplayStartRequest` を再利用すること。

### 5.3 ナビゲーション処理結果（Navigation Result）

```csharp
public enum SceneNavigationFailureKind
{
    None = 0,
    AlreadyLoading,
    LoadFailed
}

public sealed class SceneNavigationResult
{
    public bool IsSuccess { get; }
    public SceneNavigationFailureKind FailureKind { get; }
    public string ErrorMessage { get; }
    public Exception? Cause { get; }
}

public interface ISceneNavigator
{
    bool IsLoading { get; }
    UniTask<SceneNavigationResult> LoadGameplayAsync(GameplayStartRequest request);
    UniTask<SceneNavigationResult> LoadTitleAsync(TitleStartRequest request);
}
```

---

## 6. SceneNavigator

`SceneNavigator` は、Project Root の `ApplicationLifetimeScope` に `ISceneNavigator` として Singleton 登録される純粋 C# クラスとする。

```csharp
public sealed class SceneNavigator : ISceneNavigator
{
    public bool IsLoading { get; }

    public UniTask<SceneNavigationResult> LoadGameplayAsync(GameplayStartRequest request);
    public UniTask<SceneNavigationResult> LoadTitleAsync(TitleStartRequest request);
}
```

> **ロード実行フローと安全制御**
> 1. リクエストが `null` の場合は即座に例外をスローすること。
> 2. `IsLoading == true` の場合は、二重ロードを防止するため即座に `AlreadyLoading` 結果を返却すること。
> 3. `LifetimeScope.Enqueue` を使用して、次に生成される Scene Scope へリクエスト・インスタンスを登録すること。
> 4. `SceneManager.LoadSceneAsync` を非同期で実行し、ロード完了まで待機すること。
> 5. 成功 / 失敗に応じて適切な `SceneNavigationResult` を返却し、`finally` ブロックで `IsLoading = false` に復帰させること。

---

## 7. TitleScene の設計

### 7.1 TitleScreenModel

```csharp
public sealed class TitleScreenModel : IDisposable
{
    public ReadOnlyReactiveProperty<TitleScreenState> CurrentScreen { get; }
    public Observable<TitleScreenTransitionDirection> ScreenTransitioned { get; }
    public ReadOnlyReactiveProperty<bool> IsBusy { get; }
    public ReadOnlyReactiveProperty<string> ErrorMessage { get; }
    public ReadOnlyReactiveProperty<GameDifficulty> SelectedDifficulty { get; }

    public TitleScreenModel(
        ISceneNavigator sceneNavigator,
        OptionMenuModel optionMenuModel,
        TitleStartRequest startRequest);

    public void EnterMainMenu();
    public void OpenOptionMenu();
    public void OpenStage();
    public void CloseOptionMenu();
    public void OpenCredit();
    public void CloseCredit();
    public void OpenTutorial();
    public void CloseTutorial();
    public void CloseStage();
    public void CycleDifficulty();
    public UniTask StartGameplayAsync();
    public void Dispose();
}
```

> **画面遷移の通知仕様**
> - 各遷移メソッドは、`IsBusy == false` かつ現在の画面が遷移元と一致する場合のみ受け付けること。受け付けない要求は何も変更せず、何も通知しないこと。
> - 受け付けた遷移では、`ErrorMessage` を空にし、`CurrentScreen` を更新した後に `ScreenTransitioned` へ遷移方向を1回通知すること。
> - `Close*` は `Backward`、それ以外の遷移メソッドは `Forward` を通知すること。
> - `Dispose` 後の遷移要求は無視すること。`ScreenTransitioned` は `Dispose` 時に完了すること。

### 7.2 TitleScreenView

- `CurrentScreen` の変更を監視し、対応する画面（GameObject）のみをアクティブにすること。
- `IsBusy == true`（ロード中など）は、ボタンの連続タップや重複操作を無効化すること。

### 7.3 TitleScreenTransitionSoundPlayer

```csharp
public sealed class TitleScreenTransitionSoundPlayer : IDisposable
{
    public TitleScreenTransitionSoundPlayer(
        TitleScreenModel titleScreenModel,
        IAudioPlayer audio);

    public void StartSubscription();
    public void Dispose();
}
```

- `StartSubscription` で `TitleScreenModel.ScreenTransitioned` の購読を開始すること。
- `Backward` を受けた場合は `AudioKey.SECancelClick` を再生し、`Forward` を受けた場合は何も再生しないこと。
- 引数が `null` の場合は `ArgumentNullException`、二重開始時は `InvalidOperationException`、`Dispose` 後の開始は `ObjectDisposedException` を送出すること。
- `Dispose` で購読を解除すること。

### 7.4 OptionVolumeAudioSynchronizer

```csharp
public sealed class OptionVolumeAudioSynchronizer : IDisposable
{
    public OptionVolumeAudioSynchronizer(
        OptionMenuModel optionMenuModel,
        IAudioVolumeControl audioVolumeControl);

    public void StartSubscription();
    public void Dispose();
}
```

- `StartSubscription` で `OptionMenuModel.SEVolumeLevel` / `BgmVolumeLevel` を購読し、開始時点の値を即時に、以降は変更の都度 `IAudioVolumeControl` へ反映すること。
- 例外仕様と `Dispose` の挙動は `TitleScreenTransitionSoundPlayer` と同じとすること。

### 7.5 TitleEntryPoint

- TitleScene 開始時の初期処理のみを担当すること。`Start` で `OptionVolumeAudioSynchronizer` と `TitleScreenTransitionSoundPlayer` の購読を開始し、`AudioKey.BgmTitle` を再生すること。
- 画面遷移や設定変更への継続的な反応、および Model の破棄は担当しないこと。

> **DI仕様（TitleLifetimeScope）**
> - `OptionMenuModel`、`TitleScreenModel`、`OptionVolumeAudioSynchronizer`、`TitleScreenTransitionSoundPlayer` は `Lifetime.Scoped` で自身の型として登録し、TitleScene の Scope 破棄時に VContainer が `Dispose` すること。
> - `TitleEntryPoint` は `RegisterEntryPoint` で登録すること。
>
> ```csharp
> builder.Register<OptionMenuModel>(Lifetime.Scoped);
> builder.Register<TitleScreenModel>(Lifetime.Scoped);
> builder.Register<OptionVolumeAudioSynchronizer>(Lifetime.Scoped);
> builder.Register<TitleScreenTransitionSoundPlayer>(Lifetime.Scoped);
> builder.RegisterComponentInHierarchy<TitleScreenView>();
> builder.RegisterEntryPoint<TitleEntryPoint>();
> ```

---

## 8. GameScene の設計

### 8.1 GameScreenModel

```csharp
public sealed class GameScreenModel : IGameplayNavigation, IDisposable
{
    public ReadOnlyReactiveProperty<GameScreenState> CurrentScreen { get; }
    public ReadOnlyReactiveProperty<SessionResult?> Result { get; }
    public ReadOnlyReactiveProperty<bool> IsBusy { get; }
    public ReadOnlyReactiveProperty<string> ErrorMessage { get; }

    public GameScreenModel(
        ISceneNavigator sceneNavigator,
        IGameplayFlow gameplayFlow,
        GameplayStartRequest startRequest);

    public void ShowResult(SessionResult result);
    public UniTask RetryAsync();
    public UniTask NavigateToMainMenuAsync();
    public void Dispose();
}
```

> **リトライ（RetryAsync）の挙動**
> 1. `IsBusy` を `true` に設定し、UI操作を一時ロックすること。
> 2. 保持していた `GameplayStartRequest` を用いて `IGameplayFlow.StartSession` を再呼び出しすること。
> 3. 成功時は `Result` をクリアし、`CurrentScreen` を `Gameplay` へ戻却すること。
> 4. 失敗時は `ResultScreen` 表示を維持したまま、エラーメッセージを表示すること。

---

## 9. 所有関係とライフタイム（Lifetime）マトリクス

| 所有責任者 | 管理する状態 / オブジェクト | ライフタイム |
| --- | --- | --- |
| `ApplicationLifetimeScope` | `SceneNavigator`、Audio共通インスタンス | Application（アプリ起動期間中） |
| `TitleScreenModel` | `TitleScreenState`、ロード状態、エラーメッセージ | TitleScene |
| `OptionMenuModel` | 設定候補、現在の選択Index、設定データ | TitleScene |
| `TitleScreenTransitionSoundPlayer` | 画面遷移の購読 | TitleScene |
| `OptionVolumeAudioSynchronizer` | 音量レベルの購読 | TitleScene |
| `GameScreenModel` | `GameScreenState`、`SessionResult`、Retry状態 | GameScene |
| `GameplayController` | `GameplaySessionContext`、`GameplayState` | GameScene内セッション |
| `GameplayPresenter` | イベント購読、演出待機処理、UI用一時状態 | GameScene |

---

## 10. 受け入れ条件（Acceptance Criteria）

- タイトル画面からメインメニューへの移動、オプション画面・クレジット画面の開閉がスムーズに行えること。
- TitleScene 内で `Close*` による遷移が受け付けられた場合のみキャンセルSEが鳴り、`IsBusy` 中など受け付けられなかった操作では鳴らないこと。
- メインメニューでのゲーム開始操作時、設定スナップショットを含む `GameplayStartRequest` が正常に生成・渡されること。
- シーンロード中の重複タップによって二重ロードが発生しないこと（`AlreadyLoading` の検知）。
- ゲームオーバー時に `ResultScreen` へ切り替わり、リトライ操作によって同一シーン内で速やかに再スタートできること。
- シーン切替失敗時には元の画面表示が維持され、適切なエラーが表示されること。

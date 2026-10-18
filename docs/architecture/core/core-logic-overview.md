# CCC Unity中核ロジック関係図

> [ドキュメント一覧](../../README.md) / 区分: 設計解説

> **対象範囲**: `Ccc.Core.Model`、`Ccc.Core.Logic`、`Ccc.Core.UseCases`、およびそれらと直接対峙する Presentation 境界

---

## 1. このドキュメントの読み方

本ドキュメントは、CCC の中核ロジックの全体像および構成要素の関係を理解するための解説資料とする。以下のステップ順で参照すること。

1. **全体像の把握**: [2. 全体像：3つの塊](#2-全体像3つの塊) で、ゲーム処理の流れを3つの大きなブロックとして理解すること。
2. **詳細構造の参照**: 実装する機能に応じて、[4. 詳細A](#4-詳細amodelの構成とデータ所有) 〜 [7. 詳細D](#7-詳細dmission抽選とtimer) の詳細図を確認すること。
3. **データ保持者の確認**: 「どのオブジェクトが何のデータを保持するか」は [8. 所有関係とlifetime](#8-所有関係とlifetime) を確認すること。
4. **クラスAPIの特定**: 各クラスの具体的なAPI定義は [9. 実装時のクラス別参照先](#9-実装時のクラス別参照先) から [CCC Unityクラス設計](../unity-class-design.md) の該当節へ移動して確認すること。

### 図中の記号・矢印の凡例

| 表現 | 意味 |
| --- | --- |
| `A --> B` | AがBを呼び出す、または参照する |
| `A *-- B` | AがBをセッション中の所有構成要素として保持する（コンポジション） |
| `A o-- B` | AがBへの参照を保持するが、ライフタイムは外部（DI Scope等）が管理する（集約） |
| **実線の矢印** | 実行時の呼び出し、またはデータの直接移動 |
| **点線の矢印** | 生成物、通知、読み取りなどの間接的な関係 |

※ Modelは原則として**不変（Immutable）**とする。図中で「保持する」とある場合も、Modelのインスタンスを内部で書き換えるのではなく、「現在値への参照を保持し、状態更新時に新しい不変インスタンスへ差し替える」ことを意味する。

---

## 2. 全体像：3つの塊

中核ロジックを構成する以下の3つの大きなブロック（役割）を把握すること。

```mermaid
flowchart LR
    Setup["① Session準備\nConfig・候補式・Catalog"]
    Play["② Session進行\n入力・State遷移・Timer・抽選"]
    Output["③ 表示境界\nState／Expression／Event／Timer Snapshot"]

    Setup -->|固定Contextと初回Candidate| Play
    Play -->|R3で公開| Output
    Output -->|Submit・演出完了| Play
```

- **① Session準備**: セッション開始時に一度だけ実行され、セッション固定の `GameplaySessionContext` を生成する。
- **② Session進行**: セッション中の処理を `GameplayController` が集中的に制御する。編集中の数式は `ExpressionInput`、セッションの節目（状態遷移）は `GameplayReducer`、時間管理は `ISectionTimer` が担当する。
- **③ 表示境界**: `GameplayPresenter` が公開されたストリームを購読し、UIへ反映する。Presenter および View は Core の内部状態を直接変更できない。

### 3つのブロックの内部構造と協調関係

```mermaid
flowchart TB
    subgraph Setup["① Session準備（開始時に1回）"]
        Config[GameplayConfig]
        Preparation[MissionCatalogPreparationSettings]
        ExpressionGenerator[ExpressionCandidateGenerator]
        ParameterGenerator[MissionParameterCandidateGenerator]
        Builder[MissionCatalogBuilder]
        Context[GameplaySessionContext]
        Config --> ExpressionGenerator
        Preparation --> ParameterGenerator
        ExpressionGenerator -->|ExpressionCandidate列| ParameterGenerator
        ExpressionGenerator -->|ExpressionCandidate列| Builder
        ParameterGenerator -->|MissionParameterCandidate列| Builder
        Preparation --> Builder
        Builder -->|MissionCatalog| Context
        Config --> Context
    end

    subgraph Play["② Session進行（Session中）"]
        Controller[GameplayController]
        Input[ExpressionInput]
        Reducer[GameplayReducer]
        Draw[IMissionDrawStrategy]
        Timer[ISectionTimer]
        Controller --> Input
        Controller --> Reducer
        Controller --> Draw
        Controller --> Timer
    end

    subgraph Output["③ 表示境界"]
        Presenter[GameplayPresenter]
        View[IGameplayView]
        Navigation[IGameplayNavigation]
        Presenter --> View
        Presenter --> Navigation
    end

    Context --> Controller
    Controller -->|State・Expression・Event・Timer Snapshot| Presenter
    Presenter -->|入力・Feedback完了| Controller
```

---

## 3. 通常プレイの大まかな流れ

```mermaid
flowchart TD
    Start([Session開始])
    Build["Config検証・候補式列挙・Catalog構築"]
    FirstDraw[初回Mission抽選]
    Initial["Section 1・Life 3・MissionId 1\n初期State生成"]
    Playing["Playing\n式を編集できる"]
    Resolve["Submit／時間切れ\n現在式をsnapshotしてReducerへ"]
    Feedback["Transitioning\n成否Eventを公開し演出"]
    Life{Lifeは残っているか}
    NextDraw[次Missionを抽選]
    Present["PresentMission\nMissionId + 1"]
    Result["GameOver\nSessionResultを公開"]

    Start --> Build --> FirstDraw --> Initial --> Playing
    Playing --> Resolve --> Feedback --> Life
    Life -->|はい| NextDraw --> Present --> Playing
    Life -->|いいえ| Result
```

> **状態更新に関する重要ルール**  
> プレイヤーによる数字や演算子の変更操作は `ExpressionInput` 内部で閉じて完結し、`GameplayReducer` を経由しないこと。  
> `GameplayReducer` を経由するのは、セッション進行上の節目となる以下の4つの操作（Command）のみとする。
> 
> 1. **Submit**（数式の提出）
> 2. **TimeExpired**（セッション制限時間の切れ）
> 3. **FeedbackCompleted**（画面演出の完了通知）
> 4. **PresentMission**（新しい Mission の提示）

---

## 4. 詳細A：Modelの構成とデータ所有

本図のコンポジション関係（`*--`）は、「各不変スナップショットがどのデータを内包して保持するか」を表す。

```mermaid
classDiagram
direction LR

class GameplaySessionContext {
  +GameplayConfig Config
  +MissionCatalog MissionCatalog
}
class GameplayConfig {
  +Expression InitialExpression
  +int InitialLife
  +GetMinimumExpressionMatchCount(kind)
}
class MissionCatalog {
  +IReadOnlyList~MissionCandidate~ Candidates
}
class MissionCandidate {
  +MissionCondition Condition
  +int Tier
  +int ExpressionMatchCount
  +ExpressionCandidate AnswerExample
}
class GameplayState {
  +GameplayStatus Status
  +int Section
  +int Life
  +Mission CurrentMission
  +SubmissionResolution LastResolution
  +int CorrectAnswerCount
}
class Mission {
  +MissionId Id
  +MissionCondition Condition
  +int Tier
}
class SubmissionResolution {
  +MissionId MissionId
  +SubmissionTrigger Trigger
  +Expression SubmittedExpression
  +bool IsSuccess
  +CalculationResult CalculationResult
}
class SessionResult {
  +int FinalSection
  +Mission LastMission
  +SubmissionResolution LastResolution
  +Expression FinalExpression
  +int CorrectAnswerCount
}
class Expression
class MissionCondition
class ExpressionCandidate

GameplaySessionContext *-- GameplayConfig
GameplaySessionContext *-- MissionCatalog
GameplayConfig *-- Expression : initial
MissionCatalog *-- MissionCandidate
MissionCandidate *-- MissionCondition
MissionCandidate *-- ExpressionCandidate : answer example
GameplayState *-- Mission : current
GameplayState o-- SubmissionResolution : last or null
Mission *-- MissionCondition
SubmissionResolution *-- Expression : submitted snapshot
SessionResult *-- Mission : last
SessionResult *-- SubmissionResolution : last
```

> **重要な境界・設計制約**
> - **`GameplaySessionContext`**: セッション期間を通じて変更されない設定（Config）とカタログ（Catalog）を保持する。
> - **`GameplayState`**: セクション、ライフ、現在の Mission、直前の提出結果などを保持するセッション進行の不変スナップショットとする。
> - **編集中の式**: 編集中の数式は `GameplayState` には含めず、`ExpressionInput.Current` が独立して保持すること。
> - **提出時の式**: `SubmissionResolution.SubmittedExpression` は、提出ボタンが押された瞬間、または時間切れが確定した瞬間にスナップショット化された式とする。
> - **最終結果の式**: `SessionResult.FinalExpression` は `LastResolution.SubmittedExpression` から導出すること。
> - **正解数の導出**: 正解数 `CorrectAnswerCount` は、State や Result 内部に直接値を保持せず、現在の Section 数から動的に導出すること。

---

## 5. 詳細B：Catalog構築

### 5.1 クラス関係

```mermaid
flowchart LR
    Preparation[MissionCatalogPreparationSettings]
    Preparer[MissionCatalogPreparer]
    Gen[ExpressionCandidateGenerator]
    ParameterGen[MissionParameterCandidateGenerator]
    Calc[ExpressionCalculator]
    Expressions[ExpressionCandidate列]
    Builder[MissionCatalogBuilder]
    Matcher[MissionConditionMatcher]
    Catalog[MissionCatalog]

    Preparation --> Preparer
    Preparer --> Gen
    Preparer --> ParameterGen
    Preparer --> Builder
    Gen --> Calc
    Gen --> Expressions
    Expressions --> ParameterGen
    Preparation --> ParameterGen
    Expressions --> Builder
    ParameterGen --> Builder
    Preparation --> Builder
    Builder --> Matcher
    Builder -->|Build成功| Catalog
```

`MissionCatalogPreparer`が候補式生成、Missionパラメータ候補準備、Catalog構築を順に呼び出し、`GameplayController`とシミュレーションはこれを共有する。`MissionKindDefinitionCatalog`はパラメータ構造、固定肯定Tier、Negated可否・Tierを一元管理する。`MissionCatalogPreparer`、`MissionParameterCandidateGenerator`、`MissionCatalogBuilder`は状態を持たないためDI登録しない。

```mermaid
flowchart TD
    Normalize["Condition重複を最初の1件へ正規化"]
    Pair["MissionParameterCandidate × ExpressionCandidate"]
    Match["Matcherで各式を一致判定"]
    Analyze["Analyze\n適合数・非適合数・割合・回答例"]
    Include{ShouldInclude}
    ResolveTier["同じ適合率からTier解決"]
    Sort["Tier→Kind→A→B→Negated順"]
    Catalog[MissionCatalog]
    Exclude[除外]

    Normalize --> Pair --> Match --> Analyze --> Include
    Include -->|条件を満たす| ResolveTier --> Sort --> Catalog
    Include -->|条件外| Exclude
```

### 5.2 選出条件の責務配置

| 判定条件 | 判定を担当する場所 | 参照元データ |
| --- | --- | --- |
| Mission条件と計算結果の一致確認 | `MissionConditionMatcher` | `MissionCondition`、`CalculationResult` |
| 適合数・非適合数・適合割合の計算 | `MissionCatalogBuilder.Analyze` | 正常な `ExpressionCandidate` 配列 |
| 適合割合が許容範囲内かの検証 | `MissionCatalogBuilder.ShouldInclude` | `MissionCatalogEvaluationPolicy` |
| 最小適合式数の取得 | `MissionCatalogEvaluationPolicy.GetMinimumMatchCount` | `MissionKind` |
| Tier解決 | `MissionCandidateTierResolver` | `MissionKindDefinitionCatalog`、分析済み`Rational`適合率 |
| 重複排除と規定順へのソート | `MissionCatalogBuilder` | Tier、Kind、ParameterA、ParameterB、IsNegated |

---

## 6. 詳細C：Session進行

### 6.1 状態保持クラスと純粋Reducer

```mermaid
classDiagram
direction LR

class GameplayController {
  -GameplaySessionContext context
  -GameplayState currentState
  +StartSession(request)
  +ChangeNumber(side)
  +ChangeOperator()
  +Submit(missionId)
  +NotifyFeedbackCompleted(missionId)
}
class ExpressionInput {
  +Expression Current
  +Observable~Expression~ Expressions
  +ChangeNumber(side)
  +ChangeOperator()
  +Reset(expression)
}
class GameplayReducer {
  +Reduce(context, state, command)$ GameplayTransition
}
class GameplayTransition {
  +GameplayState NextState
  +IReadOnlyList~GameplayEvent~ Events
  +bool IsAccepted
}
class IMissionDrawStrategy {
  +Draw(context) MissionDrawResult
}
class ISectionTimer {
  +Snapshots
  +ExpiredSignals
  +Start(missionId, duration)
  +Stop(missionId)
}
class GameplaySessionContext
class GameplayState
class Expression

GameplayController o-- ExpressionInput : injected
ExpressionInput *-- Expression : current value
GameplayController *-- GameplayState : current snapshot
GameplayController *-- GameplaySessionContext : session fixed
GameplayController o-- IMissionDrawStrategy : injected
GameplayController o-- ISectionTimer : injected
GameplayController --> GameplayReducer : calls static Reduce
GameplayReducer ..> GameplayTransition : creates
GameplayTransition *-- GameplayState : next
```

- **`GameplayReducer`**: `static` クラスとして設計され、一切の内部状態を持たない。現在の Context、State、Command を受け取り、純粋関数として次の State と Event を返却すること。
- **`GameplayController`**: セッション中の現在状態（State）を保持し、イベントやデータの配信順序を管理すること。

### 6.2 Command、Transition、Event

```mermaid
flowchart LR
    subgraph Commands["Session Command"]
        Submit[SubmitCommand]
        Expired[TimeExpiredCommand]
        Feedback[FeedbackCompletedCommand]
        Present[PresentMissionCommand]
    end

    Context[GameplaySessionContext]
    State[GameplayState]
    Reducer[GameplayReducer.Reduce]
    Transition[GameplayTransition]

    subgraph Events["Presentation Event"]
        Presented[MissionPresentedEvent]
        Resolved[SubmissionResolvedEvent]
        Ended[SessionEndedEvent]
    end

    Commands --> Reducer
    Context --> Reducer
    State --> Reducer
    Reducer --> Transition
    Transition -->|NextState| State
    Transition --> Presented
    Transition --> Resolved
    Transition --> Ended
```

| 入力アクション | Commandに含まれるスナップショット | 主な出力イベント |
| --- | --- | --- |
| 手動Submit（提出） | MissionId、`ExpressionInput.Current` | `SubmissionResolvedEvent` |
| Timer期限切れ（時間切れ） | MissionId、`ExpressionInput.Current` | `SubmissionResolvedEvent` |
| 演出完了（Feedback完了） | MissionId | 次抽選の許可、または `SessionEndedEvent` |
| 次Candidate提示 | 直前MissionId、MissionCandidate | `MissionPresentedEvent` |

### 6.3 Controllerが保証する処理順序

```mermaid
sequenceDiagram
    participant P as GameplayPresenter
    participant C as GameplayController
    participant I as ExpressionInput
    participant R as GameplayReducer
    participant T as ISectionTimer

    P->>C: Submit(MissionId)
    C->>I: Currentを読む
    I-->>C: Expression snapshot
    C->>R: SubmitCommand
    R-->>C: GameplayTransition
    C-->>P: NextStateを先に公開
    C->>T: Stop(MissionId)
    C-->>P: SubmissionResolvedEventを公開
```

※ 不整合な MissionId による Submit、2重送信、または `Playing` 以外の状態での Submit / TimeExpired は、Reducer が `IsAccepted == false` として拒否すること。Controller は拒否された Transition に対してタイマー停止やイベント公開を行わない。

---

## 7. 詳細D：Mission抽選とTimer

### 7.1 Mission抽選Strategy

```mermaid
classDiagram
direction LR

class GameplayController
class IMissionDrawStrategy {
  <<interface>>
  +Draw(MissionDrawContext) MissionDrawResult
}
class SimpleMissionDrawStrategy
class FullMissionDrawStrategy
class FixedMissionDrawStrategy
class MissionDrawSelectionModule
class IDiagnosticsReporter~MissionDrawTrace~ {
  <<interface>>
  +bool IsEnabled
  +Report(MissionDrawTrace)
}
class IRandomSource {
  <<interface>>
  +NextFloat01() float
}
class TierResolver {
  +Resolve(section, progression)$ int
}
class WeightedIndexSelector {
  +SelectIndex(weights, sample)$ int
}
class UniformIndexSelector {
  +SelectIndex(count, sample)$ int
}
class MissionDrawContext {
  +GameplaySessionContext SessionContext
  +int Section
  +bool IsInitialDraw
  +Expression? CurrentExpression
  +Mission? PreviousMission
}
class MissionCandidate

GameplayController --> IMissionDrawStrategy : calls
SimpleMissionDrawStrategy ..|> IMissionDrawStrategy
FullMissionDrawStrategy ..|> IMissionDrawStrategy
FixedMissionDrawStrategy ..|> IMissionDrawStrategy
SimpleMissionDrawStrategy --> MissionDrawSelectionModule
FullMissionDrawStrategy --> MissionDrawSelectionModule
FullMissionDrawStrategy --> IDiagnosticsReporter : reports MissionDrawTrace
MissionDrawSelectionModule o-- IRandomSource : injected
MissionDrawSelectionModule --> TierResolver
MissionDrawSelectionModule --> WeightedIndexSelector
MissionDrawSelectionModule --> UniformIndexSelector
IMissionDrawStrategy --> MissionDrawContext : input
IMissionDrawStrategy ..> MissionCandidate : result
```

本実装版は、2問目以降に解放済み候補を現在式で達成できる群と達成できない群へ分割し、`GameplayConfig.CurrentExpressionSatisfiedMissionSelectionRatio`の直接確率で先に候補群を選ぶ。両群が存在するときだけ乱数を1回消費し、sampleが比率未満なら達成できる群、以上なら達成できない群を選ぶ。片方の群しかない場合、および比率0/1で選択先が確定する場合は乱数を消費しない。その後にTier、極性、直前Mission、Kind、目標適合率上位候補の順で選定する。

```mermaid
flowchart TD
    Start[CatalogとSection]
    Tier[TierResolverでCurrent Tier導出]
    Bucket[候補のあるCurrent／Lower Bucketを作る]
    Range{CurrentとLowerの両方があるか}
    Weighted[重み付き選択]
    OneRange[候補がある側を選択・乱数消費なし]
    LowerTier{Lower内Tierが複数か}
    PickLower[下位Tierを一様選択]
    Kind{Kindが複数か}
    PickKind[Kindを一様選択]
    Candidate{同Kind Candidateが複数か}
    PickCandidate[Candidateを一様選択]
    Result[MissionCandidate]

    Start --> Tier --> Bucket --> Range
    Range -->|はい| Weighted --> LowerTier
    Range -->|いいえ| OneRange --> LowerTier
    LowerTier -->|はい| PickLower --> Kind
    LowerTier -->|いいえ| Kind
    Kind -->|はい| PickKind --> Candidate
    Kind -->|いいえ| Candidate
    Candidate -->|はい| PickCandidate --> Result
    Candidate -->|いいえ| Result
```

※ `IRandomSource` から乱数値を取得するのは、選択肢が2つ以上存在する場合のみとする。Selector クラス群は与えられたサンプル値からインデックスを算出する純粋関数であり、乱数生成器の存在を直接知ることはない。

### 7.2 Timerと数式スナップショット

```mermaid
sequenceDiagram
    participant T as ISectionTimer
    participant C as GameplayController
    participant I as ExpressionInput
    participant R as GameplayReducer
    participant P as GameplayPresenter

    loop 時間経過中
        T-->>C: SectionTimerSnapshot
        C-->>P: TimerSnapshotsへ転送
    end
    T-->>C: SectionTimerExpiredSignal(MissionId)
    C->>I: Currentを読む
    I-->>C: Expression snapshot
    C->>R: TimeExpiredCommand(MissionId, Expression)
    R-->>C: Transitioning + SubmissionResolved
    C-->>P: State／Event
```

- `ISectionTimer` の実装クラスは、MissionId、開始時刻、残り時間、期限切れ通知の有無を保持する。
- `SectionTimerSnapshot` はUI表示用の構造体であり、`GameplayState` には格納しないこと。
- 時間切れシグナル自体は数式データを保持しない。シグラル受信時に `GameplayController` が `ExpressionInput.Current` の最新スナップショットを読み込んで Command に設定すること。
- 手動 Submit と時間切れが同時に競合した場合でも、Reducer の Status チェックおよび MissionId ガードにより、先に受理された一方のみを有効とすること。

---

## 8. 所有関係とライフタイム（Lifetime）

「所有」には、**データの現在値を保持する責務**と、**インスタンスを破棄管理するライフタイム責務**の2種類がある。

### 8.1 実行時（Runtime）データの保持者

| データ / インスタンス | 現在値の保持者 | 生成・更新を行う場所 | ライフタイム・破棄 |
| --- | --- | --- | --- |
| `GameplaySessionContext` | `GameplayController` | セッション開始時に Controller が生成 | Gameplay Scope |
| `GameplayConfig` | `GameplaySessionContext` | `GameplayConfigAsset.CreateRuntimeConfig` | セッション期間中不変 |
| `MissionCatalogPreparationSettings` | `GameplayStartRequest` | `GameplayEntryPoint`がProfile/Presetから変換 | Session開始要求中不変 |
| `MissionCatalog` | `GameplaySessionContext` | `MissionCatalogBuilder.Build` | セッション期間中不変 |
| 現在の `GameplayState` | `GameplayController` | 初期値は Controller、更新値は Reducer | 不変インスタンスを Controller が差し替え |
| 編集中の `Expression` | `ExpressionInput` | `ChangeNumber` / `ChangeOperator` / `Reset` | Scoped / Disposable |
| タイマー内部状態 | `ISectionTimer` 実装 | `Start` / Tick / `Stop` | Scoped / Disposable |
| `SectionTimerSnapshot` | Timer が生成し Controller が転送 | `ISectionTimer` 実装 | 通知ごとの一時値 |
| `SubmissionResolution` | `GameplayState.LastResolution` | Reducer | 不変値 |
| `SessionResult` | `SessionEndedEvent` の Payload | Reducer | Result画面へ引き渡す不変値 |
| State/Event 配信 Subject | `GameplayController` | Controller | Gameplay Scope 終了時に破棄 |

### 8.2 DI（依存注入）における所有構造

```mermaid
flowchart TB
    Scope[GameplayLifetimeScope]
    Controller[GameplayController]
    Flow[IGameplayFlow]
    Input[ExpressionInput]
    Strategy[IMissionDrawStrategy]
    Random[IRandomSource]
    Timer[ISectionTimer]
    Presenter[GameplayPresenter]

    Scope -->|Scoped instance| Controller
    Scope -->|Scoped instance| Input
    Scope -->|Scoped instance| Strategy
    Scope -->|Scoped instance| Random
    Scope -->|Scoped instance| Timer
    Scope -->|Scoped instance| Presenter
    Controller -.実装.-> Flow
    Controller -.参照.-> Input
    Controller -.参照.-> Strategy
    Controller -.参照.-> Timer
    Strategy -.参照.-> Random
    Presenter -.参照.-> Flow
```

- インスタンスの生成および破棄の管理は、すべて `GameplayLifetimeScope` が所有すること。
- `GameplayController` は注入された `ExpressionInput`、Strategy、Timer を進行上の協調オブジェクトとして参照すること。（Controller が直接 `new` しないこと）
- `GameplayReducer`、`ExpressionCalculator`、Matcher、候補Generator、Builder、Tier Resolver、Selectorは状態を持たない`static`クラスであり、DI登録の対象外とする。
- Presenter は公開ストリームの購読者であり、`GameplayState` やタイマー内部状態の所有者ではない。

### 8.3 状態の変更権限マトリクス

| 変更対象データ | 変更を指示できるクラス | 直接変更が禁止されているクラス |
| --- | --- | --- |
| 編集中の数式 | `GameplayController` を経由した `ExpressionInput` | Presenter、View、Reducer |
| セッション State | `GameplayReducer` の Transition を適用する `GameplayController` | Presenter、View、Timer、Strategy |
| タイマー状態 | `GameplayController` | Presenter、View、Reducer |
| 次 Mission 候補 | `IMissionDrawStrategy` を呼び出す `GameplayController` | Presenter、View、Reducer 単独 |
| 表示用 DTO | `GameplayPresenter` | Core、View |
| Unity Asset | Unity Editor / Infrastructure層 | Core Model / Logic |

---

## 9. 実装時のクラス別参照先ドキュメント

| 実装対象 | 役割概要 | 詳細API定義のリンク |
| --- | --- | --- |
| `Rational` | 正確な有理数計算と同値性 | [クラス設計 §4.1](../unity-class-design.md) |
| `CalculationResult` | 正常値とゼロ除算エラーの分離 | [クラス設計 §4.2](../unity-class-design.md) |
| `Expression` | 左数字・演算子・右数字の数式構造 | [クラス設計 §4.3](../unity-class-design.md) |
| `MissionCondition` | Missionクリア条件の不変値 | [クラス設計 §4.4](../unity-class-design.md) |
| `Mission` / `MissionId` | 提示中Missionおよびセッション内連番 | [クラス設計 §4.5](../unity-class-design.md) |
| `MissionCatalog` / `MissionCandidate` | セッション固定の出題候補リスト | [クラス設計 §4.6](../unity-class-design.md) |
| `GameplayConfig` | 数値範囲・閾値・重み設定 | [クラス設計 §4.7](../unity-class-design.md) |
| `GameplayState` / `GameplaySessionContext` | 進行状態スナップショットと固定Context | [クラス設計 §4.8](../unity-class-design.md) |
| `SessionResult` | ゲームオーバー時の最終実績データ | [クラス設計 §4.9](../unity-class-design.md) |
| `ExpressionCalculator` | 数式評価ロジック | [クラス設計 §5.1](../unity-class-design.md) |
| `ExpressionCandidateGenerator` | 正常な候補式の網羅的生成 | [クラス設計 §5.2](../unity-class-design.md) |
| `MissionConditionMatcher` | Mission条件への一致判定 | [クラス設計 §5.3](../unity-class-design.md) |
| `MissionParameterCandidateGenerator` | Profileと到達可能結果から候補生成 | [クラス設計 §5.4](../unity-class-design.md) |
| `MissionCatalogBuilder` | 候補分析・選出・Catalog構築 | [クラス設計 §5.4](../unity-class-design.md) |
| `WeightedIndexSelector` / `UniformIndexSelector` | サンプル値からのインデックス決定 | [クラス設計 §5.5](../unity-class-design.md) |
| `GameplayReducer` | 4つのセッション状態遷移 | [クラス設計 §6](../unity-class-design.md) |
| `ExpressionInput` | プレイヤーによる数式編集状態 | [クラス設計 §7.2](../unity-class-design.md) |
| `GameplayController` | 状態保持と副作用の調停層 | [クラス設計 §7.3](../unity-class-design.md) |
| `IMissionDrawStrategy` | カタログからの次 Mission 抽選 | [クラス設計 §7.4](../unity-class-design.md) |
| `ISectionTimer` | タイマースナップショットと時間切れ通知 | [クラス設計 §7.1 / §7.5](../unity-class-design.md) |

---

## 10. クラス境界判断のための原則ルール

- **データと不変条件** → `Ccc.Core.Model` に配置すること。
- **入力と出力が1対1の同期的・決定的な計算** → `Ccc.Core.Logic` に配置すること。
- **現在状態の保持、乱数、タイマー、非同期イベントの調停** → `Ccc.Core.UseCases` に配置すること。
- **数字や演算子の入力変更操作** → `ExpressionInput` のみ更新すること。
- **セッションの節目（状態遷移）** → Command を生成し `GameplayReducer` に渡すこと。
- **Reducerの制約** → Reducer 内から Unity API、R3 Subject、タイマー、乱数生成器を呼び出さないこと。
- **PresenterとViewの役割** → Presenter は Core の出力を DTO / 演出データへ変換し、View は完成された DTO を描画・表示するのみとすること。
- **Coreの完全隔離** → Unity アセットや `MonoBehaviour` を Core の Model / Logic に渡さないこと。

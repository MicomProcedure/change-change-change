# CCC Unity中核ロジック関係図

> [ドキュメント一覧](../../README.md) / 区分: 設計解説
>
> 正本: [Cosense「CCC Unity中核ロジック関係図」](https://scrapbox.io/proto-TeamL/CCC%20Unity%E4%B8%AD%E6%A0%B8%E3%83%AD%E3%82%B8%E3%83%83%E3%82%AF%E9%96%A2%E4%BF%82%E5%9B%B3)
> 取得時点の原文更新日時: 2026-08-28T09:48:31.000Z


> 対象：`Ccc.Core.Model`、`Ccc.Core.Logic`、`Ccc.Core.UseCases`と、その直接のPresentation境界

## 1. このページの読み方

このページは、中核ロジックを次の順番で把握するための図解である。

- 1. §2でゲーム全体を3つの塊として把握する。
- 2. 実装対象に応じて§4〜§7の詳細図を見る。
- 3. 「誰が何を保持するか」は§8で確認する。
- 4. 各クラスのAPI定義は§9から[CCC Unityクラス設計](../unity-class-design.md)の該当節へ移動して確認する。

図の矢印と記号は次の意味で使用する。

| 表現 | 意味 |
| --- | --- |
| `A --> B` | AがBを呼ぶ、または参照する |
| `A *-- B` | AがBをSession中の構成要素として保持する |
| `A o-- B` | AがBへの参照を保持するが、lifetimeはDI scopeなど外側が管理する |
| 実線の処理矢印 | runtimeの呼び出しまたはデータ移動 |
| 点線の矢印 | 生成物、通知、読み取りなどの間接的な関係 |

Modelは原則として不変である。図中の「保持」は、Modelを後から書き換えるという意味ではなく、現在値への参照を保持し、更新時には新しい値へ差し替えることを表す。

## 2. 全体像：3つの塊

最初は個別クラスではなく、次の3ブロックだけを押さえる。

```mermaid
flowchart LR
    Setup["① Session準備\nConfig・候補式・Catalog"]
    Play["② Session進行\n入力・State遷移・Timer・抽選"]
    Output["③ 表示境界\nState／Expression／Event／Timer Snapshot"]

    Setup -->|固定Contextと初回Candidate| Play
    Play -->|R3で公開| Output
    Output -->|Submit・演出完了| Play
```

- **Session準備**は開始時に1回だけ実行し、`GameplaySessionContext`を作る。
- **Session進行**の中心は`GameplayController`である。編集中の式は`ExpressionInput`、Sessionの節目は`GameplayReducer`、時間は`ISectionTimer`が担当する。
- **表示境界**では`GameplayPresenter`が公開ストリームを購読する。PresenterとViewはCoreの状態を直接変更しない。

3ブロックの内部構成は次のとおりである。

```mermaid
flowchart TB
    subgraph Setup["① Session準備（開始時に1回）"]
        Config[GameplayConfig]
        Params[MissionParameterCandidate列]
        Generator[ExpressionCandidateGenerator]
        Builder[MissionCatalogBuilder]
        Context[GameplaySessionContext]
        Config --> Builder
        Params --> Builder
        Generator -->|ExpressionCandidate列| Builder
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

式の数字・演算子変更は`ExpressionInput`内で完結し、`GameplayReducer`を通らない。Reducerを通るのは次の4つのSession遷移だけである。

- Submit
- TimeExpired
- FeedbackCompleted
- PresentMission

## 4. 詳細A：Modelの構成とデータ所有

この図の合成関係は「その不変スナップショットがどのデータを内包するか」を示す。

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

重要な境界は次のとおりである。

- `GameplaySessionContext`はSession中に変わらないConfigとCatalogをまとめる。
- `GameplayState`はSection、Life、現在Mission、最後の提出結果を持つSession進行スナップショットである。
- 編集中の式は`GameplayState`に入らず、`ExpressionInput.Current`が持つ。
- `SubmissionResolution.SubmittedExpression`は提出または時間切れの瞬間に確定した式である。
- `SessionResult.FinalExpression`は`LastResolution.SubmittedExpression`から導出する。
- `CorrectAnswerCount`はState／ResultのどちらでもSectionから導出し、格納しない。

## 5. 詳細B：Catalog構築

### 5.1 クラス関係

```mermaid
flowchart LR
    Config[GameplayConfig]
    Params[MissionParameterCandidate列]
    Gen[ExpressionCandidateGenerator]
    Calc[ExpressionCalculator]
    Expressions[ExpressionCandidate列]
    Builder[MissionCatalogBuilder]
    Matcher[MissionConditionMatcher]
    Catalog[MissionCatalog]

    Gen --> Calc
    Gen --> Expressions
    Expressions --> Builder
    Params --> Builder
    Config --> Builder
    Builder --> Matcher
    Builder -->|Build成功| Catalog
```

`MissionCatalogBuilder`内の候補分析と選出判定はprivate static methodであり、独立クラスではない。

```mermaid
flowchart TD
    Pair["MissionParameterCandidate × ExpressionCandidate"]
    Match["Matcherで各式を一致判定"]
    Analyze["Analyze\n適合数・非適合数・割合・回答例"]
    Include{ShouldInclude}
    Normalize["重複を除去し固定順に並べる"]
    Catalog[MissionCatalog]
    Exclude[除外]

    Pair --> Match --> Analyze --> Include
    Include -->|条件を満たす| Normalize --> Catalog
    Include -->|条件外| Exclude
```

### 5.2 選出条件の所有者

| 条件 | 判定する場所 | 参照元 |
| --- | --- | --- |
| Mission条件と計算結果が一致するか | `MissionConditionMatcher` | `MissionCondition`、`CalculationResult` |
| 適合数・非適合数・割合 | `MissionCatalogBuilder.Analyze` | 正常な`ExpressionCandidate`列 |
| 割合が許容範囲内か | `MissionCatalogBuilder.ShouldInclude` | `GameplayConfig` |
| 必要な最小適合式数 | `GameplayConfig.GetMinimumExpressionMatchCount` | MissionKind |
| Zero／Primeは1件以上、その他は2件以上 | `GameplayConfig`の設定をBuilderが適用 | `ZeroAndPrimeMinimumExpressionMatchCount`／`MinimumExpressionMatchCount` |
| 重複排除と固定順 | `MissionCatalogBuilder` | Tier、Kind、ParameterA、ParameterB、IsNegated |

## 6. 詳細C：Session進行

### 6.1 状態を持つクラスと純粋Reducer

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

`GameplayReducer`はstaticで内部状態を持たない。現在のContext、State、Commandをすべて引数で受け、次StateとEventを返す。対して`GameplayController`はSession中の現在値と配信順序を管理する。

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

| 入力 | Commandに入るsnapshot | 主な出力 |
| --- | --- | --- |
| 手動Submit | MissionId、`ExpressionInput.Current` | `SubmissionResolvedEvent` |
| Timer期限切れ | MissionId、`ExpressionInput.Current` | `SubmissionResolvedEvent` |
| 演出完了 | MissionId | 次抽選の許可、または`SessionEndedEvent` |
| 次Candidate提示 | 直前MissionId、MissionCandidate | `MissionPresentedEvent` |

### 6.3 Controllerが保証する処理順

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

無効なMissionId、2回目のSubmit、`Playing`以外のSubmit／TimeExpiredはReducerが`IsAccepted == false`として拒否する。Controllerは拒否されたTransitionからTimer操作やPresentation Event公開を行わない。

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
class FixedMissionDrawStrategy
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
}
class MissionCandidate

GameplayController --> IMissionDrawStrategy : calls
SimpleMissionDrawStrategy ..|> IMissionDrawStrategy
FixedMissionDrawStrategy ..|> IMissionDrawStrategy
SimpleMissionDrawStrategy o-- IRandomSource : injected
SimpleMissionDrawStrategy --> TierResolver
SimpleMissionDrawStrategy --> WeightedIndexSelector
SimpleMissionDrawStrategy --> UniformIndexSelector
IMissionDrawStrategy --> MissionDrawContext : input
IMissionDrawStrategy ..> MissionCandidate : result
```

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

`IRandomSource`から値を取るのは選択肢が2件以上ある段階だけである。Selectorは受け取ったsampleからIndexを求める純粋関数であり、乱数源を知らない。

### 7.2 Timerと式snapshot

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

- `ISectionTimer`実装が保持するのはMissionId、開始時刻、残り時間、期限切れ通知済みかどうかである。
- `SectionTimerSnapshot`は表示専用で、`GameplayState`には入れない。
- Expired Signal自体は式を持たない。受信時点でControllerが`ExpressionInput.Current`を読み、Commandへ格納する。
- 手動Submitと期限切れが競合しても、ReducerのStatusとMissionIdガードにより先に届いた一方だけが受理される。

## 8. 所有関係とlifetime

「所有」には、データの現在値を保持する責務と、instanceを破棄するlifetime責務の2種類がある。両者を混同しない。

### 8.1 Runtimeデータの保持者

| データ／instance | 現在値の保持者 | 値を作る／更新する場所 | lifetime・破棄 |
| --- | --- | --- | --- |
| `GameplaySessionContext` | `GameplayController` | Session開始時にControllerが組み立てる | Gameplay scope |
| `GameplayConfig` | `GameplaySessionContext` | `GameplayConfigAsset.CreateRuntimeConfig` | Session中は不変 |
| `MissionCatalog` | `GameplaySessionContext` | `MissionCatalogBuilder.Build` | Session中は不変 |
| 現在の`GameplayState` | `GameplayController` | 初期値はController、次値はReducer | 不変値をControllerが差し替える |
| 編集中の`Expression` | `ExpressionInput` | `ChangeNumber`／`ChangeOperator`／`Reset` | `ExpressionInput`はScoped／Disposable |
| Timer内部状態 | `ISectionTimer`実装 | `Start`／Tick／`Stop` | Scoped／Disposable |
| `SectionTimerSnapshot` | Timerが生成しControllerが転送 | `ISectionTimer`実装 | 通知ごとの値 |
| `SubmissionResolution` | `GameplayState.LastResolution` | Reducer | 不変値 |
| `SessionResult` | `SessionEndedEvent`のpayload | Reducer | Result遷移先へ渡す不変値 |
| State／Event配信Subject | `GameplayController` | Controller | Gameplay scope終了時に破棄 |

### 8.2 DI上の所有

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

- instanceの生成・破棄は`GameplayLifetimeScope`が所有する。
- `GameplayController`は注入された`ExpressionInput`、Strategy、Timerを進行上の協力オブジェクトとして参照する。
- Controllerが協力オブジェクトを直接newしない。
- `GameplayReducer`、`ExpressionCalculator`、Matcher、Builder、Selectorは状態を持たないstatic classであり、DI登録しない。
- Presenterは公開ストリームの購読者であり、`GameplayState`やTimer内部状態の所有者ではない。

### 8.3 変更権限

| 対象 | 変更を指示できるクラス | 直接変更してはいけないクラス |
| --- | --- | --- |
| 編集中の式 | `GameplayController`経由の`ExpressionInput` | Presenter、View、Reducer |
| Session State | `GameplayReducer`のTransitionを採用する`GameplayController` | Presenter、View、Timer、Strategy |
| Timer | `GameplayController` | Presenter、View、Reducer |
| 次Mission候補 | `IMissionDrawStrategy`を呼ぶ`GameplayController` | Presenter、View、Reducer単独 |
| 表示DTO | `GameplayPresenter` | Core、View |
| Unity Asset | Unity Editor／Infrastructure | Core Model／Logic |

## 9. 実装時のクラス別参照先

| 実装対象 | 役割 | API定義 |
| --- | --- | --- |
| `Rational` | 正確な有理数と同値性 | [クラス設計 §4.1](https://scrapbox.io/proto-TeamL/CCC_Unityクラス設計#6a902ea2c9a3d3000000150cd6) |
| `CalculationResult` | 正常値と0除算の分離 | [クラス設計 §4.2](https://scrapbox.io/proto-TeamL/CCC_Unityクラス設計#6a902ea2c9a3d300000026ba99) |
| `Expression` | 左数字・共有演算子・右数字 | [クラス設計 §4.3](https://scrapbox.io/proto-TeamL/CCC_Unityクラス設計#6a902ea2c9a3d3000000fad28d) |
| `MissionCondition` | Mission条件の不変値 | [クラス設計 §4.4](https://scrapbox.io/proto-TeamL/CCC_Unityクラス設計#6a902ea2c9a3d30000005b2c36) |
| `Mission`／`MissionId` | 提示済みMissionとSession内連番 | [クラス設計 §4.5](https://scrapbox.io/proto-TeamL/CCC_Unityクラス設計#6a902ea2c9a3d300000012ed52) |
| `MissionCatalog`／`MissionCandidate` | Session固定の出題候補 | [クラス設計 §4.6](https://scrapbox.io/proto-TeamL/CCC_Unityクラス設計#6a902ea2c9a3d30000002a8c0c) |
| `GameplayConfig` | 数字範囲・閾値・重み | [クラス設計 §4.7](https://scrapbox.io/proto-TeamL/CCC_Unityクラス設計#6a902ea2c9a3d30000004ce412) |
| `GameplayState`／`GameplaySessionContext` | Session進行値と固定Context | [クラス設計 §4.8](https://scrapbox.io/proto-TeamL/CCC_Unityクラス設計#6a902ea2c9a3d30000009743bf) |
| `SessionResult` | GameOver時の最終snapshot | [クラス設計 §4.9](https://scrapbox.io/proto-TeamL/CCC_Unityクラス設計#6a902ea2c9a3d3000000b7d2ba) |
| `ExpressionCalculator` | 式評価 | [クラス設計 §5.1](https://scrapbox.io/proto-TeamL/CCC_Unityクラス設計#6a902ea2c9a3d3000000fade6a) |
| `ExpressionCandidateGenerator` | 正常な候補式列挙 | [クラス設計 §5.2](https://scrapbox.io/proto-TeamL/CCC_Unityクラス設計#6a902ea2c9a3d3000000c5509e) |
| `MissionConditionMatcher` | Mission一致判定 | [クラス設計 §5.3](https://scrapbox.io/proto-TeamL/CCC_Unityクラス設計#6a902ea2c9a3d3000000666d90) |
| `MissionCatalogBuilder` | 分析・選出・Catalog構築 | [クラス設計 §5.4](https://scrapbox.io/proto-TeamL/CCC_Unityクラス設計#6a902ea2c9a3d30000006088ac) |
| `WeightedIndexSelector`／`UniformIndexSelector` | sampleからIndexを決定 | [クラス設計 §5.5](https://scrapbox.io/proto-TeamL/CCC_Unityクラス設計#6a902ea2c9a3d3000000fd3362) |
| `GameplayReducer` | 4つのSession遷移 | [クラス設計 §6](https://scrapbox.io/proto-TeamL/CCC_Unityクラス設計#6a902ea2c9a3d300000084748b) |
| `ExpressionInput` | 編集中の式 | [クラス設計 §7.2](https://scrapbox.io/proto-TeamL/CCC_Unityクラス設計#6a902ea2c9a3d3000000857b1c) |
| `GameplayController` | 状態保持と副作用の調停 | [クラス設計 §7.3](https://scrapbox.io/proto-TeamL/CCC_Unityクラス設計#6a9119c8c9a3d300000038e43b) |
| `IMissionDrawStrategy` | Catalogから次Candidateを抽選 | [クラス設計 §7.4](https://scrapbox.io/proto-TeamL/CCC_Unityクラス設計#6a902ea2c9a3d3000000d9fb0b) |
| `ISectionTimer` | 時間snapshotと期限切れ通知 | [クラス設計 §7.1](https://scrapbox.io/proto-TeamL/CCC_Unityクラス設計#6a902ea2c9a3d3000000e675c0)／[§7.5](https://scrapbox.io/proto-TeamL/CCC_Unityクラス設計#6a902ea2c9a3d3000000ab8cd7) |

主要クラスの間をつなぐ補助型は次の位置づけである。

| 補助型 | 接続する場所 | 所有・状態 |
| --- | --- | --- |
| `RationalMath` | `Rational`を使う四則演算、剰余、桁取得 | static。状態を持たない |
| `TimerConfig`／`TierProgression` | `GameplayConfig`の構成値 | Configに内包され、Session中は不変 |
| `ExpressionCandidate` | Generatorの出力、Builderの分析対象、Candidateの回答例 | 不変値 |
| `MissionParameterCandidate` | Asset境界からBuilderへ渡すMission候補 | 不変値 |
| `GameplayStateQueries` | StateとContextから現在Tierなどを導出 | static。状態を持たない |
| `MissionCatalogBuildResult` | Builderの成功Catalogまたは理由付き失敗 | 呼び出しごとの戻り値 |
| `MissionDrawResult` | Strategyの成功Candidateまたは理由付き失敗 | 呼び出しごとの戻り値 |
| `GameplayStartResult` | ControllerのSession開始結果 | 呼び出しごとの戻り値 |
| `GameplayFlowFailure` | ControllerからPresentationへ公開する続行不能理由 | Eventストリーム上の値 |
| `SectionTimerSnapshot` | Timerから表示へ渡す残り時間 | 通知ごとの値。Stateへ格納しない |
| `SectionTimerExpiredSignal` | TimerからControllerへ渡す期限切れ事実 | 通知ごとの値。式はControllerが補う |

## 10. 境界を判断するための短い規則

- 値そのものと不変条件は`Ccc.Core.Model`へ置く。
- 同じ入力から同じ結果を返す計算は`Ccc.Core.Logic`へ置く。
- 現在値の保持、乱数、Timer、配信順序の調停は`Ccc.Core.UseCases`へ置く。
- 数字・演算子変更では`ExpressionInput`だけを更新する。
- Sessionの節目ではCommandを作り`GameplayReducer`へ渡す。
- ReducerからUnity API、R3 Subject、Timer、乱数を呼ばない。
- PresenterはCoreの出力をDTOと演出へ変換し、Viewは完成済みDTOを描画する。
- Unity AssetとMonoBehaviourをCore Model／Logicへ渡さない。

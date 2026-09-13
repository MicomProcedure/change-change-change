using System;
using System.Collections.Generic;
using Ccc.Core.Logic;
using Ccc.Core.Model;
using R3;

namespace Ccc.Core.UseCases
{
    /// <summary>
    /// Gameplayの現在値、入力、Timer、Reducerを同期的に調停するControllerです。
    /// </summary>
    public sealed class GameplayController : IGameplayFlow, IDisposable
    {
        private readonly GameplayConfig _config;
        private readonly ExpressionInput _expressionInput;
        private readonly ISectionTimer _sectionTimer;
        private readonly IMissionDrawStrategy _missionDrawStrategy;
        private readonly Subject<GameplayState> _statesSubject;
        private readonly Subject<Expression> _expressionsSubject;
        private readonly Subject<GameplayEvent> _eventsSubject;
        private readonly Subject<GameplayFlowFailure> _failuresSubject;
        private readonly Subject<SectionTimerSnapshot> _timerSnapshotsSubject;
        private readonly IDisposable _expressionSubscription;
        private readonly IDisposable _timerSnapshotSubscription;
        private readonly IDisposable _expiredSignalSubscription;

        private GameplaySessionContext _sessionContext;
        private GameplayState _currentState;
        private bool _isDisposed;

        /// <summary>
        /// GameplayControllerが公開する状態ストリームを取得します。
        /// </summary>
        public Observable<GameplayState> States { get; }

        /// <summary>
        /// GameplayControllerが公開する式ストリームを取得します。
        /// </summary>
        public Observable<Expression> Expressions { get; }

        /// <summary>
        /// GameplayControllerが公開するEventストリームを取得します。
        /// </summary>
        public Observable<GameplayEvent> Events { get; }

        /// <summary>
        /// GameplayControllerが公開するFailureストリームを取得します。
        /// </summary>
        public Observable<GameplayFlowFailure> Failures { get; }

        /// <summary>
        /// Timer Snapshotの転送ストリームを取得します。
        /// </summary>
        public Observable<SectionTimerSnapshot> TimerSnapshots { get; }

        /// <summary>
        /// GameplayControllerを生成します。
        /// </summary>
        /// <param name="config">検証済みのGameplay設定。</param>
        /// <param name="expressionInput">提出式の入力所有者。</param>
        /// <param name="sectionTimer">SectionTimer。</param>
        /// <param name="missionDrawStrategy">Mission抽選Strategy。</param>
        /// <exception cref="ArgumentNullException">依存のいずれかがnullの場合。</exception>
        public GameplayController(
            GameplayConfig config,
            ExpressionInput expressionInput,
            ISectionTimer sectionTimer,
            IMissionDrawStrategy missionDrawStrategy)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _expressionInput = expressionInput ?? throw new ArgumentNullException(nameof(expressionInput));
            _sectionTimer = sectionTimer ?? throw new ArgumentNullException(nameof(sectionTimer));
            _missionDrawStrategy = missionDrawStrategy ?? throw new ArgumentNullException(nameof(missionDrawStrategy));

            _statesSubject = new Subject<GameplayState>();
            _expressionsSubject = new Subject<Expression>();
            _eventsSubject = new Subject<GameplayEvent>();
            _failuresSubject = new Subject<GameplayFlowFailure>();
            _timerSnapshotsSubject = new Subject<SectionTimerSnapshot>();

            States = _statesSubject;
            Expressions = _expressionsSubject;
            Events = _eventsSubject;
            Failures = _failuresSubject;
            TimerSnapshots = _timerSnapshotsSubject;

            _expressionSubscription = _expressionInput.Expressions.Subscribe(HandleExpressionChanged);
            _timerSnapshotSubscription = _sectionTimer.Snapshots.Subscribe(HandleTimerSnapshot);
            _expiredSignalSubscription = _sectionTimer.ExpiredSignals.Subscribe(HandleExpiredSignal);
        }

        /// <summary>
        /// Sessionを構築し、初期状態と初期Missionを同期的に公開します。
        /// </summary>
        /// <param name="request">セッション開始Request。</param>
        /// <returns>Session開始結果。</returns>
        /// <exception cref="ObjectDisposedException">破棄済みの場合。</exception>
        /// <exception cref="ArgumentNullException">Requestがnullの場合。</exception>
        public GameplayStartResult StartSession(GameplayStartRequest request)
        {
            ThrowIfDisposed();
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (_currentState != null &&
                (_currentState.Status == GameplayStatus.Playing ||
                 _currentState.Status == GameplayStatus.Transitioning))
            {
                GameplayFlowFailure failure = CreateFailure(
                    GameplayFlowFailureKind.SessionAlreadyRunning,
                    "A Gameplay session is already running.");
                _failuresSubject.OnNext(failure);
                return GameplayStartResult.FailureResult(failure);
            }

            _sectionTimer.StopAll();
            _sessionContext = null;
            _currentState = null;

            IReadOnlyList<ExpressionCandidate> expressionCandidates;
            try
            {
                expressionCandidates = ExpressionCandidateGenerator.Generate(
                    _config.MinimumNumber,
                    _config.MaximumNumber);
            }
            catch (Exception exception)
            {
                return FailStart(
                    GameplayFlowFailureKind.InvalidConfiguration,
                    $"Expression candidate generation failed: {exception.Message}");
            }

            MissionCatalogBuildResult catalogBuildResult;
            try
            {
                catalogBuildResult = MissionCatalogBuilder.Build(
                    expressionCandidates,
                    request.MissionParameterCandidates,
                    _config);
            }
            catch (Exception exception)
            {
                return FailStart(
                    GameplayFlowFailureKind.CatalogBuildFailed,
                    $"Mission catalog build failed: {exception.Message}");
            }

            if (!catalogBuildResult.IsSuccess)
            {
                return FailStart(
                    GameplayFlowFailureKind.CatalogBuildFailed,
                    catalogBuildResult.FailureMessage);
            }

            GameplaySessionContext sessionContext =
                new GameplaySessionContext(_config, catalogBuildResult.Catalog);
            MissionDrawResult drawResult;
            try
            {
                drawResult = _missionDrawStrategy.Draw(
                    MissionDrawContext.CreateForInitialMission(sessionContext, 1));
            }
            catch (Exception exception)
            {
                return FailStart(
                    GameplayFlowFailureKind.MissionDrawFailed,
                    $"Initial mission draw failed: {exception.Message}");
            }

            if (!drawResult.IsSuccess)
            {
                return FailStart(
                    GameplayFlowFailureKind.MissionDrawFailed,
                    drawResult.FailureMessage);
            }

            var initialMission = new Mission(
                new MissionId(1),
                drawResult.Candidate.Condition,
                drawResult.Candidate.Tier);
            var initialState = new GameplayState(
                GameplayStatus.Playing,
                1,
                _config.InitialLife,
                initialMission,
                null);

            _expressionInput.ResetSilently(_config.InitialExpression);
            _sessionContext = sessionContext;
            _currentState = initialState;

            _statesSubject.OnNext(initialState);
            _expressionsSubject.OnNext(_expressionInput.Current);

            TimeSpan duration = _config.Timer.GetDuration(initialState.Section);
            _sectionTimer.Start(initialMission.Id, duration);
            _eventsSubject.OnNext(new MissionPresentedEvent(initialMission, duration));

            return GameplayStartResult.Success(sessionContext, initialState);
        }

        /// <summary>
        /// Playing中の場合に指定された側の数字を変更します。
        /// </summary>
        /// <param name="side">変更対象の左右。</param>
        /// <exception cref="ObjectDisposedException">破棄済みの場合。</exception>
        public void ChangeNumber(PlayerSide side)
        {
            ThrowIfDisposed();
            if (_currentState == null || _currentState.Status != GameplayStatus.Playing)
            {
                return;
            }

            _expressionInput.ChangeNumber(side);
        }

        /// <summary>
        /// Playing中の場合に演算子を変更します。
        /// </summary>
        /// <exception cref="ObjectDisposedException">破棄済みの場合。</exception>
        public void ChangeOperator()
        {
            ThrowIfDisposed();
            if (_currentState == null || _currentState.Status != GameplayStatus.Playing)
            {
                return;
            }

            _expressionInput.ChangeOperator();
        }

        /// <summary>
        /// Playing中かつMissionId一致時に現在式を提出します。
        /// </summary>
        /// <param name="missionId">提出対象のMissionId。</param>
        /// <exception cref="ObjectDisposedException">破棄済みの場合。</exception>
        public void Submit(MissionId missionId)
        {
            ThrowIfDisposed();
            if (!CanAcceptCurrentMission(missionId, GameplayStatus.Playing))
            {
                return;
            }

            ProcessCommand(new SubmitCommand(missionId, _expressionInput.Current));
        }

        /// <summary>
        /// Transitioning中かつMissionId一致時にフィードバック完了を通知します。
        /// </summary>
        /// <param name="missionId">演出対象のMissionId。</param>
        /// <exception cref="ObjectDisposedException">破棄済みの場合。</exception>
        public void NotifyFeedbackCompleted(MissionId missionId)
        {
            ThrowIfDisposed();
            if (!CanAcceptCurrentMission(missionId, GameplayStatus.Transitioning))
            {
                return;
            }

            GameplayTransition feedbackTransition = GameplayReducer.Reduce(
                _sessionContext,
                _currentState,
                new FeedbackCompletedCommand(missionId));
            if (!feedbackTransition.IsAccepted)
            {
                return;
            }

            ApplyTransition(feedbackTransition);
            if (_currentState.Status != GameplayStatus.Transitioning || _currentState.Life <= 0)
            {
                return;
            }

            MissionDrawResult drawResult;
            try
            {
                drawResult = _missionDrawStrategy.Draw(
                    MissionDrawContext.CreateForNextMission(
                        _sessionContext,
                        _currentState.Section,
                        _expressionInput.Current,
                        _currentState.CurrentMission));
            }
            catch (Exception exception)
            {
                PublishFailure(
                    GameplayFlowFailureKind.MissionDrawFailed,
                    $"Next mission draw failed: {exception.Message}");
                return;
            }

            if (!drawResult.IsSuccess)
            {
                PublishFailure(
                    GameplayFlowFailureKind.MissionDrawFailed,
                    drawResult.FailureMessage);
                return;
            }

            GameplayTransition presentTransition = GameplayReducer.Reduce(
                _sessionContext,
                _currentState,
                new PresentMissionCommand(missionId, drawResult.Candidate));
            if (presentTransition.IsAccepted)
            {
                ApplyTransition(presentTransition);
            }
        }

        /// <summary>
        /// Timerと購読、公開Subjectを破棄します。
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            _expressionSubscription.Dispose();
            _timerSnapshotSubscription.Dispose();
            _expiredSignalSubscription.Dispose();
            try
            {
                _sectionTimer.StopAll();
            }
            catch (ObjectDisposedException)
            {
                // VContainerまたは外部Ownerによる先行Disposeを許容します。
            }

            try
            {
                _expressionInput.Dispose();
            }
            catch (ObjectDisposedException)
            {
                // VContainerまたは外部Ownerによる先行Disposeを許容します。
            }
            _sessionContext = null;
            _currentState = null;

            _statesSubject.OnCompleted();
            _expressionsSubject.OnCompleted();
            _eventsSubject.OnCompleted();
            _failuresSubject.OnCompleted();
            _timerSnapshotsSubject.OnCompleted();

            _statesSubject.Dispose();
            _expressionsSubject.Dispose();
            _eventsSubject.Dispose();
            _failuresSubject.Dispose();
            _timerSnapshotsSubject.Dispose();
        }

        private void HandleExpressionChanged(Expression expression)
        {
            if (!_isDisposed)
            {
                _expressionsSubject.OnNext(expression);
            }
        }

        private void HandleTimerSnapshot(SectionTimerSnapshot snapshot)
        {
            if (!_isDisposed)
            {
                _timerSnapshotsSubject.OnNext(snapshot);
            }
        }

        private void HandleExpiredSignal(SectionTimerExpiredSignal signal)
        {
            if (_isDisposed || !CanAcceptCurrentMission(signal.MissionId, GameplayStatus.Playing))
            {
                return;
            }

            ProcessCommand(new TimeExpiredCommand(signal.MissionId, _expressionInput.Current));
        }

        private void ProcessCommand(GameplayCommand command)
        {
            try
            {
                GameplayTransition transition = GameplayReducer.Reduce(
                    _sessionContext,
                    _currentState,
                    command);
                if (transition.IsAccepted)
                {
                    ApplyTransition(transition);
                }
            }
            catch (Exception exception)
            {
                PublishFailure(
                    GameplayFlowFailureKind.CatalogBuildFailed,
                    $"Gameplay command processing failed: {exception.Message}");
            }
        }

        private void ApplyTransition(GameplayTransition transition)
        {
            _currentState = transition.NextState;
            _statesSubject.OnNext(_currentState);

            for (int index = 0; index < transition.Events.Count; index++)
            {
                switch (transition.Events[index])
                {
                    case SubmissionResolvedEvent resolvedEvent:
                        _sectionTimer.Stop(resolvedEvent.Resolution.MissionId);
                        break;
                    case MissionPresentedEvent presentedEvent:
                        _sectionTimer.Start(presentedEvent.Mission.Id, presentedEvent.Duration);
                        break;
                }
            }

            for (int index = 0; index < transition.Events.Count; index++)
            {
                _eventsSubject.OnNext(transition.Events[index]);
            }
        }

        private bool CanAcceptCurrentMission(MissionId missionId, GameplayStatus requiredStatus)
        {
            return _sessionContext != null &&
                   _currentState != null &&
                   _currentState.Status == requiredStatus &&
                   _currentState.CurrentMission.Id == missionId;
        }

        private GameplayStartResult FailStart(GameplayFlowFailureKind kind, string message)
        {
            _sectionTimer.StopAll();
            _sessionContext = null;
            _currentState = null;
            GameplayFlowFailure failure = CreateFailure(kind, message);
            _failuresSubject.OnNext(failure);
            return GameplayStartResult.FailureResult(failure);
        }

        private void PublishFailure(GameplayFlowFailureKind kind, string message)
        {
            _sectionTimer.StopAll();
            _failuresSubject.OnNext(CreateFailure(kind, message));
        }

        private static GameplayFlowFailure CreateFailure(GameplayFlowFailureKind kind, string message)
        {
            return GameplayFlowFailure.Create(
                kind,
                string.IsNullOrWhiteSpace(message) ? "Gameplay flow failed." : message);
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(GameplayController));
            }
        }
    }
}

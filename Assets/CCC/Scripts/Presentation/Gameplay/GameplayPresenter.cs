using System;
using System.Globalization;
using System.Threading;
using Ccc.Core.Logic;
using Ccc.Core.Model;
using Ccc.Core.UseCases;
using Ccc.Presentation.Audio;
using Cysharp.Threading.Tasks;
using R3;

namespace Ccc.Presentation
{
    /// <summary>
    /// Gameplayの入力・状態・EventをView表示とNavigationへ変換するPresenterです。
    /// </summary>
    public sealed class GameplayPresenter : IDisposable
    {
        private const int TimeWarningThresholdSeconds = 3;

        private readonly IGameplayFlow _flow;
        private readonly IGameplayView _view;
        private readonly IGameplayNavigation _navigation;
        private readonly IAudioPlayer _audio;
        private readonly MissionTextFormatter _missionTextFormatter;
        private readonly RationalDecimalFormatter _resultFormatter;

        private readonly CompositeDisposable _subscriptions;
        private CancellationTokenSource _lifecycleCancellationSource;
        private IDisposable _lifecycleCancellationRegistration;
        private CancellationTokenSource _feedbackCancellationSource;
        private GameplayState _latestState;
        private Expression _latestExpression;
        private SectionTimerSnapshot _latestTimerSnapshot;
        private bool _hasExpression;
        private bool _hasTimerSnapshot;
        private bool _isFeedbackRunning;
        private MissionId _timeWarningMissionId;
        private int _lastTimeWarningSecond;
        private bool _hasTimeWarning;
        private bool _isStarted;
        private bool _isDisposed;

        /// <summary>
        /// GameplayPresenterを生成します。
        /// </summary>
        /// <param name="flow">Gameplayフロー。</param>
        /// <param name="view">Gameplay View。</param>
        /// <param name="navigation">画面遷移ポート。</param>
        /// <param name="audio">Audio再生ポート。</param>
        /// <param name="rationalFormatter">計算結果Formatter。</param>
        /// <param name="missionTextFormatter">Mission表示Formatter。</param>
        /// <exception cref="ArgumentNullException">依存のいずれかがnullの場合。</exception>
        public GameplayPresenter(
            IGameplayFlow flow,
            IGameplayView view,
            IGameplayNavigation navigation,
            IAudioPlayer audio,
            RationalDecimalFormatter rationalFormatter,
            MissionTextFormatter missionTextFormatter)
        {
            _flow = flow ?? throw new ArgumentNullException(nameof(flow));
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
            _audio = audio ?? throw new ArgumentNullException(nameof(audio));
            _missionTextFormatter = missionTextFormatter ?? throw new ArgumentNullException(nameof(missionTextFormatter));
            _resultFormatter = rationalFormatter ?? throw new ArgumentNullException(nameof(rationalFormatter));
            _subscriptions = new CompositeDisposable();
        }

        /// <summary>
        /// ViewとGameplayフローの購読を開始します。
        /// </summary>
        /// <param name="cancellationToken">Gameplay scopeの終了Token。</param>
        /// <exception cref="InvalidOperationException">二重開始または破棄済みの場合。</exception>
        public void Start(CancellationToken cancellationToken)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(GameplayPresenter));
            }

            if (_isStarted)
            {
                throw new InvalidOperationException("GameplayPresenter has already been started.");
            }

            _isStarted = true;
            _lifecycleCancellationSource =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _lifecycleCancellationRegistration =
                _lifecycleCancellationSource.Token.Register(Dispose);

            _subscriptions.Add(_view.Inputs.Subscribe(HandleInput));
            _subscriptions.Add(_flow.States.Subscribe(HandleState));
            _subscriptions.Add(_flow.Expressions.Subscribe(HandleExpression));
            _subscriptions.Add(_flow.TimerSnapshots.Subscribe(HandleTimerSnapshot));
            _subscriptions.Add(_flow.Events.Subscribe(HandleEvent));
            _subscriptions.Add(_flow.Failures.Subscribe(HandleFailure));
        }

        /// <summary>
        /// View・購読・進行中のFeedback演出を破棄します。
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            _lifecycleCancellationRegistration?.Dispose();
            _lifecycleCancellationRegistration = null;
            if (_lifecycleCancellationSource != null)
            {
                if (!_lifecycleCancellationSource.IsCancellationRequested)
                {
                    _lifecycleCancellationSource.Cancel();
                }

                _lifecycleCancellationSource.Dispose();
                _lifecycleCancellationSource = null;
            }

            _feedbackCancellationSource?.Cancel();
            _feedbackCancellationSource?.Dispose();
            _feedbackCancellationSource = null;
            _isFeedbackRunning = false;
            _subscriptions.Dispose();
        }

        private void HandleInput(GameplayInput input)
        {
            if (_isDisposed)
            {
                return;
            }

            switch (input.Kind)
            {
                case GameplayInputKind.ChangeLeftNumber:
                    if (!CanForwardGameplayInput())
                    {
                        return;
                    }

                    _audio.PlaySE(AudioKey.SEChangeClick);
                    _flow.ChangeNumber(PlayerSide.Left);
                    break;
                case GameplayInputKind.ChangeRightNumber:
                    if (!CanForwardGameplayInput())
                    {
                        return;
                    }

                    _audio.PlaySE(AudioKey.SEChangeClick);
                    _flow.ChangeNumber(PlayerSide.Right);
                    break;
                case GameplayInputKind.ChangeLeftOperator:
                case GameplayInputKind.ChangeRightOperator:
                    if (!CanForwardGameplayInput())
                    {
                        return;
                    }

                    _audio.PlaySE(AudioKey.SEChangeClick);
                    _flow.ChangeOperator();
                    break;
                case GameplayInputKind.Submit:
                    if (CanForwardGameplayInput())
                    {
                        _audio.PlaySE(AudioKey.SENormalClick);
                        _flow.Submit(_latestState.CurrentMission.Id);
                    }
                    break;
                case GameplayInputKind.ReturnToMainMenu:
                    NavigateToMainMenu();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(input));
            }
        }

        private void HandleState(GameplayState state)
        {
            if (_latestState == null ||
                _latestState.CurrentMission.Id != state.CurrentMission.Id)
            {
                _hasTimerSnapshot = false;
                _hasTimeWarning = false;
                _lastTimeWarningSecond = 0;
                _timeWarningMissionId = state.CurrentMission.Id;
            }

            _latestState = state;
            RenderLatestViewState();
        }

        private void HandleExpression(Expression expression)
        {
            _latestExpression = expression;
            _hasExpression = true;
            RenderLatestViewState();
        }

        private void HandleTimerSnapshot(SectionTimerSnapshot snapshot)
        {
            if (_latestState == null || snapshot.MissionId != _latestState.CurrentMission.Id)
            {
                return;
            }

            _latestTimerSnapshot = snapshot;
            _hasTimerSnapshot = true;
            PlayTimeWarningIfNeeded(snapshot);
            RenderLatestViewState();
        }

        private void HandleEvent(GameplayEvent gameplayEvent)
        {
            switch (gameplayEvent)
            {
                case MissionPresentedEvent:
                    _audio.PlaySE(AudioKey.SEMissionPresented);
                    break;
                case SubmissionResolvedEvent resolvedEvent:
                    if (!_isFeedbackRunning)
                    {
                        _isFeedbackRunning = true;
                        PlayFeedbackAsync(resolvedEvent).Forget(HandleAsyncFailure);
                    }
                    break;
                case SessionEndedEvent endedEvent:
                    _navigation.ShowResult(endedEvent.Result);
                    _audio.PlaySE(AudioKey.SEGameOver);
                    _audio.PlayBgm(AudioKey.BgmResult);
                    break;
            }
        }

        private void HandleFailure(GameplayFlowFailure failure)
        {
            if (!_isDisposed)
            {
                _view.ShowFatalError(failure.Message);
            }
        }

        private async UniTask PlayFeedbackAsync(SubmissionResolvedEvent resolvedEvent)
        {
            if (_isDisposed || _lifecycleCancellationSource == null)
            {
                _isFeedbackRunning = false;
                return;
            }

            CancellationTokenSource feedbackCancellationSource =
                CancellationTokenSource.CreateLinkedTokenSource(
                    _lifecycleCancellationSource.Token);
            _feedbackCancellationSource = feedbackCancellationSource;
            bool isFeedbackCompleted = false;
            try
            {
                _audio.PlaySE(
                    resolvedEvent.Resolution.IsSuccess
                        ? AudioKey.SECorrect
                        : AudioKey.SEWrong);

                FeedbackViewDataDto feedback = new FeedbackViewDataDto(
                    resolvedEvent.Resolution.MissionId,
                    resolvedEvent.Resolution.IsSuccess,
                    _resultFormatter.Format(resolvedEvent.AnswerExample.Result),
                    resolvedEvent.RemainingLife);

                await _view.PlayFeedbackAsync(
                    feedback,
                    feedbackCancellationSource.Token);
                isFeedbackCompleted = true;
            }
            catch (OperationCanceledException)
            {
                // Scope終了または画面遷移による正常なキャンセルです。
            }
            catch (Exception exception)
            {
                HandleAsyncFailure(exception);
            }

            try
            {
                if (isFeedbackCompleted &&
                    !_isDisposed &&
                    !feedbackCancellationSource.IsCancellationRequested &&
                    _lifecycleCancellationSource != null &&
                    !_lifecycleCancellationSource.IsCancellationRequested)
                {
                    _flow.NotifyFeedbackCompleted(resolvedEvent.Resolution.MissionId);
                }
            }
            finally
            {
                feedbackCancellationSource.Dispose();
                if (ReferenceEquals(_feedbackCancellationSource, feedbackCancellationSource))
                {
                    _feedbackCancellationSource = null;
                }

                _isFeedbackRunning = false;
            }
        }

        private void RenderLatestViewState()
        {
            if (_isDisposed || _latestState == null || !_hasExpression)
            {
                return;
            }

            _view.Render(CreateViewStateDto(
                null,
                _latestState,
                _latestExpression,
                _hasTimerSnapshot ? _latestTimerSnapshot : (SectionTimerSnapshot?)null));
        }

        private GameplayViewStateDto CreateViewStateDto(
            GameplaySessionContext context,
            GameplayState gameplayState,
            Expression expression,
            SectionTimerSnapshot? timerSnapshot)
        {
            if (gameplayState == null)
            {
                throw new ArgumentNullException(nameof(gameplayState));
            }

            string missionText = _missionTextFormatter.Format(gameplayState.CurrentMission.Condition);
            CalculationResult calculationResult = ExpressionCalculator.Evaluate(expression);
            string answerText = _resultFormatter.Format(calculationResult);
            bool hasRemainingTime = timerSnapshot.HasValue &&
                                    timerSnapshot.Value.MissionId == gameplayState.CurrentMission.Id;
            float remainingSeconds = hasRemainingTime
                ? (float)timerSnapshot.Value.RemainingTime.TotalSeconds
                : 0f;

            return new GameplayViewStateDto(
                gameplayState.Section,
                gameplayState.CurrentMission.Tier,
                gameplayState.Life,
                missionText,
                expression.Left.ToString(CultureInfo.InvariantCulture),
                FormatOperator(expression.Operator),
                expression.Right.ToString(CultureInfo.InvariantCulture),
                answerText,
                hasRemainingTime,
                remainingSeconds,
                false,
                gameplayState.IsAcceptingSubmit);
        }

        private void NavigateToMainMenu()
        {
            if (_isDisposed || _lifecycleCancellationSource == null)
            {
                return;
            }

            _lifecycleCancellationSource.Cancel();
            NavigateToMainMenuAsync().Forget(HandleAsyncFailure);
        }

        private async UniTask NavigateToMainMenuAsync()
        {
            await _navigation.NavigateToMainMenuAsync();
        }

        private void HandleAsyncFailure(Exception exception)
        {
            if (!_isDisposed && !(exception is OperationCanceledException))
            {
                _view.ShowFatalError($"Gameplay presentation failed: {exception.Message}");
            }
        }

        private bool CanForwardGameplayInput()
        {
            return !_isDisposed &&
                   _latestState != null &&
                   _latestState.Status == GameplayStatus.Playing;
        }

        private void PlayTimeWarningIfNeeded(SectionTimerSnapshot snapshot)
        {
            if (snapshot.Status != SectionTimerStatus.Running ||
                snapshot.RemainingTime > TimeSpan.FromSeconds(TimeWarningThresholdSeconds))
            {
                return;
            }

            int remainingSecond = Math.Max(
                0,
                (int)Math.Ceiling(snapshot.RemainingTime.TotalSeconds));
            if (remainingSecond > TimeWarningThresholdSeconds ||
                (_hasTimeWarning &&
                 _timeWarningMissionId == snapshot.MissionId &&
                 _lastTimeWarningSecond == remainingSecond))
            {
                return;
            }

            _hasTimeWarning = true;
            _timeWarningMissionId = snapshot.MissionId;
            _lastTimeWarningSecond = remainingSecond;
            _audio.PlaySE(AudioKey.SETimeClock);
        }

        private static string FormatOperator(OperatorType @operator)
        {
            switch (@operator)
            {
                case OperatorType.Add:
                    return "+";
                case OperatorType.Subtract:
                    return "−";
                case OperatorType.Multiply:
                    return "×";
                case OperatorType.Divide:
                    return "÷";
                default:
                    throw new ArgumentOutOfRangeException(nameof(@operator), @operator, null);
            }
        }
    }
}

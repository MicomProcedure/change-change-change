using System;
using System.Collections.Generic;
using Ccc.Presentation;
using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;

namespace Ccc.Infrastructure.Gameplay
{
    /// <summary>
    /// Gameplay画面の表示とユーザー入力をPresentationポートへ適合させるViewです。
    /// </summary>
    public sealed class GameplayView : MonoBehaviour, IGameplayView, IGameplayCountdownView
    {
        [Header("Gameplay Views")]
        [SerializeField] private MissionView _missionView;
        [SerializeField] private LifeView _lifeView;
        [SerializeField] private TimerView _timerView;
        [SerializeField] private ExpressionView _expressionView;
        [SerializeField] private SubmitView _submitView;
        [SerializeField] private GameplayNavigationView _gameplayNavigationView;
        [SerializeField] private GameplayErrorView _gameplayErrorView;
        [SerializeField] private DotweenFeedbackView _feedbackView;
        [SerializeField] private TMP_Text _gameModeText;
        [SerializeField] private TMP_Text _difficultyText;

        [Header("Countdown")]
        [SerializeField] private CountdownView _countdownView;
        [SerializeField] private CanvasGroup _inputBlocker;

        private Subject<GameplayInput> _inputsSubject;
        private readonly List<IDisposable> _inputSubscriptions = new List<IDisposable>();
        private bool _isDestroyed;
        private bool _inputsConnected;
        private bool _expressionInputConnected;
        private bool _submitInputConnected;
        private bool _navigationInputConnected;
        private IRuntimeMode _runtimeMode;
        private bool _referencesValidated;
        private bool _isGameplayInputEnabled;
        private GameplayViewStateDto _latestState;

        /// <summary>
        /// Gameplay画面入力のストリームを取得します。
        /// </summary>
        public Observable<GameplayInput> Inputs { get; private set; }

        /// <summary>
        /// Viewを初期化し、入力Subjectを生成します。
        /// </summary>
        private void Awake()
        {
            _inputsSubject = new Subject<GameplayInput>();
            Inputs = _inputsSubject;
            _isGameplayInputEnabled = false;
            HideCountdown();
            SetInputEnabled(false);

            if (_gameplayNavigationView != null)
            {
                _gameplayNavigationView.HideResult();
            }
        }

        /// <summary>
        /// VContainerから注入された後にUI入力を接続します。
        /// </summary>
        [VContainer.Inject]
        private void InjectRuntimeMode(IRuntimeMode runtimeMode)
        {
            _runtimeMode = runtimeMode;
            ValidateReferences();
            ConnectInputSources();
        }

        /// <summary>
        /// VContainerを使わないScene実行でも入力を接続します。
        /// </summary>
        private void Start()
        {
            ValidateReferences();
            ConnectInputSources();
        }

        /// <summary>
        /// 表示状態を各子Viewへ反映します。
        /// </summary>
        /// <param name="state">表示対象状態。</param>
        public void Render(GameplayViewStateDto state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            _latestState = state;

            if (_missionView != null)
            {
                _missionView.Render(state.Section, state.MissionText);
            }

            if (_gameModeText != null)
            {
                _gameModeText.text = state.GameModeText;
            }

            if (_difficultyText != null)
            {
                _difficultyText.text = state.DifficultyText;
            }

            if (_lifeView != null)
            {
                _lifeView.Render(state.Life);
            }

            if (_timerView != null)
            {
                _timerView.Render(
                    state.HasRemainingTime,
                    state.RemainingSeconds,
                    state.SectionDurationSeconds);
            }

            if (_expressionView != null)
            {
                _expressionView.Render(
                    state.LeftText,
                    state.OperatorText,
                    state.RightText,
                    state.LeftIndex,
                    state.OperatorIndex,
                    state.RightIndex,
                    state.NumberChoiceCount,
                    state.OperatorChoiceCount,
                    state.AnswerText,
                    state.CanSubmit && _isGameplayInputEnabled);
            }

            if (_submitView != null)
            {
                _submitView.Render(state.CanSubmit && _isGameplayInputEnabled);
            }
        }

        /// <summary>
        /// 表示設定を各子Viewへ適用します。
        /// 式のスタイルに対応するSelector一式が未設定の場合はログを出し、式の表示と入力を無効にして処理を継続します。
        /// </summary>
        /// <param name="settings">適用する表示設定。</param>
        public void ApplyDisplaySettings(GameplayDisplaySettingsDto settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (_expressionView == null ||
                _expressionView.TryApplyDisplayStyle(settings.ExpressionDisplayStyle))
            {
                return;
            }

            LogConfigurationProblem(
                $"{nameof(GameplayView)}: {nameof(ExpressionView)}にExpressionDisplayStyle.{settings.ExpressionDisplayStyle}の" +
                "Selector一式が未設定です。式の表示と入力を無効にして処理を継続します。");
        }

        /// <summary>
        /// Countdownの残り秒数を中央に表示します。
        /// </summary>
        /// <param name="remainingSeconds">表示する残り秒数。</param>
        public void ShowCountdown(int remainingSeconds)
        {
            if (_countdownView == null)
            {
                return;
            }

            _countdownView.ShowCountdown(remainingSeconds);
        }

        /// <summary>
        /// Countdown表示を非表示にします。
        /// </summary>
        public void HideCountdown()
        {
            if (_countdownView == null)
            {
                return;
            }

            _countdownView.HideCountdown();
        }

        /// <summary>
        /// Gameplay入力の受付状態と入力ブロックUIを更新します。
        /// </summary>
        /// <param name="isEnabled">入力を受け付ける場合はtrue。</param>
        public void SetInputEnabled(bool isEnabled)
        {
            _isGameplayInputEnabled = isEnabled;

            if (_inputBlocker != null)
            {
                _inputBlocker.alpha = 0f;
                _inputBlocker.interactable = false;
                _inputBlocker.blocksRaycasts = !isEnabled;
            }

            if (_expressionView != null)
            {
                _expressionView.SetInputEnabled(isEnabled);
            }

            if (_submitView != null)
            {
                _submitView.SetInputEnabled(isEnabled);
            }

            if (_latestState != null)
            {
                Render(_latestState);
            }
        }

        /// <summary>
        /// Feedback演出を再生します。
        /// </summary>
        /// <param name="feedback">演出表示データ。</param>
        /// <param name="cancellationToken">演出キャンセルToken。</param>
        /// <returns>演出完了を表す非同期処理。</returns>
        public UniTask PlayFeedbackAsync(
            FeedbackViewDataDto feedback,
            System.Threading.CancellationToken cancellationToken)
        {
            if (_feedbackView == null)
            {
                return UniTask.CompletedTask;
            }

            return _feedbackView.PlayAsync(feedback, cancellationToken);
        }

        /// <summary>
        /// Result画面を表示します。
        /// </summary>
        /// <param name="result">Result画面の表示状態。</param>
        public void ShowResult(GameplayResultViewStateDto result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (_gameplayNavigationView != null)
            {
                _gameplayNavigationView.RenderResult(result);
            }
        }

        /// <summary>
        /// 設定エラー一覧を画面へ表示します。
        /// </summary>
        /// <param name="errors">表示するエラー一覧。</param>
        public void ShowConfigurationError(IReadOnlyList<string> errors)
        {
            if (errors == null)
            {
                throw new ArgumentNullException(nameof(errors));
            }

            if (_gameplayErrorView != null)
            {
                _gameplayErrorView.ShowConfigurationError(errors);
            }
        }

        /// <summary>
        /// 回復不能エラーを画面へ表示します。
        /// </summary>
        /// <param name="message">表示するエラーメッセージ。</param>
        public void ShowFatalError(string message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (_gameplayErrorView != null)
            {
                _gameplayErrorView.ShowFatalError(message);
            }
        }

        /// <summary>
        /// Viewを破棄し、入力ストリームを完了します。
        /// </summary>
        private void OnDestroy()
        {
            _isDestroyed = true;
            for (int index = 0; index < _inputSubscriptions.Count; index++)
            {
                _inputSubscriptions[index].Dispose();
            }

            _inputSubscriptions.Clear();
            if (_inputsSubject != null)
            {
                _inputsSubject.OnCompleted();
                _inputsSubject.Dispose();
            }
        }

        private void ConnectInputSources()
        {
            if (_inputsConnected || _isDestroyed || _inputsSubject == null)
            {
                return;
            }

            if (!_expressionInputConnected)
            {
                _expressionInputConnected = _expressionView == null ||
                                            SubscribeInputStream(_expressionView.Inputs);
            }

            if (!_submitInputConnected)
            {
                _submitInputConnected = _submitView == null ||
                                        SubscribeInputStream(_submitView.Inputs);
            }

            if (!_navigationInputConnected)
            {
                _navigationInputConnected = _gameplayNavigationView == null ||
                                            SubscribeInputStream(_gameplayNavigationView.Inputs);
            }

            _inputsConnected = _expressionInputConnected &&
                               _submitInputConnected &&
                               _navigationInputConnected;
        }

        private void ValidateReferences()
        {
            if (_referencesValidated)
            {
                return;
            }

            _referencesValidated = true;
            LogMissingReferenceIfNecessary(_missionView, nameof(_missionView));
            LogMissingReferenceIfNecessary(_lifeView, nameof(_lifeView));
            LogMissingReferenceIfNecessary(_timerView, nameof(_timerView));
            LogMissingReferenceIfNecessary(_expressionView, nameof(_expressionView));
            LogMissingReferenceIfNecessary(_submitView, nameof(_submitView));
            LogMissingReferenceIfNecessary(
                _gameplayNavigationView,
                nameof(_gameplayNavigationView));
            LogMissingReferenceIfNecessary(_gameplayErrorView, nameof(_gameplayErrorView));
            LogMissingReferenceIfNecessary(_feedbackView, nameof(_feedbackView));
            LogMissingReferenceIfNecessary(_gameModeText, nameof(_gameModeText));
            LogMissingReferenceIfNecessary(_difficultyText, nameof(_difficultyText));
            LogMissingReferenceIfNecessary(_countdownView, nameof(_countdownView));
            LogMissingReferenceIfNecessary(_inputBlocker, nameof(_inputBlocker));
        }

        private void LogMissingReferenceIfNecessary(
            UnityEngine.Object reference,
            string fieldName)
        {
            if (reference != null)
            {
                return;
            }

            string message =
                $"{nameof(GameplayView)}: {fieldName}が未設定です。" +
                (IsDebugMode
                    ? "DebugModeのため処理を継続します。"
                    : "該当するUI表示または入力を無効にして処理を継続します。");
            LogConfigurationProblem(message);
        }

        private void LogConfigurationProblem(string message)
        {
            if (IsDebugMode)
            {
                Debug.LogError(message, this);
            }
            else
            {
                Debug.LogWarning(message, this);
            }
        }

        private bool IsDebugMode
        {
            get { return _runtimeMode != null && _runtimeMode.IsDebugMode; }
        }

        private bool SubscribeInputStream(Observable<GameplayInput> inputStream)
        {
            if (inputStream == null || _inputsSubject == null)
            {
                return false;
            }

            _inputSubscriptions.Add(inputStream.Subscribe(ForwardInput));
            return true;
        }

        private void ForwardInput(GameplayInput input)
        {
            if (_isGameplayInputEnabled && _inputsSubject != null)
            {
                _inputsSubject.OnNext(input);
            }
        }
    }
}

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
    public sealed class GameplayView : MonoBehaviour, IGameplayView
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

        private Subject<GameplayInput> _inputsSubject;
        private readonly List<IDisposable> _inputSubscriptions = new List<IDisposable>();
        private bool _isDestroyed;
        private bool _inputsConnected;
        private bool _expressionInputConnected;
        private bool _submitInputConnected;
        private bool _navigationInputConnected;
        private IRuntimeMode _runtimeMode;
        private bool _referencesValidated;

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
                _timerView.Render(state.HasRemainingTime, state.RemainingSeconds);
            }

            if (_expressionView != null)
            {
                _expressionView.Render(
                    state.LeftText,
                    state.OperatorText,
                    state.RightText,
                    state.AnswerText,
                    state.CanSubmit);
            }

            if (_submitView != null)
            {
                _submitView.Render(state.CanSubmit);
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

            _inputSubscriptions.Add(inputStream.Subscribe(input => _inputsSubject.OnNext(input)));
            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using Ccc.Core.UseCases;
using Ccc.Infrastructure.Assets;
using Ccc.Presentation;
using Ccc.Presentation.Audio;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;
using NavigationGameplayStartRequest = Ccc.Presentation.Navigation.GameplayNavigationRequest;
using CoreGameplayStartRequest = Ccc.Core.UseCases.GameplayStartRequest;

namespace Ccc.Bootstrap
{
    /// <summary>
    /// Gameplay Scopeの構築完了後に設定検証とSession開始を行うEntryPointです。
    /// </summary>
    public sealed class GameplayEntryPoint : IStartable, IDisposable
    {
        private const int CountdownStartSeconds = 3;
        private const int CountdownEndSeconds = 1;
        private static readonly TimeSpan CountdownInterval = TimeSpan.FromSeconds(1);

        private readonly GameplayStartContext _startContext;
        private readonly IGameplayFlow _flow;
        private readonly GameplayPresenter _presenter;
        private readonly IGameplayView _view;
        private readonly IGameplayCountdownView _countdownView;
        private readonly IAudioPlayer _audio;
        private readonly IRuntimeMode _runtimeMode;
        private CancellationTokenSource _startupCancellationSource;
        private bool _isStarted;
        private bool _isDisposed;

        /// <summary>
        /// GameplayEntryPointを生成します。
        /// </summary>
        /// <param name="startContext">Gameplay開始RequestとConfig Entryの組。</param>
        /// <param name="flow">Gameplayフロー。</param>
        /// <param name="presenter">Gameplay Presenter。</param>
        /// <param name="view">Gameplay View。</param>
        /// <param name="countdownView">Gameplay開始前のCountdown View。</param>
        /// <param name="audio">Audio再生ポート。</param>
        /// <param name="runtimeMode">アプリケーションの実行モード。</param>
        /// <exception cref="ArgumentNullException">依存のいずれかがnullの場合。</exception>
        public GameplayEntryPoint(
            GameplayStartContext startContext,
            IGameplayFlow flow,
            GameplayPresenter presenter,
            IGameplayView view,
            IGameplayCountdownView countdownView,
            IAudioPlayer audio,
            IRuntimeMode runtimeMode)
        {
            _startContext = startContext ?? throw new ArgumentNullException(nameof(startContext));
            _flow = flow ?? throw new ArgumentNullException(nameof(flow));
            _presenter = presenter ?? throw new ArgumentNullException(nameof(presenter));
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _countdownView = countdownView ?? throw new ArgumentNullException(nameof(countdownView));
            _audio = audio ?? throw new ArgumentNullException(nameof(audio));
            _runtimeMode = runtimeMode ?? throw new ArgumentNullException(nameof(runtimeMode));
        }

        /// <summary>
        /// Assetを検証し、問題がなければPresenterとGameplay Sessionを開始します。
        /// </summary>
        /// <exception cref="InvalidOperationException">二重開始の場合。</exception>
        /// <exception cref="ObjectDisposedException">破棄済みの場合。</exception>
        public void Start()
        {
            ThrowIfDisposed();
            if (_isStarted)
            {
                throw new InvalidOperationException("GameplayEntryPoint has already been started.");
            }

            _isStarted = true;
            _countdownView.HideCountdown();
            _countdownView.SetInputEnabled(false);
            var validationErrors = new List<string>();
            GameplayConfigEntry configEntry = _startContext.ConfigEntry;
            validationErrors.AddRange(configEntry.GameplayConfigAsset.Validate());
            validationErrors.AddRange(configEntry.MissionPresetAsset.Validate());
            AddMissionTextValidationErrors(configEntry.MissionTextAsset, validationErrors);
            ValidateRequest(_startContext.Request, validationErrors);

            IReadOnlyList<Ccc.Core.Model.MissionParameterCandidate> candidates = null;
            if (validationErrors.Count == 0)
            {
                try
                {
                    candidates = configEntry.MissionPresetAsset.CreateCandidates();
                }
                catch (Exception exception)
                {
                    validationErrors.Add(
                        $"Mission preset conversion failed: {exception.Message}");
                }
            }

            if (validationErrors.Count > 0)
            {
                _view.ShowConfigurationError(validationErrors);
                return;
            }

            MonoBehaviour viewBehaviour = _view as MonoBehaviour;
            CancellationToken destroyCancellationToken = viewBehaviour == null
                ? CancellationToken.None
                : viewBehaviour.destroyCancellationToken;
            _audio.PlayBgm(AudioKey.BgmGameplayNormal);
            _startupCancellationSource =
                CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            StartGameplayAsync(candidates, _startupCancellationSource.Token).Forget(HandleAsyncFailure);
        }

        /// <summary>
        /// Presenterを先に停止し、続けてGameplayフローを破棄します。
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            _startupCancellationSource?.Cancel();
            _startupCancellationSource?.Dispose();
            _startupCancellationSource = null;
            _presenter.Dispose();
            if (_flow is IDisposable disposableFlow)
            {
                disposableFlow.Dispose();
            }
        }

        private void AddMissionTextValidationErrors(
            MissionTextAsset missionTextAsset,
            ICollection<string> validationErrors)
        {
            if (missionTextAsset == null)
            {
                const string errorMessage = "GameplayConfigEntry has no MissionTextAsset.";
                if (_runtimeMode.IsDebugMode)
                {
                    Debug.LogWarning(
                        $"[GameplayEntryPoint] {errorMessage} " +
                        "DebugMode allows fallback mission text.");
                }
                else
                {
                    validationErrors.Add(errorMessage);
                }

                return;
            }

            IReadOnlyList<string> errors = missionTextAsset.Validate();
            for (int index = 0; index < errors.Count; index++)
            {
                if (_runtimeMode.IsDebugMode)
                {
                    Debug.LogWarning(
                        $"[GameplayEntryPoint] DebugMode allows MissionTextAsset fallback: {errors[index]}");
                }
                else
                {
                    validationErrors.Add($"Mission text asset: {errors[index]}");
                }
            }
        }

        private static void ValidateRequest(
            NavigationGameplayStartRequest request,
            ICollection<string> errors)
        {
            if (request.GameMode !=
                Ccc.Presentation.Navigation.GameMode.LifeEndless)
            {
                errors.Add(
                    $"Unsupported gameplay mode: {request.GameMode}.");
            }

            if (request.Difficulty !=
                Ccc.Presentation.Navigation.GameDifficulty.Normal)
            {
                errors.Add(
                    $"Unsupported gameplay difficulty: {request.Difficulty}.");
            }
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(GameplayEntryPoint));
            }
        }

        private async UniTask StartGameplayAsync(
            IReadOnlyList<Ccc.Core.Model.MissionParameterCandidate> candidates,
            CancellationToken cancellationToken)
        {
            for (int remainingSeconds = CountdownStartSeconds;
                 remainingSeconds >= CountdownEndSeconds;
                 remainingSeconds--)
            {
                if (_isDisposed)
                {
                    return;
                }

                _countdownView.ShowCountdown(remainingSeconds);
                if (remainingSeconds == CountdownStartSeconds)
                {
                    _audio.PlaySE(AudioKey.SeCuntdown);
                }

                await UniTask.Delay(
                    CountdownInterval,
                    cancellationToken: cancellationToken);
            }

            if (_isDisposed)
            {
                return;
            }

            _countdownView.HideCountdown();
            _presenter.Start(cancellationToken);

            CoreGameplayStartRequest sessionRequest =
                new CoreGameplayStartRequest(
                    ConvertGameMode(_startContext.Request.GameMode),
                    ConvertDifficulty(_startContext.Request.Difficulty),
                    candidates);
            GameplayStartResult startResult = _flow.StartSession(sessionRequest);
            if (!startResult.IsSuccess)
            {
                _view.ShowFatalError(startResult.Failure.Message);
                _presenter.Dispose();
                return;
            }

            _audio.PlaySE(AudioKey.SEHizimere);
            _countdownView.SetInputEnabled(true);
        }

        private void HandleAsyncFailure(Exception exception)
        {
            if (_isDisposed || exception is OperationCanceledException)
            {
                return;
            }

            _countdownView.HideCountdown();
            _countdownView.SetInputEnabled(false);
            _presenter.Dispose();
            _view.ShowFatalError($"Gameplay startup failed: {exception.Message}");
        }

        private static Ccc.Core.Model.GameMode ConvertGameMode(
            Ccc.Presentation.Navigation.GameMode gameMode)
        {
            switch (gameMode)
            {
                case Ccc.Presentation.Navigation.GameMode.LifeEndless:
                    return Ccc.Core.Model.GameMode.LifeEndless;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(gameMode),
                        gameMode,
                        null);
            }
        }
        private static Ccc.Core.Model.GameDifficulty ConvertDifficulty(
            Ccc.Presentation.Navigation.GameDifficulty difficulty)
        {
            switch (difficulty)
            {
                case Ccc.Presentation.Navigation.GameDifficulty.Normal:
                    return Ccc.Core.Model.GameDifficulty.Normal;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(difficulty),
                        difficulty,
                        null);
            }
        }
    }
}

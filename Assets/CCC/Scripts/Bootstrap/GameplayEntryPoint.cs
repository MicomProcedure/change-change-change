using System;
using System.Collections.Generic;
using System.Threading;
using Ccc.Core.UseCases;
using Ccc.Infrastructure.Assets;
using Ccc.Presentation;
using Ccc.Presentation.Audio;
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
        private readonly NavigationGameplayStartRequest _request;
        private readonly GameplayConfigAsset _configAsset;
        private readonly MissionPresetAsset _missionPresetAsset;
        private readonly MissionTextAsset _missionTextAsset;
        private readonly IGameplayFlow _flow;
        private readonly GameplayPresenter _presenter;
        private readonly IGameplayView _view;
        private readonly IAudioPlayer _audio;
        private bool _isStarted;
        private bool _isDisposed;

        /// <summary>
        /// GameplayEntryPointを生成します。
        /// </summary>
        /// <param name="request">MainMenuから注入されたSession開始Request。</param>
        /// <param name="configAsset">Gameplay設定Asset。</param>
        /// <param name="missionPresetAsset">Mission候補Preset Asset。</param>
        /// <param name="missionTextAsset">Mission表示文言Asset。</param>
        /// <param name="flow">Gameplayフロー。</param>
        /// <param name="presenter">Gameplay Presenter。</param>
        /// <param name="view">Gameplay View。</param>
        /// <param name="audio">Audio再生ポート。</param>
        /// <exception cref="ArgumentNullException">依存のいずれかがnullの場合。</exception>
        public GameplayEntryPoint(
            NavigationGameplayStartRequest request,
            GameplayConfigAsset configAsset,
            MissionPresetAsset missionPresetAsset,
            MissionTextAsset missionTextAsset,
            IGameplayFlow flow,
            GameplayPresenter presenter,
            IGameplayView view,
            IAudioPlayer audio)
        {
            _request = request ?? throw new ArgumentNullException(nameof(request));
            _configAsset = configAsset ?? throw new ArgumentNullException(nameof(configAsset));
            _missionPresetAsset = missionPresetAsset ?? throw new ArgumentNullException(nameof(missionPresetAsset));
            _missionTextAsset = missionTextAsset ?? throw new ArgumentNullException(nameof(missionTextAsset));
            _flow = flow ?? throw new ArgumentNullException(nameof(flow));
            _presenter = presenter ?? throw new ArgumentNullException(nameof(presenter));
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _audio = audio ?? throw new ArgumentNullException(nameof(audio));
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
            var validationErrors = new List<string>();
            validationErrors.AddRange(_configAsset.Validate());
            validationErrors.AddRange(_missionPresetAsset.Validate());
            validationErrors.AddRange(_missionTextAsset.Validate());
            ValidateRequest(validationErrors);

            IReadOnlyList<Ccc.Core.Model.MissionParameterCandidate> candidates = null;
            if (validationErrors.Count == 0)
            {
                try
                {
                    candidates = _missionPresetAsset.CreateCandidates();
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
            _presenter.Start(destroyCancellationToken);

            CoreGameplayStartRequest sessionRequest =
                new CoreGameplayStartRequest(
                ConvertGameMode(_request.GameMode),
                ConvertDifficulty(_request.Difficulty),
                candidates);
            GameplayStartResult startResult = _flow.StartSession(sessionRequest);
            if (!startResult.IsSuccess)
            {
                _view.ShowFatalError(startResult.Failure.Message);
                _presenter.Dispose();
                return;
            }

            _audio.PlayBgm(AudioKey.BgmGameplay);
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
            _presenter.Dispose();
            if (_flow is IDisposable disposableFlow)
            {
                disposableFlow.Dispose();
            }
        }

        private void ValidateRequest(ICollection<string> errors)
        {
            if (_request.GameMode !=
                Ccc.Presentation.Navigation.GameMode.LifeEndless)
            {
                errors.Add(
                    $"Unsupported gameplay mode: {_request.GameMode}.");
            }

            if (_request.Difficulty !=
                Ccc.Presentation.Navigation.GameDifficulty.Normal)
            {
                errors.Add(
                    $"Unsupported gameplay difficulty: {_request.Difficulty}.");
            }
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(GameplayEntryPoint));
            }
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

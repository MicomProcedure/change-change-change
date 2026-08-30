using System;
using Ccc.Infrastructure.Navigation;
using Ccc.Presentation.Audio;
using Ccc.Presentation.Navigation;
using R3;
using VContainer.Unity;

namespace Ccc.Bootstrap
{
    /// <summary>
    /// TitleScene開始時の初期処理を担当します。
    /// </summary>
    public sealed class TitleEntryPoint : IStartable, IDisposable
    {
        private readonly TitleScreenModel _model;
        private readonly IAudioPlayer _audio;
        private IDisposable _screenSubscription;
        private TitleScreenState _previousScreen;

        /// <summary>
        /// TitleEntryPointを生成します。
        /// </summary>
        /// <param name="model">Title画面Model。</param>
        /// <param name="audio">Audio再生ポート。</param>
        public TitleEntryPoint(
            TitleScreenModel model,
            IAudioPlayer audio)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _audio = audio ?? throw new ArgumentNullException(nameof(audio));
        }

        /// <summary>
        /// Title BGMを再生します。
        /// </summary>
        public void Start()
        {
            _previousScreen = _model.CurrentScreen.CurrentValue;
            _screenSubscription = _model.CurrentScreen.Subscribe(HandleScreenChanged);
            _audio.PlayBgm(AudioKey.BgmTitle);
        }

        /// <summary>
        /// Title画面Modelを破棄します。
        /// </summary>
        public void Dispose()
        {
            _screenSubscription?.Dispose();
            _screenSubscription = null;
            _model.Dispose();
        }

        private void HandleScreenChanged(TitleScreenState currentScreen)
        {
            if ((_previousScreen == TitleScreenState.OptionMenu ||
                 _previousScreen == TitleScreenState.Credit) &&
                currentScreen == TitleScreenState.MainMenu)
            {
                _audio.PlaySE(AudioKey.SECancelClick);
            }

            _previousScreen = currentScreen;
        }
    }
}

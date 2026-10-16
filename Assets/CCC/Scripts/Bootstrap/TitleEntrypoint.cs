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
        private readonly OptionMenuModel _optionMenuModel;
        private readonly IAudioPlayer _audio;
        private readonly IAudioVolumeControl _audioVolumeControl;
        private IDisposable _screenSubscription;
        private IDisposable _seVolumeSubscription;
        private IDisposable _bgmVolumeSubscription;
        private TitleScreenState _previousScreen;

        /// <summary>
        /// TitleEntryPointを生成します。
        /// </summary>
        /// <param name="model">Title画面Model。</param>
        /// <param name="optionMenuModel">Option設定Model。</param>
        /// <param name="audio">Audio再生ポート。</param>
        /// <param name="audioVolumeControl">音量レベル変更ポート。</param>
        public TitleEntryPoint(
            TitleScreenModel model,
            OptionMenuModel optionMenuModel,
            IAudioPlayer audio,
            IAudioVolumeControl audioVolumeControl)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _optionMenuModel = optionMenuModel ?? throw new ArgumentNullException(nameof(optionMenuModel));
            _audio = audio ?? throw new ArgumentNullException(nameof(audio));
            _audioVolumeControl = audioVolumeControl ?? throw new ArgumentNullException(nameof(audioVolumeControl));
        }

        /// <summary>
        /// Title BGMを再生します。
        /// </summary>
        public void Start()
        {
            _previousScreen = _model.CurrentScreen.CurrentValue;
            _screenSubscription = _model.CurrentScreen.Subscribe(HandleScreenChanged);
            _seVolumeSubscription =
                _optionMenuModel.SeVolumeLevel.Subscribe(_audioVolumeControl.SetSeVolumeLevel);
            _bgmVolumeSubscription =
                _optionMenuModel.BgmVolumeLevel.Subscribe(_audioVolumeControl.SetBgmVolumeLevel);
            _audio.PlayBgm(AudioKey.BgmTitle);
        }

        /// <summary>
        /// Title画面Modelを破棄します。
        /// </summary>
        public void Dispose()
        {
            _screenSubscription?.Dispose();
            _screenSubscription = null;
            _seVolumeSubscription?.Dispose();
            _seVolumeSubscription = null;
            _bgmVolumeSubscription?.Dispose();
            _bgmVolumeSubscription = null;
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

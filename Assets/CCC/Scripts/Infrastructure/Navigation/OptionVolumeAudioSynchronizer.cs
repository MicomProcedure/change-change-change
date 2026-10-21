using System;
using Ccc.Presentation.Audio;
using R3;

namespace Ccc.Infrastructure.Navigation
{
    /// <summary>
    /// OptionMenuModelの音量レベルをAudioへ反映し続けます。
    /// </summary>
    public sealed class OptionVolumeAudioSynchronizer : IDisposable
    {
        private readonly OptionMenuModel _optionMenuModel;
        private readonly IAudioVolumeControl _audioVolumeControl;
        private IDisposable _seVolumeSubscription;
        private IDisposable _bgmVolumeSubscription;
        private bool _isDisposed;

        /// <summary>
        /// OptionVolumeAudioSynchronizerを生成します。
        /// </summary>
        /// <param name="optionMenuModel">音量レベルを保持するOption設定Model。</param>
        /// <param name="audioVolumeControl">音量レベル変更ポート。</param>
        /// <exception cref="ArgumentNullException">optionMenuModelまたはaudioVolumeControlがnullの場合。</exception>
        public OptionVolumeAudioSynchronizer(
            OptionMenuModel optionMenuModel,
            IAudioVolumeControl audioVolumeControl)
        {
            _optionMenuModel =
                optionMenuModel ?? throw new ArgumentNullException(nameof(optionMenuModel));
            _audioVolumeControl =
                audioVolumeControl ?? throw new ArgumentNullException(nameof(audioVolumeControl));
        }

        /// <summary>
        /// 音量レベルの購読を開始します。開始時点の音量レベルも即時に反映します。
        /// </summary>
        /// <exception cref="ObjectDisposedException">破棄済みの場合。</exception>
        /// <exception cref="InvalidOperationException">既に購読を開始している場合。</exception>
        public void StartSubscription()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(OptionVolumeAudioSynchronizer));
            }

            if (_seVolumeSubscription != null)
            {
                throw new InvalidOperationException(
                    $"{nameof(OptionVolumeAudioSynchronizer)}は既に購読を開始しています。");
            }

            _seVolumeSubscription =
                _optionMenuModel.SEVolumeLevel.Subscribe(_audioVolumeControl.SetSEVolumeLevel);
            _bgmVolumeSubscription =
                _optionMenuModel.BgmVolumeLevel.Subscribe(_audioVolumeControl.SetBgmVolumeLevel);
        }

        /// <summary>
        /// 音量レベルの購読を解除します。
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            _seVolumeSubscription?.Dispose();
            _seVolumeSubscription = null;
            _bgmVolumeSubscription?.Dispose();
            _bgmVolumeSubscription = null;
        }
    }
}

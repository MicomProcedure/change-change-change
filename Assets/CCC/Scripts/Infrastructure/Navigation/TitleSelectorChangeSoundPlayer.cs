using System;
using Ccc.Presentation.Audio;
using R3;

namespace Ccc.Infrastructure.Navigation
{
    /// <summary>
    /// TitleScene内の選択値（難易度・SE音量・BGM音量）が切り替わったときにchange操作のSEを再生します。
    /// </summary>
    public sealed class TitleSelectorChangeSoundPlayer : IDisposable
    {
        private readonly TitleScreenModel _titleScreenModel;
        private readonly OptionMenuModel _optionMenuModel;
        private readonly IAudioPlayer _audio;
        private IDisposable _selectorChangeSubscription;
        private bool _isDisposed;

        /// <summary>
        /// TitleSelectorChangeSoundPlayerを生成します。
        /// </summary>
        /// <param name="titleScreenModel">選択中の難易度を通知するTitle画面Model。</param>
        /// <param name="optionMenuModel">音量レベルを通知するOption設定Model。</param>
        /// <param name="audio">Audio再生ポート。</param>
        /// <exception cref="ArgumentNullException">titleScreenModel、optionMenuModel、audioのいずれかがnullの場合。</exception>
        public TitleSelectorChangeSoundPlayer(
            TitleScreenModel titleScreenModel,
            OptionMenuModel optionMenuModel,
            IAudioPlayer audio)
        {
            _titleScreenModel =
                titleScreenModel ?? throw new ArgumentNullException(nameof(titleScreenModel));
            _optionMenuModel =
                optionMenuModel ?? throw new ArgumentNullException(nameof(optionMenuModel));
            _audio = audio ?? throw new ArgumentNullException(nameof(audio));
        }

        /// <summary>
        /// 選択値の購読を開始します。開始時点の値ではSEを再生しません。
        /// </summary>
        /// <exception cref="ObjectDisposedException">破棄済みの場合。</exception>
        /// <exception cref="InvalidOperationException">既に購読を開始している場合。</exception>
        public void StartSubscription()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(TitleSelectorChangeSoundPlayer));
            }

            if (_selectorChangeSubscription != null)
            {
                throw new InvalidOperationException(
                    $"{nameof(TitleSelectorChangeSoundPlayer)}は既に購読を開始しています。");
            }

            // ReadOnlyReactivePropertyは購読時に現在値を通知するため、初回分を読み飛ばす
            _selectorChangeSubscription = Observable.Merge(
                    _titleScreenModel.SelectedDifficulty.Skip(1).AsUnitObservable(),
                    _optionMenuModel.SEVolumeLevel.Skip(1).AsUnitObservable(),
                    _optionMenuModel.BgmVolumeLevel.Skip(1).AsUnitObservable())
                .Subscribe(_ => _audio.PlaySE(AudioKey.SEChangeClick));
        }

        /// <summary>
        /// 選択値の購読を解除します。
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            _selectorChangeSubscription?.Dispose();
            _selectorChangeSubscription = null;
        }
    }
}

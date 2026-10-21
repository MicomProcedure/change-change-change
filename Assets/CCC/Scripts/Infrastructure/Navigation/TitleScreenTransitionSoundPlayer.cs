using System;
using Ccc.Presentation.Audio;
using Ccc.Presentation.Navigation;
using R3;

namespace Ccc.Infrastructure.Navigation
{
    /// <summary>
    /// TitleScene内で受け付けた画面遷移の方向に対応するSEを再生します。
    /// </summary>
    public sealed class TitleScreenTransitionSoundPlayer : IDisposable
    {
        private readonly TitleScreenModel _titleScreenModel;
        private readonly IAudioPlayer _audio;
        private IDisposable _screenTransitionSubscription;
        private bool _isDisposed;

        /// <summary>
        /// TitleScreenTransitionSoundPlayerを生成します。
        /// </summary>
        /// <param name="titleScreenModel">画面遷移を通知するTitle画面Model。</param>
        /// <param name="audio">Audio再生ポート。</param>
        /// <exception cref="ArgumentNullException">titleScreenModelまたはaudioがnullの場合。</exception>
        public TitleScreenTransitionSoundPlayer(
            TitleScreenModel titleScreenModel,
            IAudioPlayer audio)
        {
            _titleScreenModel =
                titleScreenModel ?? throw new ArgumentNullException(nameof(titleScreenModel));
            _audio = audio ?? throw new ArgumentNullException(nameof(audio));
        }

        /// <summary>
        /// 画面遷移の購読を開始します。
        /// </summary>
        /// <exception cref="ObjectDisposedException">破棄済みの場合。</exception>
        /// <exception cref="InvalidOperationException">既に購読を開始している場合。</exception>
        public void StartSubscription()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(TitleScreenTransitionSoundPlayer));
            }

            if (_screenTransitionSubscription != null)
            {
                throw new InvalidOperationException(
                    $"{nameof(TitleScreenTransitionSoundPlayer)}は既に購読を開始しています。");
            }

            _screenTransitionSubscription =
                _titleScreenModel.ScreenTransitioned.Subscribe(HandleScreenTransitioned);
        }

        /// <summary>
        /// 画面遷移の購読を解除します。
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            _screenTransitionSubscription?.Dispose();
            _screenTransitionSubscription = null;
        }

        private void HandleScreenTransitioned(TitleScreenTransitionDirection direction)
        {
            switch (direction)
            {
                case TitleScreenTransitionDirection.Forward:
                    break;
                case TitleScreenTransitionDirection.Backward:
                    _audio.PlaySE(AudioKey.SECancelClick);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(direction),
                        direction,
                        "未定義の画面遷移方向です。");
            }
        }
    }
}

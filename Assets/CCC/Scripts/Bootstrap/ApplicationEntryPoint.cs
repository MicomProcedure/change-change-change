using System;
using VContainer.Unity;

namespace Ccc.Bootstrap
{
    /// <summary>
    /// アプリ起動時の初期化処理をまとめて実行するApplication Scopeの起点です。
    /// </summary>
    public sealed class ApplicationEntryPoint : IStartable
    {
        private readonly OptionAudioInitializer _optionAudioInitializer;

        /// <summary>
        /// ApplicationEntryPointを生成します。
        /// </summary>
        /// <param name="optionAudioInitializer">保存済み音量レベルの反映処理。</param>
        public ApplicationEntryPoint(OptionAudioInitializer optionAudioInitializer)
        {
            _optionAudioInitializer =
                optionAudioInitializer ?? throw new ArgumentNullException(nameof(optionAudioInitializer));
        }

        /// <summary>
        /// アプリ起動時の初期化処理を実行します。
        /// </summary>
        public void Start()
        {
            _optionAudioInitializer.ApplySavedVolumeLevels();
        }
    }
}
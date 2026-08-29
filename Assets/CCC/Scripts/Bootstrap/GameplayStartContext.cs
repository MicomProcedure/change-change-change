using System;
using Ccc.Infrastructure.Assets;
using Ccc.Presentation.Navigation;

namespace Ccc.Bootstrap
{
    /// <summary>
    /// Gameplay起動時に使用するRequestとConfig Entryの組を保持します。
    /// </summary>
    public sealed class GameplayStartContext
    {
        /// <summary>
        /// Gameplay開始Requestを取得します。
        /// </summary>
        public GameplayNavigationRequest Request { get; }

        /// <summary>
        /// Requestに対応するConfig Entryを取得します。
        /// </summary>
        public GameplayConfigEntry ConfigEntry { get; }

        /// <summary>
        /// GameplayStartContextを生成します。
        /// </summary>
        /// <param name="request">Gameplay開始Request。</param>
        /// <param name="configEntry">対応するConfig Entry。</param>
        /// <exception cref="ArgumentNullException">引数のいずれかがnullの場合。</exception>
        public GameplayStartContext(
            GameplayNavigationRequest request,
            GameplayConfigEntry configEntry)
        {
            Request = request ?? throw new ArgumentNullException(nameof(request));
            ConfigEntry = configEntry ?? throw new ArgumentNullException(nameof(configEntry));
        }
    }
}

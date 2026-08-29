using System;

namespace Ccc.Core.Model
{
    /// <summary>
    /// Reducer がセッション遷移を決定するために参照する不変の設定とミッションカタログ。
    /// </summary>
    public sealed class GameplaySessionContext
    {
        /// <summary>
        /// セッション設定を取得します。
        /// </summary>
        public GameplayConfig Config { get; }

        /// <summary>
        /// セッション中に提示可能なミッション候補カタログを取得します。
        /// </summary>
        public MissionCatalog MissionCatalog { get; }

        /// <summary>
        /// 指定された設定とミッションカタログからコンテキストを生成します。
        /// </summary>
        /// <param name="config">セッション設定。</param>
        /// <param name="missionCatalog">提示可能なミッション候補カタログ。</param>
        public GameplaySessionContext(GameplayConfig config, MissionCatalog missionCatalog)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            MissionCatalog = missionCatalog ?? throw new ArgumentNullException(nameof(missionCatalog));
        }
    }
}

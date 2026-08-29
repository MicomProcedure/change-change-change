#nullable enable
using System;
using Ccc.Core.Model;

namespace Ccc.Core.Logic
{
    /// <summary>
    /// ミッション抽選に必要な文脈情報を保持する不変クラス。
    /// </summary>
    public sealed class MissionDrawContext
    {
        /// <summary>
        /// 抽選対象のミッション候補を含むカタログを取得します。
        /// </summary>
        public MissionCatalog Catalog { get; }

        /// <summary>
        /// ゲームプレイ設定を取得します。
        /// </summary>
        public GameplayConfig Config { get; }

        /// <summary>
        /// 現在のセクション番号（1以上）を取得します。
        /// </summary>
        public int Section { get; }

        /// <summary>
        /// 発行する新しいミッションIDを取得します。
        /// </summary>
        public MissionId NextMissionId { get; }

        /// <summary>
        /// 直前のミッションIDを取得します（初回抽選時は null）。
        /// </summary>
        public MissionId? PreviousMissionId { get; }

        /// <summary>
        /// 抽選に使用する乱数生成器を取得します。
        /// </summary>
        public IRandomSource RandomSource { get; }

        /// <summary>
        /// <see cref="MissionDrawContext"/> の新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="catalog">ミッションカタログ。</param>
        /// <param name="config">ゲームプレイ設定。</param>
        /// <param name="section">セクション番号（1以上）。</param>
        /// <param name="nextMissionId">割り当てる新しいミッションID。</param>
        /// <param name="previousMissionId">直前のミッションID（任意）。</param>
        /// <param name="randomSource">乱数sampleの供給元。</param>
        public MissionDrawContext(
            MissionCatalog catalog,
            GameplayConfig config,
            int section,
            MissionId nextMissionId,
            MissionId? previousMissionId,
            IRandomSource randomSource)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (section <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(section), section, "Section must be greater than zero.");
            }

            if (randomSource == null)
            {
                throw new ArgumentNullException(nameof(randomSource));
            }

            Catalog = catalog;
            Config = config;
            Section = section;
            NextMissionId = nextMissionId;
            PreviousMissionId = previousMissionId;
            RandomSource = randomSource;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ccc.Core.Model
{
    /// <summary>
    /// SectionからMission Tierを決定する不変の進行定義です。
    /// </summary>
    public sealed class TierProgression
    {
        private static readonly int[] DefaultMinimumSections = { 1, 4, 8, 12 };
        private readonly IReadOnlyList<int> _minimumSectionsByTier;

        /// <summary>
        /// 各Tierの開始Sectionを昇順で取得します。
        /// </summary>
        public IReadOnlyList<int> MinimumSectionsByTier => _minimumSectionsByTier;

        /// <summary>
        /// 既定のTier境界から進行定義を生成します。
        /// </summary>
        public TierProgression()
            : this(DefaultMinimumSections)
        {
        }

        /// <summary>
        /// Tier開始Section一覧から進行定義を生成します。
        /// </summary>
        /// <param name="minimumSectionsByTier">Tier 1から順に並んだ開始Section。</param>
        /// <exception cref="ArgumentNullException">一覧がnullの場合。</exception>
        /// <exception cref="ArgumentException">一覧が空、先頭が1以外、または昇順でない場合。</exception>
        public TierProgression(IReadOnlyList<int> minimumSectionsByTier)
        {
            if (minimumSectionsByTier == null)
            {
                throw new ArgumentNullException(nameof(minimumSectionsByTier));
            }

            if (minimumSectionsByTier.Count == 0 || minimumSectionsByTier[0] != 1)
            {
                throw new ArgumentException(
                    "Tier boundaries must contain a first tier beginning at section 1.",
                    nameof(minimumSectionsByTier));
            }

            var snapshot = new int[minimumSectionsByTier.Count];
            for (int index = 0; index < minimumSectionsByTier.Count; index++)
            {
                int minimumSection = minimumSectionsByTier[index];
                if (minimumSection <= 0 ||
                    (index > 0 && minimumSection <= minimumSectionsByTier[index - 1]))
                {
                    throw new ArgumentException(
                        "Tier boundaries must be positive and strictly ascending.",
                        nameof(minimumSectionsByTier));
                }

                snapshot[index] = minimumSection;
            }

            _minimumSectionsByTier = new ReadOnlyCollection<int>(snapshot);
        }

        /// <summary>
        /// 指定Sectionに対応するTierを取得します。
        /// </summary>
        /// <param name="section">対象Section。</param>
        /// <returns>対象SectionのTier番号。</returns>
        /// <exception cref="ArgumentOutOfRangeException">Sectionが1未満の場合。</exception>
        public int GetTier(int section)
        {
            if (section <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(section), section, null);
            }

            for (int index = _minimumSectionsByTier.Count - 1; index >= 0; index--)
            {
                if (section >= _minimumSectionsByTier[index])
                {
                    return index + 1;
                }
            }

            return 1;
        }
    }
}

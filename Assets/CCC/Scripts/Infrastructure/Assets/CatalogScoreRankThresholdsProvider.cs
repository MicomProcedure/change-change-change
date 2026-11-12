using System;
using Ccc.Core.Model;
using Ccc.Presentation.ScoreRank;

namespace Ccc.Infrastructure.Assets
{
    /// <summary>
    /// GameplayConfigCatalogAssetに登録されたEntryから、評価段階の基準を提供します。
    /// </summary>
    public sealed class CatalogScoreRankThresholdsProvider : IScoreRankThresholdsProvider
    {
        private readonly GameplayConfigCatalogAsset _catalog;

        /// <summary>
        /// CatalogScoreRankThresholdsProviderを生成します。
        /// </summary>
        /// <param name="catalog">基準を検索するGameplay設定Catalog。</param>
        /// <exception cref="ArgumentNullException"><paramref name="catalog"/>がnullの場合。</exception>
        public CatalogScoreRankThresholdsProvider(GameplayConfigCatalogAsset catalog)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        }

        /// <inheritdoc/>
        public ScoreRankThresholds Get(GameMode gameMode, GameDifficulty difficulty)
        {
            if (!_catalog.TryGetEntry(gameMode, difficulty, out GameplayConfigEntry entry))
            {
                throw new InvalidOperationException(
                    $"No GameplayConfigCatalog entry matches GameMode.{gameMode} and GameDifficulty.{difficulty}.");
            }

            return entry.CreateScoreRankThresholds();
        }
    }
}

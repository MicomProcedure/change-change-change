#nullable enable
using System;
using System.Collections.Generic;
using Ccc.Core.Model;

namespace Ccc.Core.Logic
{
    /// <summary>
    /// MissionCatalog準備の失敗種別を表します。
    /// </summary>
    public enum MissionCatalogPreparationFailureKind
    {
        /// <summary>
        /// 成功Resultで使用される値です。
        /// </summary>
        None = 0,

        /// <summary>
        /// 候補式生成またはMissionパラメータ候補準備の設定が不正です。
        /// </summary>
        InvalidConfiguration,

        /// <summary>
        /// 候補からMissionCatalogを構築できませんでした。
        /// </summary>
        CatalogBuildFailed
    }

    /// <summary>
    /// MissionCatalog準備の成否、構築したCatalog、およびCatalog構築に使った候補式一覧を保持するResultです。
    /// </summary>
    public sealed class MissionCatalogPreparationResult
    {
        private readonly MissionCatalog? _catalog;
        private readonly IReadOnlyList<ExpressionCandidate>? _expressionCandidates;

        private MissionCatalogPreparationResult(
            MissionCatalog? catalog,
            IReadOnlyList<ExpressionCandidate>? expressionCandidates,
            MissionCatalogPreparationFailureKind failureKind,
            string failureMessage)
        {
            _catalog = catalog;
            _expressionCandidates = expressionCandidates;
            FailureKind = failureKind;
            FailureMessage = failureMessage;
        }

        /// <summary>
        /// 準備に成功したかどうかを取得します。
        /// </summary>
        public bool IsSuccess => FailureKind == MissionCatalogPreparationFailureKind.None;

        /// <summary>
        /// 失敗種別を取得します。成功時は<see cref="MissionCatalogPreparationFailureKind.None"/>です。
        /// </summary>
        public MissionCatalogPreparationFailureKind FailureKind { get; }

        /// <summary>
        /// 失敗の技術的理由を取得します。成功時は空文字です。
        /// </summary>
        public string FailureMessage { get; }

        /// <summary>
        /// 構築したMissionCatalogを取得します。
        /// </summary>
        /// <exception cref="InvalidOperationException">失敗Resultの場合。</exception>
        public MissionCatalog Catalog => IsSuccess && _catalog != null
            ? _catalog
            : throw CreateFailureAccessException(nameof(Catalog));

        /// <summary>
        /// Catalog構築に使った候補式一覧を取得します。
        /// </summary>
        /// <exception cref="InvalidOperationException">失敗Resultの場合。</exception>
        public IReadOnlyList<ExpressionCandidate> ExpressionCandidates => IsSuccess && _expressionCandidates != null
            ? _expressionCandidates
            : throw CreateFailureAccessException(nameof(ExpressionCandidates));

        /// <summary>
        /// 成功Resultを生成します。
        /// </summary>
        /// <param name="catalog">構築したMissionCatalog。</param>
        /// <param name="expressionCandidates">Catalog構築に使った候補式一覧。</param>
        /// <returns>成功Result。</returns>
        /// <exception cref="ArgumentNullException">引数がnullの場合。</exception>
        public static MissionCatalogPreparationResult Success(
            MissionCatalog catalog,
            IReadOnlyList<ExpressionCandidate> expressionCandidates)
        {
            return new MissionCatalogPreparationResult(
                catalog ?? throw new ArgumentNullException(nameof(catalog)),
                expressionCandidates ?? throw new ArgumentNullException(nameof(expressionCandidates)),
                MissionCatalogPreparationFailureKind.None,
                string.Empty);
        }

        /// <summary>
        /// 失敗Resultを生成します。
        /// </summary>
        /// <param name="failureKind">None以外の失敗種別。</param>
        /// <param name="failureMessage">失敗の技術的理由。</param>
        /// <returns>失敗Result。</returns>
        /// <exception cref="ArgumentException">失敗種別がNoneの場合。</exception>
        public static MissionCatalogPreparationResult Failure(
            MissionCatalogPreparationFailureKind failureKind,
            string failureMessage)
        {
            if (failureKind == MissionCatalogPreparationFailureKind.None)
            {
                throw new ArgumentException("FailureKind cannot be None for a failure result.", nameof(failureKind));
            }

            return new MissionCatalogPreparationResult(null, null, failureKind, failureMessage ?? string.Empty);
        }

        private InvalidOperationException CreateFailureAccessException(string memberName)
        {
            return new InvalidOperationException(
                $"Cannot access {memberName} on preparation failure ({FailureKind}): {FailureMessage}");
        }
    }
}

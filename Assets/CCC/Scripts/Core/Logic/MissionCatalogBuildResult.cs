#nullable enable
using System;
using Ccc.Core.Model;

namespace Ccc.Core.Logic
{
    public enum MissionCatalogBuildFailureKind
    {
        None = 0,
        NoExpressions,
        NoParameters,
        EmptyCatalog
    }

    /// <summary>
    /// MissionCatalogBuilder の構築結果を表すクラス。
    /// 失敗時は Catalog プロパティへのアクセスで例外が発生します。
    /// </summary>
    public sealed class MissionCatalogBuildResult
    {
        private readonly MissionCatalog? _catalog;

        public bool IsSuccess => FailureKind == MissionCatalogBuildFailureKind.None;

        public MissionCatalog Catalog
        {
            get
            {
                if (!IsSuccess || _catalog == null)
                {
                    throw new InvalidOperationException($"Cannot access Catalog on build failure ({FailureKind}): {FailureMessage}");
                }
                return _catalog;
            }
        }

        public MissionCatalogBuildFailureKind FailureKind { get; }
        public string FailureMessage { get; }

        private MissionCatalogBuildResult(MissionCatalog? catalog, MissionCatalogBuildFailureKind failureKind, string failureMessage)
        {
            _catalog = catalog;
            FailureKind = failureKind;
            FailureMessage = failureMessage;
        }

        public static MissionCatalogBuildResult Success(MissionCatalog catalog)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }
            return new MissionCatalogBuildResult(catalog, MissionCatalogBuildFailureKind.None, string.Empty);
        }

        public static MissionCatalogBuildResult Failure(MissionCatalogBuildFailureKind failureKind, string failureMessage)
        {
            if (failureKind == MissionCatalogBuildFailureKind.None)
            {
                throw new ArgumentException("FailureKind cannot be None for a failure result.", nameof(failureKind));
            }
            return new MissionCatalogBuildResult(null, failureKind, failureMessage ?? string.Empty);
        }
    }
}

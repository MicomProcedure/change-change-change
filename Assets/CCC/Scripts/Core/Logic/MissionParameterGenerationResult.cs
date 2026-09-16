#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ccc.Core.Model;

namespace Ccc.Core.Logic
{
    /// <summary>
    /// Missionパラメータ候補生成の失敗種別を表します。
    /// </summary>
    public enum MissionParameterGenerationFailureKind
    {
        None = 0,
        NoCandidatesForEnabledKind = 1,
        KindCandidateLimitExceeded = 2,
        TotalCandidateLimitExceeded = 3,
        CandidateCountOverflow = 4
    }

    /// <summary>
    /// Missionパラメータ候補の生成結果を表します。
    /// </summary>
    public sealed class MissionParameterGenerationResult
    {
        private readonly IReadOnlyList<MissionParameterCandidate>? _candidates;

        /// <summary>生成に成功したかを取得します。</summary>
        public bool IsSuccess => FailureKind == MissionParameterGenerationFailureKind.None;

        /// <summary>失敗種別を取得します。</summary>
        public MissionParameterGenerationFailureKind FailureKind { get; }

        /// <summary>失敗理由を取得します。</summary>
        public string FailureMessage { get; }

        /// <summary>成功時の決定的な候補列を取得します。</summary>
        public IReadOnlyList<MissionParameterCandidate> Candidates
        {
            get
            {
                if (!IsSuccess || _candidates == null)
                {
                    throw new InvalidOperationException(
                        $"Cannot access Candidates on generation failure ({FailureKind}): {FailureMessage}");
                }

                return _candidates;
            }
        }

        private MissionParameterGenerationResult(
            IReadOnlyList<MissionParameterCandidate>? candidates,
            MissionParameterGenerationFailureKind failureKind,
            string failureMessage)
        {
            _candidates = candidates;
            FailureKind = failureKind;
            FailureMessage = failureMessage;
        }

        /// <summary>
        /// 成功結果を生成します。
        /// </summary>
        /// <param name="candidates">生成済み候補。</param>
        /// <returns>成功結果。</returns>
        public static MissionParameterGenerationResult Success(
            IReadOnlyList<MissionParameterCandidate> candidates)
        {
            if (candidates == null)
            {
                throw new ArgumentNullException(nameof(candidates));
            }

            if (candidates.Count == 0)
            {
                throw new ArgumentException("Candidates cannot be empty.", nameof(candidates));
            }

            var snapshot = new MissionParameterCandidate[candidates.Count];
            for (int index = 0; index < candidates.Count; index++)
            {
                snapshot[index] = candidates[index];
            }

            return new MissionParameterGenerationResult(
                new ReadOnlyCollection<MissionParameterCandidate>(snapshot),
                MissionParameterGenerationFailureKind.None,
                string.Empty);
        }

        /// <summary>
        /// 失敗結果を生成します。
        /// </summary>
        /// <param name="failureKind">失敗種別。</param>
        /// <param name="failureMessage">失敗理由。</param>
        /// <returns>失敗結果。</returns>
        public static MissionParameterGenerationResult Failure(
            MissionParameterGenerationFailureKind failureKind,
            string failureMessage)
        {
            if (failureKind == MissionParameterGenerationFailureKind.None)
            {
                throw new ArgumentException(
                    "Failure kind cannot be None.",
                    nameof(failureKind));
            }

            return new MissionParameterGenerationResult(
                null,
                failureKind,
                failureMessage ?? string.Empty);
        }
    }
}

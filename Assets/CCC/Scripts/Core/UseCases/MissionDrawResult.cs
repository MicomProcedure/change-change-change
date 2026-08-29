using System;
using Ccc.Core.Model;

namespace Ccc.Core.UseCases
{
    /// <summary>
    /// Mission抽選の成否と選出値を保持するResultです。
    /// </summary>
    public sealed class MissionDrawResult
    {
        private readonly MissionCandidate _candidate;
        private readonly string _failureMessage;

        /// <summary>
        /// Mission抽選に成功したかどうかを取得します。
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// 選出されたMission候補を取得します。
        /// </summary>
        /// <exception cref="InvalidOperationException">失敗Resultの場合。</exception>
        public MissionCandidate Candidate
        {
            get
            {
                if (!IsSuccess || _candidate == null)
                {
                    throw new InvalidOperationException("Candidate is available only on a successful draw result.");
                }

                return _candidate;
            }
        }

        /// <summary>
        /// Mission抽選失敗の技術的理由を取得します。
        /// </summary>
        /// <exception cref="InvalidOperationException">成功Resultの場合。</exception>
        public string FailureMessage
        {
            get
            {
                if (IsSuccess || string.IsNullOrWhiteSpace(_failureMessage))
                {
                    throw new InvalidOperationException("FailureMessage is available only on a failed draw result.");
                }

                return _failureMessage;
            }
        }

        private MissionDrawResult(bool isSuccess, MissionCandidate candidate, string failureMessage)
        {
            IsSuccess = isSuccess;
            _candidate = candidate;
            _failureMessage = failureMessage;
        }

        /// <summary>
        /// 成功Resultを生成します。
        /// </summary>
        /// <param name="candidate">選出された候補。</param>
        /// <returns>成功Result。</returns>
        internal static MissionDrawResult Success(MissionCandidate candidate)
        {
            return new MissionDrawResult(
                true,
                candidate ?? throw new ArgumentNullException(nameof(candidate)),
                null);
        }

        /// <summary>
        /// 失敗Resultを生成します。
        /// </summary>
        /// <param name="failureMessage">技術的な失敗理由。</param>
        /// <returns>失敗Result。</returns>
        internal static MissionDrawResult Failure(string failureMessage)
        {
            if (string.IsNullOrWhiteSpace(failureMessage))
            {
                throw new ArgumentException("Failure message cannot be empty.", nameof(failureMessage));
            }

            return new MissionDrawResult(false, null, failureMessage);
        }
    }
}

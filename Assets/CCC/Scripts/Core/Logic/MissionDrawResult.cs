#nullable enable
using System;
using Ccc.Core.Model;

namespace Ccc.Core.Logic
{
    /// <summary>
    /// ミッション抽選の失敗要因を表す列挙型。
    /// </summary>
    public enum MissionDrawFailureKind
    {
        None = 0,
        EmptyCatalog,
        NoCandidateForTier,
        AllCandidatesExcluded
    }

    /// <summary>
    /// ミッション抽選の実行結果を表す不変クラス。
    /// 失敗時は SelectedMission および SelectedCandidate へのアクセスで例外が発生します。
    /// </summary>
    public sealed class MissionDrawResult
    {
        private readonly Mission? _selectedMission;
        private readonly MissionCandidate? _selectedCandidate;

        /// <summary>
        /// 抽選が成功したかどうかを取得します。
        /// </summary>
        public bool IsSuccess => FailureKind == MissionDrawFailureKind.None;

        /// <summary>
        /// 抽選されたミッションを取得します。
        /// 失敗時は <see cref="InvalidOperationException"/> がスローされます。
        /// </summary>
        public Mission SelectedMission
        {
            get
            {
                if (!IsSuccess || _selectedMission == null)
                {
                    throw new InvalidOperationException($"Cannot access SelectedMission on draw failure ({FailureKind}): {FailureMessage}");
                }
                return _selectedMission;
            }
        }

        /// <summary>
        /// 抽選されたミッション候補情報を取得します。
        /// 失敗時は <see cref="InvalidOperationException"/> がスローされます。
        /// </summary>
        public MissionCandidate SelectedCandidate
        {
            get
            {
                if (!IsSuccess || _selectedCandidate == null)
                {
                    throw new InvalidOperationException($"Cannot access SelectedCandidate on draw failure ({FailureKind}): {FailureMessage}");
                }
                return _selectedCandidate;
            }
        }

        /// <summary>
        /// 失敗要因の種類を取得します。成功時は None です。
        /// </summary>
        public MissionDrawFailureKind FailureKind { get; }

        /// <summary>
        /// 失敗に関する詳細メッセージを取得します。
        /// </summary>
        public string FailureMessage { get; }

        private MissionDrawResult(
            Mission? selectedMission,
            MissionCandidate? selectedCandidate,
            MissionDrawFailureKind failureKind,
            string failureMessage)
        {
            _selectedMission = selectedMission;
            _selectedCandidate = selectedCandidate;
            FailureKind = failureKind;
            FailureMessage = failureMessage;
        }

        /// <summary>
        /// 成功結果を生成します。
        /// </summary>
        /// <param name="selectedMission">抽選されたミッション。</param>
        /// <param name="selectedCandidate">抽選されたミッション候補情報。</param>
        /// <returns>成功を表す <see cref="MissionDrawResult"/>。</returns>
        public static MissionDrawResult Success(Mission selectedMission, MissionCandidate selectedCandidate)
        {
            if (selectedMission == null)
            {
                throw new ArgumentNullException(nameof(selectedMission));
            }

            if (selectedCandidate == null)
            {
                throw new ArgumentNullException(nameof(selectedCandidate));
            }

            return new MissionDrawResult(
                selectedMission,
                selectedCandidate,
                MissionDrawFailureKind.None,
                string.Empty);
        }

        /// <summary>
        /// 失敗結果を生成します。
        /// </summary>
        /// <param name="failureKind">失敗要因の種別。</param>
        /// <param name="failureMessage">失敗メッセージ。</param>
        /// <returns>失敗を表す <see cref="MissionDrawResult"/>。</returns>
        public static MissionDrawResult Failure(MissionDrawFailureKind failureKind, string failureMessage)
        {
            if (failureKind == MissionDrawFailureKind.None)
            {
                throw new ArgumentException("FailureKind cannot be None for a failure result.", nameof(failureKind));
            }

            return new MissionDrawResult(
                null,
                null,
                failureKind,
                failureMessage ?? string.Empty);
        }
    }
}

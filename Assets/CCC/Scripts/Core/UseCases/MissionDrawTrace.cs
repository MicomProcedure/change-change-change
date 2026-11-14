#nullable enable
using System;
using System.Collections.Generic;
using Ccc.Core.Model;

namespace Ccc.Core.UseCases
{
    /// <summary>
    /// 1回のMission抽選における各段階の選定結果と目標適合率順位を保持する不変の診断Traceです。
    /// </summary>
    public sealed class MissionDrawTrace
    {
        /// <summary>
        /// 抽選対象のSectionを取得します。
        /// </summary>
        public int Section { get; }

        /// <summary>
        /// 初回Missionの抽選であるかどうかを取得します。
        /// </summary>
        public bool IsInitialDraw { get; }

        /// <summary>
        /// Sectionから解決したCurrent Tierを取得します。
        /// </summary>
        public int CurrentTier { get; }

        /// <summary>
        /// 抽選に使用したGameplay設定を取得します。
        /// </summary>
        public GameplayConfig Config { get; }

        /// <summary>
        /// 極性設定で除外されなかった候補数を取得します。
        /// </summary>
        public int EnabledCandidateCount { get; }

        /// <summary>
        /// Current Tier以下で解放済みの候補数を取得します。
        /// </summary>
        public int UnlockedCandidateCount { get; }

        /// <summary>
        /// 解放済み候補のうち現在式で達成できる候補数を取得します。初回抽選では0です。
        /// </summary>
        public int SatisfiedCandidateCount { get; }

        /// <summary>
        /// 解放済み候補のうち現在式で達成できない候補数を取得します。初回抽選では0です。
        /// </summary>
        public int UnmatchedCandidateCount { get; }

        /// <summary>
        /// 現在式段階で選ばれた候補群を取得します。
        /// </summary>
        public CurrentExpressionCandidateGroup SelectedExpressionGroup { get; }

        /// <summary>
        /// 現在式段階で選ばれた候補群の候補数を取得します。
        /// </summary>
        public int ExpressionStageCandidateCount => SelectedExpressionGroup switch
        {
            CurrentExpressionCandidateGroup.Satisfied => SatisfiedCandidateCount,
            CurrentExpressionCandidateGroup.Unmatched => UnmatchedCandidateCount,
            _ => UnlockedCandidateCount
        };

        /// <summary>
        /// 最終的に選出された候補のTierを取得します。
        /// 下位Tierが選ばれた場合は複数Tierの候補群から選出されるため、選出候補のTierを記録します。
        /// </summary>
        public int SelectedTier => SelectedCandidate.Tier;

        /// <summary>
        /// Tier段階で選ばれた候補数を取得します。
        /// </summary>
        public int TierStageCandidateCount { get; }

        /// <summary>
        /// 極性段階でNegated候補群が選ばれたかどうかを取得します。
        /// </summary>
        public bool IsNegatedPolaritySelected { get; }

        /// <summary>
        /// 極性段階で選ばれた候補数を取得します。
        /// </summary>
        public int PolarityStageCandidateCount { get; }

        /// <summary>
        /// 直前Missionの除外を適用した後の候補数を取得します。
        /// </summary>
        public int RepeatFilteredCandidateCount { get; }

        /// <summary>
        /// Kind段階で選ばれたMissionKindを取得します。
        /// </summary>
        public MissionKind SelectedKind { get; }

        /// <summary>
        /// この抽選で目標式適合割合を選んだ範囲を取得します。
        /// </summary>
        public TargetExpressionMatchRatioRange TargetExpressionMatchRatioRange { get; }

        /// <summary>
        /// この抽選で範囲から選び、候補の順位付けに使った目標式適合割合を取得します。
        /// </summary>
        public float TargetExpressionMatchRatio { get; }

        /// <summary>
        /// Kind段階で選ばれた全候補を、この抽選の目標式適合割合との差の昇順に並べた一覧を取得します。
        /// </summary>
        public IReadOnlyList<MissionCandidate> RankedCandidates { get; }

        /// <summary>
        /// 順位先頭から抽選対象にした候補数を取得します。
        /// </summary>
        public int PoolSize { get; }

        /// <summary>
        /// 最終的に選出された候補を取得します。
        /// </summary>
        public MissionCandidate SelectedCandidate { get; }

        /// <summary>
        /// 選出候補の1始まりの順位を取得します。
        /// </summary>
        public int SelectedRank { get; }

        /// <summary>
        /// 各段階の選定結果から抽選Traceを生成します。
        /// </summary>
        /// <param name="section">抽選対象のSection。</param>
        /// <param name="isInitialDraw">初回Missionの抽選であるかどうか。</param>
        /// <param name="currentTier">Sectionから解決したCurrent Tier。</param>
        /// <param name="config">抽選に使用したGameplay設定。</param>
        /// <param name="enabledCandidateCount">極性設定で除外されなかった候補数。</param>
        /// <param name="unlockedCandidateCount">Current Tier以下で解放済みの候補数。</param>
        /// <param name="satisfiedCandidateCount">現在式で達成できる候補数。</param>
        /// <param name="unmatchedCandidateCount">現在式で達成できない候補数。</param>
        /// <param name="selectedExpressionGroup">現在式段階で選ばれた候補群。</param>
        /// <param name="tierStageCandidateCount">Tier段階で選ばれた候補数。</param>
        /// <param name="isNegatedPolaritySelected">極性段階でNegated候補群が選ばれたかどうか。</param>
        /// <param name="polarityStageCandidateCount">極性段階で選ばれた候補数。</param>
        /// <param name="repeatFilteredCandidateCount">直前Mission除外後の候補数。</param>
        /// <param name="selectedKind">Kind段階で選ばれたMissionKind。</param>
        /// <param name="targetExpressionMatchRatioRange">この抽選で目標式適合割合を選んだ範囲。</param>
        /// <param name="targetExpressionMatchRatio">範囲から選んだ目標式適合割合。</param>
        /// <param name="rankedCandidates">目標式適合割合との差の昇順に並べた候補一覧。呼び出し側は生成後に変更しないこと。</param>
        /// <param name="poolSize">順位先頭から抽選対象にした候補数。</param>
        /// <param name="selectedCandidate">最終的に選出された候補。</param>
        /// <exception cref="ArgumentNullException">参照型の引数がnullの場合。</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// 候補数が負の場合、PoolSizeが1以上順位候補数以下でない場合、または目標式適合割合が範囲外の場合。
        /// </exception>
        /// <exception cref="ArgumentException">選出候補がPool内の順位候補に含まれない場合。</exception>
        public MissionDrawTrace(
            int section,
            bool isInitialDraw,
            int currentTier,
            GameplayConfig config,
            int enabledCandidateCount,
            int unlockedCandidateCount,
            int satisfiedCandidateCount,
            int unmatchedCandidateCount,
            CurrentExpressionCandidateGroup selectedExpressionGroup,
            int tierStageCandidateCount,
            bool isNegatedPolaritySelected,
            int polarityStageCandidateCount,
            int repeatFilteredCandidateCount,
            MissionKind selectedKind,
            TargetExpressionMatchRatioRange targetExpressionMatchRatioRange,
            float targetExpressionMatchRatio,
            IReadOnlyList<MissionCandidate> rankedCandidates,
            int poolSize,
            MissionCandidate selectedCandidate)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            TargetExpressionMatchRatioRange = targetExpressionMatchRatioRange ??
                throw new ArgumentNullException(nameof(targetExpressionMatchRatioRange));
            if (!targetExpressionMatchRatioRange.Contains(targetExpressionMatchRatio))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(targetExpressionMatchRatio),
                    targetExpressionMatchRatio,
                    "Target expression match ratio must be within the target range.");
            }

            TargetExpressionMatchRatio = targetExpressionMatchRatio;
            RankedCandidates = rankedCandidates ?? throw new ArgumentNullException(nameof(rankedCandidates));
            SelectedCandidate = selectedCandidate ?? throw new ArgumentNullException(nameof(selectedCandidate));
            EnabledCandidateCount = RequireNonNegative(enabledCandidateCount, nameof(enabledCandidateCount));
            UnlockedCandidateCount = RequireNonNegative(unlockedCandidateCount, nameof(unlockedCandidateCount));
            SatisfiedCandidateCount = RequireNonNegative(satisfiedCandidateCount, nameof(satisfiedCandidateCount));
            UnmatchedCandidateCount = RequireNonNegative(unmatchedCandidateCount, nameof(unmatchedCandidateCount));
            TierStageCandidateCount = RequireNonNegative(tierStageCandidateCount, nameof(tierStageCandidateCount));
            PolarityStageCandidateCount =
                RequireNonNegative(polarityStageCandidateCount, nameof(polarityStageCandidateCount));
            RepeatFilteredCandidateCount =
                RequireNonNegative(repeatFilteredCandidateCount, nameof(repeatFilteredCandidateCount));
            if (poolSize < 1 || poolSize > rankedCandidates.Count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(poolSize),
                    poolSize,
                    "Pool size must be between one and the ranked candidate count.");
            }

            Section = section;
            IsInitialDraw = isInitialDraw;
            CurrentTier = currentTier;
            SelectedExpressionGroup = selectedExpressionGroup;
            IsNegatedPolaritySelected = isNegatedPolaritySelected;
            SelectedKind = selectedKind;
            PoolSize = poolSize;
            SelectedRank = ResolveSelectedRank(rankedCandidates, poolSize, selectedCandidate);
        }

        private static int RequireNonNegative(int count, string paramName)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(paramName, count, "Candidate count must not be negative.");
            }

            return count;
        }

        private static int ResolveSelectedRank(
            IReadOnlyList<MissionCandidate> rankedCandidates,
            int poolSize,
            MissionCandidate selectedCandidate)
        {
            for (int index = 0; index < poolSize; index++)
            {
                if (ReferenceEquals(rankedCandidates[index], selectedCandidate))
                {
                    return index + 1;
                }
            }

            throw new ArgumentException(
                "Selected candidate must be one of the pooled ranked candidates.",
                nameof(selectedCandidate));
        }
    }
}

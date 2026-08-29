using System;

namespace Ccc.Core.Model
{
    /// <summary>
    /// 1 回の提出または時間切れに対する判定結果を表す不変データ。
    /// </summary>
    public sealed class SubmissionResolution
    {
        /// <summary>
        /// 判定対象のミッション識別子を取得します。
        /// </summary>
        public MissionId MissionId { get; }

        /// <summary>
        /// 判定を発生させたトリガーを取得します。
        /// </summary>
        public SubmissionTrigger Trigger { get; }

        /// <summary>
        /// 判定対象になった式の snapshot を取得します。
        /// </summary>
        public Expression SubmittedExpression { get; }

        /// <summary>
        /// ミッション条件を満たしたかを取得します。
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// 式の計算結果を取得します。
        /// </summary>
        public CalculationResult CalculationResult { get; }

        /// <summary>
        /// 指定された提出判定から結果を生成します。
        /// </summary>
        /// <param name="missionId">判定対象のミッション識別子。</param>
        /// <param name="trigger">判定を発生させたトリガー。</param>
        /// <param name="submittedExpression">判定対象の式。</param>
        /// <param name="isSuccess">ミッション条件を満たしたか。</param>
        /// <param name="calculationResult">式の計算結果。</param>
        public SubmissionResolution(
            MissionId missionId,
            SubmissionTrigger trigger,
            Expression submittedExpression,
            bool isSuccess,
            CalculationResult calculationResult)
        {
            if (!Enum.IsDefined(typeof(SubmissionTrigger), trigger))
            {
                throw new ArgumentOutOfRangeException(nameof(trigger), trigger, null);
            }

            if (calculationResult.Kind != CalculationResultKind.Defined && isSuccess)
            {
                throw new ArgumentException("Only a defined calculation result may be marked as a success.", nameof(isSuccess));
            }

            MissionId = missionId;
            Trigger = trigger;
            SubmittedExpression = submittedExpression;
            IsSuccess = isSuccess;
            CalculationResult = calculationResult;
        }
    }
}

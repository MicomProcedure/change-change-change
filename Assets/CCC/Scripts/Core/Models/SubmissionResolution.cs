using System;

namespace Ccc.Core.Model
{
    public sealed class SubmissionResolution
    {
        public MissionId MissionId { get; }
        public SubmissionTrigger Trigger { get; }
        public Expression SubmittedExpression { get; }
        public bool IsSuccess { get; }
        public CalculationResult CalculationResult { get; }

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

            if (calculationResult == null)
            {
                throw new ArgumentNullException(nameof(calculationResult));
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

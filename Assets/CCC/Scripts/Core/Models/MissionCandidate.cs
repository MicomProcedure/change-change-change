using System;

namespace Ccc.Core.Model
{
    /// <summary>
    /// カタログ選出条件を通過した出題候補ミッションを表す不変クラス。
    /// </summary>
    public sealed class MissionCandidate
    {
        public MissionCondition Condition { get; }
        public int Tier { get; }
        public int ExpressionMatchCount { get; }
        public int EvaluatedExpressionCount { get; }
        public float ExpressionMatchRatio { get; }
        public ExpressionCandidate AnswerExample { get; }

        public MissionCandidate(
            MissionCondition condition,
            int tier,
            int expressionMatchCount,
            int evaluatedExpressionCount,
            ExpressionCandidate answerExample)
        {
            if (tier <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(tier), "Tier must be greater than zero.");
            }

            if (evaluatedExpressionCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(evaluatedExpressionCount), "Evaluated expression count must be greater than zero.");
            }

            if (expressionMatchCount < 1 || expressionMatchCount > evaluatedExpressionCount)
            {
                throw new ArgumentOutOfRangeException(nameof(expressionMatchCount), "Expression match count must be between 1 and evaluatedExpressionCount.");
            }

            if (!answerExample.Result.IsDefined)
            {
                throw new ArgumentException(
                    "AnswerExample must contain a defined calculation result.",
                    nameof(answerExample));
            }

            Condition = condition;
            Tier = tier;
            ExpressionMatchCount = expressionMatchCount;
            EvaluatedExpressionCount = evaluatedExpressionCount;
            ExpressionMatchRatio = (float)expressionMatchCount / evaluatedExpressionCount;
            AnswerExample = answerExample;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Ccc.Core.Model;

namespace Ccc.Core.Logic
{
    /// <summary>
    /// 到達可能な式結果と検証済みProfileからMissionパラメータ候補を決定的に生成します。
    /// </summary>
    public static class MissionParameterCandidateGenerator
    {
        /// <summary>
        /// MissionKind、ParameterA、ParameterB、肯定、Negatedの順で候補を生成します。
        /// </summary>
        /// <param name="expressions">Sessionで到達可能な式候補。</param>
        /// <param name="settings">検証済み候補生成設定。</param>
        /// <returns>候補生成結果。</returns>
        public static MissionParameterGenerationResult Generate(
            IReadOnlyList<ExpressionCandidate> expressions,
            MissionParameterGenerationSettings settings)
        {
            if (expressions == null)
            {
                throw new ArgumentNullException(nameof(expressions));
            }

            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            ReachableResults reachableResults = AnalyzeReachableResults(expressions);
            long totalCandidateCount = 0L;

            for (int ruleIndex = 0; ruleIndex < settings.Rules.Count; ruleIndex++)
            {
                MissionKindGenerationRule rule = settings.Rules[ruleIndex];
                if (!rule.IsEnabled)
                {
                    continue;
                }

                long kindCandidateCount;
                try
                {
                    kindCandidateCount = CountConditions(
                        rule,
                        reachableResults,
                        rule.MaximumCandidateCount);
                }
                catch (OverflowException)
                {
                    return MissionParameterGenerationResult.Failure(
                        MissionParameterGenerationFailureKind.CandidateCountOverflow,
                        $"MissionKind.{rule.Kind} candidate count overflowed Int64 range.");
                }
                if (kindCandidateCount == 0L)
                {
                    return MissionParameterGenerationResult.Failure(
                        MissionParameterGenerationFailureKind.NoCandidatesForEnabledKind,
                        $"MissionKind.{rule.Kind} generated zero candidates.");
                }

                if (kindCandidateCount > int.MaxValue)
                {
                    return MissionParameterGenerationResult.Failure(
                        MissionParameterGenerationFailureKind.CandidateCountOverflow,
                        $"MissionKind.{rule.Kind} candidate count {kindCandidateCount} exceeds Int32 range.");
                }

                if (kindCandidateCount > rule.MaximumCandidateCount)
                {
                    return MissionParameterGenerationResult.Failure(
                        MissionParameterGenerationFailureKind.KindCandidateLimitExceeded,
                        $"MissionKind.{rule.Kind} planned candidate count is at least {kindCandidateCount} " +
                        $"exceeds configured limit {rule.MaximumCandidateCount}.");
                }

                try
                {
                    totalCandidateCount = checked(totalCandidateCount + kindCandidateCount);
                }
                catch (OverflowException)
                {
                    return MissionParameterGenerationResult.Failure(
                        MissionParameterGenerationFailureKind.CandidateCountOverflow,
                        "Total candidate count overflowed Int64 range.");
                }

            }

            if (totalCandidateCount > int.MaxValue)
            {
                return MissionParameterGenerationResult.Failure(
                    MissionParameterGenerationFailureKind.CandidateCountOverflow,
                    $"Total candidate count {totalCandidateCount} exceeds Int32 range.");
            }

            if (totalCandidateCount > settings.MaximumTotalCandidateCount)
            {
                return MissionParameterGenerationResult.Failure(
                    MissionParameterGenerationFailureKind.TotalCandidateLimitExceeded,
                    $"Total planned candidate count {totalCandidateCount} exceeds configured limit " +
                    $"{settings.MaximumTotalCandidateCount}.");
            }

            var candidates = new List<MissionParameterCandidate>((int)totalCandidateCount);
            for (int ruleIndex = 0; ruleIndex < settings.Rules.Count; ruleIndex++)
            {
                MissionKindGenerationRule rule = settings.Rules[ruleIndex];
                if (!rule.IsEnabled)
                {
                    continue;
                }

                foreach (MissionCondition condition in EnumerateConditions(rule, reachableResults))
                {
                    candidates.Add(new MissionParameterCandidate(condition));
                }
            }

            return MissionParameterGenerationResult.Success(candidates.ToArray());
        }

        private static long CountConditions(
            MissionKindGenerationRule rule,
            ReachableResults reachableResults,
            int stopAfterCount)
        {
            long count = 0L;
            foreach (MissionCondition condition in EnumerateConditions(rule, reachableResults))
            {
                _ = condition;
                count = checked(count + 1L);
                if (count > stopAfterCount)
                {
                    return count;
                }
            }

            return count;
        }

        private static IEnumerable<MissionCondition> EnumerateConditions(
            MissionKindGenerationRule rule,
            ReachableResults reachableResults)
        {
            MissionKindDefinition definition = MissionKindDefinitionCatalog.Get(rule.Kind);
            switch (definition.ParameterStructure)
            {
                case MissionParameterStructure.None:
                    foreach (MissionCondition condition in ExpandPolarity(
                                 MissionCondition.Create(rule.Kind),
                                 rule))
                    {
                        yield return condition;
                    }
                    yield break;

                case MissionParameterStructure.Single:
                    foreach (int parameter in rule.ParameterADomain.EnumerateValues())
                    {
                        if (!IsReachableSingleParameter(rule.Kind, parameter, reachableResults))
                        {
                            continue;
                        }

                        foreach (MissionCondition condition in ExpandPolarity(
                                     MissionCondition.Create(rule.Kind, parameter),
                                     rule))
                        {
                            yield return condition;
                        }
                    }
                    yield break;

                case MissionParameterStructure.Pair:
                    foreach (int parameterA in rule.ParameterADomain.EnumerateValues())
                    {
                        foreach (int parameterB in rule.ParameterBDomain.EnumerateValues())
                        {
                            if (!TryCreateReachablePairCondition(
                                    rule.Kind,
                                    parameterA,
                                    parameterB,
                                    reachableResults,
                                    out MissionCondition positiveCondition))
                            {
                                continue;
                            }

                            foreach (MissionCondition condition in ExpandPolarity(
                                         positiveCondition,
                                         rule))
                            {
                                yield return condition;
                            }
                        }
                    }
                    yield break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(definition.ParameterStructure),
                        definition.ParameterStructure,
                        null);
            }
        }

        private static IEnumerable<MissionCondition> ExpandPolarity(
            MissionCondition positiveCondition,
            MissionKindGenerationRule rule)
        {
            yield return positiveCondition;
            if (!rule.GeneratesNegated)
            {
                yield break;
            }

            switch (MissionKindDefinitionCatalog.Get(rule.Kind).ParameterStructure)
            {
                case MissionParameterStructure.None:
                    yield return MissionCondition.Create(rule.Kind, true);
                    break;
                case MissionParameterStructure.Single:
                    yield return MissionCondition.Create(
                        rule.Kind,
                        positiveCondition.ParameterA,
                        true);
                    break;
                case MissionParameterStructure.Pair when rule.Kind == MissionKind.Between:
                    yield return MissionCondition.Between(
                        positiveCondition.ParameterA,
                        positiveCondition.ParameterB,
                        true);
                    break;
                case MissionParameterStructure.Pair when rule.Kind == MissionKind.Modulo:
                    yield return MissionCondition.Modulo(
                        positiveCondition.ParameterA,
                        positiveCondition.ParameterB,
                        true);
                    break;
                default:
                    throw new InvalidOperationException(
                        $"MissionKind.{rule.Kind} has no polarity expansion path.");
            }
        }

        private static bool IsReachableSingleParameter(
            MissionKind kind,
            int parameter,
            ReachableResults reachableResults)
        {
            switch (kind)
            {
                case MissionKind.AtLeast:
                case MissionKind.AtMost:
                case MissionKind.Exact:
                    return reachableResults.Integers.Contains(parameter);
                case MissionKind.Multiple:
                    return parameter >= 2 && reachableResults.Integers.Any(
                        reachable => reachable % parameter == 0);
                case MissionKind.Absolute:
                    return parameter >= 1 &&
                           (reachableResults.Integers.Contains(parameter) ||
                            reachableResults.Integers.Contains(-parameter));
                case MissionKind.OnesDigit:
                    return parameter >= 0 &&
                           parameter <= 9 &&
                           reachableResults.Integers.Any(
                               reachable => Math.Abs((long)reachable) % 10L == parameter);
                case MissionKind.DecimalFirstDigit:
                    return parameter >= 0 &&
                           parameter <= 9 &&
                           reachableResults.DecimalFirstDigits.Contains(parameter);
                case MissionKind.Divisor:
                    return parameter >= 1 &&
                           reachableResults.Integers.Any(
                               reachable => reachable != 0 && parameter % reachable == 0);
                case MissionKind.Power:
                    return parameter >= 2 &&
                           reachableResults.Integers.Any(
                               reachable => IntegerPropertyCalculator.IsPositivePowerOf(reachable, parameter));
                case MissionKind.TensDigit:
                    return parameter >= 0 &&
                           parameter <= 9 &&
                           reachableResults.Integers.Any(
                               reachable => IntegerPropertyCalculator.GetTensDigit(reachable) == parameter);
                case MissionKind.DigitSum:
                    return parameter >= 1 &&
                           reachableResults.Integers.Any(
                               reachable => IntegerPropertyCalculator.SumDigits(reachable) == parameter);
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
            }
        }

        private static bool TryCreateReachablePairCondition(
            MissionKind kind,
            int parameterA,
            int parameterB,
            ReachableResults reachableResults,
            out MissionCondition condition)
        {
            condition = default;
            switch (kind)
            {
                case MissionKind.Between:
                    if (parameterB < 1)
                    {
                        return false;
                    }

                    int upperBound;
                    try
                    {
                        upperBound = checked(parameterA + parameterB);
                    }
                    catch (OverflowException)
                    {
                        return false;
                    }

                    if (!reachableResults.Integers.Contains(parameterA) ||
                        !reachableResults.Integers.Contains(upperBound))
                    {
                        return false;
                    }

                    condition = MissionCondition.Between(parameterA, parameterB);
                    return true;

                case MissionKind.Modulo:
                    if (parameterA < 2 || parameterB < 0 || parameterB >= parameterA)
                    {
                        return false;
                    }

                    if (!reachableResults.Integers.Any(
                            reachable => EuclideanModulo(reachable, parameterA) == parameterB))
                    {
                        return false;
                    }

                    condition = MissionCondition.Modulo(parameterA, parameterB);
                    return true;

                default:
                    throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
            }
        }

        private static ReachableResults AnalyzeReachableResults(
            IReadOnlyList<ExpressionCandidate> expressions)
        {
            var integers = new SortedSet<int>();
            var decimalFirstDigits = new HashSet<int>();
            for (int index = 0; index < expressions.Count; index++)
            {
                CalculationResult result = expressions[index].Result;
                if (!result.IsDefined)
                {
                    continue;
                }

                Rational value = result.Value;
                if (value.IsInteger)
                {
                    if (value.Numerator >= int.MinValue && value.Numerator <= int.MaxValue)
                    {
                        integers.Add((int)value.Numerator);
                    }
                }
                else
                {
                    long fractionalNumerator = Math.Abs(
                        value.Numerator % value.Denominator);
                    int firstDigit = (int)(fractionalNumerator * 10L / value.Denominator);
                    decimalFirstDigits.Add(firstDigit);
                }
            }

            return new ReachableResults(integers, decimalFirstDigits);
        }

        private static int EuclideanModulo(int dividend, int divisor)
        {
            int remainder = dividend % divisor;
            return remainder < 0 ? remainder + divisor : remainder;
        }

        private sealed class ReachableResults
        {
            public HashSet<int> Integers { get; }
            public HashSet<int> DecimalFirstDigits { get; }

            public ReachableResults(
                IEnumerable<int> integers,
                IEnumerable<int> decimalFirstDigits)
            {
                Integers = new HashSet<int>(integers);
                DecimalFirstDigits = new HashSet<int>(decimalFirstDigits);
            }
        }
    }
}

using System;
using Ccc.Core.Logic;
using Ccc.Core.Model;
using NUnit.Framework;

namespace Ccc.Tests
{
    /// <summary>
    /// 約数・累乗・桁・無限小数系MissionKindの成否判定と条件生成時の検証を確認します。
    /// </summary>
    public sealed class MissionConditionMatcherTests
    {
        /// <summary>
        /// Nの約数は負の約数を含み、0と非整数を含まないことを検証します。
        /// </summary>
        [TestCase(12, 3L, 1L, true)]
        [TestCase(12, 12L, 1L, true)]
        [TestCase(12, 1L, 1L, true)]
        [TestCase(12, -4L, 1L, true)]
        [TestCase(12, 5L, 1L, false)]
        [TestCase(12, 24L, 1L, false)]
        [TestCase(12, 0L, 1L, false)]
        [TestCase(12, 1L, 2L, false)]
        public void IsMatch_Divisor_ShouldJudgeSignInsensitiveIntegerDivisor(
            int parameter,
            long numerator,
            long denominator,
            bool expected)
        {
            AssertMatch(MissionCondition.Create(MissionKind.Divisor, parameter), numerator, denominator, expected);
        }

        /// <summary>
        /// Nの累乗は指数1以上の正の整数だけを含むことを検証します。
        /// </summary>
        [TestCase(2, 2L, 1L, true)]
        [TestCase(2, 8L, 1L, true)]
        [TestCase(2, 16L, 1L, true)]
        [TestCase(3, 9L, 1L, true)]
        [TestCase(3, 27L, 1L, true)]
        [TestCase(2, 1L, 1L, false)]
        [TestCase(2, 6L, 1L, false)]
        [TestCase(3, 6L, 1L, false)]
        [TestCase(2, -8L, 1L, false)]
        [TestCase(2, 0L, 1L, false)]
        [TestCase(2, 1L, 2L, false)]
        public void IsMatch_Power_ShouldJudgePositiveExponentPower(
            int parameter,
            long numerator,
            long denominator,
            bool expected)
        {
            AssertMatch(MissionCondition.Create(MissionKind.Power, parameter), numerator, denominator, expected);
        }

        /// <summary>
        /// 累乗判定が大きな値でもオーバーフローせずに判定できることを検証します。
        /// </summary>
        [Test]
        public void IsMatch_PowerWithLargeValue_ShouldNotOverflow()
        {
            MissionCondition condition = MissionCondition.Create(MissionKind.Power, 3);

            AssertMatch(condition, long.MaxValue, 1L, false);
            AssertMatch(condition, 4052555153018976267L, 1L, true);
        }

        /// <summary>
        /// 0と1の間は両端を含まない正の値だけを含むことを検証します。
        /// </summary>
        [TestCase(1L, 2L, true)]
        [TestCase(4L, 5L, true)]
        [TestCase(0L, 1L, false)]
        [TestCase(1L, 1L, false)]
        [TestCase(-1L, 2L, false)]
        [TestCase(3L, 2L, false)]
        public void IsMatch_BetweenZeroAndOne_ShouldJudgeOpenUnitInterval(
            long numerator,
            long denominator,
            bool expected)
        {
            AssertMatch(MissionCondition.Create(MissionKind.BetweenZeroAndOne), numerator, denominator, expected);
        }

        /// <summary>
        /// 2桁は符号を除いて10以上99以下の整数だけを含むことを検証します。
        /// </summary>
        [TestCase(10L, 1L, true)]
        [TestCase(99L, 1L, true)]
        [TestCase(-12L, 1L, true)]
        [TestCase(9L, 1L, false)]
        [TestCase(100L, 1L, false)]
        [TestCase(0L, 1L, false)]
        [TestCase(25L, 2L, false)]
        public void IsMatch_TwoDigit_ShouldJudgeTwoDigitInteger(
            long numerator,
            long denominator,
            bool expected)
        {
            AssertMatch(MissionCondition.Create(MissionKind.TwoDigit), numerator, denominator, expected);
        }

        /// <summary>
        /// 十の位は符号を除いた整数で判定し、1桁の数を十の位0とみなすことを検証します。
        /// </summary>
        [TestCase(0, 7L, 1L, true)]
        [TestCase(0, 0L, 1L, true)]
        [TestCase(0, -5L, 1L, true)]
        [TestCase(0, 105L, 1L, true)]
        [TestCase(0, 12L, 1L, false)]
        [TestCase(1, 12L, 1L, true)]
        [TestCase(1, -15L, 1L, true)]
        [TestCase(1, 112L, 1L, true)]
        [TestCase(1, 7L, 1L, false)]
        [TestCase(1, 25L, 2L, false)]
        [TestCase(2, -20L, 1L, true)]
        public void IsMatch_TensDigit_ShouldJudgeUnsignedTensDigit(
            int parameter,
            long numerator,
            long denominator,
            bool expected)
        {
            AssertMatch(MissionCondition.Create(MissionKind.TensDigit, parameter), numerator, denominator, expected);
        }

        /// <summary>
        /// 各桁の和は符号を除いた整数の各桁を合計して判定することを検証します。
        /// </summary>
        [TestCase(7, 16L, 1L, true)]
        [TestCase(7, -25L, 1L, true)]
        [TestCase(7, 7L, 1L, true)]
        [TestCase(7, 70L, 1L, true)]
        [TestCase(7, 8L, 1L, false)]
        [TestCase(7, 0L, 1L, false)]
        [TestCase(7, 7L, 2L, false)]
        [TestCase(1, 100L, 1L, true)]
        public void IsMatch_DigitSum_ShouldJudgeUnsignedDigitSum(
            int parameter,
            long numerator,
            long denominator,
            bool expected)
        {
            AssertMatch(MissionCondition.Create(MissionKind.DigitSum, parameter), numerator, denominator, expected);
        }

        /// <summary>
        /// 無限小数は分母に2と5以外の素因数を含む非整数だけを含むことを検証します。
        /// </summary>
        [TestCase(1L, 3L, true)]
        [TestCase(-2L, 3L, true)]
        [TestCase(1L, 6L, true)]
        [TestCase(1L, 7L, true)]
        [TestCase(1L, 2L, false)]
        [TestCase(1L, 8L, false)]
        [TestCase(3L, 5L, false)]
        [TestCase(1L, 20L, false)]
        [TestCase(2L, 1L, false)]
        [TestCase(0L, 1L, false)]
        public void IsMatch_RepeatingDecimal_ShouldJudgeNonTerminatingDecimal(
            long numerator,
            long denominator,
            bool expected)
        {
            AssertMatch(MissionCondition.Create(MissionKind.RepeatingDecimal), numerator, denominator, expected);
        }

        /// <summary>
        /// 新しいKindでもNegated条件が判定を反転することを検証します。
        /// </summary>
        [Test]
        public void IsMatch_NegatedDivisor_ShouldInvertJudgement()
        {
            MissionCondition condition = MissionCondition.Create(MissionKind.Divisor, 12, true);

            AssertMatch(condition, 5L, 1L, true);
            AssertMatch(condition, 3L, 1L, false);
        }

        /// <summary>
        /// 新しいKindでも0除算はNegated条件を含めて常に失敗することを検証します。
        /// </summary>
        [Test]
        public void IsMatch_DivisionByZeroWithNegatedRepeatingDecimal_ShouldFail()
        {
            MissionCondition condition = MissionCondition.Create(MissionKind.RepeatingDecimal, true);

            Assert.That(
                MissionConditionMatcher.IsMatch(condition, CalculationResult.DivisionByZero()),
                Is.False);
        }

        /// <summary>
        /// 1パラメータの新しいKindが範囲外のパラメータを拒否することを検証します。
        /// </summary>
        [TestCase(MissionKind.Divisor, 0)]
        [TestCase(MissionKind.Power, 1)]
        [TestCase(MissionKind.TensDigit, -1)]
        [TestCase(MissionKind.TensDigit, 10)]
        [TestCase(MissionKind.DigitSum, 0)]
        public void Create_WithOutOfRangeParameter_ShouldThrow(MissionKind kind, int parameter)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => MissionCondition.Create(kind, parameter));
        }

        /// <summary>
        /// 1パラメータの新しいKindが範囲境界のパラメータを受け入れることを検証します。
        /// </summary>
        [TestCase(MissionKind.Divisor, 1)]
        [TestCase(MissionKind.Power, 2)]
        [TestCase(MissionKind.TensDigit, 0)]
        [TestCase(MissionKind.TensDigit, 9)]
        [TestCase(MissionKind.DigitSum, 1)]
        public void Create_WithBoundaryParameter_ShouldSucceed(MissionKind kind, int parameter)
        {
            MissionCondition condition = MissionCondition.Create(kind, parameter);

            Assert.That(condition.ParameterA, Is.EqualTo(parameter));
        }

        private static void AssertMatch(
            MissionCondition condition,
            long numerator,
            long denominator,
            bool expected)
        {
            CalculationResult result = CalculationResult.Defined(Rational.Create(numerator, denominator));

            Assert.That(MissionConditionMatcher.IsMatch(condition, result), Is.EqualTo(expected));
        }
    }
}

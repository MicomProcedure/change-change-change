using System;
using Ccc.Core.Model;

namespace Ccc.Core.Logic
{
    /// <summary>
    /// 計算結果 (CalculationResult) が MissionCondition を満たしているかを同期・決定的に判定するクラス。
    /// </summary>
    public static class MissionConditionMatcher
    {
        private const int TwoDigitCount = 2;

        // 有限小数になる分母が持ち得る素因数（10の素因数）
        private static readonly long[] TerminatingDecimalPrimeFactors = { 2L, 5L };

        /// <summary>
        /// 指定された条件と計算結果が一致するか判定します。
        /// </summary>
        /// <param name="condition">ミッション条件</param>
        /// <param name="result">計算結果</param>
        /// <returns>条件を満たす場合は true。0除算や未初期化の場合は常に false</returns>
        public static bool IsMatch(MissionCondition condition, CalculationResult result)
        {
            // 0除算・未初期化などの非正常値は、否定(IsNegated)の有無にかかわらず常に失敗
            if (!result.IsDefined)
            {
                return false;
            }

            Rational value = result.Value;
            bool isMatch = EvaluateCondition(condition, value);

            // IsNegated が有効な場合は最後に1回だけ結果を反転
            return condition.IsNegated ? !isMatch : isMatch;
        }

        private static bool EvaluateCondition(MissionCondition condition, Rational value)
        {
            switch (condition.Kind)
            {
                // ==========================================
                // Tier 1
                // ==========================================
                case MissionKind.Even:
                    // 偶数: 整数 かつ 絶対値を2で割った余りが0（0を含む）
                    return value.IsInteger && Math.Abs(value.Numerator) % 2 == 0;

                case MissionKind.Odd:
                    // 奇数: 整数 かつ 絶対値を2で割った余りが1
                    return value.IsInteger && Math.Abs(value.Numerator) % 2 == 1;

                case MissionKind.Integer:
                    // 整数: 分母が1
                    return value.IsInteger;

                case MissionKind.Decimal:
                    // 小数: 分母が1以外
                    return !value.IsInteger;

                case MissionKind.Positive:
                    // 正の数: 0より大きい（分母は正規化により常に正）
                    return value.Numerator > 0;

                case MissionKind.Negative:
                    // 負の数: 0より小さい
                    return value.Numerator < 0;

                case MissionKind.AtLeast:
                    // N以上: Rational との比較（両端を含む）
                    return value.CompareTo(Rational.Create(condition.ParameterA, 1)) >= 0;

                case MissionKind.AtMost:
                    // N以下: Rational との比較（両端を含む）
                    return value.CompareTo(Rational.Create(condition.ParameterA, 1)) <= 0;

                // ==========================================
                // Tier 2
                // ==========================================
                case MissionKind.Exact:
                    // Nにしろ: 整数 かつ 値がN
                    return value.IsInteger && value.Numerator == condition.ParameterA;

                case MissionKind.Prime:
                    // 素数: 2以上の正の整数 かつ 約数が1と自身のみ
                    return value.IsInteger && IsPrime(value.Numerator);

                case MissionKind.Multiple:
                    // Nの倍数: 整数のみ。負の倍数と0を含む
                    return value.IsInteger && value.Numerator % condition.ParameterA == 0;

                case MissionKind.Zero:
                    // 0にしろ: 整数 かつ 値が0
                    return value.IsInteger && value.Numerator == 0;

                case MissionKind.Square:
                    // 平方数: 0以上の整数平方（0と1を含む）
                    return value.IsInteger && value.Numerator >= 0 && IsSquare(value.Numerator);

                case MissionKind.Between:
                    // N以上M以下: 下限 ParameterA、幅 ParameterB (上限 = ParameterA + ParameterB)
                    checked
                    {
                        var lowerBound = Rational.Create(condition.ParameterA, 1);
                        var upperBound = Rational.Create(condition.ParameterA + condition.ParameterB, 1);
                        return value.CompareTo(lowerBound) >= 0 && value.CompareTo(upperBound) <= 0;
                    }

                // ==========================================
                // Tier 3 / Tier 4 (将来用拡張)
                // ==========================================
                case MissionKind.Absolute:
                    return value.IsInteger && Math.Abs(value.Numerator) == condition.ParameterA;

                case MissionKind.OnesDigit:
                    return value.IsInteger && Math.Abs(value.Numerator) % 10 == condition.ParameterA;

                case MissionKind.Composite:
                    // 合成数: 1より大きく素数ではない整数
                    return value.IsInteger && value.Numerator > 1 && !IsPrime(value.Numerator);

                case MissionKind.Modulo:
                    // ユークリッド剰余: 0 <= 余り < divisor
                    return value.IsInteger && EuclideanModulo(value.Numerator, condition.ParameterA) == condition.ParameterB;

                case MissionKind.DecimalFirstDigit:
                    // 小数第1位（有限小数の筆算方式）
                    return !value.IsInteger && (Math.Abs(value.Numerator) * 10 / value.Denominator) % 10 == condition.ParameterA;

                // ==========================================
                // 約数・累乗・桁・無限小数
                // ==========================================
                case MissionKind.Divisor:
                    // Nの約数: 0以外の整数で、負の約数を含む
                    return value.IsInteger &&
                           value.Numerator != 0 &&
                           condition.ParameterA % value.Numerator == 0;

                case MissionKind.Power:
                    // Nの累乗: 指数1以上（N, N^2, ...）。1は含まない
                    return value.IsInteger &&
                           IntegerPropertyCalculator.IsPositivePowerOf(value.Numerator, condition.ParameterA);

                case MissionKind.BetweenZeroAndOne:
                    // 0と1の間: 両端を含まない
                    return value.Numerator > 0 && value.CompareTo(Rational.Create(1, 1)) < 0;

                case MissionKind.TwoDigit:
                    // 2桁: 符号を除いた整数の桁数が2
                    return value.IsInteger &&
                           IntegerPropertyCalculator.CountDigits(value.Numerator) == TwoDigitCount;

                case MissionKind.TensDigit:
                    // 十の位: 符号を除いた整数で判定し、1桁の数は十の位0とみなす
                    return value.IsInteger &&
                           IntegerPropertyCalculator.GetTensDigit(value.Numerator) == condition.ParameterA;

                case MissionKind.DigitSum:
                    // 各桁の和: 符号を除いた整数の各桁を合計する
                    return value.IsInteger &&
                           IntegerPropertyCalculator.SumDigits(value.Numerator) == condition.ParameterA;

                case MissionKind.RepeatingDecimal:
                    // 無限小数: 約分済みの分母に2と5以外の素因数を含む
                    return !value.IsInteger && HasRepeatingDecimalExpansion(value.Denominator);

                default:
                    throw new ArgumentOutOfRangeException(nameof(condition.Kind), $"Unsupported MissionKind: {condition.Kind}");
            }
        }

        private static bool IsPrime(long number)
        {
            if (number < 2)
            {
                return false;
            }

            if (number == 2)
            {
                return true;
            }

            if (number % 2 == 0)
            {
                return false;
            }

            long limit = (long)Math.Sqrt(number);
            for (long i = 3; i <= limit; i += 2)
            {
                if (number % i == 0)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsSquare(long number)
        {
            if (number < 0)
            {
                return false;
            }

            long root = (long)Math.Sqrt(number);
            return root * root == number;
        }

        private static bool HasRepeatingDecimalExpansion(long denominator)
        {
            long remaining = denominator;
            for (int index = 0; index < TerminatingDecimalPrimeFactors.Length; index++)
            {
                long primeFactor = TerminatingDecimalPrimeFactors[index];
                while (remaining % primeFactor == 0)
                {
                    remaining /= primeFactor;
                }
            }

            return remaining != 1;
        }

        private static long EuclideanModulo(long dividend, int divisor)
        {
            long remainder = dividend % divisor;
            return remainder < 0 ? remainder + divisor : remainder;
        }
    }
}

using System;

namespace Ccc.Core.Model
{
    /// <summary>
    /// Rational の四則演算を提供する純粋関数クラス。
    /// float/double への変換は行わず、正確な有理数計算を行います。
    /// </summary>
    public static class RationalMath
    {
        public static Rational Add(Rational left, Rational right)
        {
            checked
            {
                long num = left.Numerator * right.Denominator + right.Numerator * left.Denominator;
                long den = left.Denominator * right.Denominator;
                return Rational.Create(num, den);
            }
        }

        public static Rational Subtract(Rational left, Rational right)
        {
            checked
            {
                long num = left.Numerator * right.Denominator - right.Numerator * left.Denominator;
                long den = left.Denominator * right.Denominator;
                return Rational.Create(num, den);
            }
        }

        public static Rational Multiply(Rational left, Rational right)
        {
            checked
            {
                long num = left.Numerator * right.Numerator;
                long den = left.Denominator * right.Denominator;
                return Rational.Create(num, den);
            }
        }

        public static Rational Divide(Rational left, Rational right)
        {
            if (right.Numerator == 0)
            {
                throw new DivideByZeroException("Cannot divide by zero rational.");
            }

            checked
            {
                long num = left.Numerator * right.Denominator;
                long den = left.Denominator * right.Numerator;
                return Rational.Create(num, den);
            }
        }
    }
}

using System;
using System.Globalization;

namespace Ccc.Core.Model
{
    /// <summary>
    /// 正確な有理数を表す不変値オブジェクト。
    /// 常に既約分数（分母 > 0、最大公約数で約分済み、0 は 0/1）として正規化されます。
    /// </summary>
    public readonly struct Rational : IEquatable<Rational>, IComparable<Rational>
    {
        public long Numerator { get; }
        public long Denominator { get; }
        public bool IsInteger => Denominator == 1;

        /// <summary>
        /// 値が負数かどうかを取得します。
        /// </summary>
        public bool IsNegative => Numerator < 0;

        private Rational(long numerator, long denominator)
        {
            Numerator = numerator;
            Denominator = denominator;
        }

        /// <summary>
        /// 分子と分母から正規化された Rational を生成します。
        /// </summary>
        /// <param name="numerator">分子</param>
        /// <param name="denominator">分母（0 は不可）</param>
        /// <exception cref="ArgumentOutOfRangeException">分母が 0 の場合</exception>
        /// <exception cref="OverflowException">正規化計算中に long の範囲を超えた場合</exception>
        public static Rational Create(long numerator, long denominator)
        {
            if (denominator == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(denominator), "Denominator cannot be zero.");
            }

            if (numerator == 0)
            {
                return new Rational(0, 1);
            }

            checked
            {
                // 分母を正数にする
                if (denominator < 0)
                {
                    numerator = -numerator;
                    denominator = -denominator;
                }

                // 最大公約数で約分
                long gcd = GreatestCommonDivisor(Math.Abs(numerator), denominator);
                numerator /= gcd;
                denominator /= gcd;

                return new Rational(numerator, denominator);
            }
        }

        /// <summary>
        /// Decimalを正規化されたRationalへ変換します。
        /// </summary>
        /// <param name="value">変換対象のDecimal。</param>
        /// <returns>変換されたRational。</returns>
        /// <exception cref="OverflowException">Decimalの仮数がlongの範囲を超える場合。</exception>
        public static Rational FromDecimal(decimal value)
        {
            int[] bits = decimal.GetBits(value);
            int scale = (bits[3] >> 16) & 0x7F;
            bool isNegative = (bits[3] & int.MinValue) != 0;

            decimal scaleFactor = 1m;
            for (int index = 0; index < scale; index++)
            {
                scaleFactor *= 10m;
            }

            long numerator = checked((long)(decimal.Abs(value) * scaleFactor));
            if (isNegative)
            {
                numerator = checked(-numerator);
            }

            long denominator = 1;
            for (int index = 0; index < scale; index++)
            {
                denominator = checked(denominator * 10);
            }

            return Create(numerator, denominator);
        }

        /// <summary>
        /// Inspectorで設定されたSingleの十進表記をRationalへ変換します。
        /// </summary>
        /// <param name="value">変換対象のSingle。</param>
        /// <returns>Singleの十進表記をもとにしたRational。</returns>
        /// <exception cref="ArgumentOutOfRangeException">値が有限値でない場合。</exception>
        public static Rational FromSingle(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be finite.");
            }

            decimal decimalValue = decimal.Parse(
                value.ToString(CultureInfo.InvariantCulture),
                NumberStyles.Float,
                CultureInfo.InvariantCulture);
            return FromDecimal(decimalValue);
        }

        public int CompareTo(Rational other)
        {
            if (Denominator == other.Denominator)
            {
                return Numerator.CompareTo(other.Numerator);
            }

            // float/double に変換せず、交差乗算で比較
            checked
            {
                long leftCross = Numerator * other.Denominator;
                long rightCross = other.Numerator * Denominator;
                return leftCross.CompareTo(rightCross);
            }
        }

        public bool Equals(Rational other)
        {
            return Numerator == other.Numerator && Denominator == other.Denominator;
        }

        public override bool Equals(object obj)
        {
            return obj is Rational other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Numerator, Denominator);
        }

        public override string ToString()
        {
            if (IsInteger)
            {
                return Numerator.ToString(CultureInfo.InvariantCulture);
            }

            return $"{Numerator.ToString(CultureInfo.InvariantCulture)}/{Denominator.ToString(CultureInfo.InvariantCulture)}";
        }

        public static bool operator ==(Rational left, Rational right) => left.Equals(right);
        public static bool operator !=(Rational left, Rational right) => !left.Equals(right);
        public static bool operator <(Rational left, Rational right) => left.CompareTo(right) < 0;
        public static bool operator <=(Rational left, Rational right) => left.CompareTo(right) <= 0;
        public static bool operator >(Rational left, Rational right) => left.CompareTo(right) > 0;
        public static bool operator >=(Rational left, Rational right) => left.CompareTo(right) >= 0;

        private static long GreatestCommonDivisor(long a, long b)
        {
            while (b != 0)
            {
                long temp = a % b;
                a = b;
                b = temp;
            }
            return a;
        }
    }
}

using System;
using System.Globalization;
using Ccc.Core.Model;

namespace Ccc.Presentation
{
    /// <summary>
    /// Rationalを小数点以下2桁の表示文言へ変換します。
    /// </summary>
    public sealed class RationalDecimalFormatter
    {
        private const int DecimalScale = 100;
        private const string TruncationMark = "……";
        private const string DivisionByZeroText = "？";

        /// <summary>
        /// Rationalを小数点以下2桁へ切り捨てて表示します。
        /// 有限小数でない場合は、切り捨てた値に継続記号を付けます。
        /// </summary>
        /// <param name="rational">表示対象の正常なRational。</param>
        /// <returns>小数表示文言。</returns>
        public string Format(Rational rational)
        {
            decimal numerator = rational.Numerator;
            bool isNegative = rational.IsNegative;
            decimal absoluteNumerator = isNegative ? -numerator : numerator;
            decimal denominator = rational.Denominator;
            decimal scaledValue = decimal.Truncate(
                absoluteNumerator * DecimalScale / denominator);
            decimal integerPart = decimal.Truncate(scaledValue / DecimalScale);
            int fractionalPart = (int)(scaledValue % DecimalScale);
            bool hasRemainingDigits =
                absoluteNumerator * DecimalScale % denominator != 0m;

            string integerText = integerPart.ToString("0", CultureInfo.InvariantCulture);
            string fractionalText = fractionalPart.ToString("00", CultureInfo.InvariantCulture);
            string result = integerText + "." + fractionalText;
            if (hasRemainingDigits)
            {
                result += TruncationMark;
            }

            return isNegative ? "-" + result : result;
        }

        /// <summary>
        /// 計算結果を表示文言へ変換します。
        /// 0除算は「？」として表示し、未初期化結果は受け付けません。
        /// </summary>
        /// <param name="calculationResult">表示対象の計算結果。</param>
        /// <returns>計算結果の表示文言。</returns>
        /// <exception cref="ArgumentOutOfRangeException">未初期化結果の場合。</exception>
        public string Format(CalculationResult calculationResult)
        {
            switch (calculationResult.Kind)
            {
                case CalculationResultKind.Defined:
                    return Format(calculationResult.Value);
                case CalculationResultKind.DivisionByZero:
                    return DivisionByZeroText;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(calculationResult),
                        calculationResult.Kind,
                        "Only defined or division-by-zero calculation results can be formatted.");
            }
        }
    }
}

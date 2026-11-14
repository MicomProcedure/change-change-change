using System;
using System.Globalization;
using Ccc.Core.Model;

namespace Ccc.Presentation
{
    /// <summary>
    /// Rationalを小数点以下最大2桁の表示文言へ変換します。
    /// </summary>
    public sealed class RationalDecimalFormatter
    {
        // DecimalScaleとTruncatedDecimalFormatは、どちらも小数点以下2桁を表す
        private const int DecimalScale = 100;
        private const string TruncatedDecimalFormat = "0.##";
        private const string DivisionByZeroText = "？";

        /// <summary>
        /// Rationalを小数点以下2桁へ切り捨て、末尾の0を省いて表示します。
        /// 整数は小数点以下を付けずに表示します。
        /// </summary>
        /// <param name="rational">表示対象の正常なRational。</param>
        /// <returns>整数または小数の表示文言。</returns>
        public string Format(Rational rational)
        {
            decimal numerator = rational.Numerator;
            bool isNegative = rational.IsNegative;
            decimal absoluteNumerator = isNegative ? -numerator : numerator;
            decimal denominator = rational.Denominator;
            decimal truncatedAbsoluteValue = decimal.Truncate(
                absoluteNumerator * DecimalScale / denominator) / DecimalScale;

            string absoluteText = truncatedAbsoluteValue.ToString(
                TruncatedDecimalFormat,
                CultureInfo.InvariantCulture);

            // 切り捨てで0になった負数に"-0"を表示しない
            return isNegative && truncatedAbsoluteValue != 0m
                ? "-" + absoluteText
                : absoluteText;
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

namespace Ccc.Core.Logic
{
    /// <summary>
    /// Mission判定と候補生成で共有する、整数の桁と累乗に関する純粋計算を提供します。
    /// 桁の計算はすべて符号を除いた値に対して行います。
    /// </summary>
    internal static class IntegerPropertyCalculator
    {
        private const long DecimalRadix = 10L;

        /// <summary>
        /// 符号を除いた整数の桁数を取得します。0は1桁とします。
        /// </summary>
        /// <param name="number">対象の整数。</param>
        /// <returns>桁数。</returns>
        internal static int CountDigits(long number)
        {
            int digitCount = 1;
            for (long remaining = number / DecimalRadix; remaining != 0; remaining /= DecimalRadix)
            {
                digitCount++;
            }

            return digitCount;
        }

        /// <summary>
        /// 符号を除いた整数の十の位を取得します。1桁の数は0を返します。
        /// </summary>
        /// <param name="number">対象の整数。</param>
        /// <returns>十の位の数字。</returns>
        internal static long GetTensDigit(long number)
        {
            long tensDigit = number / DecimalRadix % DecimalRadix;
            return tensDigit < 0 ? -tensDigit : tensDigit;
        }

        /// <summary>
        /// 符号を除いた整数の各桁の和を取得します。
        /// </summary>
        /// <param name="number">対象の整数。</param>
        /// <returns>各桁の和。</returns>
        internal static long SumDigits(long number)
        {
            long digitSum = 0L;
            for (long remaining = number; remaining != 0; remaining /= DecimalRadix)
            {
                long digit = remaining % DecimalRadix;
                digitSum += digit < 0 ? -digit : digit;
            }

            return digitSum;
        }

        /// <summary>
        /// 整数が指定した底の1乗以上の累乗であるかを判定します。
        /// </summary>
        /// <param name="number">対象の整数。</param>
        /// <param name="powerBase">2以上の底。</param>
        /// <returns>指数1以上の累乗である場合はtrue。</returns>
        internal static bool IsPositivePowerOf(long number, long powerBase)
        {
            if (number < powerBase)
            {
                return false;
            }

            long power = powerBase;
            while (power < number)
            {
                // 次の乗算がnumberを超える場合は一致し得ないため、オーバーフロー前に打ち切る
                if (power > number / powerBase)
                {
                    return false;
                }

                power *= powerBase;
            }

            return power == number;
        }
    }
}

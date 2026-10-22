using System;

namespace Ccc.Core.Model
{
    /// <summary>
    /// 1回のMission抽選で目標式適合割合を選ぶ、下限以上上限以下の閉区間です。
    /// </summary>
    public sealed class TargetExpressionMatchRatioRange
    {
        /// <summary>
        /// 目標式適合割合の下限を取得します。
        /// </summary>
        public float LowerBound { get; }

        /// <summary>
        /// 目標式適合割合の上限を取得します。
        /// </summary>
        public float UpperBound { get; }

        /// <summary>
        /// 下限と上限が等しく、目標式適合割合が1つに定まるかどうかを取得します。
        /// </summary>
        public bool IsSingleValue => LowerBound == UpperBound;

        /// <summary>
        /// 下限と上限から目標式適合割合の範囲を生成します。
        /// </summary>
        /// <param name="lowerBound">目標式適合割合の下限。0以上上限以下とします。</param>
        /// <param name="upperBound">目標式適合割合の上限。下限以上1以下とします。</param>
        /// <exception cref="ArgumentOutOfRangeException">値が有限でない場合、0以上1以下でない場合、または下限が上限を超える場合。</exception>
        public TargetExpressionMatchRatioRange(float lowerBound, float upperBound)
        {
            ValidateRatio(lowerBound, nameof(lowerBound));
            ValidateRatio(upperBound, nameof(upperBound));
            if (lowerBound > upperBound)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(lowerBound),
                    lowerBound,
                    "Lower bound must not exceed the upper bound.");
            }

            LowerBound = lowerBound;
            UpperBound = upperBound;
        }

        /// <summary>
        /// 目標式適合割合が範囲内にあるかどうかを判定します。
        /// </summary>
        /// <param name="ratio">判定する目標式適合割合。</param>
        /// <returns>下限以上上限以下の場合はtrue。</returns>
        public bool Contains(float ratio)
        {
            return ratio >= LowerBound && ratio <= UpperBound;
        }

        private static void ValidateRatio(float ratio, string parameterName)
        {
            if (float.IsNaN(ratio) || float.IsInfinity(ratio) || ratio < 0f || ratio > 1f)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    ratio,
                    "Target expression match ratio bounds must be finite and between zero and one.");
            }
        }
    }
}

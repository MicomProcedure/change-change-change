using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ccc.Core.Model
{
    /// <summary>
    /// Missionパラメータに許可する整数集合を、明示値または包含範囲で表します。
    /// </summary>
    public sealed class IntegerParameterDomain
    {
        private readonly IReadOnlyList<int> _explicitValues;
        private readonly bool _usesRange;
        private readonly int _minimum;
        private readonly int _maximum;
        private readonly int _step;

        /// <summary>正規化後のドメイン要素数を取得します。</summary>
        public long Count { get; }

        private IntegerParameterDomain(IReadOnlyList<int> explicitValues)
        {
            _explicitValues = explicitValues;
            Count = explicitValues.Count;
        }

        private IntegerParameterDomain(int minimum, int maximum, int step)
        {
            _usesRange = true;
            _minimum = minimum;
            _maximum = maximum;
            _step = step;
            Count = ((long)maximum - minimum) / step + 1L;
            _explicitValues = Array.Empty<int>();
        }

        /// <summary>
        /// 明示整数値を重複除去し、昇順へ正規化したドメインを生成します。
        /// </summary>
        /// <param name="values">許可する整数値。</param>
        /// <returns>正規化済みドメイン。</returns>
        /// <exception cref="ArgumentNullException">valuesがnullの場合。</exception>
        /// <exception cref="ArgumentException">valuesが空の場合。</exception>
        public static IntegerParameterDomain FromExplicitValues(IEnumerable<int> values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            int[] normalized = values.Distinct().OrderBy(value => value).ToArray();
            if (normalized.Length == 0)
            {
                throw new ArgumentException("Explicit values cannot be empty.", nameof(values));
            }

            return new IntegerParameterDomain(
                new ReadOnlyCollection<int>(normalized));
        }

        /// <summary>
        /// minimum以上maximum以下をstep間隔で列挙するドメインを生成します。
        /// </summary>
        /// <param name="minimum">包含する最小値。</param>
        /// <param name="maximum">包含する最大境界。</param>
        /// <param name="step">正の増分。</param>
        /// <returns>包含範囲ドメイン。</returns>
        /// <exception cref="ArgumentOutOfRangeException">範囲またはstepが不正な場合。</exception>
        public static IntegerParameterDomain FromInclusiveRange(
            int minimum,
            int maximum,
            int step)
        {
            if (maximum < minimum)
            {
                throw new ArgumentOutOfRangeException(nameof(maximum), maximum, null);
            }

            if (step <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(step), step, null);
            }

            return new IntegerParameterDomain(minimum, maximum, step);
        }

        /// <summary>
        /// 正規化された整数値を昇順で遅延列挙します。
        /// </summary>
        /// <returns>昇順の整数列。</returns>
        public IEnumerable<int> EnumerateValues()
        {
            if (!_usesRange)
            {
                for (int index = 0; index < _explicitValues.Count; index++)
                {
                    yield return _explicitValues[index];
                }

                yield break;
            }

            for (long current = _minimum; current <= _maximum; current += _step)
            {
                yield return (int)current;
            }
        }

        /// <summary>
        /// 指定された整数がドメインに含まれるかを取得します。
        /// </summary>
        /// <param name="candidate">判定対象の整数。</param>
        /// <returns>含まれる場合はtrue。</returns>
        public bool Contains(int candidate)
        {
            if (_usesRange)
            {
                return candidate >= _minimum &&
                       candidate <= _maximum &&
                       ((long)candidate - _minimum) % _step == 0L;
            }

            int minimumIndex = 0;
            int maximumIndex = _explicitValues.Count - 1;
            while (minimumIndex <= maximumIndex)
            {
                int middleIndex = minimumIndex + ((maximumIndex - minimumIndex) / 2);
                int middleValue = _explicitValues[middleIndex];
                if (middleValue == candidate)
                {
                    return true;
                }

                if (middleValue < candidate)
                {
                    minimumIndex = middleIndex + 1;
                }
                else
                {
                    maximumIndex = middleIndex - 1;
                }
            }

            return false;
        }
    }
}

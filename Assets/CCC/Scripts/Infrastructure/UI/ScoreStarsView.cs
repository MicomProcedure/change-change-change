using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Ccc.Infrastructure.UI
{
    /// <summary>
    /// 並んだ星のうち、スコアとして獲得した数だけを満たされた状態で表示するViewです。
    /// 星1つ1つをどう満たして見せるかは<see cref="ScoreStar"/>に任せ、このViewは各星を満たすかどうかだけを決めます。
    /// </summary>
    public sealed class ScoreStarsView : MonoBehaviour
    {
        [Tooltip("先頭から獲得数だけ満たされる星。表示上の並び順に設定する。")]
        [SerializeField] private List<ScoreStar> _stars;

        /// <summary>
        /// 獲得した星の数を表示に反映します。先頭から<paramref name="earnedStarCount"/>個の星を満たし、残りの星を満たされていない状態にします。
        /// 返されたUniTaskは、すべての星の切り替えの表現が完了した時点で完了します。
        /// </summary>
        /// <param name="earnedStarCount">獲得した星の数。0以上、設定された星の数以下。</param>
        /// <param name="cancellationToken">切り替えの表現を中断するためのトークン。</param>
        /// <returns>すべての星の切り替えの表現の完了を表すUniTask。</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="earnedStarCount"/>が負の値、または設定された星の数を超える場合。</exception>
        public UniTask RenderAsync(int earnedStarCount, CancellationToken cancellationToken)
        {
            if (earnedStarCount < 0 || earnedStarCount > _stars.Count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(earnedStarCount),
                    earnedStarCount,
                    $"ScoreStarsView: earnedStarCount must be between 0 and {_stars.Count}.");
            }

            var starTasks = new UniTask[_stars.Count];
            for (int index = 0; index < _stars.Count; index++)
            {
                bool isFilled = index < earnedStarCount;
                starTasks[index] = _stars[index].SetFilledAsync(isFilled, cancellationToken);
            }

            // 星同士の再生順序はこのViewで決めず、各ScoreStarの表現に委ねるため、すべての星を同時に開始して完了を待つ
            return UniTask.WhenAll(starTasks);
        }
    }
}

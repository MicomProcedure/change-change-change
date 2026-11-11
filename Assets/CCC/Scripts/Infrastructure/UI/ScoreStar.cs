using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Ccc.Infrastructure.UI
{
    /// <summary>
    /// スコア表示に並ぶ星1つにアタッチし、その星が満たされているかどうかをどう見せるかを担当するViewです。
    /// 現在は塗りの星のGameObjectを表示・非表示にすることで表現します。
    /// </summary>
    public sealed class ScoreStar : MonoBehaviour
    {
        [Tooltip("星が満たされているときだけ表示する、塗りの星のGameObject。")]
        [SerializeField] private GameObject _fillObject;

        /// <summary>
        /// 星を満たされた見た目、または満たされていない見た目へ切り替えます。
        /// 返されたUniTaskは、切り替えの表現が完了した時点で完了します。
        /// </summary>
        /// <param name="isFilled">星を満たされた見た目にする場合はtrue、満たされていない見た目にする場合はfalse。</param>
        /// <param name="cancellationToken">切り替えの表現を中断するためのトークン。</param>
        /// <returns>切り替えの表現の完了を表すUniTask。</returns>
        public UniTask SetFilledAsync(bool isFilled, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _fillObject.SetActive(isFilled);
            return UniTask.CompletedTask;
        }
    }
}

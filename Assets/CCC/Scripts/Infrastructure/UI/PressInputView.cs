using R3;
using UnityEngine;

namespace Ccc.Infrastructure.UI
{
    /// <summary>
    ///     入力デバイスを問わず、1 回の押下を 1 回の押下イベントとして公開する入力コンポーネントの基底。
    /// </summary>
    public abstract class PressInputView : MonoBehaviour
    {
        /// <summary>
        ///     押下イベントの発行元。
        /// </summary>
        public abstract Observable<Unit> Pressed { get; }

        /// <summary>
        ///     入力の受付可否を切り替える。
        /// </summary>
        /// <param name="isInteractable">入力受付可否。</param>
        public abstract void SetInteractable(bool isInteractable);
    }
}

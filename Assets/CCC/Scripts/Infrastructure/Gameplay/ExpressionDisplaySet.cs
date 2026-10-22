using Ccc.Infrastructure.UI;
using UnityEngine;

namespace Ccc.Infrastructure.Gameplay
{
    /// <summary>
    /// 一つの表示スタイルで使う、式の左辺・演算子・右辺のSelector一式を保持します。
    /// </summary>
    public sealed class ExpressionDisplaySet : MonoBehaviour
    {
        [SerializeField] private CycleSelectorView _leftNumberView;
        [SerializeField] private CycleSelectorView _operatorView;
        [SerializeField] private CycleSelectorView _rightNumberView;

        /// <summary>
        /// 式の左辺を表示するSelectorを取得します。
        /// </summary>
        public CycleSelectorView LeftNumberView
        {
            get { return _leftNumberView; }
        }

        /// <summary>
        /// 式の演算子を表示するSelectorを取得します。
        /// </summary>
        public CycleSelectorView OperatorView
        {
            get { return _operatorView; }
        }

        /// <summary>
        /// 式の右辺を表示するSelectorを取得します。
        /// </summary>
        public CycleSelectorView RightNumberView
        {
            get { return _rightNumberView; }
        }

        /// <summary>
        /// Selector一式の表示状態を切り替えます。
        /// </summary>
        /// <param name="isVisible">表示する場合はtrue。</param>
        public void SetVisible(bool isVisible)
        {
            gameObject.SetActive(isVisible);
        }
    }
}

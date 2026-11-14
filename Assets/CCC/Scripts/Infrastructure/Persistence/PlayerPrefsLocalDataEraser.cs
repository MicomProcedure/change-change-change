using Ccc.Core.UseCases;
using UnityEngine;

namespace Ccc.Infrastructure.Persistence
{
    /// <summary>
    /// PlayerPrefsの全消去APIでローカルの永続化情報を消去します。
    /// キーを列挙しないため、今後追加される保存項目も消去対象に含まれます。
    /// </summary>
    public sealed class PlayerPrefsLocalDataEraser : ILocalDataEraser
    {
        /// <summary>
        /// PlayerPrefsの内容を全て消去します。Unityが保存するウィンドウサイズなどの情報も消去されます。
        /// </summary>
        public void EraseAll()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }
    }
}

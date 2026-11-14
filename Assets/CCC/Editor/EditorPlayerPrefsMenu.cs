using Ccc.Infrastructure.Persistence;
using UnityEditor;
using UnityEngine;

namespace Ccc.Editor
{
    /// <summary>
    /// Editor上のPlayerPrefsを確認のうえ削除するメニューを提供します。
    /// </summary>
    public static class EditorPlayerPrefsMenu
    {
        private const string MenuPath = "CCC/Delete Editor PlayerPrefs";
        private const bool IsValidationMenuItem = true;
        private const string DialogTitle = "Delete Editor PlayerPrefs";
        private const string DialogMessage =
            "Editor上のPlayerPrefsを全て削除します。ハイスコアや音量設定のほか、Unityが保存するウィンドウサイズなども消えます。元に戻せません。";
        private const string DeleteButtonLabel = "削除";
        private const string CancelButtonLabel = "キャンセル";
        private const string DeletedLogMessage = "[EditorPlayerPrefsMenu] Editor上のPlayerPrefsを削除しました。";

        [MenuItem(MenuPath)]
        private static void DeletePlayerPrefs()
        {
            bool isConfirmed = EditorUtility.DisplayDialog(
                DialogTitle,
                DialogMessage,
                DeleteButtonLabel,
                CancelButtonLabel);
            if (!isConfirmed)
            {
                return;
            }

            new PlayerPrefsLocalDataEraser().EraseAll();
            Debug.Log(DeletedLogMessage);
        }

        [MenuItem(MenuPath, IsValidationMenuItem)]
        private static bool CanDeletePlayerPrefs()
        {
            // 実行中のゲームが読み込み済みの設定と、保存内容が食い違わないようにする
            return !EditorApplication.isPlaying;
        }
    }
}

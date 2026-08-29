using System;
using System.Threading;
using Ccc.Presentation;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ccc.Infrastructure.Gameplay
{
    /// <summary>
    /// Gameplayから結果画面とMainMenuへ遷移するUnity実装です。
    /// </summary>
    public sealed class GameplayNavigation : MonoBehaviour, IGameplayNavigation
    {
        [SerializeField] private string mainMenuSceneName = "TitleScene";
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private TMP_Text resultText;

        /// <summary>
        /// SessionResultを結果パネルへ表示します。
        /// </summary>
        /// <param name="result">表示対象のSessionResult。</param>
        public void ShowResult(Ccc.Core.Model.SessionResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (resultText != null)
            {
                resultText.text = result.CorrectAnswerCount.ToString();
            }

            if (resultPanel != null)
            {
                resultPanel.SetActive(true);
            }
        }

        /// <summary>
        /// Inspectorで指定されたMainMenu Sceneを非同期ロードします。
        /// </summary>
        /// <returns>Scene遷移完了を表す非同期処理。</returns>
        public async UniTask NavigateToMainMenuAsync()
        {
            if (string.IsNullOrWhiteSpace(mainMenuSceneName))
            {
                throw new InvalidOperationException("Main menu scene name is not configured.");
            }

            AsyncOperation operation = SceneManager.LoadSceneAsync(mainMenuSceneName);
            if (operation == null)
            {
                throw new InvalidOperationException(
                    $"Main menu scene could not be loaded: {mainMenuSceneName}.");
            }

            await operation.ToUniTask(cancellationToken: CancellationToken.None);
        }
    }
}

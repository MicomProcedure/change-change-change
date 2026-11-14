using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ccc.Infrastructure.UI
{
    /// <summary>
    /// 同じGameObjectのImageに、チュートリアル画像をページ送りで表示します。
    /// 表示されるたびに1ページ目から表示します。
    /// </summary>
    [RequireComponent(typeof(Image))]
    public sealed class TutorialPageView : MonoBehaviour
    {
        private const int FirstPageIndex = 0;

        [SerializeField] private List<Sprite> _pageSprites = new();
        [SerializeField] private Button _previousPageButton;
        [SerializeField] private Button _nextPageButton;

        private Image _pageImage;
        private int _currentPageIndex;

        private void Awake()
        {
            _pageImage = GetComponent<Image>();
            ValidateReferences();

            if (_previousPageButton != null)
            {
                _previousPageButton.onClick.AddListener(ShowPreviousPage);
            }

            if (_nextPageButton != null)
            {
                _nextPageButton.onClick.AddListener(ShowNextPage);
            }
        }

        private void OnEnable()
        {
            ShowPage(FirstPageIndex);
        }

        private void OnDestroy()
        {
            if (_previousPageButton != null)
            {
                _previousPageButton.onClick.RemoveListener(ShowPreviousPage);
            }

            if (_nextPageButton != null)
            {
                _nextPageButton.onClick.RemoveListener(ShowNextPage);
            }
        }

        private void ShowPreviousPage()
        {
            ShowPage(_currentPageIndex - 1);
        }

        private void ShowNextPage()
        {
            ShowPage(_currentPageIndex + 1);
        }

        private void ShowPage(int pageIndex)
        {
            if (_pageSprites == null || _pageSprites.Count == 0)
            {
                SetActiveIfAvailable(_previousPageButton, false);
                SetActiveIfAvailable(_nextPageButton, false);
                return;
            }

            _currentPageIndex = Mathf.Clamp(pageIndex, FirstPageIndex, _pageSprites.Count - 1);
            _pageImage.sprite = _pageSprites[_currentPageIndex];

            bool hasPreviousPage = _currentPageIndex > FirstPageIndex;
            bool hasNextPage = _currentPageIndex < _pageSprites.Count - 1;

            SetActiveIfAvailable(_previousPageButton, hasPreviousPage);
            SetActiveIfAvailable(_nextPageButton, hasNextPage);
        }

        private void ValidateReferences()
        {
            if (_pageSprites == null || _pageSprites.Count == 0)
            {
                LogMissingReference(nameof(_pageSprites));
            }

            if (_previousPageButton == null)
            {
                LogMissingReference(nameof(_previousPageButton));
            }

            if (_nextPageButton == null)
            {
                LogMissingReference(nameof(_nextPageButton));
            }
        }

        private void LogMissingReference(string fieldName)
        {
            Debug.LogWarning(
                $"{nameof(TutorialPageView)}: {fieldName}が未設定です。該当する表示または入力を無効にして処理を継続します。",
                this);
        }

        private static void SetActiveIfAvailable(Button button, bool isActive)
        {
            if (button != null)
            {
                button.gameObject.SetActive(isActive);
            }
        }
    }
}
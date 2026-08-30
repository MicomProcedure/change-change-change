using System;
using System.Threading;
using Ccc.Presentation;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Ccc.Infrastructure.Gameplay
{
    /// <summary>
    /// CorrectCircleまたはPekeを再生した後、Feedback表示を再生するViewです。
    /// </summary>
    public sealed class DotweenFeedbackView : MonoBehaviour
    {
        [Header("Feedback References")]
        [SerializeField] private TMP_Text _feedbackText;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Result Animation References")]
        [SerializeField] private CircleShaderDrawer _correctCircle;
        [SerializeField] private PekeView _peke;

        [Header("Result Animation Durations")]
        [SerializeField] private float _correctCircleDurationSeconds;
        [SerializeField] private float _pekeDurationSeconds;

        [Header("Feedback CanvasGroup Animation")]
        [SerializeField] private float _fadeInDurationSeconds;
        [SerializeField] private float _visibleDurationSeconds;
        [SerializeField] private float _fadeOutDurationSeconds;
        [SerializeField] private Ease _fadeInEase;
        [SerializeField] private Ease _fadeOutEase;

        private Tween _currentTween;

        /// <summary>
        /// 正解または不正解のフィードバック演出を再生します。
        /// </summary>
        /// <param name="feedback">表示するフィードバックデータ。</param>
        /// <param name="cancellationToken">演出をキャンセルするToken。</param>
        /// <returns>演出完了を表す非同期処理。</returns>
        public async UniTask PlayAsync(
            FeedbackViewDataDto feedback,
            CancellationToken cancellationToken)
        {
            if (feedback == null)
            {
                throw new ArgumentNullException(nameof(feedback));
            }

            if (_canvasGroup == null)
            {
                return;
            }

            cancellationToken.ThrowIfCancellationRequested();

            HideResultAnimations();

            gameObject.SetActive(true);
            _canvasGroup.gameObject.SetActive(true);

            if (_feedbackText != null)
            {
                _feedbackText.text = feedback.IsSuccess
                    ? $"正解！\n{feedback.AnswerText}"
                    : $"不正解…\n正解例：{feedback.AnswerText}";
            }

            _canvasGroup.DOKill();
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            try
            {
                await PlayResultAnimationAsync(
                    feedback.IsSuccess,
                    cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                _canvasGroup.blocksRaycasts = true;

                _currentTween = _canvasGroup
                    .DOFade(1f, _fadeInDurationSeconds)
                    .SetEase(_fadeInEase);

                await _currentTween.ToUniTask(
                    TweenCancelBehaviour.KillAndCancelAwait,
                    cancellationToken);

                await UniTask.Delay(
                    TimeSpan.FromSeconds(_visibleDurationSeconds),
                    cancellationToken: cancellationToken);

                _currentTween = _canvasGroup
                    .DOFade(0f, _fadeOutDurationSeconds)
                    .SetEase(_fadeOutEase);

                await _currentTween.ToUniTask(
                    TweenCancelBehaviour.KillAndCancelAwait,
                    cancellationToken);
            }
            finally
            {
                _currentTween?.Kill();
                _currentTween = null;

                _canvasGroup.DOKill();
                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;

                HideResultAnimations();
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 正解または不正解の専用アニメーションを再生します。
        /// </summary>
        /// <param name="isSuccess">正解の場合はtrue。</param>
        /// <param name="cancellationToken">演出をキャンセルするToken。</param>
        /// <returns>専用演出完了を表す非同期処理。</returns>
        private async UniTask PlayResultAnimationAsync(
            bool isSuccess,
            CancellationToken cancellationToken)
        {
            if (isSuccess)
            {
                if (_correctCircle == null)
                {
                    return;
                }

                _correctCircle.gameObject.SetActive(true);

                await UniTask.Delay(
                    TimeSpan.FromSeconds(_correctCircleDurationSeconds),
                    cancellationToken: cancellationToken);

                return;
            }

            if (_peke == null)
            {
                return;
            }

            _peke.gameObject.SetActive(true);

            await UniTask.Delay(
                TimeSpan.FromSeconds(_pekeDurationSeconds),
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// 正解・不正解のアニメーションオブジェクトを非表示にします。
        /// </summary>
        private void HideResultAnimations()
        {
            if (_correctCircle != null)
            {
                _correctCircle.gameObject.SetActive(false);
            }

            if (_peke != null)
            {
                _peke.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 破棄時に実行中の演出を停止します。
        /// </summary>
        private void OnDestroy()
        {
            _currentTween?.Kill();

            if (_canvasGroup != null)
            {
                _canvasGroup.DOKill();
            }
        }
    }
}
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
    /// DOTweenで提出結果のFeedback表示を再生する子Viewです。
    /// </summary>
    public sealed class DotweenFeedbackView : MonoBehaviour
    {
        private const float DefaultSuccessAnimationDurationSeconds = 0.4f;
        private const float DefaultFailureAnimationDurationSeconds = 1.5f;

        [SerializeField] private TMP_Text feedbackText;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float successAnimationDurationSeconds = DefaultSuccessAnimationDurationSeconds;
        [SerializeField] private float failureAnimationDurationSeconds = DefaultFailureAnimationDurationSeconds;

        /// <summary>
        /// Feedback表示をフェードイン・フェードアウトします。
        /// </summary>
        /// <param name="feedback">表示するFeedbackデータ。</param>
        /// <param name="cancellationToken">演出キャンセルToken。</param>
        /// <returns>演出完了を表す非同期処理。</returns>
        public async UniTask PlayAsync(
            FeedbackViewDataDto feedback,
            CancellationToken cancellationToken)
        {
            if (feedback == null)
            {
                throw new ArgumentNullException(nameof(feedback));
            }

            cancellationToken.ThrowIfCancellationRequested();
            if (feedbackText != null)
            {
                feedbackText.text = feedback.IsSuccess
                    ? $"正解！ {feedback.AnswerText}"
                    : $"不正解… {feedback.AnswerText}";
            }

            float animationDurationSeconds = feedback.IsSuccess
                ? successAnimationDurationSeconds
                : failureAnimationDurationSeconds;
            if (canvasGroup == null || animationDurationSeconds <= 0f)
            {
                return;
            }

            gameObject.SetActive(true);
            canvasGroup.DOKill();
            canvasGroup.alpha = 0f;
            await canvasGroup
                .DOFade(1f, animationDurationSeconds)
                .ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken);
            await canvasGroup
                .DOFade(0f, animationDurationSeconds)
                .ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken);
        }

        /// <summary>
        /// 破棄時にDOTween演出を停止します。
        /// </summary>
        private void OnDestroy()
        {
            if (canvasGroup != null)
            {
                canvasGroup.DOKill();
            }
        }
    }
}

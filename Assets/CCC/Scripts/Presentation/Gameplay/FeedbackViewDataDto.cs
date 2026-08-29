using System;
using Ccc.Core.Model;

namespace Ccc.Presentation
{
    /// <summary>
    /// 提出結果の演出に必要な、変換済みの表示データです。
    /// </summary>
    public sealed class FeedbackViewDataDto
    {
        /// <summary>
        /// フィードバック対象のミッション識別子を取得します。
        /// </summary>
        public MissionId MissionId { get; }

        /// <summary>
        /// 提出が成功したかどうかを取得します。
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// 答えの表示文言を取得します。
        /// </summary>
        public string AnswerText { get; }

        /// <summary>
        /// フィードバック後の残りLifeを取得します。
        /// </summary>
        public int RemainingLife { get; }

        /// <summary>
        /// 指定された値から提出結果の表示データを生成します。
        /// </summary>
        /// <param name="missionId">フィードバック対象のミッション識別子。</param>
        /// <param name="isSuccess">提出が成功したかどうか。</param>
        /// <param name="answerText">答えの表示文言。</param>
        /// <param name="remainingLife">フィードバック後の残りLife。</param>
        /// <exception cref="ArgumentNullException"><paramref name="answerText"/>がnullの場合。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="remainingLife"/>が負の場合。</exception>
        public FeedbackViewDataDto(
            MissionId missionId,
            bool isSuccess,
            string answerText,
            int remainingLife)
        {
            if (answerText == null)
            {
                throw new ArgumentNullException(nameof(answerText));
            }

            if (remainingLife < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(remainingLife), remainingLife, null);
            }

            MissionId = missionId;
            IsSuccess = isSuccess;
            AnswerText = answerText;
            RemainingLife = remainingLife;
        }
    }
}

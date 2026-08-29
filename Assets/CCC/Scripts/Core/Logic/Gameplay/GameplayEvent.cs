using System;
using Ccc.Core.Model;

namespace Ccc.Core.Logic
{
    /// <summary>
    /// ゲームプレイ中に発生した論理事実を表す基底 Event です。
    /// </summary>
    public abstract class GameplayEvent
    {
    }

    /// <summary>
    /// ミッションが提示された事実を表します。
    /// </summary>
    public sealed class MissionPresentedEvent : GameplayEvent
    {
        /// <summary>
        /// 提示されたミッションを取得します。
        /// </summary>
        public Mission Mission { get; }

        /// <summary>
        /// 提示されたミッションの制限時間を取得します。
        /// </summary>
        public TimeSpan Duration { get; }

        /// <summary>
        /// ミッションと制限時間から Event を生成します。
        /// </summary>
        /// <param name="mission">提示されたミッション。</param>
        /// <param name="duration">提示されたミッションの制限時間。</param>
        /// <exception cref="ArgumentNullException"><paramref name="mission"/> が null の場合。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="duration"/> が正でない場合。</exception>
        public MissionPresentedEvent(Mission mission, TimeSpan duration)
        {
            Mission = mission ?? throw new ArgumentNullException(nameof(mission));
            if (duration <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(duration), duration, "Duration must be positive.");
            }

            Duration = duration;
        }
    }

    /// <summary>
    /// 提出判定が確定した事実を表します。
    /// </summary>
    public sealed class SubmissionResolvedEvent : GameplayEvent
    {
        /// <summary>
        /// 確定した提出判定を取得します。
        /// </summary>
        public SubmissionResolution Resolution { get; }

        /// <summary>
        /// 正解例の式候補を取得します。
        /// </summary>
        public ExpressionCandidate AnswerExample { get; }

        /// <summary>
        /// 判定後に残っている Life を取得します。
        /// </summary>
        public int RemainingLife { get; }

        /// <summary>
        /// 提出判定、正解例、残り Life から Event を生成します。
        /// </summary>
        /// <param name="resolution">確定した提出判定。</param>
        /// <param name="answerExample">正解例の式候補。</param>
        /// <param name="remainingLife">判定後に残っている Life。</param>
        /// <exception cref="ArgumentNullException"><paramref name="resolution"/> が null の場合。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="remainingLife"/> が負の場合。</exception>
        public SubmissionResolvedEvent(
            SubmissionResolution resolution,
            ExpressionCandidate answerExample,
            int remainingLife)
        {
            Resolution = resolution ?? throw new ArgumentNullException(nameof(resolution));
            if (remainingLife < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(remainingLife), remainingLife, "Remaining life cannot be negative.");
            }

            AnswerExample = answerExample;
            RemainingLife = remainingLife;
        }
    }

    /// <summary>
    /// セッションが終了した事実を表します。
    /// </summary>
    public sealed class SessionEndedEvent : GameplayEvent
    {
        /// <summary>
        /// セッション結果を取得します。
        /// </summary>
        public SessionResult Result { get; }

        /// <summary>
        /// セッション結果から Event を生成します。
        /// </summary>
        /// <param name="result">確定したセッション結果。</param>
        /// <exception cref="ArgumentNullException"><paramref name="result"/> が null の場合。</exception>
        public SessionEndedEvent(SessionResult result)
        {
            Result = result ?? throw new ArgumentNullException(nameof(result));
        }
    }
}

using System;

namespace Ccc.Presentation
{
    /// <summary>
    /// Gameplay画面の描画に必要な、変換済みの表示状態です。
    /// </summary>
    public sealed class GameplayViewStateDto
    {
        /// <summary>
        /// 現在のゲームモード表示文言を取得します。
        /// </summary>
        public string GameModeText { get; }

        /// <summary>
        /// 現在の難易度表示文言を取得します。
        /// </summary>
        public string DifficultyText { get; }

        /// <summary>
        /// 現在のSectionを取得します。
        /// </summary>
        public int Section { get; }

        /// <summary>
        /// 残りLifeを取得します。
        /// </summary>
        public int Life { get; }

        /// <summary>
        /// ミッションの表示文言を取得します。
        /// </summary>
        public string MissionText { get; }

        /// <summary>
        /// 式の左側の表示文言を取得します。
        /// </summary>
        public string LeftText { get; }

        /// <summary>
        /// 式の演算子の表示文言を取得します。
        /// </summary>
        public string OperatorText { get; }

        /// <summary>
        /// 式の右側の表示文言を取得します。
        /// </summary>
        public string RightText { get; }

        /// <summary>
        /// 答えの表示文言を取得します。
        /// </summary>
        public string AnswerText { get; }

        /// <summary>
        /// 残り時間が有効かどうかを取得します。
        /// </summary>
        public bool HasRemainingTime { get; }

        /// <summary>
        /// 残り秒数を取得します。
        /// </summary>
        public float RemainingSeconds { get; }

        /// <summary>
        /// 現在Sectionの制限時間を秒単位で取得します。
        /// </summary>
        public float SectionDurationSeconds { get; }

        /// <summary>
        /// 時間警告中かどうかを取得します。
        /// </summary>
        public bool IsTimeWarning { get; }

        /// <summary>
        /// 式の提出が可能かどうかを取得します。
        /// </summary>
        public bool CanSubmit { get; }

        /// <summary>
        /// 式の左側の数値が、選択肢の中で何番目かを0始まりで取得します。
        /// </summary>
        public int LeftIndex { get; }

        /// <summary>
        /// 式の演算子が、選択肢の中で何番目かを0始まりで取得します。
        /// </summary>
        public int OperatorIndex { get; }

        /// <summary>
        /// 式の右側の数値が、選択肢の中で何番目かを0始まりで取得します。
        /// </summary>
        public int RightIndex { get; }

        /// <summary>
        /// 数値の選択肢の総数を取得します。
        /// </summary>
        public int NumberChoiceCount { get; }

        /// <summary>
        /// 演算子の選択肢の総数を取得します。
        /// </summary>
        public int OperatorChoiceCount { get; }

        /// <summary>
        /// 指定された表示値からGameplay画面の表示状態を生成します。
        /// </summary>
        /// <param name="gameModeText">ゲームモード表示文言。</param>
        /// <param name="difficultyText">難易度表示文言。</param>
        /// <param name="section">現在のSection。</param>
        /// <param name="life">残りLife。</param>
        /// <param name="missionText">ミッションの表示文言。</param>
        /// <param name="leftText">式の左側の表示文言。</param>
        /// <param name="operatorText">式の演算子の表示文言。</param>
        /// <param name="rightText">式の右側の表示文言。</param>
        /// <param name="leftIndex">式の左側の数値の、選択肢の中での位置。</param>
        /// <param name="operatorIndex">式の演算子の、選択肢の中での位置。</param>
        /// <param name="rightIndex">式の右側の数値の、選択肢の中での位置。</param>
        /// <param name="numberChoiceCount">数値の選択肢の総数。</param>
        /// <param name="operatorChoiceCount">演算子の選択肢の総数。</param>
        /// <param name="answerText">答えの表示文言。</param>
        /// <param name="hasRemainingTime">残り時間が有効かどうか。</param>
        /// <param name="remainingSeconds">残り秒数。</param>
        /// <param name="sectionDurationSeconds">現在Sectionの制限時間。</param>
        /// <param name="isTimeWarning">時間警告中かどうか。</param>
        /// <param name="canSubmit">式を提出できるかどうか。</param>
        /// <exception cref="ArgumentNullException">文字列引数のいずれかがnullの場合。</exception>
        /// <exception cref="ArgumentOutOfRangeException">残り秒数が不正な場合。</exception>
        public GameplayViewStateDto(
            string gameModeText,
            string difficultyText,
            int section,
            int life,
            string missionText,
            string leftText,
            string operatorText,
            string rightText,
            int leftIndex,
            int operatorIndex,
            int rightIndex,
            int numberChoiceCount,
            int operatorChoiceCount,
            string answerText,
            bool hasRemainingTime,
            float remainingSeconds,
            float sectionDurationSeconds,
            bool isTimeWarning,
            bool canSubmit)
        {
            if (gameModeText == null)
            {
                throw new ArgumentNullException(nameof(gameModeText));
            }

            if (difficultyText == null)
            {
                throw new ArgumentNullException(nameof(difficultyText));
            }

            if (missionText == null)
            {
                throw new ArgumentNullException(nameof(missionText));
            }

            if (leftText == null)
            {
                throw new ArgumentNullException(nameof(leftText));
            }

            if (operatorText == null)
            {
                throw new ArgumentNullException(nameof(operatorText));
            }

            if (rightText == null)
            {
                throw new ArgumentNullException(nameof(rightText));
            }

            if (answerText == null)
            {
                throw new ArgumentNullException(nameof(answerText));
            }

            if (float.IsNaN(remainingSeconds) ||
                float.IsInfinity(remainingSeconds) ||
                remainingSeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(remainingSeconds),
                    remainingSeconds,
                    "Remaining seconds must be finite and zero or positive.");
            }

            if (float.IsNaN(sectionDurationSeconds) ||
                float.IsInfinity(sectionDurationSeconds) ||
                sectionDurationSeconds <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(sectionDurationSeconds),
                    sectionDurationSeconds,
                    "Section duration must be finite and positive.");
            }

            GameModeText = gameModeText;
            DifficultyText = difficultyText;
            Section = section;
            Life = life;
            MissionText = missionText;
            LeftText = leftText;
            OperatorText = operatorText;
            RightText = rightText;
            LeftIndex = leftIndex;
            OperatorIndex = operatorIndex;
            RightIndex = rightIndex;
            NumberChoiceCount = numberChoiceCount;
            OperatorChoiceCount = operatorChoiceCount;
            AnswerText = answerText;
            HasRemainingTime = hasRemainingTime;
            RemainingSeconds = hasRemainingTime ? remainingSeconds : 0f;
            SectionDurationSeconds = sectionDurationSeconds;
            IsTimeWarning = isTimeWarning;
            CanSubmit = canSubmit;
        }
    }
}

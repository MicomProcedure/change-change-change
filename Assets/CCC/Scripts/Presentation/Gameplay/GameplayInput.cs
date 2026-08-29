using System;

namespace Ccc.Presentation
{
    /// <summary>
    /// Gameplay画面からPresenterへ通知する入力の種類です。
    /// </summary>
    public enum GameplayInputKind
    {
        /// <summary>
        /// 左側の数字変更を要求します。
        /// </summary>
        ChangeLeftNumber = 0,

        /// <summary>
        /// 左側の演算子変更を要求します。
        /// </summary>
        ChangeLeftOperator,

        /// <summary>
        /// 右側の数字変更を要求します。
        /// </summary>
        ChangeRightNumber,

        /// <summary>
        /// 右側の演算子変更を要求します。
        /// </summary>
        ChangeRightOperator,

        /// <summary>
        /// 現在の式の提出を要求します。
        /// </summary>
        Submit,

        /// <summary>
        /// MainMenuへの復帰を要求します。
        /// </summary>
        ReturnToMainMenu
    }

    /// <summary>
    /// Gameplay画面で発生した、意味付け済みの入力事実です。
    /// </summary>
    public readonly struct GameplayInput
    {
        /// <summary>
        /// 入力の種類を取得します。
        /// </summary>
        public GameplayInputKind Kind { get; }

        /// <summary>
        /// 指定された種類のGameplay入力を生成します。
        /// </summary>
        /// <param name="kind">入力の種類。</param>
        /// <exception cref="ArgumentOutOfRangeException">未定義の入力種類の場合。</exception>
        public GameplayInput(GameplayInputKind kind)
        {
            if (!Enum.IsDefined(typeof(GameplayInputKind), kind))
            {
                throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
            }

            Kind = kind;
        }
    }
}

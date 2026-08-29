using System;

namespace Ccc.Presentation.Navigation
{
    /// <summary>
    /// Scene遷移の結果を表します。
    /// </summary>
    public sealed class SceneNavigationResult
    {
        /// <summary>
        /// SceneNavigationResultを生成します。
        /// </summary>
        /// <param name="isSuccess">遷移に成功したかどうか。</param>
        /// <param name="failureKind">失敗種別。</param>
        /// <param name="errorMessage">エラーメッセージ。</param>
        /// <param name="cause">発生原因のException。</param>
        internal SceneNavigationResult(
            bool isSuccess,
            SceneNavigationFailureKind failureKind,
            string errorMessage,
            Exception cause)
        {
            if (errorMessage == null)
            {
                throw new ArgumentNullException(nameof(errorMessage));
            }

            if (isSuccess)
            {
                if (failureKind != SceneNavigationFailureKind.None ||
                    errorMessage.Length != 0 ||
                    cause != null)
                {
                    throw new ArgumentException(
                        "成功Resultの値が不正です。",
                        nameof(isSuccess));
                }
            }
            else
            {
                if (failureKind == SceneNavigationFailureKind.None ||
                    errorMessage.Length == 0)
                {
                    throw new ArgumentException(
                        "失敗Resultの値が不正です。",
                        nameof(failureKind));
                }

                if (failureKind ==
                    SceneNavigationFailureKind.AlreadyLoading &&
                    cause != null)
                {
                    throw new ArgumentException(
                        "AlreadyLoadingではCauseを保持できません。",
                        nameof(cause));
                }

                if (failureKind ==
                    SceneNavigationFailureKind.LoadFailed &&
                    cause == null)
                {
                    throw new ArgumentNullException(nameof(cause));
                }
            }

            IsSuccess = isSuccess;
            FailureKind = failureKind;
            ErrorMessage = errorMessage;
            Cause = cause;
        }

        /// <summary>
        /// 遷移に成功したかどうかを取得します。
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// 失敗種別を取得します。
        /// </summary>
        public SceneNavigationFailureKind FailureKind { get; }

        /// <summary>
        /// エラーメッセージを取得します。
        /// </summary>
        public string ErrorMessage { get; }

        /// <summary>
        /// 発生原因のExceptionを取得します。
        /// </summary>
        public Exception Cause { get; }
    }
}
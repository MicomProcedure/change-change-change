using System;
using Ccc.Core.Model;

namespace Ccc.Core.UseCases
{
    /// <summary>
    /// Gameplayセッション開始の成否と生成値を保持するResultです。
    /// </summary>
    public sealed class GameplayStartResult
    {
        private readonly GameplaySessionContext _context;
        private readonly GameplayState _initialState;
        private readonly GameplayFlowFailure _failure;

        /// <summary>
        /// セッション開始に成功したかどうかを取得します。
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// 構築済みのセッションコンテキストを取得します。
        /// </summary>
        /// <exception cref="InvalidOperationException">失敗Resultの場合。</exception>
        public GameplaySessionContext Context
        {
            get
            {
                if (!IsSuccess || _context == null)
                {
                    throw new InvalidOperationException("Context is available only on a successful start result.");
                }

                return _context;
            }
        }

        /// <summary>
        /// 初期Gameplay状態を取得します。
        /// </summary>
        /// <exception cref="InvalidOperationException">失敗Resultの場合。</exception>
        public GameplayState InitialState
        {
            get
            {
                if (!IsSuccess || _initialState == null)
                {
                    throw new InvalidOperationException("InitialState is available only on a successful start result.");
                }

                return _initialState;
            }
        }

        /// <summary>
        /// セッション開始失敗の詳細を取得します。
        /// </summary>
        /// <exception cref="InvalidOperationException">成功Resultの場合。</exception>
        public GameplayFlowFailure Failure
        {
            get
            {
                if (IsSuccess || _failure == null)
                {
                    throw new InvalidOperationException("Failure is available only on a failed start result.");
                }

                return _failure;
            }
        }

        private GameplayStartResult(
            bool isSuccess,
            GameplaySessionContext context,
            GameplayState initialState,
            GameplayFlowFailure failure)
        {
            IsSuccess = isSuccess;
            _context = context;
            _initialState = initialState;
            _failure = failure;
        }

        /// <summary>
        /// 成功Resultを生成します。
        /// </summary>
        /// <param name="context">構築済みのセッションコンテキスト。</param>
        /// <param name="initialState">初期Gameplay状態。</param>
        /// <returns>成功Result。</returns>
        internal static GameplayStartResult Success(
            GameplaySessionContext context,
            GameplayState initialState)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (initialState == null)
            {
                throw new ArgumentNullException(nameof(initialState));
            }

            return new GameplayStartResult(true, context, initialState, null);
        }

        /// <summary>
        /// 失敗Resultを生成します。
        /// </summary>
        /// <param name="failure">失敗理由。</param>
        /// <returns>失敗Result。</returns>
        internal static GameplayStartResult FailureResult(GameplayFlowFailure failure)
        {
            return new GameplayStartResult(
                false,
                null,
                null,
                failure ?? throw new ArgumentNullException(nameof(failure)));
        }
    }
}

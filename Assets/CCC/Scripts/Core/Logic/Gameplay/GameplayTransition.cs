using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ccc.Core.Model;

namespace Ccc.Core.Logic
{
    /// <summary>
    /// Command に対する次状態、発生 Event、受理結果を保持する不変の遷移結果です。
    /// </summary>
    public sealed class GameplayTransition
    {
        /// <summary>
        /// 遷移後に公開する状態を取得します。
        /// </summary>
        public GameplayState NextState { get; }

        /// <summary>
        /// 遷移で発生した Event を発生順に取得します。
        /// </summary>
        public IReadOnlyList<GameplayEvent> Events { get; }

        /// <summary>
        /// Command が受理されたかを取得します。
        /// </summary>
        public bool IsAccepted { get; }

        private GameplayTransition(
            GameplayState nextState,
            IReadOnlyList<GameplayEvent> events,
            bool isAccepted)
        {
            NextState = nextState;
            Events = events;
            IsAccepted = isAccepted;
        }

        /// <summary>
        /// 受理された遷移を生成します。
        /// </summary>
        /// <param name="nextState">遷移後の状態。</param>
        /// <param name="events">発生順に並んだ Event。</param>
        /// <returns>受理済みの遷移。</returns>
        internal static GameplayTransition Accepted(
            GameplayState nextState,
            params GameplayEvent[] events)
        {
            if (nextState == null)
            {
                throw new ArgumentNullException(nameof(nextState));
            }

            if (events == null)
            {
                throw new ArgumentNullException(nameof(events));
            }

            var eventSnapshot = new GameplayEvent[events.Length];
            for (int i = 0; i < events.Length; i++)
            {
                eventSnapshot[i] = events[i] ?? throw new ArgumentException(
                    "Events cannot contain null elements.",
                    nameof(events));
            }

            IReadOnlyList<GameplayEvent> readOnlyEvents =
                new ReadOnlyCollection<GameplayEvent>(eventSnapshot);
            return new GameplayTransition(nextState, readOnlyEvents, true);
        }

        /// <summary>
        /// 状態を変更しない未受理の遷移を生成します。
        /// </summary>
        /// <param name="currentState">現在の状態。</param>
        /// <returns>未受理の遷移。</returns>
        internal static GameplayTransition Ignored(GameplayState currentState)
        {
            if (currentState == null)
            {
                throw new ArgumentNullException(nameof(currentState));
            }

            IReadOnlyList<GameplayEvent> emptyEvents =
                new ReadOnlyCollection<GameplayEvent>(Array.Empty<GameplayEvent>());
            return new GameplayTransition(currentState, emptyEvents, false);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ccc.Core.Model;

namespace Ccc.Core.UseCases
{
    /// <summary>
    /// Gameplayセッション開始に必要なミッションパラメータを保持する不変のRequestです。
    /// </summary>
    public sealed class GameplayStartRequest
    {
        private readonly IReadOnlyList<MissionParameterCandidate> _missionParameterCandidates;

        /// <summary>
        /// Sessionで選択されたゲームモードを取得します。
        /// </summary>
        public GameMode GameMode { get; }

        /// <summary>
        /// Sessionで選択された難易度を取得します。
        /// </summary>
        public GameDifficulty GameDifficulty { get; }

        /// <summary>
        /// カタログ構築に使用するミッションパラメータ候補を取得します。
        /// </summary>
        public IReadOnlyList<MissionParameterCandidate> MissionParameterCandidates => _missionParameterCandidates;

        /// <summary>
        /// ゲームモードと難易度から候補未設定のセッション開始Requestを生成します。
        /// 候補はGameplayEntryPointがAsset境界から補完します。
        /// </summary>
        /// <param name="gameMode">選択されたゲームモード。</param>
        /// <param name="gameDifficulty">選択された難易度。</param>
        public GameplayStartRequest(GameMode gameMode, GameDifficulty gameDifficulty)
            : this(gameMode, gameDifficulty, Array.Empty<MissionParameterCandidate>())
        {
        }

        /// <summary>
        /// ゲームモード、難易度、ミッションパラメータ候補からRequestを生成します。
        /// </summary>
        /// <param name="gameMode">選択されたゲームモード。</param>
        /// <param name="gameDifficulty">選択された難易度。</param>
        /// <param name="missionParameterCandidates">カタログ構築に使用する候補。</param>
        /// <exception cref="ArgumentNullException">候補一覧がnullの場合。</exception>
        /// <exception cref="ArgumentOutOfRangeException">未定義のモードまたは難易度の場合。</exception>
        public GameplayStartRequest(
            GameMode gameMode,
            GameDifficulty gameDifficulty,
            IReadOnlyList<MissionParameterCandidate> missionParameterCandidates)
        {
            if (!Enum.IsDefined(typeof(GameMode), gameMode))
            {
                throw new ArgumentOutOfRangeException(nameof(gameMode), gameMode, null);
            }

            if (!Enum.IsDefined(typeof(GameDifficulty), gameDifficulty))
            {
                throw new ArgumentOutOfRangeException(nameof(gameDifficulty), gameDifficulty, null);
            }

            if (missionParameterCandidates == null)
            {
                throw new ArgumentNullException(nameof(missionParameterCandidates));
            }

            var snapshot = new MissionParameterCandidate[missionParameterCandidates.Count];
            for (int index = 0; index < missionParameterCandidates.Count; index++)
            {
                snapshot[index] = missionParameterCandidates[index];
            }

            _missionParameterCandidates = new ReadOnlyCollection<MissionParameterCandidate>(snapshot);
            GameMode = gameMode;
            GameDifficulty = gameDifficulty;
        }

        /// <summary>
        /// Normal・LifeEndlessの候補付きRequestを生成します。
        /// </summary>
        /// <param name="missionParameterCandidates">カタログ構築に使用する候補。</param>
        /// <exception cref="ArgumentNullException">候補一覧がnullの場合。</exception>
        public GameplayStartRequest(IReadOnlyList<MissionParameterCandidate> missionParameterCandidates)
            : this(GameMode.LifeEndless, GameDifficulty.Normal, missionParameterCandidates)
        {
        }
    }
}

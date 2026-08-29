namespace Ccc.Core.UseCases
{
    /// <summary>
    /// Gameplayで提示するMission候補を選出するポートです。
    /// </summary>
    public interface IMissionDrawStrategy
    {
        /// <summary>
        /// Contextに応じてMission候補を選出します。
        /// </summary>
        /// <param name="context">抽選Context。</param>
        /// <returns>抽選結果。</returns>
        MissionDrawResult Draw(MissionDrawContext context);
    }
}

namespace Ccc.Presentation.Audio
{
    /// <summary>
    ///     サウンドの識別キー。
    /// </summary>
    public enum AudioKey
    {
        None = 0,

        BgmTitle = 100,
        BgmGameplayNormal = 101,
        BgmResult = 102,

        SEChangeClick = 200,
        SEMissionPresented = 201,
        SENormalClick = 202,
        SECorrect = 203,
        SEWrong = 204,
        SETimeClock = 205,
        SEGameOver = 206,

        BgmGameplayDifficult = 103,

        SECancelClick = 207,
        SECountdown = 208,
        SEHizimere = 209,
        SESubmit = 210,
    }
}

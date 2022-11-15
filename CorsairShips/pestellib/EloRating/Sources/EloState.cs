using System;
using MessagePack;

namespace EloRating
{
    [Serializable]
    [MessagePackObject]
    public class EloPlayer
    {
        [Key(1)]
        public int Rating;

        [Key(2)]
        public int PlayedGames;
    }

    public enum EloRatingGameResult
    {
        GameResultDraw = 0,
        GameResultWin = 1,
        GameResultLoose = 2,
        GameResultNone = 3
    }
}

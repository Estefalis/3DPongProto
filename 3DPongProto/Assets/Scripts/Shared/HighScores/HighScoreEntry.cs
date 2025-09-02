using System;

namespace ThreeDeePongProto.Shared.Highscores
{
    [Serializable]
    public class HighScoreEntry
    {
        //MatchResult Data
        public string WinningPlayerName;
        public int TotalPoints;
        public float TotalPlaytime;         //Seconds for correct sorting
        public long MatchWinTimestamp;      //Ticks for correct sorting

        //Filter-Context for Matches
        public EGameMode GameMode;
        public int SetRounds;
        public int SetPointsEachRound;
    }
}
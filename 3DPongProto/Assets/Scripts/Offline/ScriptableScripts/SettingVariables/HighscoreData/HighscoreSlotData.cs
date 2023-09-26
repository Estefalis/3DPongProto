using System;

namespace ThreeDeePongProto.Offline.Highscores
{
    [Serializable]
    public struct HighscoreSlotData
    {
        //Details-Structure for each listed Object.
        public int SetMaxRounds;
        public int SetMaxPoints;
        public double TotalPoints;
        public string WinningPlayer;
        public string MatchWinDate;
        public float TotalPlaytime;
    }
}
using System;

namespace ThreeDeePongProto.Offline.Highscores
{
    [Serializable]
    public struct HighscoreEntryData
    {
        //Details-Structure for each listed Object.
        public int SetMaxRounds;
        public int SetMaxPoints;
        public double TotalPoints;
        public string WinningPlayer;
        public string MatchWinDate;
        public float TotalPlaytime;

        public HighscoreEntryData(int _rounds, int _maxPoints, double _totalPoints, string _winningPlayer, string _winDate, float _totalPlaytime)
        {
            SetMaxRounds = _rounds;
            SetMaxPoints = _maxPoints;
            TotalPoints = _totalPoints;
            WinningPlayer = _winningPlayer;
            MatchWinDate = _winDate;
            TotalPlaytime = _totalPlaytime;
        } 
    }
}
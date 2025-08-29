using System;

namespace ThreeDeePongProto.Shared.Highscores
{
    [Serializable]
    public struct HighScoreEntryData
    {
        //Details-Structure for each listed Object.
        public string WinningPlayer;
        public int SetMaxRounds;
        public int SetMaxPoints;
        public double TotalPoints;
        public string MatchWinDate;
        public float TotalPlaytime;

        public HighScoreEntryData(string _winningPlayer, int _rounds, int _maxPoints, double _totalPoints, string _winDate, float _totalPlaytime)
        {
            WinningPlayer = _winningPlayer;
            SetMaxRounds = _rounds;
            SetMaxPoints = _maxPoints;
            TotalPoints = _totalPoints;
            MatchWinDate = _winDate;
            TotalPlaytime = _totalPlaytime;
        } 
    }
}
using System;

namespace ThreeDeePongProto.Shared.Highscores
{
    [Serializable]
    public class HighScoreEntry
    {
        //MatchResult Data
        public string WinningPlayerName;
        public int TotalPoints;
        public float TotalPlaytimeSeconds;  //Seconds for correct sorting
        public long MatchWinTimestamp;      //Ticks for correct sorting

        //Filter-Context for Matches
        public EGameMode GameMode;
        public int RoundsSetting;
        public int PointsSetting;

        ////Details-Structure for each listed Object.
        //public string WinningPlayer;
        //public int SetMaxRounds;
        //public int SetMaxPoints;
        //public double TotalPoints;
        //public string MatchWinDate;
        //public float TotalPlaytime;

        //public HighScoreEntry(string _winningPlayer, int _rounds, int _maxPoints, double _totalPoints, string _winDate, float _totalPlaytime)
        //{
        //    WinningPlayer = _winningPlayer;
        //    SetMaxRounds = _rounds;
        //    SetMaxPoints = _maxPoints;
        //    TotalPoints = _totalPoints;
        //    MatchWinDate = _winDate;
        //    TotalPlaytime = _totalPlaytime;
        //} 
    }
}
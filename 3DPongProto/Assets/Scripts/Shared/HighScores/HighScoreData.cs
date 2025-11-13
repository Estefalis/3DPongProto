using System;
using System.Collections.Generic;

namespace ThreeDeePongProto.Shared.Highscores
{
    [Serializable]
    public class HighScoreData
    {
        public List<HighScoreEntry> highScores = new List<HighScoreEntry>();
    }
}
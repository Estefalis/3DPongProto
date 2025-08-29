using System;
using System.Collections.Generic;

namespace ThreeDeePongProto.Shared.Highscores
{
    [Serializable]
    public class HighScoreList
    {
        public List<HighScoreEntryData> highscores = new();
    }
}
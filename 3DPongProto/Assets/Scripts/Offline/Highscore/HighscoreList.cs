using System;
using System.Collections.Generic;

namespace ThreeDeePongProto.Offline.Highscores
{
    [Serializable]
    public class HighscoreList
    {
        public List<HighscoreEntryData> highscores = new();
    }
}
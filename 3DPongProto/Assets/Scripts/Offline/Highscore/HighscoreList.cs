using System;
using System.Collections.Generic;

namespace ThreeDeePongProto.Shared.Highscores
{
    [Serializable]
    public class HighscoreList
    {
        public List<HighscoreEntryData> highscores = new();
    }
}
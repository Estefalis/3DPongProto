using System;
using System.Collections.Generic;

namespace ThreeDeePongProto.Offline.Highscores
{
    [Serializable]
    public class HighscoreListData
    {
        public List<HighscoreSlotData> highscores = new();
    }
}
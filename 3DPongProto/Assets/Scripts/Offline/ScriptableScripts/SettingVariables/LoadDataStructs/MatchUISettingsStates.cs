using System;

[Serializable]
public struct MatchUISettingsStates
{
    public bool InfiniteRounds;
    public bool InfinitePoints;
    public int LastRoundDdIndex;
    public int LastMaxPointDdIndex;
    //public int SetMaxRounds;
    //public int SetMaxPoints;
    public bool FixRatio;
    public int LastFieldWidthDdIndex;
    public int LastFieldLengthDdIndex;
    public int TPOneBacklineDdIndex;
    public int TPTwoBacklineDdIndex;
    public int TPOneFrontlineDdIndex;
    public int TPTwoFrontlineDdIndex;
}
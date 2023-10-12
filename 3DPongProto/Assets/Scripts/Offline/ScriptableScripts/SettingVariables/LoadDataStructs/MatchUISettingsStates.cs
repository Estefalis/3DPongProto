using System;

[Serializable]
public struct MatchUISettingsStates
{
    public bool InfiniteMatch;
    public int LastRoundDdIndex;
    public int LastMaxPointDdIndex;
    public int MaxRounds;
    public int MaxPoints;
    public bool FixRatio;
    public int LastFieldWidthDdIndex;
    public int LastFieldLengthDdIndex;
    public int TPOneBacklineDdIndex;
    public int TPTwoBacklineDdIndex;
    public int TPOneFrontlineDdIndex;
    public int TPTwoFrontlineDdIndex;
}
using System;

[Serializable]
public struct MatchUISettingsStates
{
    public int LastRoundDdIndex;
    public bool InfiniteRounds;
    public int LastMaxPointDdIndex;
    public bool InfinitePoints;
    public int LastFieldWidthDdIndex;
    public int LastFieldLengthDdIndex;
    public bool FixRatio;
    public int TPOneFrontDdIndex;
    public int TPTwoFrontDdIndex;
    public int TPOneBacklineIndex;
    public int TPTwoBacklineIndex;
}
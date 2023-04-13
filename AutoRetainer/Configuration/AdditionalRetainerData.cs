namespace AutoRetainer.Configuration;

[Serializable]
public class AdditionalRetainerData
{
    public bool EntrustDuplicates = false;
    public bool WithdrawGil = false;
    public int WithdrawGilPercent = 100;
    public bool Deposit = false;
    public VenturePlan VenturePlan = new();
    public uint VenturePlanIndex = 0;
    public bool EnablePlanner = false;
    public int Ilvl = 0;
    public int Gathering = 0;
}

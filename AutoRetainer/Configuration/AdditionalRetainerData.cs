namespace AutoRetainer.Configuration;

[Serializable]
public class AdditionalRetainerData
{
    public bool EntrustDuplicates = false;
    public bool WithdrawGil = false;
    public int WithdrawGilPercent = 100;
}

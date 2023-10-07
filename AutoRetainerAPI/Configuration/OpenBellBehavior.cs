using System.Reflection;

namespace AutoRetainerAPI.Configuration;

[Obfuscation(Exclude = true, ApplyToMembers = true)]
public enum OpenBellBehavior
{
    Do_nothing,
    Enable_AutoRetainer,
    Disable_AutoRetainer,
    Pause_AutoRetainer,
}

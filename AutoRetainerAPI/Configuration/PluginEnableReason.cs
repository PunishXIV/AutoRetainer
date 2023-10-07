using System.Reflection;

namespace AutoRetainerAPI.Configuration;

[Obfuscation(Exclude = true, ApplyToMembers = true)]
public enum PluginEnableReason
{
    Access, Manual, Auto, MultiMode, Artisan
}

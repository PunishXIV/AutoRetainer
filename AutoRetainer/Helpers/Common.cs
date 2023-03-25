using FFXIVClientStructs.FFXIV.Component.GUI;
using ValueType = FFXIVClientStructs.FFXIV.Component.GUI.ValueType;

namespace AutoRetainer.Helpers;

internal unsafe static class Common
{
    public const int UnitListCount = 18;
    public static AtkUnitBase* GetAddonByID(uint id)
    {
        return AtkStage.GetSingleton()->RaptureAtkUnitManager->GetAddonById((ushort)id);
    }

    public static string ValueString(this AtkValue v)
    {
        return v.Type switch
        {
            ValueType.Int => $"{v.Int}",
            ValueType.String => Marshal.PtrToStringUTF8(new IntPtr(v.String)),
            ValueType.UInt => $"{v.UInt}",
            ValueType.Bool => $"{v.Byte != 0}",
            ValueType.Float => $"{v.Float}",
            ValueType.Vector => "[Vector]",
            ValueType.AllocatedString => Marshal.PtrToStringUTF8(new IntPtr(v.String))?.TrimEnd('\0') ?? string.Empty,
            ValueType.AllocatedVector => "[Allocated Vector]",
            _ => $"Unknown Type: {v.Type}"
        };
    }

    public static void GenerateCallback(AtkUnitBase* unitBase, params object[] values)
    {
        var atkValues = CreateAtkValueArray(values);
        if (atkValues == null) return;
        try
        {
            unitBase->FireCallback(values.Length, atkValues);
        }
        finally
        {
            for (var i = 0; i < values.Length; i++)
            {
                if (atkValues[i].Type == ValueType.String)
                {
                    Marshal.FreeHGlobal(new IntPtr(atkValues[i].String));
                }
            }
            Marshal.FreeHGlobal(new IntPtr(atkValues));
        }
    }

    public static AtkValue* CreateAtkValueArray(params object[] values)
    {
        var atkValues = (AtkValue*)Marshal.AllocHGlobal(values.Length * sizeof(AtkValue));
        if (atkValues == null) return null;
        try
        {
            for (var i = 0; i < values.Length; i++)
            {
                var v = values[i];
                switch (v)
                {
                    case uint uintValue:
                        atkValues[i].Type = ValueType.UInt;
                        atkValues[i].UInt = uintValue;
                        break;
                    case int intValue:
                        atkValues[i].Type = ValueType.Int;
                        atkValues[i].Int = intValue;
                        break;
                    case float floatValue:
                        atkValues[i].Type = ValueType.Float;
                        atkValues[i].Float = floatValue;
                        break;
                    case bool boolValue:
                        atkValues[i].Type = ValueType.Bool;
                        atkValues[i].Byte = (byte)(boolValue ? 1 : 0);
                        break;
                    case string stringValue:
                        {
                            atkValues[i].Type = ValueType.String;
                            var stringBytes = Encoding.UTF8.GetBytes(stringValue);
                            var stringAlloc = Marshal.AllocHGlobal(stringBytes.Length + 1);
                            Marshal.Copy(stringBytes, 0, stringAlloc, stringBytes.Length);
                            Marshal.WriteByte(stringAlloc, stringBytes.Length, 0);
                            atkValues[i].String = (byte*)stringAlloc;
                            break;
                        }
                    default:
                        throw new ArgumentException($"Unable to convert type {v.GetType()} to AtkValue");
                }
            }
        }
        catch
        {
            return null;
        }

        return atkValues;
    }

}

using Dalamud.Memory;

namespace AutoRetainer.UI.Dbg;

internal static unsafe class DebugVenture
{
    internal static void Draw()
    {
        if (ImGui.CollapsingHeader("Ventures"))
        {
            var data = CSFramework.Instance()->UIModule->GetRaptureAtkModule()->AtkModule.GetStringArrayData(95);
            if(data != null)
            {
                for (int i = 0; i < data->AtkArrayData.Size; i++)
                {
                    var item = data->StringArray[i];
                    if(item != null)
                    {
                        var str = MemoryHelper.ReadSeStringNullTerminated((nint)item);
                        ImGuiEx.Text($"{i}: {str.ExtractText()}");
                    }
                    else
                    {
                        ImGuiEx.Text($"{i}: null");
                    }
                }
            }
        }
    }
}

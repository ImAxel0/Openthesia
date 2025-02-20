using ImGuiNET;

namespace Openthesia.Ui.Helpers;

public class AutoFont : IDisposable
{
    public AutoFont(ImFontPtr font)
    {
        ImGui.PushFont(font);
    }

    public void Dispose()
    {
        ImGui.PopFont();
    }
}

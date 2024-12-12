using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Openthesia;

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Openthesia.Settings;

public static class PluginsPathManager
{
    public static string InstrumentPath { get; private set; } = string.Empty;
    public static List<string> EffectsPath { get; private set; } = new();

    public static void SetInstrumentPath(string instrumentPath)
    {
        InstrumentPath = instrumentPath;
    }

    public static void SetEffectsPath(List<string> paths)
    {
        if (paths.Count == 0)
            return;

        EffectsPath = paths;
    }
}

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

    public static void LoadValidInstrumentPath(string instrumentPath)
    {
        if (File.Exists(instrumentPath) && Path.GetExtension(instrumentPath) == ".dll")
            InstrumentPath = instrumentPath;
    }

    public static void LoadValidEffectsPath(List<string> paths)
    {
        foreach (var filePath in paths)
        {
            if (File.Exists(filePath) && Path.GetExtension(filePath) == ".dll" && !EffectsPath.Contains(filePath))
                EffectsPath.Add(filePath);
        }
    }
}

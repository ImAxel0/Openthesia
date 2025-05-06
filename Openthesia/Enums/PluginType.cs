using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Openthesia.Enums;

/// <summary>
/// The type of the plugin.
/// </summary>
public enum PluginType
{
    /// <summary>
    /// Plugin is an effect. (e.g VST)
    /// </summary>
    Effect,

    /// <summary>
    /// Plugin is an instrument. (e.g VSTi)
    /// </summary>
    Instrument
}

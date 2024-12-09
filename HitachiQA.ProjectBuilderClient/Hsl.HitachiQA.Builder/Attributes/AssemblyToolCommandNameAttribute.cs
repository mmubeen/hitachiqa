using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hsl.HitachiQA.Builder.Attributes;

public sealed class AssemblyToolCommandNameAttribute : Attribute
{
    public string ToolCommandName { get; init; }
    public AssemblyToolCommandNameAttribute(string toolCommandName)
    {
        ToolCommandName = toolCommandName;
    }
}

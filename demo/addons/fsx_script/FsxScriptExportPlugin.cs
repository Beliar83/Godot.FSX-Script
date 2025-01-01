#if TOOLS
using Godot;
using Godot.Collections;

namespace FsxScript;

public partial class FsxScriptExportPlugin : EditorExportPlugin
{
    private ulong hash;

    /// <inheritdoc />
    public override string _GetName()
    {
        return "Fsx";
    }

    /// <inheritdoc />
    public override void _ExportBegin(string[] features, bool isDebug, string path, uint flags)
    {
        ConfigFile configFile = new();
        Dictionary<string, string> dictionary = new() { { "res://node.fsx", "res://TestNode.cs" } };
        configFile.SetValue("Scripts", "Mapping", dictionary);
        configFile.Save("res://fsx_script.ini");
        AddFile("res://fsx_script.ini", FileAccess.GetFileAsBytes("res://fsx_script.ini"), false);
    }

    /// <inheritdoc />
    public override void _ExportFile(string path, string type, string[] features)
    {
        if (type == "Script" && path.EndsWith(".fsx"))
        {
            Skip();
        }
    }
}
#endif

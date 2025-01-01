#if TOOLS
using System.Threading.Tasks;
using Godot;
using Godot.Collections;

namespace FsxScript;

// ReSharper disable once Godot.MissingParameterlessConstructor
public partial class InteropInstance : GodotObject
{
    internal ScriptData? scriptData = null;

    internal object? state;
    private GodotObject internalObject;

    private Variant CallFsxMethod(StringName methodName, Array<Variant> args)
    {
        while (state is null || scriptData is null)
        {
            Task.Delay(1).Wait();
        }

        object?[] parameters = [internalObject, methodName, state, args];
        Variant result = (Variant)(scriptData.Call.Invoke(null, parameters) ?? new Variant());
        state = parameters[2];
        return result;
    }

    internal InteropInstance(GodotObject godotObject)
    {
        internalObject = godotObject;
    }
}
#endif

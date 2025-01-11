#if TOOLS
using System.Threading.Tasks;
using Godot;
using Godot.Collections;

namespace FsxScriptAddon;

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

    private void SetFsxValue(StringName propertyName, Variant value)
    {
        while (state is null || scriptData is null)
        {
            Task.Delay(1).Wait();
        }

        object?[] parameters = [state, propertyName, value];
        state = scriptData.SetValue.Invoke(null, parameters);
    }

    private Variant GetFsxValue(StringName propertyName)
    {
        while (state is null || scriptData is null)
        {
            Task.Delay(1).Wait();
        }

        object?[] parameters = [state, propertyName];
        Variant variant = (Variant)scriptData.GetValue.Invoke(null, parameters)!;
        return variant;
    }

    internal InteropInstance(GodotObject godotObject)
    {
        internalObject = godotObject;
    }
}
#endif

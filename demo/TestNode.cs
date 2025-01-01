using System.Diagnostics;
using Godot;

namespace FsxScript;

// ReSharper disable InconsistentNaming
// ReSharper disable once Godot.MissingParameterlessConstructor
public partial class TestNode : Sprite2D
{
#if TOOLS
    // private static readonly StringName ScriptName = new("res://node.fsx");
    // private static MethodInfo? _processMethodInfo;
    // private static MethodInfo? setValueMethodInfo;
    // private static MethodInfo? getValueMethod;
    // private static MethodInfo? storeStateMethod;
    // private static MethodInfo? restoreStateMethod;
    // private static MethodInfo? defaultStateMethod;
    // internal object? state;
    // private GodotObject internalObject;
    // private static readonly List<TestNode> scriptInstances = [];
    //
    //
    // private static bool Load(AssemblyLoadContext loadContext, Godot.Collections.Dictionary<GodotObject, Dictionary> scripts)
    // {
    //     const string scriptPath = "res://node.fsx";
    //
    //     string assemblyPath = Path.ChangeExtension(ProjectSettings.GlobalizePath(scriptPath),"dll");
    //     Assembly assembly = loadContext.LoadFromAssemblyPath(assemblyPath);
    //     
    //     Type? stateType = assembly.GetType("TestNode+State");
    //     if (stateType == null)
    //     {
    //         GD.PrintErr("Can't find State type in 'node.fsx'");
    //         HandleError();
    //         return false;
    //     }
    //
    //     Type? testNodeType = assembly.GetType("TestNode");
    //     if (testNodeType == null)
    //     {
    //         GD.PrintErr("Can't find 'TestNode' module in 'node.fsx'");
    //         HandleError();
    //         return false;
    //     }
    //     #region Methods defined in node.fsx
    //
    //     MethodInfo? methodInfo = testNodeType.GetMethod("_process", BindingFlags.Public | BindingFlags.Static, [typeof(Sprite2D), stateType, typeof(float)]);
    //     if (methodInfo == null)
    //     {
    //         GD.PrintErr("Can't find '_process' method in 'node.fsx'");
    //         HandleError();
    //         return false;
    //     }
    //
    //     #endregion
    //         
    //     _processMethodInfo = methodInfo;
    //             
    //     #region Methods added for interop
    //
    //     MethodInfo? setStateValueMethodInfo = testNodeType.GetMethod("__set", BindingFlags.Public | BindingFlags.Static, [stateType, typeof(StringName), typeof(Variant)]);
    //     if (setStateValueMethodInfo == null)
    //     {
    //         GD.PrintErr("Can't find '__set' function in 'node.fsx'");
    //         return false;
    //     }
    //     setValueMethodInfo = setStateValueMethodInfo;
    //
    //     MethodInfo? getStateValueMethodInfo = testNodeType.GetMethod("__get", BindingFlags.Public | BindingFlags.Static, [stateType, typeof(StringName)]);
    //     if (getStateValueMethodInfo == null)
    //     {
    //         GD.PrintErr("Can't find '__get' function in 'node.fsx'");
    //         return false;
    //
    //     }
    //     getValueMethod = getStateValueMethodInfo;
    //
    //         
    //     defaultStateMethod = testNodeType.GetMethod("__get_default_state", BindingFlags.Public | BindingFlags.Static, []);
    //     if (defaultStateMethod == null)
    //     {
    //         GD.PrintErr("Can't find '__get_default_state' function in 'node.fsx'");
    //         return false;
    //
    //     }
    //             
    //             
    //     storeStateMethod = testNodeType.GetMethod("__storeState", BindingFlags.Public | BindingFlags.Static, [stateType]);
    //     if (storeStateMethod == null)
    //     {
    //         GD.PrintErr("Can't find '__storeState' function in 'node.fsx'");
    //         return false;
    //
    //     }
    //
    //     restoreStateMethod = testNodeType.GetMethod("__restoreState", BindingFlags.Public | BindingFlags.Static, [typeof(Dictionary)]);
    //     if (restoreStateMethod == null)
    //     {
    //         GD.PrintErr("Can't find '__restoreState' function in 'node.fsx'");
    //         return false;
    //
    //     }
    //
    //     #endregion
    //
    //     foreach (TestNode instance in scriptInstances)
    //     {
    //         if (scripts.TryGetValue(instance, out Dictionary? stateDict))
    //         {
    //             instance.state = restoreStateMethod.Invoke(null, [stateDict]) ??
    //                                 defaultStateMethod.Invoke(null, null);
    //         }
    //         else
    //         {
    //             instance.state = defaultStateMethod.Invoke(null, null);
    //         }
    //     }
    //     
    //     // if (pendingScripts is null)
    //     // {
    //     //     return null;
    //     // }
    //     //
    //     // while (pendingScripts.TryDequeue(out GodotObject? instance))
    //     // {
    //     //     scriptInstances.Add((TestNode)instance);
    //     // }
    //
    //     return true;
    //     
    //     void HandleError()
    //     {
    //         ResetInteropMethods();
    //     }
    // }
    //
    // private static Godot.Collections.Dictionary<GodotObject, Dictionary> Unload()
    // {
    //     Godot.Collections.Dictionary<GodotObject, Dictionary> scripts = [];
    //     foreach (TestNode instance in scriptInstances)
    //     {
    //
    //         scripts[instance] =
    //             (Dictionary?)storeStateMethod?.Invoke(null, [instance.state]) ??
    //             new Dictionary();
    //         instance.state = null;
    //     }
    //     ResetInteropMethods();
    //
    //     return scripts;
    // }
    //
    // private static void ResetInteropMethods()
    // {
    //     setValueMethodInfo = null;
    //     getValueMethod = null;
    //     _processMethodInfo = null;
    //     storeStateMethod = null;
    //     restoreStateMethod = null;
    //     defaultStateMethod = null;
    // }
    //
    // private static TestNode CreateForObject(GodotObject godotObject)
    // {
    //     return new TestNode(godotObject);
    // }
#else
    private global::TestNode.State state = global::TestNode.State.Default();
#endif


#if TOOLS
    // private TestNode(GodotObject godotObject)
    // {
    //     internalObject = godotObject;
    //     scriptInstances.Add(this);
    //     state = defaultStateMethod?.Invoke(null, null);
    // }
#endif

    [Export]
    private int Value
    {
        get
        {
#if TOOLS
            throw new UnreachableException();
            // return (int)(getValueMethod?.Invoke(null, [state, PropertyName.Value]) ?? 0);
#else
            return state.value;
#endif
        }

        set
        {
#if TOOLS
            // state = setValueMethodInfo?.Invoke(null, [state, PropertyName.Value, value]);
#else
            global::TestNode.__set(state, PropertyName.Value, value);
#endif
        }
    }

    private void _process(float delta)
    {
#if TOOLS
        // state = _processMethodInfo?.Invoke(null, [internalObject, state, delta]) ?? state;
#else
        state = global::TestNode._process(this, state, delta);
#endif
    }

    private void methodWithNodeParam(Node node)
    {
#if TOOLS
#else
        state = global::TestNode.methodWithNodeParam(this, state, node);
#endif
    }

    private void methodThatChangesState(int newValue)
    {
#if TOOLS
#else
        state = global::TestNode.methodThatChangesState(this, state, newValue);
#endif
    }
}
// ReSharper restore InconsistentNaming

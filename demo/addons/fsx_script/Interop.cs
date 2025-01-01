#if TOOLS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Godot;
using Godot.Collections;

namespace FsxScript;

internal record ScriptData(
    AssemblyLoadContext Context,
    MethodInfo SetValue,
    MethodInfo GetValue,
    MethodInfo Call,
    MethodInfo DefaultState,
    MethodInfo StoreState,
    List<InteropInstance> ScriptInstances);

public partial class Interop : GodotObject
{
    private static readonly System.Collections.Generic.Dictionary<StringName, ScriptData> ScriptData = new();

    private static readonly System.Collections.Generic.Dictionary<StringName, Queue<InteropInstance>>
        PendingScriptsToAdd = [];

    public static Godot.Collections.Dictionary<InteropInstance, Dictionary> Unload(string scriptPath,
        out WeakReference? weakReference)
    {
        Godot.Collections.Dictionary<InteropInstance, Dictionary> scripts = [];


        if (ScriptData.TryGetValue(scriptPath,
                out ScriptData? data))
        {
            foreach (InteropInstance instance in data.ScriptInstances)
            {
                if (instance.state is not null)
                {
                    scripts[instance] =
                        (Dictionary)(data.StoreState?.Invoke(null, [instance.state]) ??
                                     new Dictionary());
                    instance.scriptData = null;
                    instance.state = null;
                }
                else
                {
                    scripts[instance] = new Dictionary();
                }
            }

            weakReference = new WeakReference(data.Context, true);
            data.Context.Resolving -= OnLoadContextOnResolving;
            data.Context.Unload();
            ScriptData.Remove(scriptPath);
        }
        else
        {
            weakReference = null;
        }

        return scripts;
    }

    public static WeakReference? Load(string scriptPath, string scriptClassName, string baseTypeName,
        Godot.Collections.Dictionary<InteropInstance, Dictionary> scripts)
    {
        if (ScriptData.ContainsKey(scriptPath))
        {
            GD.PrintErr($"Script {scriptPath} is already loaded");
            return null;
        }

        FsxScriptAssemblyLoadContext loadContext = new();
        loadContext.Resolving += OnLoadContextOnResolving;

        string assemblyPath = Path.ChangeExtension(ProjectSettings.GlobalizePath(scriptPath), "dll");
        Assembly assembly = loadContext.LoadFromAssemblyPath(assemblyPath);

        Type? stateType = assembly.GetType($"{scriptClassName}+State");
        if (stateType == null)
        {
            GD.PrintErr($"Can't find State type in '{scriptPath}'");
            return HandleError();
        }

        Type? baseType = Assembly.GetAssembly(typeof(GodotObject))?.GetType($"Godot.{baseTypeName}");
        if (baseType == null)
        {
            GD.PrintErr($"Can't find Base type in '{scriptPath}'");
            return HandleError();
        }

        Type? scriptType = assembly.GetType(scriptClassName);
        if (scriptType == null)
        {
            GD.PrintErr($"Can't find '{scriptClassName}' module in {scriptPath}'");
            return HandleError();
        }

        #region Methods added for interop

        MethodInfo? setStateValueMethod = scriptType.GetMethod("__set", BindingFlags.Public | BindingFlags.Static,
            [stateType, typeof(StringName), typeof(Variant)]);
        if (setStateValueMethod == null)
        {
            GD.PrintErr($"Can't find '__set' function in '{scriptPath}'");
            return HandleError();
        }

        MethodInfo? getStateValueMethod = scriptType.GetMethod("__get", BindingFlags.Public | BindingFlags.Static,
            [stateType, typeof(StringName)]);
        if (getStateValueMethod == null)
        {
            GD.PrintErr($"Can't find '__get' function in '{scriptPath}'");
            return HandleError();
        }

        MethodInfo? callMethod = scriptType.GetMethod("__call", BindingFlags.Public | BindingFlags.Static,
            [baseType, typeof(StringName), stateType.MakeByRefType(), typeof(Array<Variant>)]);
        if (callMethod == null)
        {
            GD.PrintErr($"Can't find '__call' function in '{scriptPath}'");
            return HandleError();
        }


        MethodInfo? defaultStateMethod =
            scriptType.GetMethod("__get_default_state", BindingFlags.Public | BindingFlags.Static, []);
        if (defaultStateMethod == null)
        {
            GD.PrintErr($"Can't find '__get_default_state' function in '{scriptPath}'");
            return HandleError();
        }


        MethodInfo? storeStateMethod =
            scriptType.GetMethod("__storeState", BindingFlags.Public | BindingFlags.Static, [stateType]);
        if (storeStateMethod == null)
        {
            GD.PrintErr($"Can't find '__storeState' function in '{scriptPath}'");
            return HandleError();
        }

        MethodInfo? restoreStateMethod = scriptType.GetMethod("__restoreState",
            BindingFlags.Public | BindingFlags.Static, [typeof(Dictionary)]);
        if (restoreStateMethod == null)
        {
            GD.PrintErr($"Can't find '__restoreState' function in '{scriptPath}'");
            return HandleError();
        }

        #endregion

        ScriptData scriptData = new(loadContext, setStateValueMethod, getStateValueMethod, callMethod,
            defaultStateMethod, storeStateMethod, scripts.Keys.ToList());
        ScriptData[scriptPath] = scriptData;

        foreach ((InteropInstance? instance, Dictionary? stateDict) in scripts)
        {
            instance.state = restoreStateMethod.Invoke(null, [stateDict]) ??
                             defaultStateMethod.Invoke(null, null);
            instance.scriptData = scriptData;
        }


        if (!PendingScriptsToAdd.TryGetValue(scriptPath, out Queue<InteropInstance>? pendingScripts))
        {
            return null;
        }

        while (pendingScripts.TryDequeue(out InteropInstance? instance))
        {
            AddScriptInstance(scriptPath, instance);
        }

        return null;

        WeakReference HandleError()
        {
            loadContext.Resolving -= OnLoadContextOnResolving;
            WeakReference weakReference = new(loadContext, true);
            loadContext.Unload();
            return weakReference;
        }
    }

    private static Assembly OnLoadContextOnResolving(AssemblyLoadContext _, AssemblyName args)
    {
        return Assembly.Load(args.Name!);
    }

    private static void AddScriptInstance(StringName scriptPath, InteropInstance instance)
    {
        if (ScriptData.TryGetValue(scriptPath, out ScriptData? data))
        {
            instance.scriptData = data;
            instance.state = data.DefaultState.Invoke(null, []);
            data.ScriptInstances.Add(instance);
        }
        else
        {
            if (!PendingScriptsToAdd.TryGetValue(scriptPath, out Queue<InteropInstance>? pendingScripts))
            {
                PendingScriptsToAdd[scriptPath] = pendingScripts = [];
            }

            pendingScripts.Enqueue(instance);
        }
    }

    private static InteropInstance CreateInstanceForObjectAndScript(GodotObject godotObject, StringName scriptPath)
    {
        InteropInstance instance = new(godotObject);
        AddScriptInstance(scriptPath, instance);
        return instance;
    }
}
#endif

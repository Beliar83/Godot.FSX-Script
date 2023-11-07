using Microsoft.CodeAnalysis;

namespace Generators;

public static class Register
{

	public enum Level
	{
		Scene,
		Editor,
	}

	public struct Data
	{
		public string name;
		public string godotName;
		public string @base;
		public string @namespace;
		public Level level;
	}

	public static Data Generate(GeneratorExecutionContext context, INamedTypeSymbol c, bool notification, bool virtualgen, SpecialBase sBase)
	{
		var gdName = (string)(c.GetAttributes().
					SingleOrDefault(x => x.AttributeClass.ToString() == "GodotSharpGDExtension.RegisterAttribute").NamedArguments.
					SingleOrDefault(x => x.Key == "name").Value.Value ?? c.Name
			);
		var isRefCounted = sBase switch
		{
			SpecialBase.Resource => true,
			SpecialBase.RefCounted => true,
			_ => false,
		};

		string registerBase = c.BaseType is not null ? $"\n\t\t{c.BaseType.ContainingNamespace.Name}.{c.BaseType.Name}.Register();" : "";
		
		var assemblyName = context.Compilation.AssemblyName ?? "NoName";
		var entryClassName = $"{assemblyName.Replace('.', '_')}ExtensionEntry";
		
		var source = $$"""
		               using System.Runtime.CompilerServices;
		               using System.Runtime.InteropServices;
		               using GodotSharpGDExtension;
		               using System;

		               using SharpGen.Runtime;
		               
		               namespace {{c.ContainingNamespace}} {
		               public unsafe partial class {{c.Name}} : {{c.BaseType.Name}} {
		               #pragma warning disable CS8618
		               	public static new StringName __godot_name;
		               #pragma warning restore CS8618
		               	private GCHandle handle;
		               #pragma warning disable CS8618
		               	public {{c.Name}}() {
		               		Initialize();
		               		handle = GCHandle.Alloc(this {{(isRefCounted ? ", GCHandleType.Weak" : "")}});
		               		GDExtensionInterface.ObjectSetInstance(internalPointer, __godot_name.internalPointer, handle.AddrOfPinnedObject());
		               	}
		               #pragma warning restore CS8618
		               	public static explicit operator IntPtr({{c.Name}} instance) => instance.handle.AddrOfPinnedObject();
		               	public static explicit operator {{c.Name}}(IntPtr ptr) => ({{c.Name}})(GCHandle.FromIntPtr(ptr).Target!);
		               	public static {{c.Name}} Construct(IntPtr ptr) {
		               		ptr = (IntPtr)((IntPtr*)ptr + 2);
		               		return ({{c.Name}})GodotSharpGDExtension.GodotObject.ConstructUnknown(ptr);
		               	}
		               	public static unsafe new void Register() {	
		               		if (!GodotSharpGDExtension.GodotObject.RegisterConstructor("{{c.Name}}", Construct)) return;{{ registerBase }}
		               		__godot_name = new StringName("{{gdName}}");
		               		var info = new GDExtensionClassCreationInfo {
		               			IsVirtual = System.Convert.ToByte(false),
		               			IsAbstract = System.Convert.ToByte({{c.IsAbstract.ToString().ToLower()}}),
		               			//SetFunc = &SetFunc,
		               			//GetFunc = &GetFunc,
		               			//GetPropertyListFunc = &GetPropertyList,
		               			//FreePropertyListFunc = &FreePropertyList,
		               			//PropertyCanRevertFunc = &PropertyCanConvert,
		               			//PropertyGetRevertFunc = &PropertyGetRevert,
		               			NotificationFunc = {{(notification ? "Engine.IsEditorHint() ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(__Notification)" : "IntPtr.Zero")}},
		               			//ToStringFunc = &ToString,
		               			//ReferenceFunc = &Reference,
		               			//UnreferenceFunc = &Unreference,
		               			CreateInstanceFunc = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr>)(&CreateObject),
		               			FreeInstanceFunc = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, void>)(&FreeObject),
		               			GetVirtualFunc = {{(virtualgen ? " (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr>)(&__RegisterVirtual)" : "IntPtr.Zero")}},
		               			//GetRidFunc = &GetRid,
		               		};
		               		GDExtensionInterface.ClassdbRegisterExtensionClass({{entryClassName}}.Library, __godot_name.internalPointer, {{c.BaseType.Name}}.__godot_name.internalPointer, ref info);
		               		RegisterMethods();
		               		RegisterExports();
		               		RegisterSignals();
		               	}
		               	
		               	[UnmanagedCallersOnly]
		               	static unsafe IntPtr CreateObject(IntPtr userdata) {
		               		var instance = new {{c.Name}}();
		               		return instance.internalPointer;
		               	}
		               	
		               	[UnmanagedCallersOnly]
		               	static unsafe void FreeObject(IntPtr userdata, IntPtr instancePtr) {
		               		var instance = ({{c.Name}})GodotObject.ConstructUnknown(instancePtr);
		               		instance.handle.Free();
		               	}
		               }
		               }
		               """;

		context.AddSource($"{c.Name}.reg.gen.cs", source);

		var editorOnly = (bool)(c.GetAttributes().
					SingleOrDefault(x => x.AttributeClass.ToString() == "GodotSharpGDExtension.RegisterAttribute").NamedArguments.
					SingleOrDefault(x => x.Key == "editorOnly").Value.Value ?? false
			);
		var level = editorOnly switch
		{
			true => Level.Editor,
			false => Level.Scene,
		};
		return new Data()
		{
			name = c.Name,
			godotName = gdName,
			@base = c.BaseType.Name,
			@namespace = c.ContainingNamespace.ToString(),
			level = level,
		};
	}
}
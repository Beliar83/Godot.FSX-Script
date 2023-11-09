using Microsoft.CodeAnalysis;

namespace Generators;

public static class Signal
{

	public static void Generate(GeneratorExecutionContext context, INamedTypeSymbol c)
	{
		var events = c.
			GetMembers().
			OfType<INamedTypeSymbol>().
			Where(x => x.GetAttributes().Any(x => x.AttributeClass.ToString() == "GodotSharpGDExtension.SignalAttribute"))
			.ToArray();

		var code = $$"""
		             using System.Runtime.CompilerServices;
		             using System.Runtime.InteropServices;
		             using GodotSharpGDExtension;

		             namespace {{c.ContainingNamespace}} {
		             public unsafe partial class {{c.Name}} : {{c.BaseType.Name}} {
		             	static unsafe void RegisterSignals() {
		             
		             """;
		var assemblyName = context.Compilation.AssemblyName ?? "NoName";
		var entryClassName = $"{assemblyName.Replace('.', '_')}ExtensionEntry";
		
		for (var i = 0; i < events.Length; i++)
		{
			var ev = events[i];
			var m = ev.DelegateInvokeMethod;

			var infosName = "null";
			if (m.Parameters.Length > 0)
			{
				infosName = $"infos{ev.Name}";
				code += $"\t\tvar {infosName} = new GDExtensionPropertyInfo[{m.Parameters.Length}];\n";
				for (var j = 0; j < m.Parameters.Length; j++)
				{
					var p = m.Parameters[j];
					code += $"\t\tinfos{ev.Name}[{j}] = {Methods.CreatePropertyInfo(p.Type, p.Name, 2)}\n";
				}
			}
			code += $"\t\tGDExtensionInterface.ClassdbRegisterExtensionClassSignal({entryClassName}.Library, __godot_name.internalPointer, new StringName(\"{Renamer.ToSnake(ev.Name)}\").internalPointer, {infosName}, {m.Parameters.Length});\n";
		}
		code += $$"""
		          	}
		          """;

		for (var i = 0; i < events.Length; i++)
		{
			var ev = events[i];
			var m = ev.DelegateInvokeMethod;
			code += $"\tpublic void EmitSignal{ev.Name}(";
			for (var j = 0; j < m.Parameters.Length; j++)
			{
				var p = m.Parameters[j];
				code += $"{p.Type.Name} {p.Name}{(j < m.Parameters.Length - 1 ? ", " : "")}";
			}
			code += $") => EmitSignal(\"{Renamer.ToSnake(ev.Name)}\"";
			for (var j = 0; j < m.Parameters.Length; j++)
			{
				var p = m.Parameters[j];
				code += $", {p.Name}";
			}
			code += ");\n";
		}
		code += $$"""
		          }
		          }
		          """;
		context.AddSource($"{c.Name}.Signal.gen.cs", code);
	}
}
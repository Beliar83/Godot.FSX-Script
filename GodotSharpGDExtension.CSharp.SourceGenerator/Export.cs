using Microsoft.CodeAnalysis;

namespace Generators;

public static class Export
{

	record struct Data(string name, string setter, string getter, ITypeSymbol type);

	public static void Generate(GeneratorExecutionContext context, INamedTypeSymbol c, Methods methods)
	{
		var members = c.GetMembers().
			Where(x => x is IPropertySymbol || x is IFieldSymbol).
			Where(x => x.GetAttributes().Any(x => x.AttributeClass.ToString() == "GodotSharpGDExtension.ExportAttribute")).
			Select(x => x switch {
				IPropertySymbol prop => new Data(prop.Name, prop.SetMethod.Name, prop.GetMethod.Name, prop.Type),
				IFieldSymbol field => new Data(field.Name, "set_" + field.Name, "get_" + field.Name, field.Type),
				_ => throw new System.NotSupportedException(),
			}).
			ToArray();
		var assemblyName = context.Compilation.AssemblyName ?? "NoName";
		var entryClassName = $"{assemblyName.Replace('.', '_')}ExtensionEntry";

		var code = $$"""
		             using System.Runtime.CompilerServices;
		             using System.Runtime.InteropServices;
		             using GodotSharpGDExtension;
		             
		             namespace {{c.ContainingNamespace}} {
		             public unsafe partial class {{c.Name}} : {{c.BaseType.Name}} {
		             	static unsafe void RegisterExports() {
		             	
		             """;

		for (var i = 0; i < members.Length; i++)
		{
			var member = members[i];

			code += $"\t\tvar __{member.name}Info = " + Methods.CreatePropertyInfo(member.type, member.name, 2);

			methods.AddMethod(new Methods.Info
			{
				name = member.setter,
				arguments = new (ITypeSymbol, string)[] { (member.type, "value") },
				ret = null,
				property = member.name,
				propertyType = member.type,
			});
			methods.AddMethod(new Methods.Info
			{
				name = member.getter,
				arguments = new (ITypeSymbol, string)[] { },
				ret = member.type,
				property = member.name,
				propertyType = member.type,
			});
			code += $"""
			          		
			          		GDExtensionInterface.ClassdbRegisterExtensionClassProperty(
			          			{entryClassName}.Library,
			          			__godot_name.internalPointer,
			          			ref __{member.name}Info,
			          			new StringName("{Renamer.ToSnake(member.setter)}").internalPointer,
			          			new StringName("{Renamer.ToSnake(member.getter)}").internalPointer
			          		);
			          """;
		}
		code += """
		        	}
		        }}
		        """;
		context.AddSource($"{c.Name}.export.gen.cs", code);
	}
}
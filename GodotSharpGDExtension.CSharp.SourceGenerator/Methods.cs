using Microsoft.CodeAnalysis;

namespace Generators;

public class Methods
{

	public struct Info
	{
		public string name;
		public (ITypeSymbol, string)[] arguments;
		public ITypeSymbol? ret;
		public string? property;
		public ITypeSymbol? propertyType;
	}

	List<Info> methods;

	public Methods()
	{
		methods = new List<Info>();
	}

	public void AddMethod(Info info)
	{
		methods.Add(info);
	}

	void AddAttributeMethods(GeneratorExecutionContext context, INamedTypeSymbol c)
	{
		var ms = c.
			GetMembers().
			OfType<IMethodSymbol>().
			Where(x => x.GetAttributes().Any(x => x.AttributeClass.ToString() == "GodotSharpGDExtension.MethodAttribute"))
			.ToArray();

		foreach (var method in ms)
		{
			var info = new Info()
			{
				name = method.Name,
				arguments = method.Parameters.Select(x => (x.Type, x.Name)).ToArray(),
				ret = method.ReturnsVoid ? null : method.ReturnType,
				property = null,
				propertyType = null,
			};
			AddMethod(info);
		}
	}

	public void Generate(GeneratorExecutionContext context, INamedTypeSymbol c)
	{
		AddAttributeMethods(context, c);

		var code = $$"""
		             using System.Runtime.CompilerServices;
		             using System.Runtime.InteropServices;
		             using GodotSharpGDExtension;
		             
		             namespace {{c.ContainingNamespace}} {
		             public unsafe partial class {{c.Name}} : {{c.BaseType.Name}} {
		             	static unsafe void RegisterMethods() {
		             """;

		for (var i = 0; i < methods.Count; i++)
		{
			code += "\t\t{\n";
			var method = methods[i];
			if (method.arguments.Length > 0)
			{
				code += $"\t\t\tvar args = stackalloc GDExtensionPropertyInfo[{method.arguments.Length}];\n";
				code += $"\t\t\tvar args_meta = stackalloc GDExtensionClassMethodArgumentMetadata[{method.arguments.Length}];\n";
				for (var j = 0; j < method.arguments.Length; j++)
				{
					var arg = method.arguments[j];
					code += $"\t\t\targs[{j}] = {CreatePropertyInfo(arg.Item1, arg.Item2, 3)}";
					code += $"\t\t\targs_meta[{j}] = GDExtensionClassMethodArgumentMetadata.GdextensionMethodArgumentMetadataNone;\n";
				}
			}
			if (method.ret != null)
			{
				code += $"\t\t\tvar ret = {CreatePropertyInfo(method.ret, "return", 3)}";
			}
			var assemblyName = context.Compilation.AssemblyName ?? "NoName";
			var entryClassName = $"{assemblyName.Replace('.', '_')}ExtensionEntry";
			
			code += $$$"""
			          			var info = new GDExtensionClassMethodInfo {
			          				Name = new StringName("{{{Renamer.ToSnake(method.name)}}}").internalPointer,
			          				MethodUserdata = new IntPtr({{{i}}}),
			          				CallFunc = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr*, uint, IntPtr, GDExtensionCallError*, void>)(&CallFunc),
			          				PtrcallFunc = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr*, IntPtr, void>)(&CallFuncPtr),
			          				MethodFlags = (uint)GDExtensionClassMethodFlags.GdextensionMethodFlagsDefault,
			          				HasReturnValue = System.Convert.ToByte({{{(method.ret != null ? "true" : "false")}}}),
			          				ReturnValueInfo = {{{(method.ret != null ? "Marshal.GetFunctionPointerForDelegate(ret)" : "IntPtr.Zero")}}},
			          				ReturnValueMetadata = GDExtensionClassMethodArgumentMetadata.GdextensionMethodArgumentMetadataNone,
			          				ArgumentCount = {{{method.arguments.Length}}},
			          				ArgumentsInfo = {{{(method.arguments.Length > 0 ? "(IntPtr)args" : "IntPtr.Zero")}}},
			          				ArgumentsMetadata = {{{(method.arguments.Length > 0 ? "(IntPtr)args_meta" : "IntPtr.Zero")}}},
			          				DefaultArgumentCount = 0,
			          				DefaultArguments = IntPtr.Zero,
			          			};
			          			GDExtensionInterface.ClassdbRegisterExtensionClassMethod({{{entryClassName}}}.Library, __godot_name.internalPointer, ref info);
			          """;
			code += "\t\t}\n";
		}

		code += $$"""
		          	}
		          	
		          	
		          	[UnmanagedCallersOnly]
		          	static void CallFuncPtr(IntPtr method_userdata, IntPtr p_instance, IntPtr* p_args, IntPtr r_ret) {
		          		var instance = ({{c.Name}})ConstructUnknown(p_instance);
		          		switch ((int)method_userdata) {
		          		
		          """;
		for (var i = 0; i < methods.Count; i++)
		{
			var method = methods[i];
			var args = "";
			for (var j = 0; j < method.arguments.Length; j++)
			{
				var arg = method.arguments[j];
				if (arg.Item1.Name == "String")
				{
					args += $"StringMarshall.ToManaged(p_args[{j}])";
				}
				else if (TypeToVariantType(arg.Item1) == "GodotObject")
				{
					args += $"({arg.Item1})GodotObject.ConstructUnknown(p_args[{j}])";
				}
				else
				{
					args += $"p_args[{j}]";
				}
				if (j < method.arguments.Length - 1)
				{
					args += ", ";
				}
			}
			code += $"\t\tcase {i}:\n\t\t\t";
			if (method.ret != null)
			{
				if (TypeToVariantType(method.ret) == "GodotObject")
				{
					code += "throw new NotImplementedException();\n";
					continue;
				}
				else
				{
					code += $"*({method.ret}*)r_ret = ";
				}
			}
			if (method.property != null)
			{
				if (method.ret != null)
				{
					code += $"instance.{method.property};\n";
				}
				else
				{
					code += $"instance.{method.property} = ({method.propertyType.Name}){args};\n";
				}
			}
			else
			{
				code += $"instance.{method.name}({args});\n";
			}
			code += $"\t\t\tbreak;\n";
		}

		code += $$"""
		          		}
		          	}
		          	
		          	[UnmanagedCallersOnly]
		          	static void CallFunc(
		          		IntPtr method_userdata,
		          		IntPtr p_instance,
		          		IntPtr* p_args,
		          		uint p_argument_count,
		          		IntPtr r_return,
		          		GDExtensionCallError* r_error
		          	) {
		          		r_return = GDExtensionInterface.VariantNewNil();
		          		var instance = ({{c.Name}})ConstructUnknown(p_instance);
		          		switch ((int)Marshal.ReadInt32(method_userdata)) {
		          		
		          """;
		for (var i = 0; i < methods.Count; i++)
		{
			var method = methods[i];
			var args = "";
			for (var j = 0; j < method.arguments.Length; j++)
			{
				var arg = method.arguments[j];
				var t = TypeToVariantType(arg.Item1);
				args += $"({arg.Item1})Variant.Get{t}FromVariant(Variant.GetObjectFromPointer(p_args[{j}]))";
				if (j < method.arguments.Length - 1)
				{
					args += ", ";
				}
			}
			code += $"\t\tcase {i}: {{\n\t\t\t";
			if (method.ret != null)
			{
				code += "var res = ";
			}
			if (method.property != null)
			{
				if (method.ret != null)
				{
					code += $"instance.{method.property};\n";
				}
				else
				{
					code += $"instance.{method.property} = {args};\n";
				}
			}
			else
			{
				code += $"instance.{method.name}({args});\n";
			}
			if (method.ret != null)
			{
				var t = TypeToVariantType(method.ret);
				code += $"\t\t\tVariant.SaveIntoPointer(res, r_return);\n";
			}
			code += $"\t\t\tbreak;\n";
			code += "\t\t\t}\n";
		}

		code += $$"""
		          		}
		          	}
		          } }
		          """;

		context.AddSource($"{c.Name}.methods.gen.cs", code);
	}

	public static string TypeToVariantType(ITypeSymbol type)
	{
		return TypeToVariantType(type, LibGodotGenerators.GetSpecialBase(type));
	}

	public static string TypeToVariantType(ITypeSymbol type, SpecialBase sBase)
	{
		return sBase switch
		{
			SpecialBase.Node => "GodotObject",
			SpecialBase.Resource => "GodotObject",
			_ => type.Name switch
			{
				"Boolean" => "Bool",
				"Int64" => "Int",
				"Int32" => "Int",
				"Double" => "Float",
				"String" => "String",
				"Vector2" => "Vector2",
				"Vector2i" => "Vector2i",
				"Rect2" => "Rect2",
				"Rect2i" => "Rect2i",
				"Vector3" => "Vector3",
				"Vector3i" => "Vector3i",
				"Transform2D" => "Transform2D",
				"Vector4" => "Vector4",
				"Vector4i" => "Vector4i",
				"Plane" => "Plane",
				"Quaternion" => "Quaternion",
				"AABB" => "AABB",
				"Basis" => "Basis",
				"Transform3D" => "Transform3D",
				"Projection" => "Projection",
				"Color" => "Color",
				"StringName" => "StringName",
				"NodePath" => "NodePath",
				"RID" => "RID",
				"Callable" => "Callable",
				"Signal" => "Signal",
				"Dictionary" => "Dictionary",
				"Array" => "Array",
				"PackedByteArray" => "PackedByteArray",
				"PackedInt32Array" => "PackedInt32Array",
				"PackedInt64Array" => "PackedInt64Array",
				"PackedFloat32Array" => "PackedFloat32Array",
				"PackedFloat64Array" => "PackedFloat64Array",
				"PackedStringArray" => "PackedStringArray",
				"PackedVector2Array" => "PackedVector2Array",
				"PackedVector3Array" => "PackedVector3Array",
				"PackedColorArray" => "PackedColorArray",
				_ => throw new Exception($"Unknown type: {type.Name}"),
			},
		};
	}

	public static string TypeToHintString(ITypeSymbol type, SpecialBase sBase)
	{
		return sBase switch
		{
			SpecialBase.Node or SpecialBase.Resource => $"StringMarshall.ToNative(\"{type.Name}\")",
			SpecialBase.None => "StringMarshall.ToNative(\"\")",
			_ => throw new Exception(),
		};
	}

	public static string TypeToHint(ITypeSymbol type, SpecialBase sBase)
	{
		return sBase switch
		{
			SpecialBase.Node => "PropertyHint.NodeType",
			SpecialBase.Resource => "PropertyHint.ResourceType",
			SpecialBase.None => "PropertyHint.None",
			_ => throw new Exception(),
		};
	}

	public static string CreatePropertyInfo(ITypeSymbol type, string name, int tabs)
	{
		var sBase = LibGodotGenerators.GetSpecialBase(type);

		var t = new String('\t', tabs);

		return $$"""
		         new GDExtensionPropertyInfo() {
		         {{t}}	Type = (GDExtensionVariantType)Variant.Type.{{TypeToVariantType(type, sBase)}},
		         {{t}}	Name = new StringName("{{Renamer.ToSnake(name)}}").internalPointer,
		         {{t}}	ClassName = __godot_name.internalPointer,
		         {{t}}	Hint = (uint){{TypeToHint(type, sBase)}},
		         {{t}}	HintString = {{TypeToHintString(type, sBase)}},
		         {{t}}	Usage = (uint)PropertyUsageFlags.Default,
		         {{t}}};
		         """;
	}
}
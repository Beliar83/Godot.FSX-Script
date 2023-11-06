using Microsoft.CodeAnalysis;

namespace Generators;

public static class Notification
{

	public static bool Generate(GeneratorExecutionContext context, INamedTypeSymbol c)
	{
		var methods = c.GetMembers().OfType<IMethodSymbol>();

		var code = $$"""
		             using System.Runtime.CompilerServices;
		             using System.Runtime.InteropServices;
		             using GodotSharpGDExtension;
		             namespace {{c.ContainingNamespace}} {
		             public unsafe partial class {{c.Name}} : {{c.BaseType.Name}} {
		             """;

		var notificationName = "";
		foreach (var method in methods)
		{
			var notificationAttribute = method.GetAttributes().SingleOrDefault(x => x.AttributeClass.ToString() == "GodotSharpGDExtension.NotificationAttribute");
			if (notificationAttribute != null)
			{
				notificationName = method.Name;
			}
		}
		if (notificationName == "")
		{
			notificationName = "_Notification";
			var has = false;
			code += $$"""
			          	void {{notificationName}}(int what) {
			          		switch (what) {
			          """;
			foreach (var method in methods)
			{
				var att = method.GetAttributes().SingleOrDefault(x => x.AttributeClass.ToString() == "GodotSharpGDExtension.NotifyAttribute");


				if (att != null)
				{
					has = true;
					var args = att.NamedArguments.SingleOrDefault(x => x.Key == "arguments").Value.Value ?? "";

					code += $$"""
					          		case {{att.ConstructorArguments[0].Value}}:
					          			{{method.Name}}({{args}});
					          			break;
					          """;
				}
			}
			if (has == false)
			{
				return false;
			}
			code += $$"""
			          		}
			          	}
			          """;
		}
		code += $$"""
		            
		            [UnmanagedCallersOnly]
		          	public static unsafe new void __Notification(IntPtr instance, int what) {
		          		var inst = ({{c.Name}})GodotObject.ConstructUnknown(instance);
		          		{{c.BaseType.Name}}.__Notification(instance, what);
		          		inst.{{notificationName}}(what);
		          	}
		          }
		          	}
		          """;

		context.AddSource($"{c.Name}.notification.gen.cs", code);
		return true;
	}
}
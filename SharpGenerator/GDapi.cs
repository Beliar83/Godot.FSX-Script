using System.Text.Json;
using System.Text.Json.Serialization;

namespace SharpGenerator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "JSON Format")]
	public sealed class Api
	{
		public static Api Create(string path)
		{
			FileStream file = File.OpenRead(path);

			var jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.General) { PropertyNameCaseInsensitive = true };
			var api = JsonSerializer.Deserialize<Api>(file,jsonSerializerOptions)!;
			file.Close();
			return api;
		}
		public record struct Argument
		{
			public string Name { get; set; }
			public string Type { get; set; }
			[JsonPropertyName("default_value")]
			public string? DefaultValue { get; set; }
			public string? Meta { get; set; }
		}

		public record struct MethodReturnValue
		{
			public string Type { get; set; }
			public string? Meta { get; set; }
		}

		public record struct Constant
		{
			public string Name { get; set; }
			public string Type { get; set; }
			public string Value { get; set; }
		}

		public record struct ClassSize
		{
			public string Name { get; set; }
			public int Size { get; set; }
		}

		public record struct Signal
		{
			public string Name { get; set; }
			public Argument[]? Arguments { get; set; }
		}

		public record struct Property
		{
			public string Type { get; set; }
			public string Name { get; set; }
			public string Setter { get; set; }
			public string Getter { get; set; }
			public int? Index { get; set; }
		}

		public record struct Singleton
		{
			public string Name { get; set; }
			public string Type { get; set; }
		}

		public record struct NativeStructure
		{
			public string Name { get; set; }
			public string Format { get; set; }
		}

		public record struct HeaderData
		{
			[JsonPropertyName("version_major")]
			public int VersionMajor { get; set; }
			[JsonPropertyName("version_minor")]
			public int VersionMinor { get; set; }
			[JsonPropertyName("version_patch")]
			public int VersionPatch { get; set; }
			[JsonPropertyName("version_status")]
			public string VersionStatus { get; set; }
			[JsonPropertyName("version_build")]
			public string VersionBuild { get; set; }
			[JsonPropertyName("version_full_name")]
			public string VersionFullName { get; set; }
		}

		public record struct BuiltinClassSizesOfConfig
		{
			[JsonPropertyName("build_configuration")]
			public string BuildConfiguration { get; set; }

			public ClassSize[] Sizes { get; set; }
		}

		public record struct MemberOffset
		{
			public string Member { get; set; }
			public int Offset { get; set; }
			public string? Meta { get; set; }
		}

		public record struct BuiltinMember
		{
			public string Name { get; set; }
			public string Type { get; set; }
		}

		public record struct ClassOffsets
		{
			public string Name { get; set; }
			public MemberOffset[] Members { get; set; }
		}

		public record struct BuiltinClassMemberOffsetsOfConfig
		{
			[JsonPropertyName("build_configuration")]
			public string BuildConfiguration { get; set; }
			public ClassOffsets[] Classes { get; set; }
		}

		public record struct ValueData
		{
			public string Name { get; set; }
			public int Value { get; set; }
		}

		public record struct Enum
		{
			public string Name { get; set; }
			[JsonPropertyName("is_bitfield")]
			public bool? IsBitfield { get; set; }
			public ValueData[] Values { get; set; }
		}

		public record struct Method
		{
			public string Name { get; set; }
			[JsonPropertyName("return_type")]
			public string? ReturnType { get; set; }
			[JsonPropertyName("is_vararg")]
			public bool IsVararg { get; set; }
			[JsonPropertyName("is_const")]
			public bool IsConst { get; set; }
			[JsonPropertyName("is_static")]
			public bool? IsStatic { get; set; }
			[JsonPropertyName("is_virtual")]
			public bool IsVirtual { get; set; }
			public uint? Hash { get; set; }
			[JsonPropertyName("return_value")] public MethodReturnValue? ReturnValue { get; set; }
			public Argument[]? Arguments { get; set; }

			public string? Category { get; set; }
		}

		public record struct Operator
		{
			public string Name { get; set; }
			[JsonPropertyName("right_type")] public string? RightType { get; set; }
			[JsonPropertyName("return_type")] public string ReturnType { get; set; }
		}

		public record Constructor
		{
			public int Index { get; set; }
			public Argument[]? Arguments { get; set; }
		}

		public record struct BuiltinClass
		{
			public string Name { get; set; }
			[JsonPropertyName("is_keyed")] public bool IsKeyed { get; set; }
			[JsonPropertyName("indexing_return_type")] public string? IndexingReturnType { get; set; }
			public BuiltinMember[]? Members { get; set; }
			public Constant[]? Constants { get; set; }
			public Enum[]? Enums { get; set; }
			public Operator[]? Operators { get; set; }
			public Method[]? Methods { get; set; }
			public Constructor[]? Constructors { get; set; }
			[JsonPropertyName("has_destructor")] public bool HasDestructor { get; set; }
		}

		public record struct Class
		{
			public string Name { get; set; }
			[JsonPropertyName("is_refcounted")] public bool IsRefcounted { get; set; }
			[JsonPropertyName("is_instantiable")] public bool IsInstantiable { get; set; }
			public string? Inherits { get; set; }
			[JsonPropertyName("api_type")] public string ApiType { get; set; }
			public ValueData[]? Constants { get; set; }
			public Enum[]? Enums { get; set; }
			public Method[]? Methods { get; set; }
			public Signal[]? Signals { get; set; }
			public Property[]? Properties { get; set; }
		}

		public HeaderData Header { get; set; }
		[JsonPropertyName("builtin_class_sizes")] public BuiltinClassSizesOfConfig[] BuiltinClassSizes { get; set; } = Array.Empty<BuiltinClassSizesOfConfig>();
		public BuiltinClassSizesOfConfig? ClassSizes(string buildConfiguration)
		{
			foreach (BuiltinClassSizesOfConfig item in BuiltinClassSizes)
			{
				if (item.BuildConfiguration == buildConfiguration)
				{
					return item;
				}
			}
			return null;
		}
		[JsonPropertyName("builtin_class_member_offsets")] public BuiltinClassMemberOffsetsOfConfig[] BuiltinClassMemberOffsets { get; set; } = Array.Empty<BuiltinClassMemberOffsetsOfConfig>();
		public BuiltinClassMemberOffsetsOfConfig? ClassMemberOffsets(string buildConfiguration)
		{
			foreach (BuiltinClassMemberOffsetsOfConfig item in BuiltinClassMemberOffsets)
			{
				if (item.BuildConfiguration == buildConfiguration)
				{
					return item;
				}
			}
			return null;
		}
		[JsonPropertyName("global_constants")] public object[] GlobalConstants { get; set; } = Array.Empty<object>();
		[JsonPropertyName("global_enums")] public Enum[] GlobalEnums { get; set; } = Array.Empty<Enum>();
		[JsonPropertyName("utility_functions")] public Method[] UtilityFunction { get; set; } = Array.Empty<Method>();
		[JsonPropertyName("builtin_classes")] public BuiltinClass[] BuiltinClasses { get; set; } = Array.Empty<BuiltinClass>();
		public Class[] Classes { get; set; } = Array.Empty<Class>();
		public Singleton[] Singletons { get; set; } = Array.Empty<Singleton>();
		[JsonPropertyName("native_structures")] public NativeStructure[] NativeStructures { get; set; } = Array.Empty<NativeStructure>();
	}
}

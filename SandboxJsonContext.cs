using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

[JsonSerializable(typeof(SandboxScenario))]
[GeneratedCode("System.Text.Json.SourceGeneration", "9.0.14.26416")]
internal class SandboxJsonContext : JsonSerializerContext, IJsonTypeInfoResolver
{
	private JsonTypeInfo<bool>? _Boolean;

	private JsonTypeInfo<double>? _Double;

	private JsonTypeInfo<float>? _Single;

	private JsonTypeInfo<SandboxPlacement>? _SandboxPlacement;

	private JsonTypeInfo<SandboxScenario>? _SandboxScenario;

	private JsonTypeInfo<List<SandboxPlacement>>? _ListSandboxPlacement;

	private JsonTypeInfo<int>? _Int32;

	private JsonTypeInfo<string>? _String;

	private static readonly JsonSerializerOptions s_defaultOptions = new JsonSerializerOptions();

	private const BindingFlags InstanceMemberBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	private static readonly JsonEncodedText PropName_Packet = JsonEncodedText.Encode("Packet");

	private static readonly JsonEncodedText PropName_Team = JsonEncodedText.Encode("Team");

	private static readonly JsonEncodedText PropName_Column = JsonEncodedText.Encode("Column");

	private static readonly JsonEncodedText PropName_Row = JsonEncodedText.Encode("Row");

	private static readonly JsonEncodedText PropName_Facing = JsonEncodedText.Encode("Facing");

	private static readonly JsonEncodedText PropName_HealthScale = JsonEncodedText.Encode("HealthScale");

	private static readonly JsonEncodedText PropName_ActionSpeed = JsonEncodedText.Encode("ActionSpeed");

	private static readonly JsonEncodedText PropName_AutoAttack = JsonEncodedText.Encode("AutoAttack");

	private static readonly JsonEncodedText PropName_OffsetX = JsonEncodedText.Encode("OffsetX");

	private static readonly JsonEncodedText PropName_Format = JsonEncodedText.Encode("Format");

	private static readonly JsonEncodedText PropName_Version = JsonEncodedText.Encode("Version");

	private static readonly JsonEncodedText PropName_Map = JsonEncodedText.Encode("Map");

	private static readonly JsonEncodedText PropName_AllowStacking = JsonEncodedText.Encode("AllowStacking");

	private static readonly JsonEncodedText PropName_Characters = JsonEncodedText.Encode("Characters");

	public JsonTypeInfo<bool> Boolean => _Boolean ?? (_Boolean = (JsonTypeInfo<bool>)base.Options.GetTypeInfo(typeof(bool)));

	public JsonTypeInfo<double> Double => _Double ?? (_Double = (JsonTypeInfo<double>)base.Options.GetTypeInfo(typeof(double)));

	public JsonTypeInfo<float> Single => _Single ?? (_Single = (JsonTypeInfo<float>)base.Options.GetTypeInfo(typeof(float)));

	public JsonTypeInfo<SandboxPlacement> SandboxPlacement => _SandboxPlacement ?? (_SandboxPlacement = (JsonTypeInfo<SandboxPlacement>)base.Options.GetTypeInfo(typeof(SandboxPlacement)));

	public JsonTypeInfo<SandboxScenario> SandboxScenario => _SandboxScenario ?? (_SandboxScenario = (JsonTypeInfo<SandboxScenario>)base.Options.GetTypeInfo(typeof(SandboxScenario)));

	public JsonTypeInfo<List<SandboxPlacement>> ListSandboxPlacement => _ListSandboxPlacement ?? (_ListSandboxPlacement = (JsonTypeInfo<List<SandboxPlacement>>)base.Options.GetTypeInfo(typeof(List<SandboxPlacement>)));

	public JsonTypeInfo<int> Int32 => _Int32 ?? (_Int32 = (JsonTypeInfo<int>)base.Options.GetTypeInfo(typeof(int)));

	public JsonTypeInfo<string> String => _String ?? (_String = (JsonTypeInfo<string>)base.Options.GetTypeInfo(typeof(string)));

	public static SandboxJsonContext Default { get; } = new SandboxJsonContext(new JsonSerializerOptions(s_defaultOptions));

	protected override JsonSerializerOptions? GeneratedSerializerOptions { get; } = s_defaultOptions;

	private JsonTypeInfo<bool> Create_Boolean(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<bool> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<bool>(options, JsonMetadataServices.BooleanConverter);
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<double> Create_Double(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<double> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<double>(options, JsonMetadataServices.DoubleConverter);
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<float> Create_Single(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<float> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<float>(options, JsonMetadataServices.SingleConverter);
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<SandboxPlacement> Create_SandboxPlacement(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<SandboxPlacement> jsonTypeInfo))
		{
			JsonObjectInfoValues<SandboxPlacement> objectInfo = new JsonObjectInfoValues<SandboxPlacement>
			{
				ObjectCreator = () => new SandboxPlacement(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => SandboxPlacementPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(SandboxPlacement).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = SandboxPlacementSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] SandboxPlacementPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[9];
		JsonPropertyInfoValues<string> propertyInfo = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(SandboxPlacement),
			Converter = null,
			Getter = (object obj) => ((SandboxPlacement)obj).Packet,
			Setter = delegate(object obj, string? value)
			{
				((SandboxPlacement)obj).Packet = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Packet",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(SandboxPlacement).GetProperty("Packet", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsRequired = true;
		JsonPropertyInfoValues<int> propertyInfo2 = new JsonPropertyInfoValues<int>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(SandboxPlacement),
			Converter = null,
			Getter = (object obj) => ((SandboxPlacement)obj).Team,
			Setter = delegate(object obj, int value)
			{
				((SandboxPlacement)obj).Team = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Team",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(SandboxPlacement).GetProperty("Team", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null)
		};
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsRequired = true;
		JsonPropertyInfoValues<int> propertyInfo3 = new JsonPropertyInfoValues<int>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(SandboxPlacement),
			Converter = null,
			Getter = (object obj) => ((SandboxPlacement)obj).Column,
			Setter = delegate(object obj, int value)
			{
				((SandboxPlacement)obj).Column = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Column",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(SandboxPlacement).GetProperty("Column", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null)
		};
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		array[2].IsRequired = true;
		JsonPropertyInfoValues<int> propertyInfo4 = new JsonPropertyInfoValues<int>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(SandboxPlacement),
			Converter = null,
			Getter = (object obj) => ((SandboxPlacement)obj).Row,
			Setter = delegate(object obj, int value)
			{
				((SandboxPlacement)obj).Row = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Row",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(SandboxPlacement).GetProperty("Row", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null)
		};
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		array[3].IsRequired = true;
		JsonPropertyInfoValues<int> propertyInfo5 = new JsonPropertyInfoValues<int>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(SandboxPlacement),
			Converter = null,
			Getter = (object obj) => ((SandboxPlacement)obj).Facing,
			Setter = delegate(object obj, int value)
			{
				((SandboxPlacement)obj).Facing = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Facing",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(SandboxPlacement).GetProperty("Facing", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null)
		};
		array[4] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo5);
		array[4].IsRequired = true;
		JsonPropertyInfoValues<double> propertyInfo6 = new JsonPropertyInfoValues<double>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(SandboxPlacement),
			Converter = null,
			Getter = (object obj) => ((SandboxPlacement)obj).HealthScale,
			Setter = delegate(object obj, double value)
			{
				((SandboxPlacement)obj).HealthScale = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "HealthScale",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(SandboxPlacement).GetProperty("HealthScale", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(double), Array.Empty<Type>(), null)
		};
		array[5] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo6);
		JsonPropertyInfoValues<double> propertyInfo7 = new JsonPropertyInfoValues<double>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(SandboxPlacement),
			Converter = null,
			Getter = (object obj) => ((SandboxPlacement)obj).ActionSpeed,
			Setter = delegate(object obj, double value)
			{
				((SandboxPlacement)obj).ActionSpeed = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "ActionSpeed",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(SandboxPlacement).GetProperty("ActionSpeed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(double), Array.Empty<Type>(), null)
		};
		array[6] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo7);
		JsonPropertyInfoValues<bool> propertyInfo8 = new JsonPropertyInfoValues<bool>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(SandboxPlacement),
			Converter = null,
			Getter = (object obj) => ((SandboxPlacement)obj).AutoAttack,
			Setter = delegate(object obj, bool value)
			{
				((SandboxPlacement)obj).AutoAttack = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "AutoAttack",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(SandboxPlacement).GetProperty("AutoAttack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(bool), Array.Empty<Type>(), null)
		};
		array[7] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo8);
		JsonPropertyInfoValues<float> propertyInfo9 = new JsonPropertyInfoValues<float>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(SandboxPlacement),
			Converter = null,
			Getter = (object obj) => ((SandboxPlacement)obj).OffsetX,
			Setter = delegate(object obj, float value)
			{
				((SandboxPlacement)obj).OffsetX = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "OffsetX",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(SandboxPlacement).GetProperty("OffsetX", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(float), Array.Empty<Type>(), null)
		};
		array[8] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo9);
		return array;
	}

	private void SandboxPlacementSerializeHandler(Utf8JsonWriter writer, SandboxPlacement? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartObject();
		writer.WriteString(PropName_Packet, value.Packet);
		writer.WriteNumber(PropName_Team, value.Team);
		writer.WriteNumber(PropName_Column, value.Column);
		writer.WriteNumber(PropName_Row, value.Row);
		writer.WriteNumber(PropName_Facing, value.Facing);
		writer.WriteNumber(PropName_HealthScale, value.HealthScale);
		writer.WriteNumber(PropName_ActionSpeed, value.ActionSpeed);
		writer.WriteBoolean(PropName_AutoAttack, value.AutoAttack);
		writer.WriteNumber(PropName_OffsetX, value.OffsetX);
		writer.WriteEndObject();
	}

	private JsonTypeInfo<SandboxScenario> Create_SandboxScenario(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<SandboxScenario> jsonTypeInfo))
		{
			JsonObjectInfoValues<SandboxScenario> objectInfo = new JsonObjectInfoValues<SandboxScenario>
			{
				ObjectCreator = () => new SandboxScenario(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => SandboxScenarioPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(SandboxScenario).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = SandboxScenarioSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] SandboxScenarioPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[5];
		JsonPropertyInfoValues<string> propertyInfo = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(SandboxScenario),
			Converter = null,
			Getter = (object obj) => ((SandboxScenario)obj).Format,
			Setter = delegate(object obj, string? value)
			{
				((SandboxScenario)obj).Format = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Format",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(SandboxScenario).GetProperty("Format", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsRequired = true;
		JsonPropertyInfoValues<int> propertyInfo2 = new JsonPropertyInfoValues<int>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(SandboxScenario),
			Converter = null,
			Getter = (object obj) => ((SandboxScenario)obj).Version,
			Setter = delegate(object obj, int value)
			{
				((SandboxScenario)obj).Version = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Version",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(SandboxScenario).GetProperty("Version", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null)
		};
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsRequired = true;
		JsonPropertyInfoValues<string> propertyInfo3 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(SandboxScenario),
			Converter = null,
			Getter = (object obj) => ((SandboxScenario)obj).Map,
			Setter = delegate(object obj, string? value)
			{
				((SandboxScenario)obj).Map = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Map",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(SandboxScenario).GetProperty("Map", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		array[2].IsRequired = true;
		JsonPropertyInfoValues<bool> propertyInfo4 = new JsonPropertyInfoValues<bool>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(SandboxScenario),
			Converter = null,
			Getter = (object obj) => ((SandboxScenario)obj).AllowStacking,
			Setter = delegate(object obj, bool value)
			{
				((SandboxScenario)obj).AllowStacking = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "AllowStacking",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(SandboxScenario).GetProperty("AllowStacking", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(bool), Array.Empty<Type>(), null)
		};
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		JsonPropertyInfoValues<List<SandboxPlacement>> propertyInfo5 = new JsonPropertyInfoValues<List<SandboxPlacement>>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(SandboxScenario),
			Converter = null,
			Getter = (object obj) => ((SandboxScenario)obj).Characters,
			Setter = delegate(object obj, List<SandboxPlacement>? value)
			{
				((SandboxScenario)obj).Characters = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Characters",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(SandboxScenario).GetProperty("Characters", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<SandboxPlacement>), Array.Empty<Type>(), null)
		};
		array[4] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo5);
		array[4].IsRequired = true;
		return array;
	}

	private void SandboxScenarioSerializeHandler(Utf8JsonWriter writer, SandboxScenario? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartObject();
		writer.WriteString(PropName_Format, value.Format);
		writer.WriteNumber(PropName_Version, value.Version);
		writer.WriteString(PropName_Map, value.Map);
		writer.WriteBoolean(PropName_AllowStacking, value.AllowStacking);
		writer.WritePropertyName(PropName_Characters);
		ListSandboxPlacementSerializeHandler(writer, value.Characters);
		writer.WriteEndObject();
	}

	private JsonTypeInfo<List<SandboxPlacement>> Create_ListSandboxPlacement(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<SandboxPlacement>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<SandboxPlacement>> collectionInfo = new JsonCollectionInfoValues<List<SandboxPlacement>>
			{
				ObjectCreator = () => new List<SandboxPlacement>(),
				SerializeHandler = ListSandboxPlacementSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<SandboxPlacement>, SandboxPlacement>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private void ListSandboxPlacementSerializeHandler(Utf8JsonWriter writer, List<SandboxPlacement>? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartArray();
		for (int i = 0; i < value.Count; i++)
		{
			SandboxPlacementSerializeHandler(writer, value[i]);
		}
		writer.WriteEndArray();
	}

	private JsonTypeInfo<int> Create_Int32(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<int> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<int>(options, JsonMetadataServices.Int32Converter);
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<string> Create_String(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<string> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<string>(options, JsonMetadataServices.StringConverter);
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	public SandboxJsonContext()
		: base(null)
	{
	}

	public SandboxJsonContext(JsonSerializerOptions options)
		: base(options)
	{
	}

	private static bool TryGetTypeInfoForRuntimeCustomConverter<TJsonMetadataType>(JsonSerializerOptions options, out JsonTypeInfo<TJsonMetadataType> jsonTypeInfo)
	{
		JsonConverter runtimeConverterForType = GetRuntimeConverterForType(typeof(TJsonMetadataType), options);
		if (runtimeConverterForType != null)
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<TJsonMetadataType>(options, runtimeConverterForType);
			return true;
		}
		jsonTypeInfo = null;
		return false;
	}

	private static JsonConverter? GetRuntimeConverterForType(Type type, JsonSerializerOptions options)
	{
		for (int i = 0; i < options.Converters.Count; i++)
		{
			JsonConverter jsonConverter = options.Converters[i];
			if (jsonConverter != null && jsonConverter.CanConvert(type))
			{
				return ExpandConverter(type, jsonConverter, options, validateCanConvert: false);
			}
		}
		return null;
	}

	private static JsonConverter ExpandConverter(Type type, JsonConverter converter, JsonSerializerOptions options, bool validateCanConvert = true)
	{
		if (validateCanConvert && !converter.CanConvert(type))
		{
			throw new InvalidOperationException($"The converter '{converter.GetType()}' is not compatible with the type '{type}'.");
		}
		if (converter is JsonConverterFactory jsonConverterFactory)
		{
			converter = jsonConverterFactory.CreateConverter(type, options);
			if (converter == null || converter is JsonConverterFactory)
			{
				throw new InvalidOperationException($"The converter '{jsonConverterFactory.GetType()}' cannot return null or a JsonConverterFactory instance.");
			}
		}
		return converter;
	}

	public override JsonTypeInfo? GetTypeInfo(Type type)
	{
		base.Options.TryGetTypeInfo(type, out JsonTypeInfo typeInfo);
		return typeInfo;
	}

	JsonTypeInfo? IJsonTypeInfoResolver.GetTypeInfo(Type type, JsonSerializerOptions options)
	{
		if (type == typeof(bool))
		{
			return Create_Boolean(options);
		}
		if (type == typeof(double))
		{
			return Create_Double(options);
		}
		if (type == typeof(float))
		{
			return Create_Single(options);
		}
		if (type == typeof(SandboxPlacement))
		{
			return Create_SandboxPlacement(options);
		}
		if (type == typeof(SandboxScenario))
		{
			return Create_SandboxScenario(options);
		}
		if (type == typeof(List<SandboxPlacement>))
		{
			return Create_ListSandboxPlacement(options);
		}
		if (type == typeof(int))
		{
			return Create_Int32(options);
		}
		if (type == typeof(string))
		{
			return Create_String(options);
		}
		return null;
	}
}

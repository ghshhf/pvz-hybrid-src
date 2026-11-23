using System;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace PVZHE.ModEditor.ModSystem;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(XWModWireMessage))]
[GeneratedCode("System.Text.Json.SourceGeneration", "9.0.14.26416")]
internal class XWModNetworkJsonContext : JsonSerializerContext, IJsonTypeInfoResolver
{
	private JsonTypeInfo<byte[]>? _ByteArray;

	private JsonTypeInfo<XWModWireMessage>? _XWModWireMessage;

	private JsonTypeInfo<int>? _Int32;

	private JsonTypeInfo<string>? _String;

	private static readonly JsonSerializerOptions s_defaultOptions = new JsonSerializerOptions
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};

	private const BindingFlags InstanceMemberBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	private static readonly JsonEncodedText PropName_owner = JsonEncodedText.Encode("owner");

	private static readonly JsonEncodedText PropName_name = JsonEncodedText.Encode("name");

	private static readonly JsonEncodedText PropName_version = JsonEncodedText.Encode("version");

	private static readonly JsonEncodedText PropName_session = JsonEncodedText.Encode("session");

	private static readonly JsonEncodedText PropName_requestId = JsonEncodedText.Encode("requestId");

	private static readonly JsonEncodedText PropName_payload = JsonEncodedText.Encode("payload");

	public JsonTypeInfo<byte[]> ByteArray => _ByteArray ?? (_ByteArray = (JsonTypeInfo<byte[]>)base.Options.GetTypeInfo(typeof(byte[])));

	public JsonTypeInfo<XWModWireMessage> XWModWireMessage => _XWModWireMessage ?? (_XWModWireMessage = (JsonTypeInfo<XWModWireMessage>)base.Options.GetTypeInfo(typeof(XWModWireMessage)));

	public JsonTypeInfo<int> Int32 => _Int32 ?? (_Int32 = (JsonTypeInfo<int>)base.Options.GetTypeInfo(typeof(int)));

	public JsonTypeInfo<string> String => _String ?? (_String = (JsonTypeInfo<string>)base.Options.GetTypeInfo(typeof(string)));

	public static XWModNetworkJsonContext Default { get; } = new XWModNetworkJsonContext(new JsonSerializerOptions(s_defaultOptions));

	protected override JsonSerializerOptions? GeneratedSerializerOptions { get; } = s_defaultOptions;

	private JsonTypeInfo<byte[]> Create_ByteArray(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<byte[]> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<byte[]>(options, JsonMetadataServices.ByteArrayConverter);
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<XWModWireMessage> Create_XWModWireMessage(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<XWModWireMessage> jsonTypeInfo))
		{
			JsonObjectInfoValues<XWModWireMessage> objectInfo = new JsonObjectInfoValues<XWModWireMessage>
			{
				ObjectCreator = () => new XWModWireMessage(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => XWModWireMessagePropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(XWModWireMessage).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = XWModWireMessageSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] XWModWireMessagePropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[6];
		JsonPropertyInfoValues<string> propertyInfo = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModWireMessage),
			Converter = null,
			Getter = (object obj) => ((XWModWireMessage)obj).Owner,
			Setter = delegate(object obj, string? value)
			{
				((XWModWireMessage)obj).Owner = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Owner",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(XWModWireMessage).GetProperty("Owner", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<string> propertyInfo2 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModWireMessage),
			Converter = null,
			Getter = (object obj) => ((XWModWireMessage)obj).Name,
			Setter = delegate(object obj, string? value)
			{
				((XWModWireMessage)obj).Name = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Name",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(XWModWireMessage).GetProperty("Name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		JsonPropertyInfoValues<int> propertyInfo3 = new JsonPropertyInfoValues<int>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModWireMessage),
			Converter = null,
			Getter = (object obj) => ((XWModWireMessage)obj).Version,
			Setter = delegate(object obj, int value)
			{
				((XWModWireMessage)obj).Version = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Version",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(XWModWireMessage).GetProperty("Version", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null)
		};
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		JsonPropertyInfoValues<string> propertyInfo4 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModWireMessage),
			Converter = null,
			Getter = (object obj) => ((XWModWireMessage)obj).Session,
			Setter = delegate(object obj, string? value)
			{
				((XWModWireMessage)obj).Session = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Session",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(XWModWireMessage).GetProperty("Session", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		JsonPropertyInfoValues<string> propertyInfo5 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModWireMessage),
			Converter = null,
			Getter = (object obj) => ((XWModWireMessage)obj).RequestId,
			Setter = delegate(object obj, string? value)
			{
				((XWModWireMessage)obj).RequestId = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "RequestId",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(XWModWireMessage).GetProperty("RequestId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[4] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo5);
		JsonPropertyInfoValues<byte[]> propertyInfo6 = new JsonPropertyInfoValues<byte[]>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModWireMessage),
			Converter = null,
			Getter = (object obj) => ((XWModWireMessage)obj).Payload,
			Setter = delegate(object obj, byte[]? value)
			{
				((XWModWireMessage)obj).Payload = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Payload",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(XWModWireMessage).GetProperty("Payload", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(byte[]), Array.Empty<Type>(), null)
		};
		array[5] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo6);
		return array;
	}

	private void XWModWireMessageSerializeHandler(Utf8JsonWriter writer, XWModWireMessage? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartObject();
		writer.WriteString(PropName_owner, value.Owner);
		writer.WriteString(PropName_name, value.Name);
		writer.WriteNumber(PropName_version, value.Version);
		writer.WriteString(PropName_session, value.Session);
		writer.WriteString(PropName_requestId, value.RequestId);
		writer.WriteBase64String(PropName_payload, value.Payload);
		writer.WriteEndObject();
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

	public XWModNetworkJsonContext()
		: base(null)
	{
	}

	public XWModNetworkJsonContext(JsonSerializerOptions options)
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
		if (type == typeof(byte[]))
		{
			return Create_ByteArray(options);
		}
		if (type == typeof(XWModWireMessage))
		{
			return Create_XWModWireMessage(options);
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

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace PVZHE.ModEditor.ModSystem;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true)]
[JsonSerializable(typeof(XWModBootJournal))]
[JsonSerializable(typeof(XWModLastGoodState))]
[GeneratedCode("System.Text.Json.SourceGeneration", "9.0.14.26416")]
internal class XWModBootJsonContext : JsonSerializerContext, IJsonTypeInfoResolver
{
	private JsonTypeInfo<bool>? _Boolean;

	private JsonTypeInfo<XWModBootJournal>? _XWModBootJournal;

	private JsonTypeInfo<XWModBootPackage>? _XWModBootPackage;

	private JsonTypeInfo<XWModLastGoodState>? _XWModLastGoodState;

	private JsonTypeInfo<XWModSelectionState>? _XWModSelectionState;

	private JsonTypeInfo<List<XWModBootPackage>>? _ListXWModBootPackage;

	private JsonTypeInfo<List<string>>? _ListString;

	private JsonTypeInfo<int>? _Int32;

	private JsonTypeInfo<string>? _String;

	private static readonly JsonSerializerOptions s_defaultOptions = new JsonSerializerOptions
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = true
	};

	private const BindingFlags InstanceMemberBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	private static readonly JsonEncodedText PropName_schemaVersion = JsonEncodedText.Encode("schemaVersion");

	private static readonly JsonEncodedText PropName_planHash = JsonEncodedText.Encode("planHash");

	private static readonly JsonEncodedText PropName_owner = JsonEncodedText.Encode("owner");

	private static readonly JsonEncodedText PropName_phase = JsonEncodedText.Encode("phase");

	private static readonly JsonEncodedText PropName_inProgress = JsonEncodedText.Encode("inProgress");

	private static readonly JsonEncodedText PropName_recoveryRequired = JsonEncodedText.Encode("recoveryRequired");

	private static readonly JsonEncodedText PropName_id = JsonEncodedText.Encode("id");

	private static readonly JsonEncodedText PropName_version = JsonEncodedText.Encode("version");

	private static readonly JsonEncodedText PropName_sha256 = JsonEncodedText.Encode("sha256");

	private static readonly JsonEncodedText PropName_selection = JsonEncodedText.Encode("selection");

	private static readonly JsonEncodedText PropName_packages = JsonEncodedText.Encode("packages");

	private static readonly JsonEncodedText PropName_enabledIds = JsonEncodedText.Encode("enabledIds");

	private static readonly JsonEncodedText PropName_preferredOrder = JsonEncodedText.Encode("preferredOrder");

	public JsonTypeInfo<bool> Boolean => _Boolean ?? (_Boolean = (JsonTypeInfo<bool>)base.Options.GetTypeInfo(typeof(bool)));

	public JsonTypeInfo<XWModBootJournal> XWModBootJournal => _XWModBootJournal ?? (_XWModBootJournal = (JsonTypeInfo<XWModBootJournal>)base.Options.GetTypeInfo(typeof(XWModBootJournal)));

	public JsonTypeInfo<XWModBootPackage> XWModBootPackage => _XWModBootPackage ?? (_XWModBootPackage = (JsonTypeInfo<XWModBootPackage>)base.Options.GetTypeInfo(typeof(XWModBootPackage)));

	public JsonTypeInfo<XWModLastGoodState> XWModLastGoodState => _XWModLastGoodState ?? (_XWModLastGoodState = (JsonTypeInfo<XWModLastGoodState>)base.Options.GetTypeInfo(typeof(XWModLastGoodState)));

	public JsonTypeInfo<XWModSelectionState> XWModSelectionState => _XWModSelectionState ?? (_XWModSelectionState = (JsonTypeInfo<XWModSelectionState>)base.Options.GetTypeInfo(typeof(XWModSelectionState)));

	public JsonTypeInfo<List<XWModBootPackage>> ListXWModBootPackage => _ListXWModBootPackage ?? (_ListXWModBootPackage = (JsonTypeInfo<List<XWModBootPackage>>)base.Options.GetTypeInfo(typeof(List<XWModBootPackage>)));

	public JsonTypeInfo<List<string>> ListString => _ListString ?? (_ListString = (JsonTypeInfo<List<string>>)base.Options.GetTypeInfo(typeof(List<string>)));

	public JsonTypeInfo<int> Int32 => _Int32 ?? (_Int32 = (JsonTypeInfo<int>)base.Options.GetTypeInfo(typeof(int)));

	public JsonTypeInfo<string> String => _String ?? (_String = (JsonTypeInfo<string>)base.Options.GetTypeInfo(typeof(string)));

	public static XWModBootJsonContext Default { get; } = new XWModBootJsonContext(new JsonSerializerOptions(s_defaultOptions));

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

	private JsonTypeInfo<XWModBootJournal> Create_XWModBootJournal(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<XWModBootJournal> jsonTypeInfo))
		{
			JsonObjectInfoValues<XWModBootJournal> objectInfo = new JsonObjectInfoValues<XWModBootJournal>
			{
				ObjectCreator = () => new XWModBootJournal(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => XWModBootJournalPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(XWModBootJournal).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = XWModBootJournalSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] XWModBootJournalPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[6];
		JsonPropertyInfoValues<int> propertyInfo = new JsonPropertyInfoValues<int>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModBootJournal),
			Converter = null,
			Getter = (object obj) => ((XWModBootJournal)obj).SchemaVersion,
			Setter = delegate(object obj, int value)
			{
				((XWModBootJournal)obj).SchemaVersion = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "SchemaVersion",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(XWModBootJournal).GetProperty("SchemaVersion", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<string> propertyInfo2 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModBootJournal),
			Converter = null,
			Getter = (object obj) => ((XWModBootJournal)obj).PlanHash,
			Setter = delegate(object obj, string? value)
			{
				((XWModBootJournal)obj).PlanHash = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "PlanHash",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(XWModBootJournal).GetProperty("PlanHash", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		JsonPropertyInfoValues<string> propertyInfo3 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModBootJournal),
			Converter = null,
			Getter = (object obj) => ((XWModBootJournal)obj).Owner,
			Setter = delegate(object obj, string? value)
			{
				((XWModBootJournal)obj).Owner = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Owner",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(XWModBootJournal).GetProperty("Owner", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		JsonPropertyInfoValues<string> propertyInfo4 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModBootJournal),
			Converter = null,
			Getter = (object obj) => ((XWModBootJournal)obj).Phase,
			Setter = delegate(object obj, string? value)
			{
				((XWModBootJournal)obj).Phase = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Phase",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(XWModBootJournal).GetProperty("Phase", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		JsonPropertyInfoValues<bool> propertyInfo5 = new JsonPropertyInfoValues<bool>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModBootJournal),
			Converter = null,
			Getter = (object obj) => ((XWModBootJournal)obj).InProgress,
			Setter = delegate(object obj, bool value)
			{
				((XWModBootJournal)obj).InProgress = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "InProgress",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(XWModBootJournal).GetProperty("InProgress", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(bool), Array.Empty<Type>(), null)
		};
		array[4] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo5);
		JsonPropertyInfoValues<bool> propertyInfo6 = new JsonPropertyInfoValues<bool>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModBootJournal),
			Converter = null,
			Getter = (object obj) => ((XWModBootJournal)obj).RecoveryRequired,
			Setter = delegate(object obj, bool value)
			{
				((XWModBootJournal)obj).RecoveryRequired = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "RecoveryRequired",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(XWModBootJournal).GetProperty("RecoveryRequired", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(bool), Array.Empty<Type>(), null)
		};
		array[5] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo6);
		return array;
	}

	private void XWModBootJournalSerializeHandler(Utf8JsonWriter writer, XWModBootJournal? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartObject();
		writer.WriteNumber(PropName_schemaVersion, value.SchemaVersion);
		writer.WriteString(PropName_planHash, value.PlanHash);
		writer.WriteString(PropName_owner, value.Owner);
		writer.WriteString(PropName_phase, value.Phase);
		writer.WriteBoolean(PropName_inProgress, value.InProgress);
		writer.WriteBoolean(PropName_recoveryRequired, value.RecoveryRequired);
		writer.WriteEndObject();
	}

	private JsonTypeInfo<XWModBootPackage> Create_XWModBootPackage(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<XWModBootPackage> jsonTypeInfo))
		{
			JsonObjectInfoValues<XWModBootPackage> objectInfo = new JsonObjectInfoValues<XWModBootPackage>
			{
				ObjectCreator = () => new XWModBootPackage(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => XWModBootPackagePropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(XWModBootPackage).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = XWModBootPackageSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] XWModBootPackagePropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[3];
		JsonPropertyInfoValues<string> propertyInfo = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModBootPackage),
			Converter = null,
			Getter = (object obj) => ((XWModBootPackage)obj).Id,
			Setter = delegate(object obj, string? value)
			{
				((XWModBootPackage)obj).Id = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Id",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(XWModBootPackage).GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<string> propertyInfo2 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModBootPackage),
			Converter = null,
			Getter = (object obj) => ((XWModBootPackage)obj).Version,
			Setter = delegate(object obj, string? value)
			{
				((XWModBootPackage)obj).Version = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Version",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(XWModBootPackage).GetProperty("Version", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		JsonPropertyInfoValues<string> propertyInfo3 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModBootPackage),
			Converter = null,
			Getter = (object obj) => ((XWModBootPackage)obj).Sha256,
			Setter = delegate(object obj, string? value)
			{
				((XWModBootPackage)obj).Sha256 = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Sha256",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(XWModBootPackage).GetProperty("Sha256", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		return array;
	}

	private void XWModBootPackageSerializeHandler(Utf8JsonWriter writer, XWModBootPackage? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartObject();
		writer.WriteString(PropName_id, value.Id);
		writer.WriteString(PropName_version, value.Version);
		writer.WriteString(PropName_sha256, value.Sha256);
		writer.WriteEndObject();
	}

	private JsonTypeInfo<XWModLastGoodState> Create_XWModLastGoodState(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<XWModLastGoodState> jsonTypeInfo))
		{
			JsonObjectInfoValues<XWModLastGoodState> objectInfo = new JsonObjectInfoValues<XWModLastGoodState>
			{
				ObjectCreator = () => new XWModLastGoodState(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => XWModLastGoodStatePropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(XWModLastGoodState).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = XWModLastGoodStateSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] XWModLastGoodStatePropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[3];
		JsonPropertyInfoValues<int> propertyInfo = new JsonPropertyInfoValues<int>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModLastGoodState),
			Converter = null,
			Getter = (object obj) => ((XWModLastGoodState)obj).SchemaVersion,
			Setter = delegate(object obj, int value)
			{
				((XWModLastGoodState)obj).SchemaVersion = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "SchemaVersion",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(XWModLastGoodState).GetProperty("SchemaVersion", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<XWModSelectionState> propertyInfo2 = new JsonPropertyInfoValues<XWModSelectionState>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModLastGoodState),
			Converter = null,
			Getter = (object obj) => ((XWModLastGoodState)obj).Selection,
			Setter = delegate(object obj, XWModSelectionState? value)
			{
				((XWModLastGoodState)obj).Selection = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Selection",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(XWModLastGoodState).GetProperty("Selection", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(XWModSelectionState), Array.Empty<Type>(), null)
		};
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		JsonPropertyInfoValues<List<XWModBootPackage>> propertyInfo3 = new JsonPropertyInfoValues<List<XWModBootPackage>>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModLastGoodState),
			Converter = null,
			Getter = (object obj) => ((XWModLastGoodState)obj).Packages,
			Setter = delegate(object obj, List<XWModBootPackage>? value)
			{
				((XWModLastGoodState)obj).Packages = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Packages",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(XWModLastGoodState).GetProperty("Packages", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<XWModBootPackage>), Array.Empty<Type>(), null)
		};
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		return array;
	}

	private void XWModLastGoodStateSerializeHandler(Utf8JsonWriter writer, XWModLastGoodState? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartObject();
		writer.WriteNumber(PropName_schemaVersion, value.SchemaVersion);
		writer.WritePropertyName(PropName_selection);
		XWModSelectionStateSerializeHandler(writer, value.Selection);
		writer.WritePropertyName(PropName_packages);
		ListXWModBootPackageSerializeHandler(writer, value.Packages);
		writer.WriteEndObject();
	}

	private JsonTypeInfo<XWModSelectionState> Create_XWModSelectionState(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<XWModSelectionState> jsonTypeInfo))
		{
			JsonObjectInfoValues<XWModSelectionState> objectInfo = new JsonObjectInfoValues<XWModSelectionState>
			{
				ObjectCreator = () => new XWModSelectionState(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => XWModSelectionStatePropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(XWModSelectionState).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = XWModSelectionStateSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] XWModSelectionStatePropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[3];
		JsonPropertyInfoValues<int> propertyInfo = new JsonPropertyInfoValues<int>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModSelectionState),
			Converter = null,
			Getter = (object obj) => ((XWModSelectionState)obj).SchemaVersion,
			Setter = delegate(object obj, int value)
			{
				((XWModSelectionState)obj).SchemaVersion = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "SchemaVersion",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(XWModSelectionState).GetProperty("SchemaVersion", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<List<string>> propertyInfo2 = new JsonPropertyInfoValues<List<string>>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModSelectionState),
			Converter = null,
			Getter = (object obj) => ((XWModSelectionState)obj).EnabledIds,
			Setter = delegate(object obj, List<string>? value)
			{
				((XWModSelectionState)obj).EnabledIds = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "EnabledIds",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(XWModSelectionState).GetProperty("EnabledIds", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<string>), Array.Empty<Type>(), null)
		};
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		JsonPropertyInfoValues<List<string>> propertyInfo3 = new JsonPropertyInfoValues<List<string>>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModSelectionState),
			Converter = null,
			Getter = (object obj) => ((XWModSelectionState)obj).PreferredOrder,
			Setter = delegate(object obj, List<string>? value)
			{
				((XWModSelectionState)obj).PreferredOrder = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "PreferredOrder",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(XWModSelectionState).GetProperty("PreferredOrder", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<string>), Array.Empty<Type>(), null)
		};
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		return array;
	}

	private void XWModSelectionStateSerializeHandler(Utf8JsonWriter writer, XWModSelectionState? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartObject();
		writer.WriteNumber(PropName_schemaVersion, value.SchemaVersion);
		writer.WritePropertyName(PropName_enabledIds);
		ListStringSerializeHandler(writer, value.EnabledIds);
		writer.WritePropertyName(PropName_preferredOrder);
		ListStringSerializeHandler(writer, value.PreferredOrder);
		writer.WriteEndObject();
	}

	private JsonTypeInfo<List<XWModBootPackage>> Create_ListXWModBootPackage(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<XWModBootPackage>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<XWModBootPackage>> collectionInfo = new JsonCollectionInfoValues<List<XWModBootPackage>>
			{
				ObjectCreator = () => new List<XWModBootPackage>(),
				SerializeHandler = ListXWModBootPackageSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<XWModBootPackage>, XWModBootPackage>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private void ListXWModBootPackageSerializeHandler(Utf8JsonWriter writer, List<XWModBootPackage>? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartArray();
		for (int i = 0; i < value.Count; i++)
		{
			XWModBootPackageSerializeHandler(writer, value[i]);
		}
		writer.WriteEndArray();
	}

	private JsonTypeInfo<List<string>> Create_ListString(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<string>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<string>> collectionInfo = new JsonCollectionInfoValues<List<string>>
			{
				ObjectCreator = () => new List<string>(),
				SerializeHandler = ListStringSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<string>, string>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private void ListStringSerializeHandler(Utf8JsonWriter writer, List<string>? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartArray();
		for (int i = 0; i < value.Count; i++)
		{
			writer.WriteStringValue(value[i]);
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

	public XWModBootJsonContext()
		: base(null)
	{
	}

	public XWModBootJsonContext(JsonSerializerOptions options)
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
		if (type == typeof(XWModBootJournal))
		{
			return Create_XWModBootJournal(options);
		}
		if (type == typeof(XWModBootPackage))
		{
			return Create_XWModBootPackage(options);
		}
		if (type == typeof(XWModLastGoodState))
		{
			return Create_XWModLastGoodState(options);
		}
		if (type == typeof(XWModSelectionState))
		{
			return Create_XWModSelectionState(options);
		}
		if (type == typeof(List<XWModBootPackage>))
		{
			return Create_ListXWModBootPackage(options);
		}
		if (type == typeof(List<string>))
		{
			return Create_ListString(options);
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

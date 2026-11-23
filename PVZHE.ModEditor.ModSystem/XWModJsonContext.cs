using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using PVZHE.ModEditor.Core;

namespace PVZHE.ModEditor.ModSystem;

[JsonSourceGenerationOptions(WriteIndented = true, PropertyNameCaseInsensitive = true, ReadCommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true)]
[JsonSerializable(typeof(XWModManifest))]
[JsonSerializable(typeof(XWModDependency))]
[JsonSerializable(typeof(ModProject))]
[JsonSerializable(typeof(ModExporter.ModInfo))]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(XWResourceAutosaveDraft))]
[GeneratedCode("System.Text.Json.SourceGeneration", "9.0.14.26416")]
internal class XWModJsonContext : JsonSerializerContext, IJsonTypeInfoResolver
{
	private JsonTypeInfo<bool>? _Boolean;

	private JsonTypeInfo<double>? _Double;

	private JsonTypeInfo<double?>? _NullableDouble;

	private JsonTypeInfo<XWResourceAutosaveDraft>? _XWResourceAutosaveDraft;

	private JsonTypeInfo<ModExporter.ModInfo>? _ModInfo;

	private JsonTypeInfo<ModProject>? _ModProject;

	private JsonTypeInfo<XWModContentEntry>? _XWModContentEntry;

	private JsonTypeInfo<XWModContentTarget>? _XWModContentTarget;

	private JsonTypeInfo<XWModDependency>? _XWModDependency;

	private JsonTypeInfo<XWModLoadingPresentation>? _XWModLoadingPresentation;

	private JsonTypeInfo<XWModManifest>? _XWModManifest;

	private JsonTypeInfo<XWModSettingDefinition>? _XWModSettingDefinition;

	private JsonTypeInfo<XWModSettingOption>? _XWModSettingOption;

	private JsonTypeInfo<Dictionary<string, List<string>>>? _DictionaryStringListString;

	private JsonTypeInfo<List<XWModContentEntry>>? _ListXWModContentEntry;

	private JsonTypeInfo<List<XWModDependency>>? _ListXWModDependency;

	private JsonTypeInfo<List<XWModSettingDefinition>>? _ListXWModSettingDefinition;

	private JsonTypeInfo<List<XWModSettingOption>>? _ListXWModSettingOption;

	private JsonTypeInfo<List<string>>? _ListString;

	private JsonTypeInfo<DateTime>? _DateTime;

	private JsonTypeInfo<JsonElement>? _JsonElement;

	private JsonTypeInfo<JsonElement?>? _NullableJsonElement;

	private JsonTypeInfo<int>? _Int32;

	private JsonTypeInfo<int?>? _NullableInt32;

	private JsonTypeInfo<string>? _String;

	private static readonly JsonSerializerOptions s_defaultOptions = new JsonSerializerOptions
	{
		AllowTrailingCommas = true,
		PropertyNameCaseInsensitive = true,
		ReadCommentHandling = JsonCommentHandling.Skip,
		WriteIndented = true
	};

	private const BindingFlags InstanceMemberBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	private static readonly JsonEncodedText PropName_Path = JsonEncodedText.Encode("Path");

	private static readonly JsonEncodedText PropName_Category = JsonEncodedText.Encode("Category");

	private static readonly JsonEncodedText PropName_ResourceType = JsonEncodedText.Encode("ResourceType");

	private static readonly JsonEncodedText PropName_SavedAt = JsonEncodedText.Encode("SavedAt");

	private static readonly JsonEncodedText PropName_Name = JsonEncodedText.Encode("Name");

	private static readonly JsonEncodedText PropName_Version = JsonEncodedText.Encode("Version");

	private static readonly JsonEncodedText PropName_Author = JsonEncodedText.Encode("Author");

	private static readonly JsonEncodedText PropName_Description = JsonEncodedText.Encode("Description");

	private static readonly JsonEncodedText PropName_PlayInstructions = JsonEncodedText.Encode("PlayInstructions");

	private static readonly JsonEncodedText PropName_Files = JsonEncodedText.Encode("Files");

	private static readonly JsonEncodedText PropName_ExportDirectory = JsonEncodedText.Encode("ExportDirectory");

	private static readonly JsonEncodedText PropName_GameDirectory = JsonEncodedText.Encode("GameDirectory");

	private static readonly JsonEncodedText PropName_CreatedDate = JsonEncodedText.Encode("CreatedDate");

	private static readonly JsonEncodedText PropName_LastModifiedDate = JsonEncodedText.Encode("LastModifiedDate");

	private static readonly JsonEncodedText PropName_category = JsonEncodedText.Encode("category");

	private static readonly JsonEncodedText PropName_key = JsonEncodedText.Encode("key");

	private static readonly JsonEncodedText PropName_path = JsonEncodedText.Encode("path");

	private static readonly JsonEncodedText PropName_target = JsonEncodedText.Encode("target");

	private static readonly JsonEncodedText PropName_providerModId = JsonEncodedText.Encode("providerModId");

	private static readonly JsonEncodedText PropName_id = JsonEncodedText.Encode("id");

	private static readonly JsonEncodedText PropName_version = JsonEncodedText.Encode("version");

	private static readonly JsonEncodedText PropName_versionRange = JsonEncodedText.Encode("versionRange");

	private static readonly JsonEncodedText PropName_startupScene = JsonEncodedText.Encode("startupScene");

	private static readonly JsonEncodedText PropName_transitionScene = JsonEncodedText.Encode("transitionScene");

	private static readonly JsonEncodedText PropName_schemaVersion = JsonEncodedText.Encode("schemaVersion");

	private static readonly JsonEncodedText PropName_name = JsonEncodedText.Encode("name");

	private static readonly JsonEncodedText PropName_postId = JsonEncodedText.Encode("postId");

	private static readonly JsonEncodedText PropName_author = JsonEncodedText.Encode("author");

	private static readonly JsonEncodedText PropName_description = JsonEncodedText.Encode("description");

	private static readonly JsonEncodedText PropName_playInstructions = JsonEncodedText.Encode("playInstructions");

	private static readonly JsonEncodedText PropName_dependencies = JsonEncodedText.Encode("dependencies");

	private static readonly JsonEncodedText PropName_optionalDependencies = JsonEncodedText.Encode("optionalDependencies");

	private static readonly JsonEncodedText PropName_loadBefore = JsonEncodedText.Encode("loadBefore");

	private static readonly JsonEncodedText PropName_loadAfter = JsonEncodedText.Encode("loadAfter");

	private static readonly JsonEncodedText PropName_requiredCapabilities = JsonEncodedText.Encode("requiredCapabilities");

	private static readonly JsonEncodedText PropName_sharedAssemblies = JsonEncodedText.Encode("sharedAssemblies");

	private static readonly JsonEncodedText PropName_contentEntries = JsonEncodedText.Encode("contentEntries");

	private static readonly JsonEncodedText PropName_privateResources = JsonEncodedText.Encode("privateResources");

	private static readonly JsonEncodedText PropName_loadingPresentation = JsonEncodedText.Encode("loadingPresentation");

	private static readonly JsonEncodedText PropName_conflicts = JsonEncodedText.Encode("conflicts");

	private static readonly JsonEncodedText PropName_provides = JsonEncodedText.Encode("provides");

	private static readonly JsonEncodedText PropName_overrides = JsonEncodedText.Encode("overrides");

	private static readonly JsonEncodedText PropName_scripts = JsonEncodedText.Encode("scripts");

	private static readonly JsonEncodedText PropName_settings = JsonEncodedText.Encode("settings");

	private static readonly JsonEncodedText PropName_runtimeAssembly = JsonEncodedText.Encode("runtimeAssembly");

	private static readonly JsonEncodedText PropName_runtimeEntryType = JsonEncodedText.Encode("runtimeEntryType");

	private static readonly JsonEncodedText PropName_runtimeApiVersion = JsonEncodedText.Encode("runtimeApiVersion");

	private static readonly JsonEncodedText PropName_runtimeAssemblyPolicy = JsonEncodedText.Encode("runtimeAssemblyPolicy");

	private static readonly JsonEncodedText PropName_blueprints = JsonEncodedText.Encode("blueprints");

	private static readonly JsonEncodedText PropName_translations = JsonEncodedText.Encode("translations");

	private static readonly JsonEncodedText PropName_resources = JsonEncodedText.Encode("resources");

	private static readonly JsonEncodedText PropName_type = JsonEncodedText.Encode("type");

	private static readonly JsonEncodedText PropName_label = JsonEncodedText.Encode("label");

	private static readonly JsonEncodedText PropName_content = JsonEncodedText.Encode("content");

	private static readonly JsonEncodedText PropName_group = JsonEncodedText.Encode("group");

	private static readonly JsonEncodedText PropName_visibleWhen = JsonEncodedText.Encode("visibleWhen");

	private static readonly JsonEncodedText PropName_enabledWhen = JsonEncodedText.Encode("enabledWhen");

	private static readonly JsonEncodedText PropName_default = JsonEncodedText.Encode("default");

	private static readonly JsonEncodedText PropName_min = JsonEncodedText.Encode("min");

	private static readonly JsonEncodedText PropName_max = JsonEncodedText.Encode("max");

	private static readonly JsonEncodedText PropName_step = JsonEncodedText.Encode("step");

	private static readonly JsonEncodedText PropName_options = JsonEncodedText.Encode("options");

	private static readonly JsonEncodedText PropName_minSelected = JsonEncodedText.Encode("minSelected");

	private static readonly JsonEncodedText PropName_maxSelected = JsonEncodedText.Encode("maxSelected");

	private static readonly JsonEncodedText PropName_applyMode = JsonEncodedText.Encode("applyMode");

	private static readonly JsonEncodedText PropName_affectsGameplay = JsonEncodedText.Encode("affectsGameplay");

	private static readonly JsonEncodedText PropName_value = JsonEncodedText.Encode("value");

	public JsonTypeInfo<bool> Boolean => _Boolean ?? (_Boolean = (JsonTypeInfo<bool>)base.Options.GetTypeInfo(typeof(bool)));

	public JsonTypeInfo<double> Double => _Double ?? (_Double = (JsonTypeInfo<double>)base.Options.GetTypeInfo(typeof(double)));

	public JsonTypeInfo<double?> NullableDouble => _NullableDouble ?? (_NullableDouble = (JsonTypeInfo<double?>)base.Options.GetTypeInfo(typeof(double?)));

	public JsonTypeInfo<XWResourceAutosaveDraft> XWResourceAutosaveDraft => _XWResourceAutosaveDraft ?? (_XWResourceAutosaveDraft = (JsonTypeInfo<XWResourceAutosaveDraft>)base.Options.GetTypeInfo(typeof(XWResourceAutosaveDraft)));

	public JsonTypeInfo<ModExporter.ModInfo> ModInfo => _ModInfo ?? (_ModInfo = (JsonTypeInfo<ModExporter.ModInfo>)base.Options.GetTypeInfo(typeof(ModExporter.ModInfo)));

	public JsonTypeInfo<ModProject> ModProject => _ModProject ?? (_ModProject = (JsonTypeInfo<ModProject>)base.Options.GetTypeInfo(typeof(ModProject)));

	public JsonTypeInfo<XWModContentEntry> XWModContentEntry => _XWModContentEntry ?? (_XWModContentEntry = (JsonTypeInfo<XWModContentEntry>)base.Options.GetTypeInfo(typeof(XWModContentEntry)));

	public JsonTypeInfo<XWModContentTarget> XWModContentTarget => _XWModContentTarget ?? (_XWModContentTarget = (JsonTypeInfo<XWModContentTarget>)base.Options.GetTypeInfo(typeof(XWModContentTarget)));

	public JsonTypeInfo<XWModDependency> XWModDependency => _XWModDependency ?? (_XWModDependency = (JsonTypeInfo<XWModDependency>)base.Options.GetTypeInfo(typeof(XWModDependency)));

	public JsonTypeInfo<XWModLoadingPresentation> XWModLoadingPresentation => _XWModLoadingPresentation ?? (_XWModLoadingPresentation = (JsonTypeInfo<XWModLoadingPresentation>)base.Options.GetTypeInfo(typeof(XWModLoadingPresentation)));

	public JsonTypeInfo<XWModManifest> XWModManifest => _XWModManifest ?? (_XWModManifest = (JsonTypeInfo<XWModManifest>)base.Options.GetTypeInfo(typeof(XWModManifest)));

	public JsonTypeInfo<XWModSettingDefinition> XWModSettingDefinition => _XWModSettingDefinition ?? (_XWModSettingDefinition = (JsonTypeInfo<XWModSettingDefinition>)base.Options.GetTypeInfo(typeof(XWModSettingDefinition)));

	public JsonTypeInfo<XWModSettingOption> XWModSettingOption => _XWModSettingOption ?? (_XWModSettingOption = (JsonTypeInfo<XWModSettingOption>)base.Options.GetTypeInfo(typeof(XWModSettingOption)));

	public JsonTypeInfo<Dictionary<string, List<string>>> DictionaryStringListString => _DictionaryStringListString ?? (_DictionaryStringListString = (JsonTypeInfo<Dictionary<string, List<string>>>)base.Options.GetTypeInfo(typeof(Dictionary<string, List<string>>)));

	public JsonTypeInfo<List<XWModContentEntry>> ListXWModContentEntry => _ListXWModContentEntry ?? (_ListXWModContentEntry = (JsonTypeInfo<List<XWModContentEntry>>)base.Options.GetTypeInfo(typeof(List<XWModContentEntry>)));

	public JsonTypeInfo<List<XWModDependency>> ListXWModDependency => _ListXWModDependency ?? (_ListXWModDependency = (JsonTypeInfo<List<XWModDependency>>)base.Options.GetTypeInfo(typeof(List<XWModDependency>)));

	public JsonTypeInfo<List<XWModSettingDefinition>> ListXWModSettingDefinition => _ListXWModSettingDefinition ?? (_ListXWModSettingDefinition = (JsonTypeInfo<List<XWModSettingDefinition>>)base.Options.GetTypeInfo(typeof(List<XWModSettingDefinition>)));

	public JsonTypeInfo<List<XWModSettingOption>> ListXWModSettingOption => _ListXWModSettingOption ?? (_ListXWModSettingOption = (JsonTypeInfo<List<XWModSettingOption>>)base.Options.GetTypeInfo(typeof(List<XWModSettingOption>)));

	public JsonTypeInfo<List<string>> ListString => _ListString ?? (_ListString = (JsonTypeInfo<List<string>>)base.Options.GetTypeInfo(typeof(List<string>)));

	public JsonTypeInfo<DateTime> DateTime => _DateTime ?? (_DateTime = (JsonTypeInfo<DateTime>)base.Options.GetTypeInfo(typeof(DateTime)));

	public JsonTypeInfo<JsonElement> JsonElement => _JsonElement ?? (_JsonElement = (JsonTypeInfo<JsonElement>)base.Options.GetTypeInfo(typeof(JsonElement)));

	public JsonTypeInfo<JsonElement?> NullableJsonElement => _NullableJsonElement ?? (_NullableJsonElement = (JsonTypeInfo<JsonElement?>)base.Options.GetTypeInfo(typeof(JsonElement?)));

	public JsonTypeInfo<int> Int32 => _Int32 ?? (_Int32 = (JsonTypeInfo<int>)base.Options.GetTypeInfo(typeof(int)));

	public JsonTypeInfo<int?> NullableInt32 => _NullableInt32 ?? (_NullableInt32 = (JsonTypeInfo<int?>)base.Options.GetTypeInfo(typeof(int?)));

	public JsonTypeInfo<string> String => _String ?? (_String = (JsonTypeInfo<string>)base.Options.GetTypeInfo(typeof(string)));

	public static XWModJsonContext Default { get; } = new XWModJsonContext(new JsonSerializerOptions(s_defaultOptions));

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

	private JsonTypeInfo<double?> Create_NullableDouble(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<double?> jsonTypeInfo))
		{
			JsonConverter nullableConverter = JsonMetadataServices.GetNullableConverter<double>(options);
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<double?>(options, nullableConverter);
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<XWResourceAutosaveDraft> Create_XWResourceAutosaveDraft(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<XWResourceAutosaveDraft> jsonTypeInfo))
		{
			JsonObjectInfoValues<XWResourceAutosaveDraft> objectInfo = new JsonObjectInfoValues<XWResourceAutosaveDraft>
			{
				ObjectCreator = () => new XWResourceAutosaveDraft(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => XWResourceAutosaveDraftPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(XWResourceAutosaveDraft).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = XWResourceAutosaveDraftSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] XWResourceAutosaveDraftPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[4];
		JsonPropertyInfoValues<string> propertyInfo = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWResourceAutosaveDraft),
			Converter = null,
			Getter = (object obj) => ((XWResourceAutosaveDraft)obj).Path,
			Setter = delegate(object obj, string? value)
			{
				((XWResourceAutosaveDraft)obj).Path = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Path",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(XWResourceAutosaveDraft).GetProperty("Path", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<string> propertyInfo2 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWResourceAutosaveDraft),
			Converter = null,
			Getter = (object obj) => ((XWResourceAutosaveDraft)obj).Category,
			Setter = delegate(object obj, string? value)
			{
				((XWResourceAutosaveDraft)obj).Category = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Category",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(XWResourceAutosaveDraft).GetProperty("Category", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		JsonPropertyInfoValues<string> propertyInfo3 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWResourceAutosaveDraft),
			Converter = null,
			Getter = (object obj) => ((XWResourceAutosaveDraft)obj).ResourceType,
			Setter = delegate(object obj, string? value)
			{
				((XWResourceAutosaveDraft)obj).ResourceType = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "ResourceType",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(XWResourceAutosaveDraft).GetProperty("ResourceType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		JsonPropertyInfoValues<string> propertyInfo4 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWResourceAutosaveDraft),
			Converter = null,
			Getter = (object obj) => ((XWResourceAutosaveDraft)obj).SavedAt,
			Setter = delegate(object obj, string? value)
			{
				((XWResourceAutosaveDraft)obj).SavedAt = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "SavedAt",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(XWResourceAutosaveDraft).GetProperty("SavedAt", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		return array;
	}

	private void XWResourceAutosaveDraftSerializeHandler(Utf8JsonWriter writer, XWResourceAutosaveDraft? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartObject();
		writer.WriteString(PropName_Path, value.Path);
		writer.WriteString(PropName_Category, value.Category);
		writer.WriteString(PropName_ResourceType, value.ResourceType);
		writer.WriteString(PropName_SavedAt, value.SavedAt);
		writer.WriteEndObject();
	}

	private JsonTypeInfo<ModExporter.ModInfo> Create_ModInfo(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<ModExporter.ModInfo> jsonTypeInfo))
		{
			JsonObjectInfoValues<ModExporter.ModInfo> objectInfo = new JsonObjectInfoValues<ModExporter.ModInfo>
			{
				ObjectCreator = () => new ModExporter.ModInfo(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => ModInfoPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(ModExporter.ModInfo).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = ModInfoSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] ModInfoPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[6];
		JsonPropertyInfoValues<string> propertyInfo = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(ModExporter.ModInfo),
			Converter = null,
			Getter = (object obj) => ((ModExporter.ModInfo)obj).Name,
			Setter = delegate(object obj, string? value)
			{
				((ModExporter.ModInfo)obj).Name = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Name",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(ModExporter.ModInfo).GetProperty("Name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<string> propertyInfo2 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(ModExporter.ModInfo),
			Converter = null,
			Getter = (object obj) => ((ModExporter.ModInfo)obj).Version,
			Setter = delegate(object obj, string? value)
			{
				((ModExporter.ModInfo)obj).Version = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Version",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(ModExporter.ModInfo).GetProperty("Version", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		JsonPropertyInfoValues<string> propertyInfo3 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(ModExporter.ModInfo),
			Converter = null,
			Getter = (object obj) => ((ModExporter.ModInfo)obj).Author,
			Setter = delegate(object obj, string? value)
			{
				((ModExporter.ModInfo)obj).Author = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Author",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(ModExporter.ModInfo).GetProperty("Author", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		JsonPropertyInfoValues<string> propertyInfo4 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(ModExporter.ModInfo),
			Converter = null,
			Getter = (object obj) => ((ModExporter.ModInfo)obj).Description,
			Setter = delegate(object obj, string? value)
			{
				((ModExporter.ModInfo)obj).Description = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Description",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(ModExporter.ModInfo).GetProperty("Description", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		JsonPropertyInfoValues<string> propertyInfo5 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(ModExporter.ModInfo),
			Converter = null,
			Getter = (object obj) => ((ModExporter.ModInfo)obj).PlayInstructions,
			Setter = delegate(object obj, string? value)
			{
				((ModExporter.ModInfo)obj).PlayInstructions = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "PlayInstructions",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(ModExporter.ModInfo).GetProperty("PlayInstructions", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[4] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo5);
		JsonPropertyInfoValues<List<string>> propertyInfo6 = new JsonPropertyInfoValues<List<string>>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(ModExporter.ModInfo),
			Converter = null,
			Getter = (object obj) => ((ModExporter.ModInfo)obj).Files,
			Setter = delegate(object obj, List<string>? value)
			{
				((ModExporter.ModInfo)obj).Files = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Files",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(ModExporter.ModInfo).GetProperty("Files", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<string>), Array.Empty<Type>(), null)
		};
		array[5] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo6);
		return array;
	}

	private void ModInfoSerializeHandler(Utf8JsonWriter writer, ModExporter.ModInfo? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartObject();
		writer.WriteString(PropName_Name, value.Name);
		writer.WriteString(PropName_Version, value.Version);
		writer.WriteString(PropName_Author, value.Author);
		writer.WriteString(PropName_Description, value.Description);
		writer.WriteString(PropName_PlayInstructions, value.PlayInstructions);
		writer.WritePropertyName(PropName_Files);
		ListStringSerializeHandler(writer, value.Files);
		writer.WriteEndObject();
	}

	private JsonTypeInfo<ModProject> Create_ModProject(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<ModProject> jsonTypeInfo))
		{
			JsonObjectInfoValues<ModProject> objectInfo = new JsonObjectInfoValues<ModProject>
			{
				ObjectCreator = () => new ModProject(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => ModProjectPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(ModProject).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = ModProjectSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] ModProjectPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[11];
		JsonPropertyInfoValues<string> propertyInfo = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(ModProject),
			Converter = null,
			Getter = (object obj) => ((ModProject)obj).Name,
			Setter = delegate(object obj, string? value)
			{
				((ModProject)obj).Name = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Name",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(ModProject).GetProperty("Name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<string> propertyInfo2 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(ModProject),
			Converter = null,
			Getter = (object obj) => ((ModProject)obj).Version,
			Setter = delegate(object obj, string? value)
			{
				((ModProject)obj).Version = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Version",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(ModProject).GetProperty("Version", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		JsonPropertyInfoValues<string> propertyInfo3 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(ModProject),
			Converter = null,
			Getter = (object obj) => ((ModProject)obj).Author,
			Setter = delegate(object obj, string? value)
			{
				((ModProject)obj).Author = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Author",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(ModProject).GetProperty("Author", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		JsonPropertyInfoValues<string> propertyInfo4 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(ModProject),
			Converter = null,
			Getter = (object obj) => ((ModProject)obj).Description,
			Setter = delegate(object obj, string? value)
			{
				((ModProject)obj).Description = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Description",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(ModProject).GetProperty("Description", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		JsonPropertyInfoValues<string> propertyInfo5 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(ModProject),
			Converter = null,
			Getter = (object obj) => ((ModProject)obj).PlayInstructions,
			Setter = delegate(object obj, string? value)
			{
				((ModProject)obj).PlayInstructions = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "PlayInstructions",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(ModProject).GetProperty("PlayInstructions", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[4] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo5);
		JsonPropertyInfoValues<string> propertyInfo6 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(ModProject),
			Converter = null,
			Getter = (object obj) => ((ModProject)obj).ExportDirectory,
			Setter = delegate(object obj, string? value)
			{
				((ModProject)obj).ExportDirectory = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "ExportDirectory",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(ModProject).GetProperty("ExportDirectory", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[5] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo6);
		JsonPropertyInfoValues<string> propertyInfo7 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(ModProject),
			Converter = null,
			Getter = (object obj) => ((ModProject)obj).GameDirectory,
			Setter = delegate(object obj, string? value)
			{
				((ModProject)obj).GameDirectory = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "GameDirectory",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(ModProject).GetProperty("GameDirectory", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[6] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo7);
		JsonPropertyInfoValues<DateTime> propertyInfo8 = new JsonPropertyInfoValues<DateTime>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(ModProject),
			Converter = null,
			Getter = (object obj) => ((ModProject)obj).CreatedDate,
			Setter = delegate(object obj, DateTime value)
			{
				((ModProject)obj).CreatedDate = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "CreatedDate",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(ModProject).GetProperty("CreatedDate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(DateTime), Array.Empty<Type>(), null)
		};
		array[7] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo8);
		JsonPropertyInfoValues<DateTime> propertyInfo9 = new JsonPropertyInfoValues<DateTime>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(ModProject),
			Converter = null,
			Getter = (object obj) => ((ModProject)obj).LastModifiedDate,
			Setter = delegate(object obj, DateTime value)
			{
				((ModProject)obj).LastModifiedDate = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "LastModifiedDate",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(ModProject).GetProperty("LastModifiedDate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(DateTime), Array.Empty<Type>(), null)
		};
		array[8] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo9);
		JsonPropertyInfoValues<string> propertyInfo10 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(ModProject),
			Converter = null,
			Getter = null,
			Setter = null,
			IgnoreCondition = JsonIgnoreCondition.Always,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "ProjectPath",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(ModProject).GetProperty("ProjectPath", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[9] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo10);
		JsonPropertyInfoValues<string> propertyInfo11 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(ModProject),
			Converter = null,
			Getter = null,
			Setter = null,
			IgnoreCondition = JsonIgnoreCondition.Always,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "ProjectFilePath",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(ModProject).GetProperty("ProjectFilePath", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[10] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo11);
		return array;
	}

	private void ModProjectSerializeHandler(Utf8JsonWriter writer, ModProject? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartObject();
		writer.WriteString(PropName_Name, value.Name);
		writer.WriteString(PropName_Version, value.Version);
		writer.WriteString(PropName_Author, value.Author);
		writer.WriteString(PropName_Description, value.Description);
		writer.WriteString(PropName_PlayInstructions, value.PlayInstructions);
		writer.WriteString(PropName_ExportDirectory, value.ExportDirectory);
		writer.WriteString(PropName_GameDirectory, value.GameDirectory);
		writer.WriteString(PropName_CreatedDate, value.CreatedDate);
		writer.WriteString(PropName_LastModifiedDate, value.LastModifiedDate);
		writer.WriteEndObject();
	}

	private JsonTypeInfo<XWModContentEntry> Create_XWModContentEntry(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<XWModContentEntry> jsonTypeInfo))
		{
			JsonObjectInfoValues<XWModContentEntry> objectInfo = new JsonObjectInfoValues<XWModContentEntry>
			{
				ObjectCreator = () => new XWModContentEntry(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => XWModContentEntryPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(XWModContentEntry).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = XWModContentEntrySerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] XWModContentEntryPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[4];
		JsonPropertyInfoValues<string> propertyInfo = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModContentEntry),
			Converter = null,
			Getter = (object obj) => ((XWModContentEntry)obj).Category,
			Setter = delegate(object obj, string? value)
			{
				((XWModContentEntry)obj).Category = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Category",
			JsonPropertyName = "category",
			AttributeProviderFactory = () => typeof(XWModContentEntry).GetProperty("Category", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<string> propertyInfo2 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModContentEntry),
			Converter = null,
			Getter = (object obj) => ((XWModContentEntry)obj).Key,
			Setter = delegate(object obj, string? value)
			{
				((XWModContentEntry)obj).Key = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Key",
			JsonPropertyName = "key",
			AttributeProviderFactory = () => typeof(XWModContentEntry).GetProperty("Key", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		JsonPropertyInfoValues<string> propertyInfo3 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModContentEntry),
			Converter = null,
			Getter = (object obj) => ((XWModContentEntry)obj).Path,
			Setter = delegate(object obj, string? value)
			{
				((XWModContentEntry)obj).Path = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Path",
			JsonPropertyName = "path",
			AttributeProviderFactory = () => typeof(XWModContentEntry).GetProperty("Path", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		JsonPropertyInfoValues<XWModContentTarget> propertyInfo4 = new JsonPropertyInfoValues<XWModContentTarget>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModContentEntry),
			Converter = null,
			Getter = (object obj) => ((XWModContentEntry)obj).Target,
			Setter = delegate(object obj, XWModContentTarget? value)
			{
				((XWModContentEntry)obj).Target = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Target",
			JsonPropertyName = "target",
			AttributeProviderFactory = () => typeof(XWModContentEntry).GetProperty("Target", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(XWModContentTarget), Array.Empty<Type>(), null)
		};
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		return array;
	}

	private void XWModContentEntrySerializeHandler(Utf8JsonWriter writer, XWModContentEntry? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartObject();
		writer.WriteString(PropName_category, value.Category);
		writer.WriteString(PropName_key, value.Key);
		writer.WriteString(PropName_path, value.Path);
		writer.WritePropertyName(PropName_target);
		XWModContentTargetSerializeHandler(writer, value.Target);
		writer.WriteEndObject();
	}

	private JsonTypeInfo<XWModContentTarget> Create_XWModContentTarget(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<XWModContentTarget> jsonTypeInfo))
		{
			JsonObjectInfoValues<XWModContentTarget> objectInfo = new JsonObjectInfoValues<XWModContentTarget>
			{
				ObjectCreator = () => new XWModContentTarget(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => XWModContentTargetPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(XWModContentTarget).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = XWModContentTargetSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] XWModContentTargetPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[3];
		JsonPropertyInfoValues<string> propertyInfo = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModContentTarget),
			Converter = null,
			Getter = (object obj) => ((XWModContentTarget)obj).ProviderModId,
			Setter = delegate(object obj, string? value)
			{
				((XWModContentTarget)obj).ProviderModId = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "ProviderModId",
			JsonPropertyName = "providerModId",
			AttributeProviderFactory = () => typeof(XWModContentTarget).GetProperty("ProviderModId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<string> propertyInfo2 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModContentTarget),
			Converter = null,
			Getter = (object obj) => ((XWModContentTarget)obj).Category,
			Setter = delegate(object obj, string? value)
			{
				((XWModContentTarget)obj).Category = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Category",
			JsonPropertyName = "category",
			AttributeProviderFactory = () => typeof(XWModContentTarget).GetProperty("Category", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		JsonPropertyInfoValues<string> propertyInfo3 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModContentTarget),
			Converter = null,
			Getter = (object obj) => ((XWModContentTarget)obj).Key,
			Setter = delegate(object obj, string? value)
			{
				((XWModContentTarget)obj).Key = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Key",
			JsonPropertyName = "key",
			AttributeProviderFactory = () => typeof(XWModContentTarget).GetProperty("Key", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		return array;
	}

	private void XWModContentTargetSerializeHandler(Utf8JsonWriter writer, XWModContentTarget? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartObject();
		writer.WriteString(PropName_providerModId, value.ProviderModId);
		writer.WriteString(PropName_category, value.Category);
		writer.WriteString(PropName_key, value.Key);
		writer.WriteEndObject();
	}

	private JsonTypeInfo<XWModDependency> Create_XWModDependency(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<XWModDependency> jsonTypeInfo))
		{
			JsonObjectInfoValues<XWModDependency> objectInfo = new JsonObjectInfoValues<XWModDependency>
			{
				ObjectCreator = () => new XWModDependency(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => XWModDependencyPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(XWModDependency).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = XWModDependencySerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] XWModDependencyPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[3];
		JsonPropertyInfoValues<string> propertyInfo = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModDependency),
			Converter = null,
			Getter = (object obj) => ((XWModDependency)obj).Id,
			Setter = delegate(object obj, string? value)
			{
				((XWModDependency)obj).Id = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Id",
			JsonPropertyName = "id",
			AttributeProviderFactory = () => typeof(XWModDependency).GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<string> propertyInfo2 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModDependency),
			Converter = null,
			Getter = (object obj) => ((XWModDependency)obj).Version,
			Setter = delegate(object obj, string? value)
			{
				((XWModDependency)obj).Version = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Version",
			JsonPropertyName = "version",
			AttributeProviderFactory = () => typeof(XWModDependency).GetProperty("Version", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		JsonPropertyInfoValues<string> propertyInfo3 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModDependency),
			Converter = null,
			Getter = (object obj) => ((XWModDependency)obj).VersionRange,
			Setter = delegate(object obj, string? value)
			{
				((XWModDependency)obj).VersionRange = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "VersionRange",
			JsonPropertyName = "versionRange",
			AttributeProviderFactory = () => typeof(XWModDependency).GetProperty("VersionRange", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		return array;
	}

	private void XWModDependencySerializeHandler(Utf8JsonWriter writer, XWModDependency? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartObject();
		writer.WriteString(PropName_id, value.Id);
		writer.WriteString(PropName_version, value.Version);
		writer.WriteString(PropName_versionRange, value.VersionRange);
		writer.WriteEndObject();
	}

	private JsonTypeInfo<XWModLoadingPresentation> Create_XWModLoadingPresentation(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<XWModLoadingPresentation> jsonTypeInfo))
		{
			JsonObjectInfoValues<XWModLoadingPresentation> objectInfo = new JsonObjectInfoValues<XWModLoadingPresentation>
			{
				ObjectCreator = () => new XWModLoadingPresentation(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => XWModLoadingPresentationPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(XWModLoadingPresentation).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = XWModLoadingPresentationSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] XWModLoadingPresentationPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[2];
		JsonPropertyInfoValues<string> propertyInfo = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModLoadingPresentation),
			Converter = null,
			Getter = (object obj) => ((XWModLoadingPresentation)obj).StartupScene,
			Setter = delegate(object obj, string? value)
			{
				((XWModLoadingPresentation)obj).StartupScene = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "StartupScene",
			JsonPropertyName = "startupScene",
			AttributeProviderFactory = () => typeof(XWModLoadingPresentation).GetProperty("StartupScene", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<string> propertyInfo2 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModLoadingPresentation),
			Converter = null,
			Getter = (object obj) => ((XWModLoadingPresentation)obj).TransitionScene,
			Setter = delegate(object obj, string? value)
			{
				((XWModLoadingPresentation)obj).TransitionScene = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "TransitionScene",
			JsonPropertyName = "transitionScene",
			AttributeProviderFactory = () => typeof(XWModLoadingPresentation).GetProperty("TransitionScene", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		return array;
	}

	private void XWModLoadingPresentationSerializeHandler(Utf8JsonWriter writer, XWModLoadingPresentation? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartObject();
		writer.WriteString(PropName_startupScene, value.StartupScene);
		writer.WriteString(PropName_transitionScene, value.TransitionScene);
		writer.WriteEndObject();
	}

	private JsonTypeInfo<XWModManifest> Create_XWModManifest(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<XWModManifest> jsonTypeInfo))
		{
			JsonObjectInfoValues<XWModManifest> objectInfo = new JsonObjectInfoValues<XWModManifest>
			{
				ObjectCreator = () => new XWModManifest(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => XWModManifestPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(XWModManifest).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = XWModManifestSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] XWModManifestPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[29];
		JsonPropertyInfoValues<int> propertyInfo = new JsonPropertyInfoValues<int>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModManifest),
			Converter = null,
			Getter = (object obj) => ((XWModManifest)obj).SchemaVersion,
			Setter = delegate(object obj, int value)
			{
				((XWModManifest)obj).SchemaVersion = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "SchemaVersion",
			JsonPropertyName = "schemaVersion",
			AttributeProviderFactory = () => typeof(XWModManifest).GetProperty("SchemaVersion", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<string> propertyInfo2 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModManifest),
			Converter = null,
			Getter = (object obj) => ((XWModManifest)obj).Id,
			Setter = delegate(object obj, string? value)
			{
				((XWModManifest)obj).Id = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Id",
			JsonPropertyName = "id",
			AttributeProviderFactory = () => typeof(XWModManifest).GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		JsonPropertyInfoValues<string> propertyInfo3 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModManifest),
			Converter = null,
			Getter = (object obj) => ((XWModManifest)obj).Name,
			Setter = delegate(object obj, string? value)
			{
				((XWModManifest)obj).Name = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Name",
			JsonPropertyName = "name",
			AttributeProviderFactory = () => typeof(XWModManifest).GetProperty("Name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		JsonPropertyInfoValues<string> propertyInfo4 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModManifest),
			Converter = null,
			Getter = (object obj) => ((XWModManifest)obj).Version,
			Setter = delegate(object obj, string? value)
			{
				((XWModManifest)obj).Version = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Version",
			JsonPropertyName = "version",
			AttributeProviderFactory = () => typeof(XWModManifest).GetProperty("Version", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		JsonPropertyInfoValues<string> propertyInfo5 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModManifest),
			Converter = null,
			Getter = (object obj) => ((XWModManifest)obj).PostId,
			Setter = delegate(object obj, string? value)
			{
				((XWModManifest)obj).PostId = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "PostId",
			JsonPropertyName = "postId",
			AttributeProviderFactory = () => typeof(XWModManifest).GetProperty("PostId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[4] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo5);
		JsonPropertyInfoValues<string> propertyInfo6 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModManifest),
			Converter = null,
			Getter = (object obj) => ((XWModManifest)obj).Author,
			Setter = delegate(object obj, string? value)
			{
				((XWModManifest)obj).Author = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Author",
			JsonPropertyName = "author",
			AttributeProviderFactory = () => typeof(XWModManifest).GetProperty("Author", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[5] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo6);
		JsonPropertyInfoValues<string> propertyInfo7 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModManifest),
			Converter = null,
			Getter = (object obj) => ((XWModManifest)obj).Description,
			Setter = delegate(object obj, string? value)
			{
				((XWModManifest)obj).Description = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Description",
			JsonPropertyName = "description",
			AttributeProviderFactory = () => typeof(XWModManifest).GetProperty("Description", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[6] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo7);
		JsonPropertyInfoValues<string> propertyInfo8 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModManifest),
			Converter = null,
			Getter = (object obj) => ((XWModManifest)obj).PlayInstructions,
			Setter = delegate(object obj, string? value)
			{
				((XWModManifest)obj).PlayInstructions = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "PlayInstructions",
			JsonPropertyName = "playInstructions",
			AttributeProviderFactory = () => typeof(XWModManifest).GetProperty("PlayInstructions", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[7] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo8);
		JsonPropertyInfoValues<List<XWModDependency>> propertyInfo9 = new JsonPropertyInfoValues<List<XWModDependency>>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModManifest),
			Converter = null,
			Getter = (object obj) => ((XWModManifest)obj).Dependencies,
			Setter = delegate(object obj, List<XWModDependency>? value)
			{
				((XWModManifest)obj).Dependencies = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Dependencies",
			JsonPropertyName = "dependencies",
			AttributeProviderFactory = () => typeof(XWModManifest).GetProperty("Dependencies", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<XWModDependency>), Array.Empty<Type>(), null)
		};
		array[8] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo9);
		JsonPropertyInfoValues<List<XWModDependency>> propertyInfo10 = new JsonPropertyInfoValues<List<XWModDependency>>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModManifest),
			Converter = null,
			Getter = (object obj) => ((XWModManifest)obj).OptionalDependencies,
			Setter = delegate(object obj, List<XWModDependency>? value)
			{
				((XWModManifest)obj).OptionalDependencies = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "OptionalDependencies",
			JsonPropertyName = "optionalDependencies",
			AttributeProviderFactory = () => typeof(XWModManifest).GetProperty("OptionalDependencies", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<XWModDependency>), Array.Empty<Type>(), null)
		};
		array[9] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo10);
		JsonPropertyInfoValues<List<string>> propertyInfo11 = new JsonPropertyInfoValues<List<string>>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModManifest),
			Converter = null,
			Getter = (object obj) => ((XWModManifest)obj).LoadBefore,
			Setter = delegate(object obj, List<string>? value)
			{
				((XWModManifest)obj).LoadBefore = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "LoadBefore",
			JsonPropertyName = "loadBefore",
			AttributeProviderFactory = () => typeof(XWModManifest).GetProperty("LoadBefore", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<string>), Array.Empty<Type>(), null)
		};
		array[10] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo11);
		JsonPropertyInfoValues<List<string>> propertyInfo12 = new JsonPropertyInfoValues<List<string>>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModManifest),
			Converter = null,
			Getter = (object obj) => ((XWModManifest)obj).LoadAfter,
			Setter = delegate(object obj, List<string>? value)
			{
				((XWModManifest)obj).LoadAfter = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "LoadAfter",
			JsonPropertyName = "loadAfter",
			AttributeProviderFactory = () => typeof(XWModManifest).GetProperty("LoadAfter", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<string>), Array.Empty<Type>(), null)
		};
		array[11] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo12);
		JsonPropertyInfoValues<List<string>> propertyInfo13 = new JsonPropertyInfoValues<List<string>>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModManifest),
			Converter = null,
			Getter = (object obj) => ((XWModManifest)obj).RequiredCapabilities,
			Setter = delegate(object obj, List<string>? value)
			{
				((XWModManifest)obj).RequiredCapabilities = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "RequiredCapabilities",
			JsonPropertyName = "requiredCapabilities",
			AttributeProviderFactory = () => typeof(XWModManifest).GetProperty("RequiredCapabilities", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<string>), Array.Empty<Type>(), null)
		};
		array[12] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo13);
		JsonPropertyInfoValues<List<string>> propertyInfo14 = new JsonPropertyInfoValues<List<string>>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModManifest),
			Converter = null,
			Getter = (object obj) => ((XWModManifest)obj).SharedAssemblies,
			Setter = delegate(object obj, List<string>? value)
			{
				((XWModManifest)obj).SharedAssemblies = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "SharedAssemblies",
			JsonPropertyName = "sharedAssemblies",
			AttributeProviderFactory = () => typeof(XWModManifest).GetProperty("SharedAssemblies", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<string>), Array.Empty<Type>(), null)
		};
		array[13] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo14);
		JsonPropertyInfoValues<List<XWModContentEntry>> propertyInfo15 = new JsonPropertyInfoValues<List<XWModContentEntry>>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModManifest),
			Converter = null,
			Getter = (object obj) => ((XWModManifest)obj).ContentEntries,
			Setter = delegate(object obj, List<XWModContentEntry>? value)
			{
				((XWModManifest)obj).ContentEntries = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "ContentEntries",
			JsonPropertyName = "contentEntries",
			AttributeProviderFactory = () => typeof(XWModManifest).GetProperty("ContentEntries", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<XWModContentEntry>), Array.Empty<Type>(), null)
		};
		array[14] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo15);
		JsonPropertyInfoValues<List<string>> propertyInfo16 = new JsonPropertyInfoValues<List<string>>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModManifest),
			Converter = null,
			Getter = (object obj) => ((XWModManifest)obj).PrivateResources,
			Setter = delegate(object obj, List<string>? value)
			{
				((XWModManifest)obj).PrivateResources = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "PrivateResources",
			JsonPropertyName = "privateResources",
			AttributeProviderFactory = () => typeof(XWModManifest).GetProperty("PrivateResources", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<string>), Array.Empty<Type>(), null)
		};
		array[15] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo16);
		JsonPropertyInfoValues<XWModLoadingPresentation> propertyInfo17 = new JsonPropertyInfoValues<XWModLoadingPresentation>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModManifest),
			Converter = null,
			Getter = (object obj) => ((XWModManifest)obj).LoadingPresentation,
			Setter = delegate(object obj, XWModLoadingPresentation? value)
			{
				((XWModManifest)obj).LoadingPresentation = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "LoadingPresentation",
			JsonPropertyName = "loadingPresentation",
			AttributeProviderFactory = () => typeof(XWModManifest).GetProperty("LoadingPresentation", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(XWModLoadingPresentation), Array.Empty<Type>(), null)
		};
		array[16] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo17);
		JsonPropertyInfoValues<List<XWModDependency>> propertyInfo18 = new JsonPropertyInfoValues<List<XWModDependency>>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModManifest),
			Converter = null,
			Getter = (object obj) => ((XWModManifest)obj).Conflicts,
			Setter = delegate(object obj, List<XWModDependency>? value)
			{
				((XWModManifest)obj).Conflicts = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Conflicts",
			JsonPropertyName = "conflicts",
			AttributeProviderFactory = () => typeof(XWModManifest).GetProperty("Conflicts", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<XWModDependency>), Array.Empty<Type>(), null)
		};
		array[17] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo18);
		JsonPropertyInfoValues<Dictionary<string, List<string>>> propertyInfo19 = new JsonPropertyInfoValues<Dictionary<string, List<string>>>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModManifest),
			Converter = null,
			Getter = (object obj) => ((XWModManifest)obj).Provides,
			Setter = delegate(object obj, Dictionary<string, List<string>>? value)
			{
				((XWModManifest)obj).Provides = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Provides",
			JsonPropertyName = "provides",
			AttributeProviderFactory = () => typeof(XWModManifest).GetProperty("Provides", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(Dictionary<string, List<string>>), Array.Empty<Type>(), null)
		};
		array[18] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo19);
		JsonPropertyInfoValues<Dictionary<string, List<string>>> propertyInfo20 = new JsonPropertyInfoValues<Dictionary<string, List<string>>>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModManifest),
			Converter = null,
			Getter = (object obj) => ((XWModManifest)obj).Overrides,
			Setter = delegate(object obj, Dictionary<string, List<string>>? value)
			{
				((XWModManifest)obj).Overrides = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Overrides",
			JsonPropertyName = "overrides",
			AttributeProviderFactory = () => typeof(XWModManifest).GetProperty("Overrides", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(Dictionary<string, List<string>>), Array.Empty<Type>(), null)
		};
		array[19] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo20);
		JsonPropertyInfoValues<List<string>> propertyInfo21 = new JsonPropertyInfoValues<List<string>>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModManifest),
			Converter = null,
			Getter = (object obj) => ((XWModManifest)obj).Scripts,
			Setter = delegate(object obj, List<string>? value)
			{
				((XWModManifest)obj).Scripts = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Scripts",
			JsonPropertyName = "scripts",
			AttributeProviderFactory = () => typeof(XWModManifest).GetProperty("Scripts", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<string>), Array.Empty<Type>(), null)
		};
		array[20] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo21);
		JsonPropertyInfoValues<List<XWModSettingDefinition>> propertyInfo22 = new JsonPropertyInfoValues<List<XWModSettingDefinition>>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModManifest),
			Converter = null,
			Getter = (object obj) => ((XWModManifest)obj).Settings,
			Setter = delegate(object obj, List<XWModSettingDefinition>? value)
			{
				((XWModManifest)obj).Settings = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Settings",
			JsonPropertyName = "settings",
			AttributeProviderFactory = () => typeof(XWModManifest).GetProperty("Settings", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<XWModSettingDefinition>), Array.Empty<Type>(), null)
		};
		array[21] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo22);
		JsonPropertyInfoValues<string> propertyInfo23 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModManifest),
			Converter = null,
			Getter = (object obj) => ((XWModManifest)obj).RuntimeAssembly,
			Setter = delegate(object obj, string? value)
			{
				((XWModManifest)obj).RuntimeAssembly = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "RuntimeAssembly",
			JsonPropertyName = "runtimeAssembly",
			AttributeProviderFactory = () => typeof(XWModManifest).GetProperty("RuntimeAssembly", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[22] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo23);
		JsonPropertyInfoValues<string> propertyInfo24 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModManifest),
			Converter = null,
			Getter = (object obj) => ((XWModManifest)obj).RuntimeEntryType,
			Setter = delegate(object obj, string? value)
			{
				((XWModManifest)obj).RuntimeEntryType = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "RuntimeEntryType",
			JsonPropertyName = "runtimeEntryType",
			AttributeProviderFactory = () => typeof(XWModManifest).GetProperty("RuntimeEntryType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[23] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo24);
		JsonPropertyInfoValues<int> propertyInfo25 = new JsonPropertyInfoValues<int>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModManifest),
			Converter = null,
			Getter = (object obj) => ((XWModManifest)obj).RuntimeApiVersion,
			Setter = delegate(object obj, int value)
			{
				((XWModManifest)obj).RuntimeApiVersion = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "RuntimeApiVersion",
			JsonPropertyName = "runtimeApiVersion",
			AttributeProviderFactory = () => typeof(XWModManifest).GetProperty("RuntimeApiVersion", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null)
		};
		array[24] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo25);
		JsonPropertyInfoValues<string> propertyInfo26 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModManifest),
			Converter = null,
			Getter = (object obj) => ((XWModManifest)obj).RuntimeAssemblyPolicy,
			Setter = delegate(object obj, string? value)
			{
				((XWModManifest)obj).RuntimeAssemblyPolicy = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "RuntimeAssemblyPolicy",
			JsonPropertyName = "runtimeAssemblyPolicy",
			AttributeProviderFactory = () => typeof(XWModManifest).GetProperty("RuntimeAssemblyPolicy", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[25] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo26);
		JsonPropertyInfoValues<List<string>> propertyInfo27 = new JsonPropertyInfoValues<List<string>>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModManifest),
			Converter = null,
			Getter = (object obj) => ((XWModManifest)obj).Blueprints,
			Setter = delegate(object obj, List<string>? value)
			{
				((XWModManifest)obj).Blueprints = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Blueprints",
			JsonPropertyName = "blueprints",
			AttributeProviderFactory = () => typeof(XWModManifest).GetProperty("Blueprints", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<string>), Array.Empty<Type>(), null)
		};
		array[26] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo27);
		JsonPropertyInfoValues<List<string>> propertyInfo28 = new JsonPropertyInfoValues<List<string>>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModManifest),
			Converter = null,
			Getter = (object obj) => ((XWModManifest)obj).Translations,
			Setter = delegate(object obj, List<string>? value)
			{
				((XWModManifest)obj).Translations = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Translations",
			JsonPropertyName = "translations",
			AttributeProviderFactory = () => typeof(XWModManifest).GetProperty("Translations", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<string>), Array.Empty<Type>(), null)
		};
		array[27] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo28);
		JsonPropertyInfoValues<List<string>> propertyInfo29 = new JsonPropertyInfoValues<List<string>>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModManifest),
			Converter = null,
			Getter = (object obj) => ((XWModManifest)obj).Resources,
			Setter = delegate(object obj, List<string>? value)
			{
				((XWModManifest)obj).Resources = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Resources",
			JsonPropertyName = "resources",
			AttributeProviderFactory = () => typeof(XWModManifest).GetProperty("Resources", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<string>), Array.Empty<Type>(), null)
		};
		array[28] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo29);
		return array;
	}

	private void XWModManifestSerializeHandler(Utf8JsonWriter writer, XWModManifest? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartObject();
		writer.WriteNumber(PropName_schemaVersion, value.SchemaVersion);
		writer.WriteString(PropName_id, value.Id);
		writer.WriteString(PropName_name, value.Name);
		writer.WriteString(PropName_version, value.Version);
		writer.WriteString(PropName_postId, value.PostId);
		writer.WriteString(PropName_author, value.Author);
		writer.WriteString(PropName_description, value.Description);
		writer.WriteString(PropName_playInstructions, value.PlayInstructions);
		writer.WritePropertyName(PropName_dependencies);
		ListXWModDependencySerializeHandler(writer, value.Dependencies);
		writer.WritePropertyName(PropName_optionalDependencies);
		ListXWModDependencySerializeHandler(writer, value.OptionalDependencies);
		writer.WritePropertyName(PropName_loadBefore);
		ListStringSerializeHandler(writer, value.LoadBefore);
		writer.WritePropertyName(PropName_loadAfter);
		ListStringSerializeHandler(writer, value.LoadAfter);
		writer.WritePropertyName(PropName_requiredCapabilities);
		ListStringSerializeHandler(writer, value.RequiredCapabilities);
		writer.WritePropertyName(PropName_sharedAssemblies);
		ListStringSerializeHandler(writer, value.SharedAssemblies);
		writer.WritePropertyName(PropName_contentEntries);
		ListXWModContentEntrySerializeHandler(writer, value.ContentEntries);
		writer.WritePropertyName(PropName_privateResources);
		ListStringSerializeHandler(writer, value.PrivateResources);
		writer.WritePropertyName(PropName_loadingPresentation);
		XWModLoadingPresentationSerializeHandler(writer, value.LoadingPresentation);
		writer.WritePropertyName(PropName_conflicts);
		ListXWModDependencySerializeHandler(writer, value.Conflicts);
		writer.WritePropertyName(PropName_provides);
		DictionaryStringListStringSerializeHandler(writer, value.Provides);
		writer.WritePropertyName(PropName_overrides);
		DictionaryStringListStringSerializeHandler(writer, value.Overrides);
		writer.WritePropertyName(PropName_scripts);
		ListStringSerializeHandler(writer, value.Scripts);
		writer.WritePropertyName(PropName_settings);
		ListXWModSettingDefinitionSerializeHandler(writer, value.Settings);
		writer.WriteString(PropName_runtimeAssembly, value.RuntimeAssembly);
		writer.WriteString(PropName_runtimeEntryType, value.RuntimeEntryType);
		writer.WriteNumber(PropName_runtimeApiVersion, value.RuntimeApiVersion);
		writer.WriteString(PropName_runtimeAssemblyPolicy, value.RuntimeAssemblyPolicy);
		writer.WritePropertyName(PropName_blueprints);
		ListStringSerializeHandler(writer, value.Blueprints);
		writer.WritePropertyName(PropName_translations);
		ListStringSerializeHandler(writer, value.Translations);
		writer.WritePropertyName(PropName_resources);
		ListStringSerializeHandler(writer, value.Resources);
		writer.WriteEndObject();
	}

	private JsonTypeInfo<XWModSettingDefinition> Create_XWModSettingDefinition(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<XWModSettingDefinition> jsonTypeInfo))
		{
			JsonObjectInfoValues<XWModSettingDefinition> objectInfo = new JsonObjectInfoValues<XWModSettingDefinition>
			{
				ObjectCreator = () => new XWModSettingDefinition(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => XWModSettingDefinitionPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(XWModSettingDefinition).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = XWModSettingDefinitionSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] XWModSettingDefinitionPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[17];
		JsonPropertyInfoValues<string> propertyInfo = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModSettingDefinition),
			Converter = null,
			Getter = (object obj) => ((XWModSettingDefinition)obj).Key,
			Setter = delegate(object obj, string? value)
			{
				((XWModSettingDefinition)obj).Key = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Key",
			JsonPropertyName = "key",
			AttributeProviderFactory = () => typeof(XWModSettingDefinition).GetProperty("Key", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<string> propertyInfo2 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModSettingDefinition),
			Converter = null,
			Getter = (object obj) => ((XWModSettingDefinition)obj).Type,
			Setter = delegate(object obj, string? value)
			{
				((XWModSettingDefinition)obj).Type = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Type",
			JsonPropertyName = "type",
			AttributeProviderFactory = () => typeof(XWModSettingDefinition).GetProperty("Type", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		JsonPropertyInfoValues<string> propertyInfo3 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModSettingDefinition),
			Converter = null,
			Getter = (object obj) => ((XWModSettingDefinition)obj).Label,
			Setter = delegate(object obj, string? value)
			{
				((XWModSettingDefinition)obj).Label = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Label",
			JsonPropertyName = "label",
			AttributeProviderFactory = () => typeof(XWModSettingDefinition).GetProperty("Label", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		JsonPropertyInfoValues<string> propertyInfo4 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModSettingDefinition),
			Converter = null,
			Getter = (object obj) => ((XWModSettingDefinition)obj).Description,
			Setter = delegate(object obj, string? value)
			{
				((XWModSettingDefinition)obj).Description = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Description",
			JsonPropertyName = "description",
			AttributeProviderFactory = () => typeof(XWModSettingDefinition).GetProperty("Description", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		JsonPropertyInfoValues<string> propertyInfo5 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModSettingDefinition),
			Converter = null,
			Getter = (object obj) => ((XWModSettingDefinition)obj).Content,
			Setter = delegate(object obj, string? value)
			{
				((XWModSettingDefinition)obj).Content = value;
			},
			IgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Content",
			JsonPropertyName = "content",
			AttributeProviderFactory = () => typeof(XWModSettingDefinition).GetProperty("Content", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[4] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo5);
		JsonPropertyInfoValues<string> propertyInfo6 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModSettingDefinition),
			Converter = null,
			Getter = (object obj) => ((XWModSettingDefinition)obj).Group,
			Setter = delegate(object obj, string? value)
			{
				((XWModSettingDefinition)obj).Group = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Group",
			JsonPropertyName = "group",
			AttributeProviderFactory = () => typeof(XWModSettingDefinition).GetProperty("Group", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[5] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo6);
		JsonPropertyInfoValues<JsonElement?> propertyInfo7 = new JsonPropertyInfoValues<JsonElement?>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModSettingDefinition),
			Converter = null,
			Getter = (object obj) => ((XWModSettingDefinition)obj).VisibleWhen,
			Setter = delegate(object obj, JsonElement? value)
			{
				((XWModSettingDefinition)obj).VisibleWhen = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "VisibleWhen",
			JsonPropertyName = "visibleWhen",
			AttributeProviderFactory = () => typeof(XWModSettingDefinition).GetProperty("VisibleWhen", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(JsonElement?), Array.Empty<Type>(), null)
		};
		array[6] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo7);
		JsonPropertyInfoValues<JsonElement?> propertyInfo8 = new JsonPropertyInfoValues<JsonElement?>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModSettingDefinition),
			Converter = null,
			Getter = (object obj) => ((XWModSettingDefinition)obj).EnabledWhen,
			Setter = delegate(object obj, JsonElement? value)
			{
				((XWModSettingDefinition)obj).EnabledWhen = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "EnabledWhen",
			JsonPropertyName = "enabledWhen",
			AttributeProviderFactory = () => typeof(XWModSettingDefinition).GetProperty("EnabledWhen", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(JsonElement?), Array.Empty<Type>(), null)
		};
		array[7] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo8);
		JsonPropertyInfoValues<JsonElement> propertyInfo9 = new JsonPropertyInfoValues<JsonElement>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModSettingDefinition),
			Converter = null,
			Getter = (object obj) => ((XWModSettingDefinition)obj).Default,
			Setter = delegate(object obj, JsonElement value)
			{
				((XWModSettingDefinition)obj).Default = value;
			},
			IgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Default",
			JsonPropertyName = "default",
			AttributeProviderFactory = () => typeof(XWModSettingDefinition).GetProperty("Default", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(JsonElement), Array.Empty<Type>(), null)
		};
		array[8] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo9);
		JsonPropertyInfoValues<double?> propertyInfo10 = new JsonPropertyInfoValues<double?>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModSettingDefinition),
			Converter = null,
			Getter = (object obj) => ((XWModSettingDefinition)obj).Min,
			Setter = delegate(object obj, double? value)
			{
				((XWModSettingDefinition)obj).Min = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Min",
			JsonPropertyName = "min",
			AttributeProviderFactory = () => typeof(XWModSettingDefinition).GetProperty("Min", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(double?), Array.Empty<Type>(), null)
		};
		array[9] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo10);
		JsonPropertyInfoValues<double?> propertyInfo11 = new JsonPropertyInfoValues<double?>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModSettingDefinition),
			Converter = null,
			Getter = (object obj) => ((XWModSettingDefinition)obj).Max,
			Setter = delegate(object obj, double? value)
			{
				((XWModSettingDefinition)obj).Max = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Max",
			JsonPropertyName = "max",
			AttributeProviderFactory = () => typeof(XWModSettingDefinition).GetProperty("Max", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(double?), Array.Empty<Type>(), null)
		};
		array[10] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo11);
		JsonPropertyInfoValues<double?> propertyInfo12 = new JsonPropertyInfoValues<double?>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModSettingDefinition),
			Converter = null,
			Getter = (object obj) => ((XWModSettingDefinition)obj).Step,
			Setter = delegate(object obj, double? value)
			{
				((XWModSettingDefinition)obj).Step = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Step",
			JsonPropertyName = "step",
			AttributeProviderFactory = () => typeof(XWModSettingDefinition).GetProperty("Step", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(double?), Array.Empty<Type>(), null)
		};
		array[11] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo12);
		JsonPropertyInfoValues<List<XWModSettingOption>> propertyInfo13 = new JsonPropertyInfoValues<List<XWModSettingOption>>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModSettingDefinition),
			Converter = null,
			Getter = (object obj) => ((XWModSettingDefinition)obj).Options,
			Setter = delegate(object obj, List<XWModSettingOption>? value)
			{
				((XWModSettingDefinition)obj).Options = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Options",
			JsonPropertyName = "options",
			AttributeProviderFactory = () => typeof(XWModSettingDefinition).GetProperty("Options", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<XWModSettingOption>), Array.Empty<Type>(), null)
		};
		array[12] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo13);
		JsonPropertyInfoValues<int?> propertyInfo14 = new JsonPropertyInfoValues<int?>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModSettingDefinition),
			Converter = null,
			Getter = (object obj) => ((XWModSettingDefinition)obj).MinSelected,
			Setter = delegate(object obj, int? value)
			{
				((XWModSettingDefinition)obj).MinSelected = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "MinSelected",
			JsonPropertyName = "minSelected",
			AttributeProviderFactory = () => typeof(XWModSettingDefinition).GetProperty("MinSelected", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int?), Array.Empty<Type>(), null)
		};
		array[13] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo14);
		JsonPropertyInfoValues<int?> propertyInfo15 = new JsonPropertyInfoValues<int?>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModSettingDefinition),
			Converter = null,
			Getter = (object obj) => ((XWModSettingDefinition)obj).MaxSelected,
			Setter = delegate(object obj, int? value)
			{
				((XWModSettingDefinition)obj).MaxSelected = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "MaxSelected",
			JsonPropertyName = "maxSelected",
			AttributeProviderFactory = () => typeof(XWModSettingDefinition).GetProperty("MaxSelected", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int?), Array.Empty<Type>(), null)
		};
		array[14] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo15);
		JsonPropertyInfoValues<string> propertyInfo16 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModSettingDefinition),
			Converter = null,
			Getter = (object obj) => ((XWModSettingDefinition)obj).ApplyMode,
			Setter = delegate(object obj, string? value)
			{
				((XWModSettingDefinition)obj).ApplyMode = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "ApplyMode",
			JsonPropertyName = "applyMode",
			AttributeProviderFactory = () => typeof(XWModSettingDefinition).GetProperty("ApplyMode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[15] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo16);
		JsonPropertyInfoValues<bool> propertyInfo17 = new JsonPropertyInfoValues<bool>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModSettingDefinition),
			Converter = null,
			Getter = (object obj) => ((XWModSettingDefinition)obj).AffectsGameplay,
			Setter = delegate(object obj, bool value)
			{
				((XWModSettingDefinition)obj).AffectsGameplay = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "AffectsGameplay",
			JsonPropertyName = "affectsGameplay",
			AttributeProviderFactory = () => typeof(XWModSettingDefinition).GetProperty("AffectsGameplay", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(bool), Array.Empty<Type>(), null)
		};
		array[16] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo17);
		return array;
	}

	private void XWModSettingDefinitionSerializeHandler(Utf8JsonWriter writer, XWModSettingDefinition? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartObject();
		writer.WriteString(PropName_key, value.Key);
		writer.WriteString(PropName_type, value.Type);
		writer.WriteString(PropName_label, value.Label);
		writer.WriteString(PropName_description, value.Description);
		string content = value.Content;
		if (content != null)
		{
			writer.WriteString(PropName_content, content);
		}
		writer.WriteString(PropName_group, value.Group);
		writer.WritePropertyName(PropName_visibleWhen);
		JsonSerializer.Serialize(writer, value.VisibleWhen, NullableJsonElement);
		writer.WritePropertyName(PropName_enabledWhen);
		JsonSerializer.Serialize(writer, value.EnabledWhen, NullableJsonElement);
		JsonElement jsonElement = value.Default;
		if (!EqualityComparer<System.Text.Json.JsonElement>.Default.Equals(default(JsonElement), jsonElement))
		{
			writer.WritePropertyName(PropName_default);
			JsonSerializer.Serialize(writer, jsonElement, JsonElement);
		}
		writer.WritePropertyName(PropName_min);
		JsonSerializer.Serialize(writer, value.Min, NullableDouble);
		writer.WritePropertyName(PropName_max);
		JsonSerializer.Serialize(writer, value.Max, NullableDouble);
		writer.WritePropertyName(PropName_step);
		JsonSerializer.Serialize(writer, value.Step, NullableDouble);
		writer.WritePropertyName(PropName_options);
		ListXWModSettingOptionSerializeHandler(writer, value.Options);
		writer.WritePropertyName(PropName_minSelected);
		JsonSerializer.Serialize(writer, value.MinSelected, NullableInt32);
		writer.WritePropertyName(PropName_maxSelected);
		JsonSerializer.Serialize(writer, value.MaxSelected, NullableInt32);
		writer.WriteString(PropName_applyMode, value.ApplyMode);
		writer.WriteBoolean(PropName_affectsGameplay, value.AffectsGameplay);
		writer.WriteEndObject();
	}

	private JsonTypeInfo<XWModSettingOption> Create_XWModSettingOption(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<XWModSettingOption> jsonTypeInfo))
		{
			JsonObjectInfoValues<XWModSettingOption> objectInfo = new JsonObjectInfoValues<XWModSettingOption>
			{
				ObjectCreator = () => new XWModSettingOption(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => XWModSettingOptionPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(XWModSettingOption).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = XWModSettingOptionSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] XWModSettingOptionPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[2];
		JsonPropertyInfoValues<string> propertyInfo = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModSettingOption),
			Converter = null,
			Getter = (object obj) => ((XWModSettingOption)obj).Value,
			Setter = delegate(object obj, string? value)
			{
				((XWModSettingOption)obj).Value = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Value",
			JsonPropertyName = "value",
			AttributeProviderFactory = () => typeof(XWModSettingOption).GetProperty("Value", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<string> propertyInfo2 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(XWModSettingOption),
			Converter = null,
			Getter = (object obj) => ((XWModSettingOption)obj).Label,
			Setter = delegate(object obj, string? value)
			{
				((XWModSettingOption)obj).Label = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Label",
			JsonPropertyName = "label",
			AttributeProviderFactory = () => typeof(XWModSettingOption).GetProperty("Label", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		return array;
	}

	private void XWModSettingOptionSerializeHandler(Utf8JsonWriter writer, XWModSettingOption? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartObject();
		writer.WriteString(PropName_value, value.Value);
		writer.WriteString(PropName_label, value.Label);
		writer.WriteEndObject();
	}

	private JsonTypeInfo<Dictionary<string, List<string>>> Create_DictionaryStringListString(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<Dictionary<string, List<string>>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<Dictionary<string, List<string>>> collectionInfo = new JsonCollectionInfoValues<Dictionary<string, List<string>>>
			{
				ObjectCreator = () => new Dictionary<string, List<string>>(),
				SerializeHandler = DictionaryStringListStringSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateDictionaryInfo<Dictionary<string, List<string>>, string, List<string>>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private void DictionaryStringListStringSerializeHandler(Utf8JsonWriter writer, Dictionary<string, List<string>>? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartObject();
		foreach (KeyValuePair<string, List<string>> item in value)
		{
			writer.WritePropertyName(item.Key);
			ListStringSerializeHandler(writer, item.Value);
		}
		writer.WriteEndObject();
	}

	private JsonTypeInfo<List<XWModContentEntry>> Create_ListXWModContentEntry(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<XWModContentEntry>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<XWModContentEntry>> collectionInfo = new JsonCollectionInfoValues<List<XWModContentEntry>>
			{
				ObjectCreator = () => new List<XWModContentEntry>(),
				SerializeHandler = ListXWModContentEntrySerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<XWModContentEntry>, XWModContentEntry>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private void ListXWModContentEntrySerializeHandler(Utf8JsonWriter writer, List<XWModContentEntry>? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartArray();
		for (int i = 0; i < value.Count; i++)
		{
			XWModContentEntrySerializeHandler(writer, value[i]);
		}
		writer.WriteEndArray();
	}

	private JsonTypeInfo<List<XWModDependency>> Create_ListXWModDependency(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<XWModDependency>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<XWModDependency>> collectionInfo = new JsonCollectionInfoValues<List<XWModDependency>>
			{
				ObjectCreator = () => new List<XWModDependency>(),
				SerializeHandler = ListXWModDependencySerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<XWModDependency>, XWModDependency>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private void ListXWModDependencySerializeHandler(Utf8JsonWriter writer, List<XWModDependency>? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartArray();
		for (int i = 0; i < value.Count; i++)
		{
			XWModDependencySerializeHandler(writer, value[i]);
		}
		writer.WriteEndArray();
	}

	private JsonTypeInfo<List<XWModSettingDefinition>> Create_ListXWModSettingDefinition(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<XWModSettingDefinition>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<XWModSettingDefinition>> collectionInfo = new JsonCollectionInfoValues<List<XWModSettingDefinition>>
			{
				ObjectCreator = () => new List<XWModSettingDefinition>(),
				SerializeHandler = ListXWModSettingDefinitionSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<XWModSettingDefinition>, XWModSettingDefinition>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private void ListXWModSettingDefinitionSerializeHandler(Utf8JsonWriter writer, List<XWModSettingDefinition>? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartArray();
		for (int i = 0; i < value.Count; i++)
		{
			XWModSettingDefinitionSerializeHandler(writer, value[i]);
		}
		writer.WriteEndArray();
	}

	private JsonTypeInfo<List<XWModSettingOption>> Create_ListXWModSettingOption(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<XWModSettingOption>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<XWModSettingOption>> collectionInfo = new JsonCollectionInfoValues<List<XWModSettingOption>>
			{
				ObjectCreator = () => new List<XWModSettingOption>(),
				SerializeHandler = ListXWModSettingOptionSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<XWModSettingOption>, XWModSettingOption>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private void ListXWModSettingOptionSerializeHandler(Utf8JsonWriter writer, List<XWModSettingOption>? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartArray();
		for (int i = 0; i < value.Count; i++)
		{
			XWModSettingOptionSerializeHandler(writer, value[i]);
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

	private JsonTypeInfo<DateTime> Create_DateTime(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<DateTime> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<DateTime>(options, JsonMetadataServices.DateTimeConverter);
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<JsonElement> Create_JsonElement(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<JsonElement> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<JsonElement>(options, JsonMetadataServices.JsonElementConverter);
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<JsonElement?> Create_NullableJsonElement(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<JsonElement?> jsonTypeInfo))
		{
			JsonConverter nullableConverter = JsonMetadataServices.GetNullableConverter<JsonElement>(options);
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<JsonElement?>(options, nullableConverter);
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
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

	private JsonTypeInfo<int?> Create_NullableInt32(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<int?> jsonTypeInfo))
		{
			JsonConverter nullableConverter = JsonMetadataServices.GetNullableConverter<int>(options);
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<int?>(options, nullableConverter);
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

	public XWModJsonContext()
		: base(null)
	{
	}

	public XWModJsonContext(JsonSerializerOptions options)
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
		if (type == typeof(double?))
		{
			return Create_NullableDouble(options);
		}
		if (type == typeof(XWResourceAutosaveDraft))
		{
			return Create_XWResourceAutosaveDraft(options);
		}
		if (type == typeof(ModExporter.ModInfo))
		{
			return Create_ModInfo(options);
		}
		if (type == typeof(ModProject))
		{
			return Create_ModProject(options);
		}
		if (type == typeof(XWModContentEntry))
		{
			return Create_XWModContentEntry(options);
		}
		if (type == typeof(XWModContentTarget))
		{
			return Create_XWModContentTarget(options);
		}
		if (type == typeof(XWModDependency))
		{
			return Create_XWModDependency(options);
		}
		if (type == typeof(XWModLoadingPresentation))
		{
			return Create_XWModLoadingPresentation(options);
		}
		if (type == typeof(XWModManifest))
		{
			return Create_XWModManifest(options);
		}
		if (type == typeof(XWModSettingDefinition))
		{
			return Create_XWModSettingDefinition(options);
		}
		if (type == typeof(XWModSettingOption))
		{
			return Create_XWModSettingOption(options);
		}
		if (type == typeof(Dictionary<string, List<string>>))
		{
			return Create_DictionaryStringListString(options);
		}
		if (type == typeof(List<XWModContentEntry>))
		{
			return Create_ListXWModContentEntry(options);
		}
		if (type == typeof(List<XWModDependency>))
		{
			return Create_ListXWModDependency(options);
		}
		if (type == typeof(List<XWModSettingDefinition>))
		{
			return Create_ListXWModSettingDefinition(options);
		}
		if (type == typeof(List<XWModSettingOption>))
		{
			return Create_ListXWModSettingOption(options);
		}
		if (type == typeof(List<string>))
		{
			return Create_ListString(options);
		}
		if (type == typeof(DateTime))
		{
			return Create_DateTime(options);
		}
		if (type == typeof(JsonElement))
		{
			return Create_JsonElement(options);
		}
		if (type == typeof(JsonElement?))
		{
			return Create_NullableJsonElement(options);
		}
		if (type == typeof(int))
		{
			return Create_Int32(options);
		}
		if (type == typeof(int?))
		{
			return Create_NullableInt32(options);
		}
		if (type == typeof(string))
		{
			return Create_String(options);
		}
		return null;
	}
}

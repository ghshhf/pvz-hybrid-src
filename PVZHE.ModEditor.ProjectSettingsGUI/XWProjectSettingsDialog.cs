using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using PVZHE.ModEditor.Core;
using PVZHE.ModEditor.ModSystem;

namespace PVZHE.ModEditor.ProjectSettingsGUI;

[ScriptPath("res://addons/ModEditor/ProjectSettings/GUI/XWProjectSettingsDialog.cs")]
public class XWProjectSettingsDialog : ConfirmationDialog
{
	[Signal]
	public delegate void ProjectSavedEventHandler();

	public new class MethodName : ConfirmationDialog.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName Populate = "Populate";

		public static readonly StringName SelectLanguage = "SelectLanguage";

		public static readonly StringName OnConfirmed = "OnConfirmed";

		public static readonly StringName SaveProjectSettings = "SaveProjectSettings";

		public static readonly StringName SaveEditorSettings = "SaveEditorSettings";

		public static readonly StringName OnExportBrowsePressed = "OnExportBrowsePressed";

		public static readonly StringName OnGameBrowsePressed = "OnGameBrowsePressed";

		public static readonly StringName ReopenAfterValidationError = "ReopenAfterValidationError";

		public static readonly StringName GetProjectExportDirectory = "GetProjectExportDirectory";

		public static readonly StringName EnsureDirectory = "EnsureDirectory";

		public static readonly StringName RemoveOldProjectFileIfRenamed = "RemoveOldProjectFileIfRenamed";
	}

	public new class PropertyName : ConfirmationDialog.PropertyName
	{
		public static readonly StringName _nameEdit = "_nameEdit";

		public static readonly StringName _modIdEdit = "_modIdEdit";

		public static readonly StringName _versionEdit = "_versionEdit";

		public static readonly StringName _postIdEdit = "_postIdEdit";

		public static readonly StringName _authorEdit = "_authorEdit";

		public static readonly StringName _descriptionEdit = "_descriptionEdit";

		public static readonly StringName _playInstructionsEdit = "_playInstructionsEdit";

		public static readonly StringName _projectPathEdit = "_projectPathEdit";

		public static readonly StringName _projectFileEdit = "_projectFileEdit";

		public static readonly StringName _exportPathEdit = "_exportPathEdit";

		public static readonly StringName _exportBrowseButton = "_exportBrowseButton";

		public static readonly StringName _gamePathEdit = "_gamePathEdit";

		public static readonly StringName _gameBrowseButton = "_gameBrowseButton";

		public static readonly StringName _autoSaveSpin = "_autoSaveSpin";

		public static readonly StringName _showToastsCheck = "_showToastsCheck";

		public static readonly StringName _languageOption = "_languageOption";

		public static readonly StringName _chineseLanguageButton = "_chineseLanguageButton";

		public static readonly StringName _englishLanguageButton = "_englishLanguageButton";

		public static readonly StringName _statusLabel = "_statusLabel";

		public static readonly StringName _exportDirDialog = "_exportDirDialog";

		public static readonly StringName _gameDirDialog = "_gameDirDialog";

		public static readonly StringName _settings = "_settings";

		public static readonly StringName _originalProjectFilePath = "_originalProjectFilePath";
	}

	public new class SignalName : ConfirmationDialog.SignalName
	{
		public static readonly StringName ProjectSaved = "ProjectSaved";
	}

	private LineEdit _nameEdit;

	private LineEdit _modIdEdit;

	private LineEdit _versionEdit;

	private LineEdit _postIdEdit;

	private LineEdit _authorEdit;

	private TextEdit _descriptionEdit;

	private TextEdit _playInstructionsEdit;

	private LineEdit _projectPathEdit;

	private LineEdit _projectFileEdit;

	private LineEdit _exportPathEdit;

	private Button _exportBrowseButton;

	private LineEdit _gamePathEdit;

	private Button _gameBrowseButton;

	private SpinBox _autoSaveSpin;

	private CheckBox _showToastsCheck;

	private OptionButton _languageOption;

	private Button _chineseLanguageButton;

	private Button _englishLanguageButton;

	private Label _statusLabel;

	private FileDialog _exportDirDialog;

	private FileDialog _gameDirDialog;

	private ModProject _project;

	private XWEditorSettings _settings;

	private string _originalProjectFilePath = "";

	private ProjectSavedEventHandler backing_ProjectSaved;

	public event ProjectSavedEventHandler ProjectSaved
	{
		add
		{
			backing_ProjectSaved = (ProjectSavedEventHandler)Delegate.Combine(backing_ProjectSaved, value);
		}
		remove
		{
			backing_ProjectSaved = (ProjectSavedEventHandler)Delegate.Remove(backing_ProjectSaved, value);
		}
	}

	public override void _Ready()
	{
		base.Title = "项目设置";
		base.OkButtonText = "保存";
		base.CancelButtonText = "取消";
		base.MinSize = new Vector2I(560, 440);
		_nameEdit = GetNode<LineEdit>("%NameEdit");
		_modIdEdit = GetNode<LineEdit>("%ModIdEdit");
		_versionEdit = GetNode<LineEdit>("%VersionEdit");
		_postIdEdit = GetNode<LineEdit>("%PostIdEdit");
		_authorEdit = GetNode<LineEdit>("%AuthorEdit");
		_descriptionEdit = GetNode<TextEdit>("%DescriptionEdit");
		_playInstructionsEdit = GetNode<TextEdit>("%PlayInstructionsEdit");
		_projectPathEdit = GetNode<LineEdit>("%ProjectPathEdit");
		_projectFileEdit = GetNode<LineEdit>("%ProjectFileEdit");
		_exportPathEdit = GetNode<LineEdit>("%ExportPathEdit");
		_exportBrowseButton = GetNode<Button>("%ExportBrowseButton");
		_gamePathEdit = GetNode<LineEdit>("%GamePathEdit");
		_gameBrowseButton = GetNode<Button>("%GameBrowseButton");
		_autoSaveSpin = GetNode<SpinBox>("%AutoSaveSpin");
		_showToastsCheck = GetNode<CheckBox>("%ShowToastsCheck");
		_languageOption = GetNode<OptionButton>("%LanguageOption");
		_chineseLanguageButton = GetNode<Button>("%ChineseLanguageButton");
		_englishLanguageButton = GetNode<Button>("%EnglishLanguageButton");
		_statusLabel = GetNode<Label>("%StatusLabel");
		_exportDirDialog = GetNode<FileDialog>("%ExportDirDialog");
		_gameDirDialog = GetNode<FileDialog>("%GameDirDialog");
		_projectPathEdit.Editable = false;
		_projectFileEdit.Editable = false;
		_languageOption.Clear();
		_languageOption.AddItem("简体中文", 0);
		_languageOption.AddItem("英语", 1);
		ButtonGroup buttonGroup = new ButtonGroup
		{
			AllowUnpress = false
		};
		_chineseLanguageButton.ButtonGroup = buttonGroup;
		_englishLanguageButton.ButtonGroup = buttonGroup;
		_chineseLanguageButton.Pressed += delegate
		{
			SelectLanguage(0);
		};
		_englishLanguageButton.Pressed += delegate
		{
			SelectLanguage(1);
		};
		_exportBrowseButton.Pressed += OnExportBrowsePressed;
		_gameBrowseButton.Pressed += OnGameBrowsePressed;
		_exportDirDialog.DirSelected += delegate(string dir)
		{
			_exportPathEdit.Text = dir;
		};
		_gameDirDialog.DirSelected += delegate(string dir)
		{
			_gamePathEdit.Text = dir;
		};
		base.Confirmed += OnConfirmed;
		Populate();
	}

	public void EditProject(ModProject project, XWEditorSettings settings)
	{
		_project = project;
		_settings = settings;
		_originalProjectFilePath = project?.ProjectFilePath ?? "";
		if (IsInsideTree())
		{
			Populate();
		}
	}

	private void Populate()
	{
		if (_nameEdit == null)
		{
			return;
		}
		bool flag = _project != null;
		_nameEdit.Editable = flag;
		_modIdEdit.Editable = flag;
		_versionEdit.Editable = flag;
		_postIdEdit.Editable = flag;
		_authorEdit.Editable = flag;
		_descriptionEdit.Editable = flag;
		_playInstructionsEdit.Editable = flag;
		_exportPathEdit.Editable = flag;
		_gamePathEdit.Editable = flag;
		_exportBrowseButton.Disabled = !flag;
		_gameBrowseButton.Disabled = !flag;
		if (flag)
		{
			_nameEdit.Text = _project.Name ?? "";
			_modIdEdit.Text = "";
			_postIdEdit.Text = "";
			try
			{
				XWModManifest xWModManifest = XWModManifest.Load(Path.Combine(_project.ProjectPath, "mod.json"));
				_modIdEdit.Text = xWModManifest?.Id ?? "";
				_postIdEdit.Text = xWModManifest?.PostId ?? "";
			}
			catch (Exception ex)
			{
				XWEditorInterface.Instance?.ShowToast("读取 Mod ID 失败：" + ex.Message);
			}
			_versionEdit.Text = _project.Version ?? "";
			_authorEdit.Text = _project.Author ?? "";
			_descriptionEdit.Text = _project.Description ?? "";
			_playInstructionsEdit.Text = _project.PlayInstructions ?? "";
			_projectPathEdit.Text = _project.ProjectPath ?? "";
			_projectFileEdit.Text = _project.ProjectFilePath ?? "";
			_exportPathEdit.Text = GetProjectExportDirectory();
			_gamePathEdit.Text = _project.GameDirectory ?? "";
			_statusLabel.Text = "正在编辑当前 Mod 工程设置";
		}
		else
		{
			_nameEdit.Text = "";
			_modIdEdit.Text = "";
			_versionEdit.Text = "";
			_postIdEdit.Text = "";
			_authorEdit.Text = "";
			_descriptionEdit.Text = "";
			_playInstructionsEdit.Text = "";
			_projectPathEdit.Text = "";
			_projectFileEdit.Text = "";
			_exportPathEdit.Text = "";
			_gamePathEdit.Text = "";
			_statusLabel.Text = "没有打开的 Mod 工程，仅可保存编辑器偏好";
		}
		XWEditorSettings obj = _settings ?? XWEditorInterface.Instance?.GetEditorSettings();
		int num = obj?.GetSetting("editor/auto_save_interval", 300) ?? 300;
		bool buttonPressed = obj?.GetSetting("editor/show_toasts", defaultValue: true) ?? true;
		string text = obj?.GetSetting("interface/language", "zh_CN") ?? "zh_CN";
		_autoSaveSpin.Value = num;
		_showToastsCheck.ButtonPressed = buttonPressed;
		SelectLanguage((text == "en_US") ? 1 : 0);
	}

	private void SelectLanguage(int index)
	{
		int num = ((index == 1) ? 1 : 0);
		_languageOption.Select(num);
		_chineseLanguageButton.SetPressedNoSignal(num == 0);
		_englishLanguageButton.SetPressedNoSignal(num == 1);
	}

	private void OnConfirmed()
	{
		if (_project == null)
		{
			SaveEditorSettings();
			XWEditorInterface.Instance?.ShowToast("项目设置已保存");
			EmitSignal(SignalName.ProjectSaved);
		}
		else if (!SaveProjectSettings())
		{
			CallDeferred("ReopenAfterValidationError");
		}
		else
		{
			SaveEditorSettings();
			XWEditorInterface.Instance?.ShowToast("项目设置已保存");
			EmitSignal(SignalName.ProjectSaved);
		}
	}

	private bool SaveProjectSettings()
	{
		string text = _nameEdit.Text.Trim();
		if (string.IsNullOrWhiteSpace(text))
		{
			XWEditorInterface.Instance?.ShowToast("项目名称不能为空");
			return false;
		}
		if (!XWModMetadataValidation.TryValidate(_authorEdit.Text, _modIdEdit.Text, _versionEdit.Text, out var diagnostic))
		{
			_statusLabel.Text = diagnostic;
			XWEditorInterface.Instance?.ShowToast(diagnostic);
			return false;
		}
		string manifestPath = Path.Combine(_project.ProjectPath, "mod.json");
		XWModManifest xWModManifest;
		try
		{
			xWModManifest = XWModManifest.Load(manifestPath) ?? new XWModManifest();
		}
		catch (Exception ex)
		{
			_statusLabel.Text = "读取 mod.json 失败：" + ex.Message;
			XWEditorInterface.Instance?.ShowToast(_statusLabel.Text);
			return false;
		}
		string text2 = _exportPathEdit.Text.Trim();
		string text3 = _postIdEdit.Text.Trim();
		if (text3.Length != 0 && !XWModManifest.IsValidPostId(text3))
		{
			XWEditorInterface.Instance?.ShowToast("官网投稿 ID 必须为不含前导零的正整数；留空可取消绑定");
			return false;
		}
		if (!string.IsNullOrWhiteSpace(text2) && !EnsureDirectory(text2, "默认导出目录"))
		{
			return false;
		}
		string text4 = _gamePathEdit.Text.Trim();
		if (!string.IsNullOrWhiteSpace(text4) && !DirAccess.DirExistsAbsolute(text4))
		{
			XWEditorInterface.Instance?.ShowToast("游戏目录不存在: " + text4);
			return false;
		}
		_project.Name = text;
		_project.Version = _versionEdit.Text;
		_project.Author = _authorEdit.Text;
		_project.Description = _descriptionEdit.Text ?? "";
		_project.PlayInstructions = _playInstructionsEdit.Text ?? "";
		_project.ExportDirectory = text2;
		_project.GameDirectory = text4;
		xWModManifest.Id = _modIdEdit.Text;
		xWModManifest.PostId = text3;
		xWModManifest.Save(manifestPath);
		_project.Save();
		RemoveOldProjectFileIfRenamed();
		_originalProjectFilePath = _project.ProjectFilePath;
		_projectFileEdit.Text = _project.ProjectFilePath;
		return true;
	}

	private void SaveEditorSettings()
	{
		XWEditorSettings xWEditorSettings = _settings ?? XWEditorInterface.Instance?.GetEditorSettings();
		if (xWEditorSettings != null)
		{
			xWEditorSettings.SetSetting("editor/auto_save_interval", Mathf.RoundToInt((float)_autoSaveSpin.Value));
			xWEditorSettings.SetSetting("editor/show_toasts", _showToastsCheck.ButtonPressed);
			xWEditorSettings.SetSetting("interface/language", (_languageOption.GetSelectedId() == 1) ? "en_US" : "zh_CN");
			xWEditorSettings.Save();
		}
	}

	private void OnExportBrowsePressed()
	{
		string text = _exportPathEdit.Text.Trim();
		_exportDirDialog.CurrentDir = (DirAccess.DirExistsAbsolute(text) ? text : ProjectSettings.GlobalizePath("user://Mods/"));
		_exportDirDialog.PopupCentered(new Vector2I(700, 500));
	}

	private void OnGameBrowsePressed()
	{
		string text = _gamePathEdit.Text.Trim();
		if (DirAccess.DirExistsAbsolute(text))
		{
			_gameDirDialog.CurrentDir = text;
		}
		_gameDirDialog.PopupCentered(new Vector2I(700, 500));
	}

	private void ReopenAfterValidationError()
	{
		PopupCenteredClamped(new Vector2I(720, 560), 0.9f);
	}

	private string GetProjectExportDirectory()
	{
		if (!string.IsNullOrWhiteSpace(_project.ExportDirectory))
		{
			return _project.ExportDirectory;
		}
		return ProjectSettings.GlobalizePath("user://Mods/");
	}

	private static bool EnsureDirectory(string path, string label)
	{
		if (DirAccess.DirExistsAbsolute(path))
		{
			return true;
		}
		if (DirAccess.MakeDirRecursiveAbsolute(path) == Error.Ok && DirAccess.DirExistsAbsolute(path))
		{
			return true;
		}
		XWEditorInterface.Instance?.ShowToast(label + "无法创建: " + path);
		return false;
	}

	private void RemoveOldProjectFileIfRenamed()
	{
		if (string.IsNullOrWhiteSpace(_originalProjectFilePath) || string.Equals(_originalProjectFilePath, _project.ProjectFilePath, StringComparison.OrdinalIgnoreCase) || !File.Exists(_originalProjectFilePath))
		{
			return;
		}
		try
		{
			File.Delete(_originalProjectFilePath);
		}
		catch (Exception ex)
		{
			GD.PrintErr("[XWProjectSettingsDialog] 无法删除旧项目文件: " + _originalProjectFilePath + "\n" + ex.Message);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(12)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.Populate, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.SelectLanguage, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Int, "index", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.OnConfirmed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.SaveProjectSettings, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.SaveEditorSettings, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.OnExportBrowsePressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.OnGameBrowsePressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ReopenAfterValidationError, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.GetProjectExportDirectory, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.EnsureDirectory, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "path", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.String, "label", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.RemoveOldProjectFileIfRenamed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Populate && args.Count == 0)
		{
			Populate();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SelectLanguage && args.Count == 1)
		{
			SelectLanguage(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnConfirmed && args.Count == 0)
		{
			OnConfirmed();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SaveProjectSettings && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(SaveProjectSettings());
			return true;
		}
		if (method == MethodName.SaveEditorSettings && args.Count == 0)
		{
			SaveEditorSettings();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnExportBrowsePressed && args.Count == 0)
		{
			OnExportBrowsePressed();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnGameBrowsePressed && args.Count == 0)
		{
			OnGameBrowsePressed();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ReopenAfterValidationError && args.Count == 0)
		{
			ReopenAfterValidationError();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetProjectExportDirectory && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<string>(GetProjectExportDirectory());
			return true;
		}
		if (method == MethodName.EnsureDirectory && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<bool>(EnsureDirectory(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<string>(in args[1])));
			return true;
		}
		if (method == MethodName.RemoveOldProjectFileIfRenamed && args.Count == 0)
		{
			RemoveOldProjectFileIfRenamed();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.EnsureDirectory && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<bool>(EnsureDirectory(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<string>(in args[1])));
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.Populate)
		{
			return true;
		}
		if (method == MethodName.SelectLanguage)
		{
			return true;
		}
		if (method == MethodName.OnConfirmed)
		{
			return true;
		}
		if (method == MethodName.SaveProjectSettings)
		{
			return true;
		}
		if (method == MethodName.SaveEditorSettings)
		{
			return true;
		}
		if (method == MethodName.OnExportBrowsePressed)
		{
			return true;
		}
		if (method == MethodName.OnGameBrowsePressed)
		{
			return true;
		}
		if (method == MethodName.ReopenAfterValidationError)
		{
			return true;
		}
		if (method == MethodName.GetProjectExportDirectory)
		{
			return true;
		}
		if (method == MethodName.EnsureDirectory)
		{
			return true;
		}
		if (method == MethodName.RemoveOldProjectFileIfRenamed)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._nameEdit)
		{
			_nameEdit = VariantUtils.ConvertTo<LineEdit>(in value);
			return true;
		}
		if (name == PropertyName._modIdEdit)
		{
			_modIdEdit = VariantUtils.ConvertTo<LineEdit>(in value);
			return true;
		}
		if (name == PropertyName._versionEdit)
		{
			_versionEdit = VariantUtils.ConvertTo<LineEdit>(in value);
			return true;
		}
		if (name == PropertyName._postIdEdit)
		{
			_postIdEdit = VariantUtils.ConvertTo<LineEdit>(in value);
			return true;
		}
		if (name == PropertyName._authorEdit)
		{
			_authorEdit = VariantUtils.ConvertTo<LineEdit>(in value);
			return true;
		}
		if (name == PropertyName._descriptionEdit)
		{
			_descriptionEdit = VariantUtils.ConvertTo<TextEdit>(in value);
			return true;
		}
		if (name == PropertyName._playInstructionsEdit)
		{
			_playInstructionsEdit = VariantUtils.ConvertTo<TextEdit>(in value);
			return true;
		}
		if (name == PropertyName._projectPathEdit)
		{
			_projectPathEdit = VariantUtils.ConvertTo<LineEdit>(in value);
			return true;
		}
		if (name == PropertyName._projectFileEdit)
		{
			_projectFileEdit = VariantUtils.ConvertTo<LineEdit>(in value);
			return true;
		}
		if (name == PropertyName._exportPathEdit)
		{
			_exportPathEdit = VariantUtils.ConvertTo<LineEdit>(in value);
			return true;
		}
		if (name == PropertyName._exportBrowseButton)
		{
			_exportBrowseButton = VariantUtils.ConvertTo<Button>(in value);
			return true;
		}
		if (name == PropertyName._gamePathEdit)
		{
			_gamePathEdit = VariantUtils.ConvertTo<LineEdit>(in value);
			return true;
		}
		if (name == PropertyName._gameBrowseButton)
		{
			_gameBrowseButton = VariantUtils.ConvertTo<Button>(in value);
			return true;
		}
		if (name == PropertyName._autoSaveSpin)
		{
			_autoSaveSpin = VariantUtils.ConvertTo<SpinBox>(in value);
			return true;
		}
		if (name == PropertyName._showToastsCheck)
		{
			_showToastsCheck = VariantUtils.ConvertTo<CheckBox>(in value);
			return true;
		}
		if (name == PropertyName._languageOption)
		{
			_languageOption = VariantUtils.ConvertTo<OptionButton>(in value);
			return true;
		}
		if (name == PropertyName._chineseLanguageButton)
		{
			_chineseLanguageButton = VariantUtils.ConvertTo<Button>(in value);
			return true;
		}
		if (name == PropertyName._englishLanguageButton)
		{
			_englishLanguageButton = VariantUtils.ConvertTo<Button>(in value);
			return true;
		}
		if (name == PropertyName._statusLabel)
		{
			_statusLabel = VariantUtils.ConvertTo<Label>(in value);
			return true;
		}
		if (name == PropertyName._exportDirDialog)
		{
			_exportDirDialog = VariantUtils.ConvertTo<FileDialog>(in value);
			return true;
		}
		if (name == PropertyName._gameDirDialog)
		{
			_gameDirDialog = VariantUtils.ConvertTo<FileDialog>(in value);
			return true;
		}
		if (name == PropertyName._settings)
		{
			_settings = VariantUtils.ConvertTo<XWEditorSettings>(in value);
			return true;
		}
		if (name == PropertyName._originalProjectFilePath)
		{
			_originalProjectFilePath = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._nameEdit)
		{
			value = VariantUtils.CreateFrom(in _nameEdit);
			return true;
		}
		if (name == PropertyName._modIdEdit)
		{
			value = VariantUtils.CreateFrom(in _modIdEdit);
			return true;
		}
		if (name == PropertyName._versionEdit)
		{
			value = VariantUtils.CreateFrom(in _versionEdit);
			return true;
		}
		if (name == PropertyName._postIdEdit)
		{
			value = VariantUtils.CreateFrom(in _postIdEdit);
			return true;
		}
		if (name == PropertyName._authorEdit)
		{
			value = VariantUtils.CreateFrom(in _authorEdit);
			return true;
		}
		if (name == PropertyName._descriptionEdit)
		{
			value = VariantUtils.CreateFrom(in _descriptionEdit);
			return true;
		}
		if (name == PropertyName._playInstructionsEdit)
		{
			value = VariantUtils.CreateFrom(in _playInstructionsEdit);
			return true;
		}
		if (name == PropertyName._projectPathEdit)
		{
			value = VariantUtils.CreateFrom(in _projectPathEdit);
			return true;
		}
		if (name == PropertyName._projectFileEdit)
		{
			value = VariantUtils.CreateFrom(in _projectFileEdit);
			return true;
		}
		if (name == PropertyName._exportPathEdit)
		{
			value = VariantUtils.CreateFrom(in _exportPathEdit);
			return true;
		}
		if (name == PropertyName._exportBrowseButton)
		{
			value = VariantUtils.CreateFrom(in _exportBrowseButton);
			return true;
		}
		if (name == PropertyName._gamePathEdit)
		{
			value = VariantUtils.CreateFrom(in _gamePathEdit);
			return true;
		}
		if (name == PropertyName._gameBrowseButton)
		{
			value = VariantUtils.CreateFrom(in _gameBrowseButton);
			return true;
		}
		if (name == PropertyName._autoSaveSpin)
		{
			value = VariantUtils.CreateFrom(in _autoSaveSpin);
			return true;
		}
		if (name == PropertyName._showToastsCheck)
		{
			value = VariantUtils.CreateFrom(in _showToastsCheck);
			return true;
		}
		if (name == PropertyName._languageOption)
		{
			value = VariantUtils.CreateFrom(in _languageOption);
			return true;
		}
		if (name == PropertyName._chineseLanguageButton)
		{
			value = VariantUtils.CreateFrom(in _chineseLanguageButton);
			return true;
		}
		if (name == PropertyName._englishLanguageButton)
		{
			value = VariantUtils.CreateFrom(in _englishLanguageButton);
			return true;
		}
		if (name == PropertyName._statusLabel)
		{
			value = VariantUtils.CreateFrom(in _statusLabel);
			return true;
		}
		if (name == PropertyName._exportDirDialog)
		{
			value = VariantUtils.CreateFrom(in _exportDirDialog);
			return true;
		}
		if (name == PropertyName._gameDirDialog)
		{
			value = VariantUtils.CreateFrom(in _gameDirDialog);
			return true;
		}
		if (name == PropertyName._settings)
		{
			value = VariantUtils.CreateFrom(in _settings);
			return true;
		}
		if (name == PropertyName._originalProjectFilePath)
		{
			value = VariantUtils.CreateFrom(in _originalProjectFilePath);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, PropertyName._nameEdit, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._modIdEdit, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._versionEdit, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._postIdEdit, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._authorEdit, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._descriptionEdit, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._playInstructionsEdit, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._projectPathEdit, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._projectFileEdit, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._exportPathEdit, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._exportBrowseButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._gamePathEdit, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._gameBrowseButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._autoSaveSpin, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._showToastsCheck, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._languageOption, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._chineseLanguageButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._englishLanguageButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._statusLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._exportDirDialog, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._gameDirDialog, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._settings, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.String, PropertyName._originalProjectFilePath, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._nameEdit, Variant.From(in _nameEdit));
		info.AddProperty(PropertyName._modIdEdit, Variant.From(in _modIdEdit));
		info.AddProperty(PropertyName._versionEdit, Variant.From(in _versionEdit));
		info.AddProperty(PropertyName._postIdEdit, Variant.From(in _postIdEdit));
		info.AddProperty(PropertyName._authorEdit, Variant.From(in _authorEdit));
		info.AddProperty(PropertyName._descriptionEdit, Variant.From(in _descriptionEdit));
		info.AddProperty(PropertyName._playInstructionsEdit, Variant.From(in _playInstructionsEdit));
		info.AddProperty(PropertyName._projectPathEdit, Variant.From(in _projectPathEdit));
		info.AddProperty(PropertyName._projectFileEdit, Variant.From(in _projectFileEdit));
		info.AddProperty(PropertyName._exportPathEdit, Variant.From(in _exportPathEdit));
		info.AddProperty(PropertyName._exportBrowseButton, Variant.From(in _exportBrowseButton));
		info.AddProperty(PropertyName._gamePathEdit, Variant.From(in _gamePathEdit));
		info.AddProperty(PropertyName._gameBrowseButton, Variant.From(in _gameBrowseButton));
		info.AddProperty(PropertyName._autoSaveSpin, Variant.From(in _autoSaveSpin));
		info.AddProperty(PropertyName._showToastsCheck, Variant.From(in _showToastsCheck));
		info.AddProperty(PropertyName._languageOption, Variant.From(in _languageOption));
		info.AddProperty(PropertyName._chineseLanguageButton, Variant.From(in _chineseLanguageButton));
		info.AddProperty(PropertyName._englishLanguageButton, Variant.From(in _englishLanguageButton));
		info.AddProperty(PropertyName._statusLabel, Variant.From(in _statusLabel));
		info.AddProperty(PropertyName._exportDirDialog, Variant.From(in _exportDirDialog));
		info.AddProperty(PropertyName._gameDirDialog, Variant.From(in _gameDirDialog));
		info.AddProperty(PropertyName._settings, Variant.From(in _settings));
		info.AddProperty(PropertyName._originalProjectFilePath, Variant.From(in _originalProjectFilePath));
		info.AddSignalEventDelegate(SignalName.ProjectSaved, backing_ProjectSaved);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._nameEdit, out var value))
		{
			_nameEdit = value.As<LineEdit>();
		}
		if (info.TryGetProperty(PropertyName._modIdEdit, out var value2))
		{
			_modIdEdit = value2.As<LineEdit>();
		}
		if (info.TryGetProperty(PropertyName._versionEdit, out var value3))
		{
			_versionEdit = value3.As<LineEdit>();
		}
		if (info.TryGetProperty(PropertyName._postIdEdit, out var value4))
		{
			_postIdEdit = value4.As<LineEdit>();
		}
		if (info.TryGetProperty(PropertyName._authorEdit, out var value5))
		{
			_authorEdit = value5.As<LineEdit>();
		}
		if (info.TryGetProperty(PropertyName._descriptionEdit, out var value6))
		{
			_descriptionEdit = value6.As<TextEdit>();
		}
		if (info.TryGetProperty(PropertyName._playInstructionsEdit, out var value7))
		{
			_playInstructionsEdit = value7.As<TextEdit>();
		}
		if (info.TryGetProperty(PropertyName._projectPathEdit, out var value8))
		{
			_projectPathEdit = value8.As<LineEdit>();
		}
		if (info.TryGetProperty(PropertyName._projectFileEdit, out var value9))
		{
			_projectFileEdit = value9.As<LineEdit>();
		}
		if (info.TryGetProperty(PropertyName._exportPathEdit, out var value10))
		{
			_exportPathEdit = value10.As<LineEdit>();
		}
		if (info.TryGetProperty(PropertyName._exportBrowseButton, out var value11))
		{
			_exportBrowseButton = value11.As<Button>();
		}
		if (info.TryGetProperty(PropertyName._gamePathEdit, out var value12))
		{
			_gamePathEdit = value12.As<LineEdit>();
		}
		if (info.TryGetProperty(PropertyName._gameBrowseButton, out var value13))
		{
			_gameBrowseButton = value13.As<Button>();
		}
		if (info.TryGetProperty(PropertyName._autoSaveSpin, out var value14))
		{
			_autoSaveSpin = value14.As<SpinBox>();
		}
		if (info.TryGetProperty(PropertyName._showToastsCheck, out var value15))
		{
			_showToastsCheck = value15.As<CheckBox>();
		}
		if (info.TryGetProperty(PropertyName._languageOption, out var value16))
		{
			_languageOption = value16.As<OptionButton>();
		}
		if (info.TryGetProperty(PropertyName._chineseLanguageButton, out var value17))
		{
			_chineseLanguageButton = value17.As<Button>();
		}
		if (info.TryGetProperty(PropertyName._englishLanguageButton, out var value18))
		{
			_englishLanguageButton = value18.As<Button>();
		}
		if (info.TryGetProperty(PropertyName._statusLabel, out var value19))
		{
			_statusLabel = value19.As<Label>();
		}
		if (info.TryGetProperty(PropertyName._exportDirDialog, out var value20))
		{
			_exportDirDialog = value20.As<FileDialog>();
		}
		if (info.TryGetProperty(PropertyName._gameDirDialog, out var value21))
		{
			_gameDirDialog = value21.As<FileDialog>();
		}
		if (info.TryGetProperty(PropertyName._settings, out var value22))
		{
			_settings = value22.As<XWEditorSettings>();
		}
		if (info.TryGetProperty(PropertyName._originalProjectFilePath, out var value23))
		{
			_originalProjectFilePath = value23.As<string>();
		}
		if (info.TryGetSignalEventDelegate<ProjectSavedEventHandler>(SignalName.ProjectSaved, out var value24))
		{
			backing_ProjectSaved = value24;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotSignalList()
	{
		return new List<MethodInfo>(1)
		{
			new MethodInfo(SignalName.ProjectSaved, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null)
		};
	}

	protected void EmitSignalProjectSaved()
	{
		EmitSignal(SignalName.ProjectSaved, default(ReadOnlySpan<Variant>));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RaiseGodotClassSignalCallbacks(in godot_string_name signal, NativeVariantPtrArgs args)
	{
		if (signal == SignalName.ProjectSaved && args.Count == 0)
		{
			backing_ProjectSaved?.Invoke();
		}
		else
		{
			base.RaiseGodotClassSignalCallbacks(in signal, args);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassSignal(in godot_string_name signal)
	{
		if (signal == SignalName.ProjectSaved)
		{
			return true;
		}
		return base.HasGodotClassSignal(in signal);
	}
}

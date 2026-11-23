using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModSelectionStore
{
	public string FilePath { get; }

	public string LegacyBackupPath => FilePath + ".legacy.bak";

	public XWModSelectionStore(string path)
	{
		FilePath = Path.GetFullPath(path);
	}

	public bool TryRead(out XWModSelectionState state, out string error)
	{
		byte[] legacy;
		return Read(out state, out legacy, out error);
	}

	private bool Read(out XWModSelectionState state, out byte[] legacy, out string error)
	{
		state = new XWModSelectionState();
		legacy = null;
		error = "";
		try
		{
			if (!File.Exists(FilePath))
			{
				return true;
			}
			byte[] array = File.ReadAllBytes(FilePath);
			using JsonDocument jsonDocument = JsonDocument.Parse(array);
			if (jsonDocument.RootElement.ValueKind == JsonValueKind.Array)
			{
				List<string> list = JsonSerializer.Deserialize(array, XWModSelectionJsonContext.Default.ListString);
				if (list == null || list.Any((string id) => id == null))
				{
					throw new InvalidDataException("旧启用列表包含无效 ID。");
				}
				state.EnabledIds = list;
				legacy = array;
			}
			else
			{
				JsonElement value = jsonDocument.RootElement;
				if (!value.TryGetProperty("schemaVersion", out var value2) || !value2.TryGetInt32(out var value3) || value3 != 2 || !jsonDocument.RootElement.TryGetProperty("enabledIds", out value) || !jsonDocument.RootElement.TryGetProperty("preferredOrder", out value))
				{
					throw new InvalidDataException("启用配置格式或版本无效。");
				}
				state = JsonSerializer.Deserialize(array, XWModSelectionJsonContext.Default.XWModSelectionState);
				if (state?.EnabledIds == null || state.PreferredOrder == null || state.EnabledIds.Any((string id) => id == null) || state.PreferredOrder.Any((string id) => id == null))
				{
					throw new InvalidDataException("启用配置包含无效列表。");
				}
			}
			state = state.Normalize();
			return true;
		}
		catch (Exception ex) when (XWModStateFile.IsStorageError(ex))
		{
			state = new XWModSelectionState();
			error = "读取启用配置失败，原文件已保留：" + ex.Message;
			return false;
		}
	}

	public bool TryWrite(XWModSelectionState state, out string error)
	{
		if (!Read(out var _, out var legacy, out error))
		{
			return false;
		}
		try
		{
			if (state == null || state.SchemaVersion != 2 || state.EnabledIds == null || state.PreferredOrder == null || state.EnabledIds.Any((string id) => id == null) || state.PreferredOrder.Any((string id) => id == null))
			{
				throw new InvalidDataException("待保存的启用配置无效。");
			}
			if (legacy != null && !File.Exists(LegacyBackupPath))
			{
				XWModStateFile.Write(LegacyBackupPath, legacy, overwrite: false);
			}
			XWModStateFile.Write(FilePath, JsonSerializer.SerializeToUtf8Bytes(state.Normalize(), XWModSelectionJsonContext.Default.XWModSelectionState));
			error = "";
			return true;
		}
		catch (Exception ex) when (XWModStateFile.IsStorageError(ex))
		{
			error = "保存启用配置失败：" + ex.Message;
			return false;
		}
	}

	internal bool TryRestoreVerified(XWModSelectionState state, out string error)
	{
		if (TryRead(out var _, out var _))
		{
			return TryWrite(state, out error);
		}
		try
		{
			if (state == null || state.SchemaVersion != 2 || state.EnabledIds == null || state.PreferredOrder == null || state.EnabledIds.Any((string id) => id == null) || state.PreferredOrder.Any((string id) => id == null))
			{
				throw new InvalidDataException("恢复配置无效。");
			}
			byte[] bytes = File.ReadAllBytes(FilePath);
			XWModStateFile.Write(FilePath + "." + Guid.NewGuid().ToString("N") + ".recovery.bak", bytes, overwrite: false);
			XWModStateFile.Write(FilePath, JsonSerializer.SerializeToUtf8Bytes(state.Normalize(), XWModSelectionJsonContext.Default.XWModSelectionState));
			error = "";
			return true;
		}
		catch (Exception ex) when (XWModStateFile.IsStorageError(ex))
		{
			error = "恢复启用配置失败：" + ex.Message;
			return false;
		}
	}
}

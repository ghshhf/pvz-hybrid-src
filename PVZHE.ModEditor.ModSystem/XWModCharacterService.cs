using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Godot;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModCharacterService
{
	private readonly string _owner;

	internal XWModCharacterService(string owner)
	{
		_owner = owner;
	}

	public IReadOnlyList<XWModCharacterSnapshot> Query()
	{
		return XWModCharacters.Query();
	}

	public bool TryGetHandle(TowerDefenseCharacter character, out XWModEntityHandle handle)
	{
		return XWModCharacters.TryGetHandle(character, out handle);
	}

	public bool TryGet(XWModEntityHandle handle, out XWModCharacterSnapshot snapshot)
	{
		snapshot = null;
		if (!XWModCharacters.TryResolve(handle, out var character))
		{
			return false;
		}
		snapshot = XWModCharacters.Snapshot(character);
		return true;
	}

	public async Task<XWModCommandResult<XWModEntityHandle>> SpawnAsync(string packetKey, int column, int row)
	{
		TaskCompletionSource<XWModCommandResult<XWModEntityHandle>> completion = new TaskCompletionSource<XWModCommandResult<XWModEntityHandle>>(TaskCreationOptions.RunContinuationsAsynchronously);
		XWModCommandResult<bool> xWModCommandResult = await XWModExtensions.ExecuteCommand(_owner, "character.spawn", delegate
		{
			TowerDefenseControlNew control = TowerDefenseManager.CurrentControl;
			TowerDefensePacketConfig packetConfig = TowerDefenseManager.GetPacketConfig(packetKey);
			if (!GodotObject.IsInstanceValid(packetConfig))
			{
				throw new InvalidOperationException("未知的角色卡牌。");
			}
			int pending = control.BeginPendingBattleOperation();
			TowerDefenseCharacter character;
			try
			{
				character = packetConfig.Plant(new Vector2I(column, row), playAudio: false, noLimit: false, TowerDefenseManager.Instance.GetLocalSunAccountId());
				if (!GodotObject.IsInstanceValid(character))
				{
					throw new InvalidOperationException("角色不满足正式生成/放置条件。");
				}
			}
			catch
			{
				control.CompletePendingBattleOperation(pending);
				throw;
			}
			XWModEntityHandle handle = XWModCharacters.GetHandle(character);
			Callable.From(delegate
			{
				try
				{
					if (!GodotObject.IsInstanceValid(control) || control != TowerDefenseManager.CurrentControl || !control.IsPendingBattleOperationCurrent(pending) || handle.SessionId != XWModExtensions.SessionId || !GodotObject.IsInstanceValid(character) || !character.IsNodeReady() || character.IsQueuedForDeletion())
					{
						completion.TrySetResult(new XWModCommandResult<XWModEntityHandle>(XWModCommandStatus.Rejected, default(XWModEntityHandle), "生成期间战斗或角色已失效。"));
					}
					else
					{
						TowerDefenseManager.PublishSpawnedCharacter(packetKey, character, useCreate: false);
						XWModCharacters.Ready(character);
						completion.TrySetResult(new XWModCommandResult<XWModEntityHandle>(XWModCommandStatus.Completed, handle, ""));
					}
				}
				catch (Exception ex)
				{
					completion.TrySetResult(new XWModCommandResult<XWModEntityHandle>(XWModCommandStatus.Failed, default(XWModEntityHandle), ex.Message));
				}
				finally
				{
					if (GodotObject.IsInstanceValid(control))
					{
						control.CompletePendingBattleOperation(pending);
					}
				}
			}).CallDeferred();
			return true;
		}, HasAuthority);
		if (!xWModCommandResult.Succeeded)
		{
			return new XWModCommandResult<XWModEntityHandle>(xWModCommandResult.Status, default(XWModEntityHandle), xWModCommandResult.Diagnostic);
		}
		return await completion.Task;
	}

	private static bool HasAuthority()
	{
		if (XWModExtensions.SessionId != Guid.Empty && GodotObject.IsInstanceValid(TowerDefenseManager.CurrentControl) && TowerDefenseManager.CurrentControl.isGameRunning)
		{
			if (Global.IsMultiplayerMode)
			{
				return MultiPlayerManager.IsHost;
			}
			return true;
		}
		return false;
	}

	public Task<XWModCommandResult<bool>> ReviveAsync(XWModEntityHandle handle)
	{
		return XWModExtensions.ExecuteCommand(_owner, "character.revive", delegate
		{
			if (!XWModCharacters.TryResolve(handle, out var character))
			{
				throw new InvalidOperationException("角色句柄失效。");
			}
			if (!TryReviveOwned(_owner, character, out var diagnostic))
			{
				throw new InvalidOperationException(diagnostic);
			}
			return true;
		}, HasAuthority, handle.Id.ToString(CultureInfo.InvariantCulture));
	}

	internal static bool TryReviveOwned(string owner, TowerDefenseCharacter character, out string diagnostic)
	{
		diagnostic = "角色没有声明当前 Mod 可用的复活组件。";
		if (!GodotObject.IsInstanceValid(character) || character.IsQueuedForDeletion() || character.isDestroy || !GodotObject.IsInstanceValid(character.instance) || (!character.die && !character.nearDie))
		{
			diagnostic = "角色已释放、已进入不可逆移除流程，或不处于死亡阶段。";
			return false;
		}
		if (!string.Equals(XWModContentCatalog.GetContentOwner("Character", character.config?.name ?? ""), owner, StringComparison.OrdinalIgnoreCase))
		{
			diagnostic = "只能请求自己提供的角色执行复活。";
			return false;
		}
		if (!GodotObject.IsInstanceValid(character.componentManager))
		{
			return false;
		}
		foreach (CharacterComponentRuntime resourceComponent in character.componentManager.ResourceComponents)
		{
			if (!resourceComponent.IsReleased && resourceComponent.Owner == character && resourceComponent is IXWModCharacterRevival iXWModCharacterRevival && CharacterComponentRuntimeTypeRegistry.TryGetOwner(resourceComponent.GetType(), out var owner2) && string.Equals(owner2, owner, StringComparison.OrdinalIgnoreCase))
			{
				if (!iXWModCharacterRevival.TryRevive())
				{
					diagnostic = "角色复活流程拒绝当前状态。";
					return false;
				}
				if (!GodotObject.IsInstanceValid(character) || character.isDestroy || character.IsQueuedForDeletion() || character.die || character.nearDie || !GodotObject.IsInstanceValid(character.instance) || character.instance.die || character.instance.nearDie || character.instance.hitpoints <= 0.0)
				{
					throw new InvalidOperationException("复活组件返回成功，但角色没有进入有效存活状态。");
				}
				XWModCharacters.Revived(character);
				diagnostic = "";
				return true;
			}
		}
		return false;
	}

	public Task<XWModCommandResult<XWModEntityHandle>> TransformAsync(XWModEntityHandle handle, string targetPacketKey)
	{
		return XWModExtensions.ExecuteCommand(_owner, "character.transform", delegate
		{
			if (Global.IsMultiplayerMode)
			{
				throw new NotSupportedException("当前变身流程不支持联机原子替换。");
			}
			if (!XWModCharacters.TryResolve(handle, out var character) || character.die || character.nearDie || character.isDestroy)
			{
				throw new InvalidOperationException("源角色不可变身。");
			}
			TowerDefensePacketConfig packetConfig = TowerDefenseManager.GetPacketConfig(targetPacketKey);
			if (!GodotObject.IsInstanceValid(packetConfig) || !GodotObject.IsInstanceValid(packetConfig.characterConfig))
			{
				throw new InvalidOperationException("目标角色不存在。");
			}
			TowerDefenseCharacter towerDefenseCharacter = character.TransformTo(packetConfig.characterConfig.name);
			if (!GodotObject.IsInstanceValid(towerDefenseCharacter))
			{
				throw new InvalidOperationException("正式变身流程拒绝目标。");
			}
			return XWModCharacters.GetHandle(towerDefenseCharacter);
		}, HasAuthority, handle.Id.ToString(CultureInfo.InvariantCulture));
	}
}

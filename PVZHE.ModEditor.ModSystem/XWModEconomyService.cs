using System;
using System.Threading.Tasks;
using Godot;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModEconomyService
{
	private readonly string _owner;

	private readonly XWModRuntimeSession _runtimeSession = XWModRuntimeSession.Current;

	internal XWModEconomyService(string owner)
	{
		_owner = owner;
	}

	public bool TryGetBalance(out long balance)
	{
		balance = 0L;
		TowerDefenseManager instance = TowerDefenseManager.Instance;
		if (XWModExtensions.SessionId != Guid.Empty && GodotObject.IsInstanceValid(instance))
		{
			return instance.TryGetSun(EconomyAccountId.Local, out balance);
		}
		return false;
	}

	private bool IsAvailable()
	{
		long balance;
		return TryGetBalance(out balance);
	}

	public Task<XWModCommandResult<bool>> Credit(long amount)
	{
		return XWModExtensions.ExecuteCommand(_owner, "economy.credit", () => TowerDefenseManager.Instance.CreditSun(EconomyAccountId.Local, amount), () => amount >= 0 && IsAvailable());
	}

	public async Task<XWModCommandResult<XWModSunRewardReceipt>> CreditReward(long amount)
	{
		XWModCommandResult<XWModCommandResult<XWModSunRewardReceipt>> xWModCommandResult = await XWModExtensions.ExecuteCommand(_owner, "economy.credit", () => TowerDefenseManager.Instance.CreditRewardSun(EconomyAccountId.Local, amount), () => amount >= 0 && IsAvailable()).ConfigureAwait(continueOnCapturedContext: false);
		return xWModCommandResult.Succeeded ? xWModCommandResult.Value : new XWModCommandResult<XWModSunRewardReceipt>(xWModCommandResult.Status, null, xWModCommandResult.Diagnostic);
	}

	public Task<XWModCommandResult<bool>> Spend(long amount)
	{
		return XWModExtensions.ExecuteCommand(_owner, "economy.spend", () => TowerDefenseManager.Instance.TrySpendSun(EconomyAccountId.Local, amount), () => amount >= 0 && IsAvailable() && TowerDefenseManager.Instance.CanAffordSun(EconomyAccountId.Local, amount));
	}

	public Task<XWModCommandResult<bool>> SetBalance(long amount)
	{
		return XWModExtensions.ExecuteCommand(_owner, "economy.set", () => TowerDefenseManager.Instance.SetSun(EconomyAccountId.Local, amount, SunTransactionReason.Reward), IsAvailable);
	}

	public XWModSunSource CreateSunSource(string instanceKey)
	{
		if (_runtimeSession == XWModRuntimeSession.Current)
		{
			XWModRuntimeSession runtimeSession = _runtimeSession;
			if (runtimeSession != null && runtimeSession.IsOwnerActive(_owner))
			{
				if (XWModExtensions.SessionId == Guid.Empty)
				{
					throw new InvalidOperationException("Sun sources require an active battle.");
				}
				if (string.IsNullOrWhiteSpace(instanceKey) || instanceKey.Length > 256 || instanceKey.Contains('\0'))
				{
					throw new ArgumentException("A source key of at most 256 characters is required.", "instanceKey");
				}
				return new XWModSunSource(XWModSunSourceKind.Mod, _owner, XWModExtensions.SessionId, instanceKey);
			}
		}
		throw new ObjectDisposedException("XWModEconomyService");
	}
}

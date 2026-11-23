using System;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModSunRewardReceipt
{
	public string Account { get; }

	public long Requested { get; }

	public long AppliedDelta { get; }

	public long Balance { get; }

	public Guid BattleSessionId { get; }

	public string Reason => "Reward";

	internal XWModSunRewardReceipt(string account, long requested, long appliedDelta, long balance, Guid battleSessionId)
	{
		Account = account;
		Requested = requested;
		AppliedDelta = appliedDelta;
		Balance = balance;
		BattleSessionId = battleSessionId;
	}
}

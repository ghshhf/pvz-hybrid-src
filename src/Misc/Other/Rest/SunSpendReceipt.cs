using System;
using PVZHE.ModEditor.ModSystem;

public sealed class SunSpendReceipt
{
	private TowerDefenseBattleFeatureSun _owner;

	public EconomyAccountId AccountId { get; }

	public long Amount { get; }

	public long AppliedDelta { get; }

	public Guid BattleSessionId { get; }

	public string Diagnostic { get; private set; }

	internal Guid OperationId { get; }

	public SunSpendReceiptState State { get; private set; }

	public bool IsActive => State == SunSpendReceiptState.Active;

	internal long LedgerGeneration { get; }

	internal long TransactionId { get; }

	internal SunSpendReceipt(TowerDefenseBattleFeatureSun owner, long ledgerGeneration, long transactionId, EconomyAccountId accountId, long amount)
		: this(owner, ledgerGeneration, transactionId, accountId, amount, SafeNegate(amount))
	{
	}

	private static long SafeNegate(long amount)
	{
		if (amount == -9223372036854775808L)
		{
			throw new ArgumentOutOfRangeException("amount");
		}
		return -amount;
	}

	internal SunSpendReceipt(TowerDefenseBattleFeatureSun owner, long ledgerGeneration, long transactionId, EconomyAccountId accountId, long amount, long appliedDelta)
	{
		Diagnostic = "";
		_owner = owner;
		LedgerGeneration = ledgerGeneration;
		TransactionId = transactionId;
		AccountId = accountId;
		Amount = amount;
		AppliedDelta = appliedDelta;
		BattleSessionId = XWModExtensions.SessionId;
		OperationId = XWModExtensions.CreateEventContext().OperationId;
		State = SunSpendReceiptState.Active;
	}

	public bool TryCommit()
	{
		return _owner?.TryCommitSunSpend(this) ?? false;
	}

	public bool TryRollback()
	{
		return _owner?.TryRollbackSunSpend(this) ?? false;
	}

	internal void Complete(SunSpendReceiptState state)
	{
		if (IsActive)
		{
			State = state;
			_owner = null;
		}
	}

	internal void AddDiagnostic(string diagnostic)
	{
		Diagnostic = (string.IsNullOrEmpty(Diagnostic) ? diagnostic : (Diagnostic + " " + diagnostic));
	}
}

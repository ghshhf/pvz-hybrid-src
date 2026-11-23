using System;
using Godot;
using PVZHE.ModEditor.ModSystem;

public readonly struct DropItemCollectionContext
{
	public Node Source { get; }

	public Vector2 Position { get; }

	public long Value { get; }

	public EconomyAccountId AccountId { get; }

	public SunDropOwnershipPolicy OwnershipPolicy { get; }

	public XWModSunSource Attribution { get; }

	internal Guid BattleSessionId { get; }

	public bool UsesLegacyLocalEconomy => OwnershipPolicy == SunDropOwnershipPolicy.LegacySharedReplica;

	internal DropItemCollectionContext WithValue(long value)
	{
		return new DropItemCollectionContext(this, value);
	}

	private DropItemCollectionContext(DropItemCollectionContext original, long value)
		: this(original.Source, original.Position, value, original.AccountId, original.OwnershipPolicy, original.Attribution)
	{
		BattleSessionId = original.BattleSessionId;
	}

	public DropItemCollectionContext(Node source, Vector2 position, long value, EconomyAccountId accountId, SunDropOwnershipPolicy ownershipPolicy = SunDropOwnershipPolicy.LegacySharedReplica)
		: this(source, position, value, accountId, ownershipPolicy, default(XWModSunSource))
	{
	}

	public DropItemCollectionContext(Node source, Vector2 position, long value, EconomyAccountId accountId, SunDropOwnershipPolicy ownershipPolicy, XWModSunSource attribution)
	{
		Attribution = attribution;
		BattleSessionId = XWModExtensions.SessionId;
		Source = source;
		Position = position;
		Value = value;
		if (accountId.IsValid)
		{
			AccountId = accountId;
			OwnershipPolicy = ownershipPolicy;
		}
		else
		{
			AccountId = EconomyAccountId.Local;
			OwnershipPolicy = SunDropOwnershipPolicy.LegacySharedReplica;
		}
	}
}

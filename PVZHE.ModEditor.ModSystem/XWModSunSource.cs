using System;

namespace PVZHE.ModEditor.ModSystem;

public readonly record struct XWModSunSource
{
	public XWModSunSourceKind Kind { get; }

	public string ModId { get; }

	public Guid SessionId { get; }

	public string InstanceKey { get; }

	public Guid InstanceId { get; }

	public XWModEntityHandle Producer { get; }

	public string ProducerContentKey { get; }

	public bool IsEmpty => InstanceId == Guid.Empty;

	public bool IsCurrentBattle
	{
		get
		{
			if (!IsEmpty)
			{
				return SessionId == XWModExtensions.SessionId;
			}
			return false;
		}
	}

	internal XWModSunSource(XWModSunSourceKind kind, string modId, Guid sessionId, string key, XWModEntityHandle producer = default(XWModEntityHandle), string producerKey = "")
	{
		Guid guid = Guid.NewGuid();
		Kind = kind;
		ModId = modId;
		SessionId = sessionId;
		InstanceKey = key;
		InstanceId = guid;
		Producer = producer;
		ProducerContentKey = producerKey;
	}

	internal static XWModSunSource FromProducer(TowerDefenseCharacter producer)
	{
		if (!(XWModExtensions.SessionId == Guid.Empty))
		{
			return new XWModSunSource((producer is TowerDefensePlant) ? XWModSunSourceKind.PlantProduction : XWModSunSourceKind.CharacterProduction, "", XWModExtensions.SessionId, "production", XWModCharacters.GetHandle(producer), producer.config?.name ?? "");
		}
		return default(XWModSunSource);
	}
}

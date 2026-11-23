using System;
using System.Collections.Generic;

namespace PVZHE.ModEditor.ModSystem;

public static class XWModHookPoints
{
	public static readonly XWModNotificationPoint<XWModBattleEvent> BattleStarted = new XWModNotificationPoint<XWModBattleEvent>("battle.started.v1");

	public static readonly XWModNotificationPoint<XWModBattleEvent> BattleEnded = new XWModNotificationPoint<XWModBattleEvent>("battle.ended.v1");

	public static readonly XWModNotificationPoint<XWModCharacterEvent> CharacterCreated = new XWModNotificationPoint<XWModCharacterEvent>("character.created.v1");

	public static readonly XWModNotificationPoint<XWModCharacterEvent> CharacterDestroyed = new XWModNotificationPoint<XWModCharacterEvent>("character.destroyed.v1");

	public static readonly XWModNotificationPoint<XWModUiMountEvent> UiMount = new XWModNotificationPoint<XWModUiMountEvent>("ui.mount.v1");

	public static readonly XWModHookPoint<XWModDamageRequest, double> DamageCalculation = new XWModHookPoint<XWModDamageRequest, double>("damage.calculate.v1");

	public static readonly XWModHookPoint<XWModPlantPlacementRequest, bool> PlantPlacement = new XWModHookPoint<XWModPlantPlacementRequest, bool>("plant.placement.v1");

	public static readonly XWModHookPoint<XWModPlantCostRequest, long> PlantCost = new XWModHookPoint<XWModPlantCostRequest, long>("plant.cost.v1");

	public static IReadOnlyList<string> AllIds { get; } = Array.AsReadOnly(new string[8] { BattleStarted.Id, BattleEnded.Id, CharacterCreated.Id, CharacterDestroyed.Id, UiMount.Id, DamageCalculation.Id, PlantPlacement.Id, PlantCost.Id });
}

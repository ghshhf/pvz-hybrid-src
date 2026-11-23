using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModCombatService
{
	private readonly string _owner;

	internal XWModCombatService(string owner)
	{
		_owner = owner;
	}

	private static bool Resolve(XWModEntityHandle handle, out TowerDefenseCharacter target)
	{
		if (XWModCharacters.TryResolve(handle, out target) && GodotObject.IsInstanceValid(target.instance) && !target.instance.die)
		{
			return !target.isDestroy;
		}
		return false;
	}

	private static Task<XWModCommandResult<T>> Rejected<T>(string reason)
	{
		return Task.FromResult(new XWModCommandResult<T>(XWModCommandStatus.Rejected, default(T), reason));
	}

	public IReadOnlyList<XWModEffectSnapshot> GetEffects(XWModEntityHandle target)
	{
		if (!Resolve(target, out var target2) || target2.buff == null)
		{
			return Array.Empty<XWModEffectSnapshot>();
		}
		List<XWModEffectSnapshot> list = new List<XWModEffectSnapshot>();
		foreach (TowerDefenseCharacterBuffConfig value in target2.buff.buffDictionary.Values)
		{
			if (GodotObject.IsInstanceValid(value))
			{
				XWModEffectParameters xWModEffectParameters = XWModCombat.Parameters(target2, value);
				double? num = XWModCombat.Elapsed(value);
				double? remainingDuration = ((xWModEffectParameters.Duration >= 0.0 && num.HasValue) ? new double?(Math.Max(0.0, xWModEffectParameters.Duration.Value - num.Value)) : ((double?)null));
				list.Add(new XWModEffectSnapshot(value.key, xWModEffectParameters.Duration, remainingDuration, xWModEffectParameters.Strength));
			}
		}
		return list.AsReadOnly();
	}

	public IReadOnlyList<XWModArmorSnapshot> GetArmor(XWModEntityHandle target)
	{
		if (!Resolve(target, out var target2))
		{
			return Array.Empty<XWModArmorSnapshot>();
		}
		List<XWModArmorSnapshot> list = new List<XWModArmorSnapshot>();
		foreach (TowerDefenseArmorInstance armor in target2.instance.armorList)
		{
			if (GodotObject.IsInstanceValid(armor) && !armor.isRemove)
			{
				list.Add(new XWModArmorSnapshot(armor.slotConfig?.armorName ?? "", armor.hitPoints, armor.hitpointsSave, armor.armorMethodFlags));
			}
		}
		return list.AsReadOnly();
	}

	public Task<XWModCommandResult<double>> Damage(XWModEntityHandle target, double amount)
	{
		if (!double.IsFinite(amount) || amount < 0.0 || amount > 3.4028234663852886E+38)
		{
			return Rejected<double>("Damage must be nonnegative and within the host numeric range.");
		}
		TowerDefenseCharacter target2;
		return XWModExtensions.ExecuteCommand(_owner, "combat.damage", delegate
		{
			Resolve(target, out target2);
			double hitpoints = target2.instance.hitpoints;
			TowerDefenseManager.Instance.damagePipeline.ApplyHurt(target2, amount);
			return Math.Max(0.0, hitpoints - target2.instance.hitpoints);
		}, () => XWModCombat.Host && Resolve(target, out target2) && TowerDefenseManager.Instance?.damagePipeline != null, target.Id.ToString());
	}

	public Task<XWModCommandResult<double>> Heal(XWModEntityHandle target, double amount)
	{
		if (!double.IsFinite(amount) || Math.Abs(amount) > 3.4028234663852886E+38)
		{
			return Rejected<double>("Healing is outside the host numeric range.");
		}
		TowerDefenseCharacter target2;
		return XWModExtensions.ExecuteCommand(_owner, "combat.heal", delegate
		{
			Resolve(target, out target2);
			double hitpoints = target2.instance.hitpoints;
			target2.Health(amount);
			return target2.instance.hitpoints - hitpoints;
		}, () => XWModCombat.Host && Resolve(target, out target2), target.Id.ToString());
	}

	public Task<XWModCommandResult<bool>> ApplyEffect(XWModEntityHandle target, string key, double? duration = null, double? strength = null)
	{
		if (string.IsNullOrWhiteSpace(key) || (duration.HasValue && !double.IsFinite(duration.Value)) || (strength.HasValue && !double.IsFinite(strength.Value)))
		{
			return Rejected<bool>("Invalid effect parameters.");
		}
		TowerDefenseCharacter target2;
		return XWModExtensions.ExecuteCommand(_owner, "combat.effect.apply", delegate
		{
			Resolve(target, out target2);
			TowerDefenseCharacterBuffConfig towerDefenseCharacterBuffConfig = TowerDefenseCharacterBuffConfig.CreateBuffByKey(key);
			if (towerDefenseCharacterBuffConfig == null)
			{
				return false;
			}
			XWModEffectParameters xWModEffectParameters = XWModCombat.Parameters(target2, towerDefenseCharacterBuffConfig);
			XWModEffectParameters xWModEffectParameters2 = xWModEffectParameters with
			{
				Duration = (duration ?? xWModEffectParameters.Duration),
				Strength = (strength ?? xWModEffectParameters.Strength)
			};
			if (XWModCombatPoints.ValidEffect(xWModEffectParameters2))
			{
				XWModCombat.ApplyParameters(towerDefenseCharacterBuffConfig, xWModEffectParameters2);
				target2.buff.AddBuff(towerDefenseCharacterBuffConfig);
				bool result = target2.buff.BuffHas(key);
				if (target2.buff.BuffGet(key) != towerDefenseCharacterBuffConfig)
				{
					towerDefenseCharacterBuffConfig.Dispose();
				}
				return result;
			}
			towerDefenseCharacterBuffConfig.Dispose();
			return false;
		}, () => XWModCombat.Host && Resolve(target, out target2) && target2.buff != null, target.Id.ToString());
	}

	public Task<XWModCommandResult<bool>> RemoveEffect(XWModEntityHandle target, string key)
	{
		if (string.IsNullOrWhiteSpace(key))
		{
			return Rejected<bool>("Missing effect key.");
		}
		TowerDefenseCharacter target2;
		return XWModExtensions.ExecuteCommand(_owner, "combat.effect.remove", delegate
		{
			Resolve(target, out target2);
			bool num = target2.buff.BuffHas(key);
			target2.buff.DeleteBuff(key);
			return num && !target2.buff.BuffHas(key);
		}, () => XWModCombat.Host && Resolve(target, out target2) && target2.buff != null, target.Id.ToString());
	}
}

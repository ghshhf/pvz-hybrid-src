using System.Collections.Generic;
using Godot;
using Godot.Collections;

public readonly struct BulletFieldStoredProjectile
{
	public readonly TowerDefenseProjectileConfig Config;

	public readonly int FireMethodFlags;

	public BulletFieldStoredProjectile(TowerDefenseProjectileConfig config, int fireMethodFlags)
	{
		Config = config;
		FireMethodFlags = fireMethodFlags;
	}

	public static Array<Dictionary> ExportSave(IEnumerable<BulletFieldStoredProjectile> projectiles)
	{
		Array<Dictionary> array = new Array<Dictionary>();
		foreach (BulletFieldStoredProjectile projectile in projectiles)
		{
			if (GodotObject.IsInstanceValid(projectile.Config))
			{
				array.Add(new Dictionary
				{
					["config"] = projectile.Config.Duplicate(deep: true),
					["fireMethodFlags"] = projectile.FireMethodFlags
				});
			}
		}
		return array;
	}

	public static BulletFieldStoredProjectile[] ImportSave(Array data)
	{
		List<BulletFieldStoredProjectile> list = new List<BulletFieldStoredProjectile>();
		foreach (Variant datum in data)
		{
			if (datum.VariantType == Variant.Type.Dictionary)
			{
				Dictionary dictionary = datum.AsGodotDictionary();
				if (dictionary.TryGetValue("config", out var value) && value.VariantType == Variant.Type.Object && value.AsGodotObject() is TowerDefenseProjectileConfig towerDefenseProjectileConfig && GodotObject.IsInstanceValid(towerDefenseProjectileConfig))
				{
					list.Add(new BulletFieldStoredProjectile(towerDefenseProjectileConfig, dictionary.GetValueOrDefault("fireMethodFlags", towerDefenseProjectileConfig.fireMethodFlags).AsInt32()));
				}
			}
		}
		return list.ToArray();
	}
}

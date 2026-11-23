using System;
using Godot;

public static class NetworkProjectileEffectRegistry
{
	public const string CherryPeaBurst = "cherry_pea_burst";

	public const string PeaCobCannonBurst = "pea_cob_cannon_burst";

	public const string ButterGloomBurst = "butter_gloom_burst";

	public const string PlanternSixBurst = "plantern_six_burst";

	public const string ShootingStarsRain = "shooting_stars_rain";

	public const string PumpkinWizardSmoke = "pumpkin_wizard_smoke";

	public const string ScaredyShroomElfSmoke = "scaredy_shroom_elf_smoke";

	public const string MagicStoneHaste = "magic_stone_haste";

	private const string CherryPeaScene = "res://Prefab/ProjectileEffect/CherryPea/TowerDefenseProjectileEffectCherryPea.tscn";

	private const string PeaCobCannonScene = "res://Prefab/ProjectileEffect/PeaCobCannonExplode/TowerDefenseProjectileEffectPeaCobCannonExplode.tscn";

	private const string ButterGloomScene = "res://Prefab/ProjectileEffect/GroomButter/TowerDefenseProjectileEffectGroomButter.tscn";

	private const string PlanternSixScene = "res://Prefab/ProjectileEffect/PlanternSix/TowerDefenseProjectileEffectPlanternSix.tscn";

	private const string ShootingStarsScene = "res://Prefab/ProjectileEffect/ShootingStars/TowerDefenseProjectileEffectShootingStars.tscn";

	public static bool TrySpawnReplay(ProjectileEffectSpawnDto dto)
	{
		if (!IsValid(dto))
		{
			return false;
		}
		if (dto.effect_id == "pumpkin_wizard_smoke" || dto.effect_id == "scaredy_shroom_elf_smoke" || dto.effect_id == "magic_stone_haste")
		{
			return TrySpawnMagicVisual(dto);
		}
		string text = dto.effect_id switch
		{
			"cherry_pea_burst" => "res://Prefab/ProjectileEffect/CherryPea/TowerDefenseProjectileEffectCherryPea.tscn", 
			"pea_cob_cannon_burst" => "res://Prefab/ProjectileEffect/PeaCobCannonExplode/TowerDefenseProjectileEffectPeaCobCannonExplode.tscn", 
			"butter_gloom_burst" => "res://Prefab/ProjectileEffect/GroomButter/TowerDefenseProjectileEffectGroomButter.tscn", 
			"plantern_six_burst" => "res://Prefab/ProjectileEffect/PlanternSix/TowerDefenseProjectileEffectPlanternSix.tscn", 
			"shooting_stars_rain" => "res://Prefab/ProjectileEffect/ShootingStars/TowerDefenseProjectileEffectShootingStars.tscn", 
			_ => "", 
		};
		if (text == "")
		{
			return false;
		}
		PackedScene packedScene = GD.Load<PackedScene>(text);
		Node2D characterNode = TowerDefenseManager.GetCharacterNode();
		if (!GodotObject.IsInstanceValid(packedScene) || !GodotObject.IsInstanceValid(characterNode))
		{
			return false;
		}
		TowerDefenseProjectileEffectBase towerDefenseProjectileEffectBase = packedScene.Instantiate<TowerDefenseProjectileEffectBase>(PackedScene.GenEditState.Disabled);
		towerDefenseProjectileEffectBase.ConfigureNetworkVariant(dto.variant);
		towerDefenseProjectileEffectBase.Init(new Vector2I(dto.grid_x, dto.grid_y), (TowerDefenseEnum.CHARACTER_CAMP)dto.camp, 0, null, dto.height);
		towerDefenseProjectileEffectBase.ConfigureNetworkReplay(dto.random_seed);
		towerDefenseProjectileEffectBase.GlobalPosition = new Vector2((float)dto.px, (float)dto.py);
		characterNode.AddChild(towerDefenseProjectileEffectBase, forceReadableName: false, Node.InternalMode.Disabled);
		return true;
	}

	private static bool TrySpawnMagicVisual(ProjectileEffectSpawnDto dto)
	{
		Node2D characterNode = TowerDefenseManager.GetCharacterNode();
		if (!GodotObject.IsInstanceValid(characterNode) || (dto.variant != "" && dto.variant != "Custom0"))
		{
			return false;
		}
		int num;
		object path;
		if (!(dto.effect_id == "pumpkin_wizard_smoke"))
		{
			num = ((dto.effect_id == "scaredy_shroom_elf_smoke") ? 1 : 0);
			if (num == 0)
			{
				path = ((dto.variant == "Custom0") ? "uid://ducvqa7sucvw7" : "uid://ji2eyuk24efl");
				goto IL_0081;
			}
		}
		else
		{
			num = 1;
		}
		path = "uid://djvfnrjg7vtqn";
		goto IL_0081;
		IL_0081:
		PackedScene scene = GD.Load<PackedScene>((string)path);
		Vector2I gridPos = new Vector2I(dto.grid_x, dto.grid_y);
		Node2D node2D = ((num != 0) ? ((TowerDefenseEffectBase)TowerDefenseManager.CreateEffectParticlesOnce(scene, gridPos)) : ((TowerDefenseEffectBase)TowerDefenseManager.CreateEffectSpriteOnce(scene, gridPos, "Fire")));
		characterNode.AddChild(node2D, forceReadableName: false, Node.InternalMode.Disabled);
		node2D.GlobalPosition = new Vector2((float)dto.px, (float)dto.py);
		return true;
	}

	private static bool IsValid(ProjectileEffectSpawnDto dto)
	{
		if (dto != null && dto.effect_id != "" && double.IsFinite(dto.px) && double.IsFinite(dto.py) && double.IsFinite(dto.height) && dto.grid_x >= -1 && dto.grid_x <= 1024 && dto.grid_y >= -1 && dto.grid_y <= 1024)
		{
			return Enum.IsDefined(typeof(TowerDefenseEnum.CHARACTER_CAMP), dto.camp);
		}
		return false;
	}
}

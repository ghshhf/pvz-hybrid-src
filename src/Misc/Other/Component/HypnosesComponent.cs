using Godot;

public sealed class HypnosesComponent : CharacterComponentRuntime
{
	private HypnosesComponentDefinition Definition => base.ComponentDefinition as HypnosesComponentDefinition;

	public void Hypnoses(float time = -1f, bool canFliter = true, TowerDefenseCharacterBuffHypnoses hypnosesConfig = null)
	{
		TowerDefenseCharacter owner = base.Owner;
		if (base.Alive && base.Lifecycle == ComponentRuntimeLifecycle.Active && GodotObject.IsInstanceValid(owner) && GodotObject.IsInstanceValid(owner.instance) && !owner.instance.hologram)
		{
			HypnosesInternal(time, canFliter, hypnosesConfig);
		}
	}

	internal void HypnosesInternal(float time, bool canFliter, TowerDefenseCharacterBuffHypnoses hypnosesConfig)
	{
		TowerDefenseCharacter owner = base.Owner;
		if (!base.Alive || base.Lifecycle != ComponentRuntimeLifecycle.Active || !GodotObject.IsInstanceValid(owner) || !GodotObject.IsInstanceValid(owner.instance))
		{
			return;
		}
		if (owner.BuffGet("Hypnoses") is TowerDefenseCharacterBuffHypnoses towerDefenseCharacterBuffHypnoses)
		{
			towerDefenseCharacterBuffHypnoses.ApplyHypnosis(time);
		}
		else if ((owner.instance.unUseBuffFlags & 8) == 0)
		{
			bool num = Definition?.playHypnosesAudio ?? true;
			string text = Definition?.hypnosesAudio ?? "Floop";
			if (num && !string.IsNullOrEmpty(text) && GodotObject.IsInstanceValid(AudioManager.Instance))
			{
				AudioManager.Instance.AudioPlay(text);
			}
			TowerDefenseCharacterBuffHypnoses towerDefenseCharacterBuffHypnoses2 = (GodotObject.IsInstanceValid(hypnosesConfig) ? (hypnosesConfig.Duplicate(deep: true) as TowerDefenseCharacterBuffHypnoses) : null);
			if (towerDefenseCharacterBuffHypnoses2 == null)
			{
				towerDefenseCharacterBuffHypnoses2 = new TowerDefenseCharacterBuffHypnoses();
			}
			towerDefenseCharacterBuffHypnoses2.time = time;
			towerDefenseCharacterBuffHypnoses2.canFliter = canFliter;
			owner.BuffAdd(towerDefenseCharacterBuffHypnoses2);
		}
	}
}

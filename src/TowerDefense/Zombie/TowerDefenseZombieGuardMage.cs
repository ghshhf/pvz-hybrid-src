using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;

[Tool]
[GlobalClass]
[ScriptPath("res://Asset/Anime/Character/Zombie/Chapter10/GuardMage/Scene/TowerDefenseZombieGuardMage.cs")]
public class TowerDefenseZombieGuardMage : TowerDefenseZombie
{
	public new class MethodName : TowerDefenseZombie.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName CanCast = "CanCast";

		public static readonly StringName ProcessSpell = "ProcessSpell";

		public static readonly StringName OnTimerTimeout = "OnTimerTimeout";

		public new static readonly StringName IdleProcessing = "IdleProcessing";

		public new static readonly StringName WalkProcessing = "WalkProcessing";

		public new static readonly StringName AttackProcessing = "AttackProcessing";

		public new static readonly StringName Walk = "Walk";

		public static readonly StringName CastEntered = "CastEntered";

		public static readonly StringName CastProcessing = "CastProcessing";

		public static readonly StringName CastExited = "CastExited";

		public new static readonly StringName AnimeEvent = "AnimeEvent";

		public static readonly StringName SpawnShieldEffect = "SpawnShieldEffect";

		public new static readonly StringName AnimeCompleted = "AnimeCompleted";

		public new static readonly StringName ExportVariantSave = "ExportVariantSave";

		public new static readonly StringName ImportVariantSave = "ImportVariantSave";
	}

	public new class PropertyName : TowerDefenseZombie.PropertyName
	{
		public new static readonly StringName CanReceiveShieldValue = "CanReceiveShieldValue";

		public static readonly StringName shieldEffect = "shieldEffect";

		public static readonly StringName _enteredField = "_enteredField";

		public static readonly StringName _spellPending = "_spellPending";

		public static readonly StringName _preserveCastOnRestore = "_preserveCastOnRestore";
	}

	public new class SignalName : TowerDefenseZombie.SignalName
	{
	}

	private const double CastInterval = 10.0;

	private const double ShieldGrantHp = 300.0;

	[Export(PropertyHint.None, "")]
	public PackedScene shieldEffect;

	private readonly List<TowerDefenseCharacter> _candidates = new List<TowerDefenseCharacter>();

	private StateHandle _castState;

	private CharacterTimerComponent _timerComponent;

	private bool _enteredField;

	private bool _spellPending;

	private bool _preserveCastOnRestore;

	public override bool CanReceiveShieldValue => false;

	public override void _Ready()
	{
		base._Ready();
		if (!Engine.IsEditorHint() && !editorPreviewMode)
		{
			_castState = base.StateMachine.GetStateById("zombie.mage.cast");
			_timerComponent = componentManager.GetRuntime<CharacterTimerComponent>("character.timer");
			CharacterTimerComponent timerComponent = _timerComponent;
			if (timerComponent != null && !timerComponent.IsReleased)
			{
				_timerComponent.OnTimeout += OnTimerTimeout;
			}
			_castState.Entered += CastEntered;
			_castState.PhysicsProcessing += CastProcessing;
			_castState.Exited += CastExited;
		}
	}

	public override void _ExitTree()
	{
		CharacterTimerComponent timerComponent = _timerComponent;
		if (timerComponent != null && !timerComponent.IsReleased)
		{
			_timerComponent.OnTimeout -= OnTimerTimeout;
		}
		if (_castState != null)
		{
			_castState.Entered -= CastEntered;
			_castState.PhysicsProcessing -= CastProcessing;
			_castState.Exited -= CastExited;
			_castState = null;
		}
		_candidates.Clear();
		base._ExitTree();
	}

	private bool CanCast()
	{
		if (CanRunAbility() && TowerDefenseManager.HasGameplayAuthority && inGame && !isRise)
		{
			return GodotObject.IsInstanceValid(base.sprite);
		}
		return false;
	}

	private void ProcessSpell()
	{
		if (!CanCast())
		{
			return;
		}
		StateHandle castState = _castState;
		if (castState == null || !castState.IsValid)
		{
			return;
		}
		CharacterTimerComponent timerComponent = _timerComponent;
		if (timerComponent != null && !timerComponent.IsReleased)
		{
			_enteredField |= base.IsInsideComponentBattlefield;
			if (_enteredField && !_timerComponent.IsRunning("Cast"))
			{
				_timerComponent.Run("Cast", 10.0);
			}
		}
	}

	private void OnTimerTimeout(string timerName)
	{
		if (!(timerName != "Cast"))
		{
			StateHandle castState = _castState;
			if ((castState == null || !castState.IsActive) && CanCast())
			{
				SendStateEvent("ToMageCast");
			}
		}
	}

	public override void IdleProcessing(double delta)
	{
		base.IdleProcessing(delta);
		ProcessSpell();
	}

	public override void WalkProcessing(double delta)
	{
		base.WalkProcessing(delta);
		ProcessSpell();
	}

	public override void AttackProcessing(double delta)
	{
		base.AttackProcessing(delta);
		ProcessSpell();
	}

	public override void Walk()
	{
		StateHandle castState = _castState;
		if (castState == null || !castState.IsActive || die || nearDie)
		{
			base.Walk();
		}
	}

	private void CastEntered()
	{
		groundMoveComponent?.SetAlive(value: false);
		startAttack = false;
		if (_preserveCastOnRestore)
		{
			_preserveCastOnRestore = false;
			return;
		}
		_timerComponent?.Run("Cast", 10.0);
		_spellPending = true;
		base.sprite.SetAnimation("Magic", loop: false);
	}

	private void CastProcessing(double delta)
	{
		base.sprite.timeScale = timeScale;
		ProcessSpell();
	}

	private void CastExited()
	{
		_spellPending = false;
	}

	public override void AnimeEvent(string command, Variant argument)
	{
		base.AnimeEvent(command, argument);
		if (command != "magic" || !_spellPending)
		{
			return;
		}
		StateHandle castState = _castState;
		if (castState == null || !castState.IsActive || !inGame || die || nearDie || isDestroy)
		{
			return;
		}
		_spellPending = false;
		_candidates.Clear();
		_candidates.AddRange(TowerDefenseManager.Instance.characterRegistry.GetCharactersForGridWindowList(base.gridPos.X, base.gridPos.Y, 1, 1));
		foreach (TowerDefenseCharacter candidate in _candidates)
		{
			if (candidate == this || candidate.camp != base.camp || candidate.die || candidate.nearDie || candidate.isDestroy || !candidate.inGame)
			{
				continue;
			}
			if (candidate is TowerDefenseZombie towerDefenseZombie)
			{
				if (towerDefenseZombie.CanReceiveShieldValue && towerDefenseZombie.instance.zombiePhysique != TowerDefenseEnum.ZOMBIE_PHYSIQUE.BOSS)
				{
					if (TowerDefenseManager.HasGameplayAuthority)
					{
						towerDefenseZombie.instance.AddShieldValue(300.0);
					}
					SpawnShieldEffect(towerDefenseZombie);
				}
			}
			else if (candidate is TowerDefensePlant towerDefensePlant && !(towerDefensePlant is TowerDefensePlantBowlingBase) && towerDefensePlant.CanReceiveShieldValue && GodotObject.IsInstanceValid(towerDefensePlant.cell))
			{
				if (TowerDefenseManager.HasGameplayAuthority)
				{
					towerDefensePlant.instance.AddShieldValue(300.0);
				}
				SpawnShieldEffect(towerDefensePlant);
			}
		}
	}

	private void SpawnShieldEffect(TowerDefenseCharacter target)
	{
		if (shieldEffect != null && GodotObject.IsInstanceValid(target.transformPoint))
		{
			TowerDefenseEffectSpriteOnce towerDefenseEffectSpriteOnce = TowerDefenseManager.CreateEffectSpriteOnce(shieldEffect, target.gridPos, "Fire");
			target.transformPoint.AddChild(towerDefenseEffectSpriteOnce, forceReadableName: false, InternalMode.Disabled);
			towerDefenseEffectSpriteOnce.GlobalPosition = target.GetLogicalGlobalPosition() + new Vector2(0f, -35f);
			towerDefenseEffectSpriteOnce.ZIndex = (int)(10 - target.itemLayer);
		}
	}

	public override void AnimeCompleted(string clip)
	{
		base.AnimeCompleted(clip);
		if (clip == "Magic")
		{
			StateHandle castState = _castState;
			if (castState != null && castState.IsActive && TowerDefenseManager.HasGameplayAuthority)
			{
				base.Walk();
			}
		}
	}

	public override Dictionary ExportVariantSave()
	{
		Dictionary dictionary = base.ExportVariantSave();
		dictionary["mageEnteredField"] = _enteredField;
		dictionary["mageSpellPending"] = _spellPending;
		dictionary["mageCasting"] = _castState?.IsActive ?? false;
		return dictionary;
	}

	public override void ImportVariantSave(Dictionary data)
	{
		base.ImportVariantSave(data);
		_enteredField = data.GetValueOrDefault("mageEnteredField", false).AsBool();
		_spellPending = data.GetValueOrDefault("mageSpellPending", false).AsBool();
		_preserveCastOnRestore = data.GetValueOrDefault("mageCasting", false).AsBool();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(17)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.CanCast, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ProcessSpell, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.OnTimerTimeout, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "timerName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.IdleProcessing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.WalkProcessing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.AttackProcessing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.Walk, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.CastEntered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.CastProcessing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.CastExited, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.AnimeEvent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "command", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Nil, "argument", PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.NilIsVariant, exported: false)
			}, null),
			new MethodInfo(MethodName.SpawnShieldEffect, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "target", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.AnimeCompleted, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "clip", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ExportVariantSave, new PropertyInfo(Variant.Type.Dictionary, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ImportVariantSave, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Dictionary, "data", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CanCast && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(CanCast());
			return true;
		}
		if (method == MethodName.ProcessSpell && args.Count == 0)
		{
			ProcessSpell();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnTimerTimeout && args.Count == 1)
		{
			OnTimerTimeout(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.IdleProcessing && args.Count == 1)
		{
			IdleProcessing(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.WalkProcessing && args.Count == 1)
		{
			WalkProcessing(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AttackProcessing && args.Count == 1)
		{
			AttackProcessing(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Walk && args.Count == 0)
		{
			Walk();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CastEntered && args.Count == 0)
		{
			CastEntered();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CastProcessing && args.Count == 1)
		{
			CastProcessing(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CastExited && args.Count == 0)
		{
			CastExited();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimeEvent && args.Count == 2)
		{
			AnimeEvent(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<Variant>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SpawnShieldEffect && args.Count == 1)
		{
			SpawnShieldEffect(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimeCompleted && args.Count == 1)
		{
			AnimeCompleted(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ExportVariantSave && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<Dictionary>(ExportVariantSave());
			return true;
		}
		if (method == MethodName.ImportVariantSave && args.Count == 1)
		{
			ImportVariantSave(VariantUtils.ConvertTo<Dictionary>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.CanCast)
		{
			return true;
		}
		if (method == MethodName.ProcessSpell)
		{
			return true;
		}
		if (method == MethodName.OnTimerTimeout)
		{
			return true;
		}
		if (method == MethodName.IdleProcessing)
		{
			return true;
		}
		if (method == MethodName.WalkProcessing)
		{
			return true;
		}
		if (method == MethodName.AttackProcessing)
		{
			return true;
		}
		if (method == MethodName.Walk)
		{
			return true;
		}
		if (method == MethodName.CastEntered)
		{
			return true;
		}
		if (method == MethodName.CastProcessing)
		{
			return true;
		}
		if (method == MethodName.CastExited)
		{
			return true;
		}
		if (method == MethodName.AnimeEvent)
		{
			return true;
		}
		if (method == MethodName.SpawnShieldEffect)
		{
			return true;
		}
		if (method == MethodName.AnimeCompleted)
		{
			return true;
		}
		if (method == MethodName.ExportVariantSave)
		{
			return true;
		}
		if (method == MethodName.ImportVariantSave)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.shieldEffect)
		{
			shieldEffect = VariantUtils.ConvertTo<PackedScene>(in value);
			return true;
		}
		if (name == PropertyName._enteredField)
		{
			_enteredField = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._spellPending)
		{
			_spellPending = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._preserveCastOnRestore)
		{
			_preserveCastOnRestore = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.CanReceiveShieldValue)
		{
			value = VariantUtils.CreateFrom<bool>(CanReceiveShieldValue);
			return true;
		}
		if (name == PropertyName.shieldEffect)
		{
			value = VariantUtils.CreateFrom(in shieldEffect);
			return true;
		}
		if (name == PropertyName._enteredField)
		{
			value = VariantUtils.CreateFrom(in _enteredField);
			return true;
		}
		if (name == PropertyName._spellPending)
		{
			value = VariantUtils.CreateFrom(in _spellPending);
			return true;
		}
		if (name == PropertyName._preserveCastOnRestore)
		{
			value = VariantUtils.CreateFrom(in _preserveCastOnRestore);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, PropertyName.shieldEffect, PropertyHint.ResourceType, "PackedScene", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Bool, PropertyName._enteredField, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._spellPending, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._preserveCastOnRestore, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.CanReceiveShieldValue, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.shieldEffect, Variant.From(in shieldEffect));
		info.AddProperty(PropertyName._enteredField, Variant.From(in _enteredField));
		info.AddProperty(PropertyName._spellPending, Variant.From(in _spellPending));
		info.AddProperty(PropertyName._preserveCastOnRestore, Variant.From(in _preserveCastOnRestore));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.shieldEffect, out var value))
		{
			shieldEffect = value.As<PackedScene>();
		}
		if (info.TryGetProperty(PropertyName._enteredField, out var value2))
		{
			_enteredField = value2.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._spellPending, out var value3))
		{
			_spellPending = value3.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._preserveCastOnRestore, out var value4))
		{
			_preserveCastOnRestore = value4.As<bool>();
		}
	}
}

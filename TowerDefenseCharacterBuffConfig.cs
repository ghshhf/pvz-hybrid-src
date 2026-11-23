using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;

[GlobalClass]
[ScriptPath("res://Resource/TowerDefense/Character/Buff/TowerDefenseCharacterBuffConfig.cs")]
public class TowerDefenseCharacterBuffConfig : Resource
{
	public new class MethodName : Resource.MethodName
	{
		public static readonly StringName CreateRuntimeInstance = "CreateRuntimeInstance";

		public static readonly StringName CreateBuiltInRuntimeRoot = "CreateBuiltInRuntimeRoot";

		public static readonly StringName _Init = "_Init";

		public static readonly StringName Enter = "Enter";

		public static readonly StringName EnterReadOnlyClient = "EnterReadOnlyClient";

		public static readonly StringName Step = "Step";

		public static readonly StringName StepReadOnlyClient = "StepReadOnlyClient";

		public static readonly StringName SyncPresentationReadOnlyClient = "SyncPresentationReadOnlyClient";

		public static readonly StringName Exit = "Exit";

		public static readonly StringName Remove = "Remove";

		public static readonly StringName ExitReadOnlyClient = "ExitReadOnlyClient";

		public static readonly StringName Cancel = "Cancel";

		public static readonly StringName Refresh = "Refresh";

		public static readonly StringName SetAttackNum = "SetAttackNum";

		public static readonly StringName Destroy = "Destroy";

		public static readonly StringName CreateBuffByKey = "CreateBuffByKey";
	}

	public new class PropertyName : Resource.PropertyName
	{
		public static readonly StringName AffectsHologram = "AffectsHologram";

		public static readonly StringName FrameMeshColorMultiplier = "FrameMeshColorMultiplier";

		public static readonly StringName MayModifyIncomingDamage = "MayModifyIncomingDamage";

		public static readonly StringName key = "key";

		public static readonly StringName refresh = "refresh";

		public static readonly StringName canFliter = "canFliter";

		public static readonly StringName character = "character";
	}

	public new class SignalName : Resource.SignalName
	{
	}

	[Export(PropertyHint.None, "")]
	public string key;

	[Export(PropertyHint.None, "")]
	public bool refresh = true;

	[Export(PropertyHint.None, "")]
	public bool canFliter = true;

	public TowerDefenseCharacter character;

	public virtual double? produceTimeScaleOverride => null;

	public virtual bool AffectsHologram => false;

	public virtual Color FrameMeshColorMultiplier => Colors.White;

	public virtual bool MayModifyIncomingDamage => GetType().Assembly != typeof(TowerDefenseCharacterBuffConfig).Assembly;

	public virtual TowerDefenseCharacterBuffConfig CreateRuntimeInstance()
	{
		TowerDefenseCharacterBuffConfig towerDefenseCharacterBuffConfig = CreateBuiltInRuntimeRoot();
		if (towerDefenseCharacterBuffConfig == null)
		{
			GD.PushError("Buff type '" + GetType().FullName + "' must override CreateRuntimeInstance(); automatic Resource duplication is disabled for runtime Buffs.");
			return null;
		}
		towerDefenseCharacterBuffConfig.key = key;
		towerDefenseCharacterBuffConfig.refresh = refresh;
		towerDefenseCharacterBuffConfig.canFliter = canFliter;
		return towerDefenseCharacterBuffConfig;
	}

	private TowerDefenseCharacterBuffConfig CreateBuiltInRuntimeRoot()
	{
		Type type = GetType();
		if (type == typeof(TowerDefenseCharacterBuffConfig))
		{
			return new TowerDefenseCharacterBuffConfig();
		}
		if (type == typeof(TowerDefenseCharacterBuffAttackSpeedDown))
		{
			TowerDefenseCharacterBuffAttackSpeedDown towerDefenseCharacterBuffAttackSpeedDown = (TowerDefenseCharacterBuffAttackSpeedDown)this;
			return new TowerDefenseCharacterBuffAttackSpeedDown
			{
				timeScaleValue = towerDefenseCharacterBuffAttackSpeedDown.timeScaleValue,
				time = towerDefenseCharacterBuffAttackSpeedDown.time,
				currentTime = towerDefenseCharacterBuffAttackSpeedDown.currentTime
			};
		}
		if (type == typeof(TowerDefenseCharacterBuffBurn))
		{
			TowerDefenseCharacterBuffBurn towerDefenseCharacterBuffBurn = (TowerDefenseCharacterBuffBurn)this;
			return new TowerDefenseCharacterBuffBurn
			{
				time = towerDefenseCharacterBuffBurn.time,
				dpsAttack = towerDefenseCharacterBuffBurn.dpsAttack,
				splatSceneType = towerDefenseCharacterBuffBurn.splatSceneType,
				splatScene = towerDefenseCharacterBuffBurn.splatScene,
				splatInterval = towerDefenseCharacterBuffBurn.splatInterval,
				currentTime = towerDefenseCharacterBuffBurn.currentTime,
				splatTime = towerDefenseCharacterBuffBurn.splatTime
			};
		}
		if (type == typeof(TowerDefenseCharacterBuffButter))
		{
			TowerDefenseCharacterBuffButter towerDefenseCharacterBuffButter = (TowerDefenseCharacterBuffButter)this;
			return new TowerDefenseCharacterBuffButter
			{
				time = towerDefenseCharacterBuffButter.time,
				currentTime = towerDefenseCharacterBuffButter.currentTime,
				splatTexture = towerDefenseCharacterBuffButter.splatTexture
			};
		}
		if (type == typeof(TowerDefenseCharacterBuffCherry))
		{
			TowerDefenseCharacterBuffCherry towerDefenseCharacterBuffCherry = (TowerDefenseCharacterBuffCherry)this;
			return new TowerDefenseCharacterBuffCherry
			{
				time = towerDefenseCharacterBuffCherry.time,
				currentTime = towerDefenseCharacterBuffCherry.currentTime
			};
		}
		if (type == typeof(TowerDefenseCharacterBuffCoffee))
		{
			TowerDefenseCharacterBuffCoffee towerDefenseCharacterBuffCoffee = (TowerDefenseCharacterBuffCoffee)this;
			return new TowerDefenseCharacterBuffCoffee
			{
				timeScaleValue = towerDefenseCharacterBuffCoffee.timeScaleValue,
				time = towerDefenseCharacterBuffCoffee.time,
				currentTime = towerDefenseCharacterBuffCoffee.currentTime,
				blink = towerDefenseCharacterBuffCoffee.blink
			};
		}
		if (type == typeof(TowerDefenseCharacterBuffEMP))
		{
			TowerDefenseCharacterBuffEMP towerDefenseCharacterBuffEMP = (TowerDefenseCharacterBuffEMP)this;
			return new TowerDefenseCharacterBuffEMP
			{
				time = towerDefenseCharacterBuffEMP.time,
				currentTime = towerDefenseCharacterBuffEMP.currentTime
			};
		}
		if (type == typeof(TowerDefenseCharacterBuffEMSpeedDown))
		{
			TowerDefenseCharacterBuffEMSpeedDown towerDefenseCharacterBuffEMSpeedDown = (TowerDefenseCharacterBuffEMSpeedDown)this;
			return new TowerDefenseCharacterBuffEMSpeedDown
			{
				time = towerDefenseCharacterBuffEMSpeedDown.time,
				currentTime = towerDefenseCharacterBuffEMSpeedDown.currentTime
			};
		}
		if (type == typeof(TowerDefenseCharacterBuffDizziness))
		{
			TowerDefenseCharacterBuffDizziness towerDefenseCharacterBuffDizziness = (TowerDefenseCharacterBuffDizziness)this;
			return new TowerDefenseCharacterBuffDizziness
			{
				time = towerDefenseCharacterBuffDizziness.time,
				magicImmune = towerDefenseCharacterBuffDizziness.magicImmune,
				currentTime = towerDefenseCharacterBuffDizziness.currentTime
			};
		}
		if (type == typeof(TowerDefenseCharacterBuffMagicImmobilize))
		{
			TowerDefenseCharacterBuffMagicImmobilize towerDefenseCharacterBuffMagicImmobilize = (TowerDefenseCharacterBuffMagicImmobilize)this;
			return new TowerDefenseCharacterBuffMagicImmobilize
			{
				time = towerDefenseCharacterBuffMagicImmobilize.time,
				currentTime = towerDefenseCharacterBuffMagicImmobilize.currentTime
			};
		}
		if (type == typeof(TowerDefenseCharacterBuffSealMagic))
		{
			TowerDefenseCharacterBuffSealMagic towerDefenseCharacterBuffSealMagic = (TowerDefenseCharacterBuffSealMagic)this;
			return new TowerDefenseCharacterBuffSealMagic
			{
				time = towerDefenseCharacterBuffSealMagic.time,
				currentTime = towerDefenseCharacterBuffSealMagic.currentTime
			};
		}
		if (type == typeof(TowerDefenseCharacterBuffFireHit))
		{
			return new TowerDefenseCharacterBuffFireHit();
		}
		if (type == typeof(TowerDefenseCharacterBuffFluorescence))
		{
			TowerDefenseCharacterBuffFluorescence towerDefenseCharacterBuffFluorescence = (TowerDefenseCharacterBuffFluorescence)this;
			return new TowerDefenseCharacterBuffFluorescence
			{
				time = towerDefenseCharacterBuffFluorescence.time,
				currentTime = towerDefenseCharacterBuffFluorescence.currentTime
			};
		}
		if (type == typeof(TowerDefenseCharacterBuffRadiance))
		{
			TowerDefenseCharacterBuffRadiance towerDefenseCharacterBuffRadiance = (TowerDefenseCharacterBuffRadiance)this;
			return new TowerDefenseCharacterBuffRadiance
			{
				permanent = towerDefenseCharacterBuffRadiance.permanent,
				time = towerDefenseCharacterBuffRadiance.time,
				flashOnDeath = towerDefenseCharacterBuffRadiance.flashOnDeath,
				radianceRemoveOnCampFlip = towerDefenseCharacterBuffRadiance.radianceRemoveOnCampFlip,
				deathFlashRequireSameCamp = towerDefenseCharacterBuffRadiance.deathFlashRequireSameCamp,
				baselineCamp = towerDefenseCharacterBuffRadiance.baselineCamp,
				currentTime = towerDefenseCharacterBuffRadiance.currentTime
			};
		}
		if (type == typeof(TowerDefenseCharacterBuffFrozen))
		{
			TowerDefenseCharacterBuffFrozen towerDefenseCharacterBuffFrozen = (TowerDefenseCharacterBuffFrozen)this;
			return new TowerDefenseCharacterBuffFrozen
			{
				time = towerDefenseCharacterBuffFrozen.time,
				iceSpeedDownTime = towerDefenseCharacterBuffFrozen.iceSpeedDownTime,
				currentTime = towerDefenseCharacterBuffFrozen.currentTime
			};
		}
		if (type == typeof(TowerDefenseCharacterBuffHypnoses))
		{
			TowerDefenseCharacterBuffHypnoses towerDefenseCharacterBuffHypnoses = (TowerDefenseCharacterBuffHypnoses)this;
			return new TowerDefenseCharacterBuffHypnoses
			{
				time = towerDefenseCharacterBuffHypnoses.time,
				currentTime = towerDefenseCharacterBuffHypnoses.currentTime,
				saveCamp = towerDefenseCharacterBuffHypnoses.saveCamp,
				enableTorchwood = towerDefenseCharacterBuffHypnoses.enableTorchwood,
				torchwoodChangeName = towerDefenseCharacterBuffHypnoses.torchwoodChangeName,
				torchwoodAudio = towerDefenseCharacterBuffHypnoses.torchwoodAudio,
				torchwoodAreaSize = towerDefenseCharacterBuffHypnoses.torchwoodAreaSize,
				enableDeathFreeze = towerDefenseCharacterBuffHypnoses.enableDeathFreeze,
				deathFreezeTime = towerDefenseCharacterBuffHypnoses.deathFreezeTime,
				deathSlowTime = towerDefenseCharacterBuffHypnoses.deathSlowTime,
				deathDamage = towerDefenseCharacterBuffHypnoses.deathDamage,
				enableSunProduce = towerDefenseCharacterBuffHypnoses.enableSunProduce,
				sunProduceInterval = towerDefenseCharacterBuffHypnoses.sunProduceInterval,
				sunProduceNum = towerDefenseCharacterBuffHypnoses.sunProduceNum,
				deathFreezeOver = towerDefenseCharacterBuffHypnoses.deathFreezeOver,
				hypnosisEntries = towerDefenseCharacterBuffHypnoses.hypnosisEntries.Duplicate(deep: true)
			};
		}
		if (type == typeof(TowerDefenseCharacterBuffIceSpeedDown))
		{
			TowerDefenseCharacterBuffIceSpeedDown towerDefenseCharacterBuffIceSpeedDown = (TowerDefenseCharacterBuffIceSpeedDown)this;
			return new TowerDefenseCharacterBuffIceSpeedDown
			{
				time = towerDefenseCharacterBuffIceSpeedDown.time,
				currentTime = towerDefenseCharacterBuffIceSpeedDown.currentTime
			};
		}
		if (type == typeof(TowerDefenseCharacterBuffTimeMagic))
		{
			TowerDefenseCharacterBuffTimeMagic towerDefenseCharacterBuffTimeMagic = (TowerDefenseCharacterBuffTimeMagic)this;
			return new TowerDefenseCharacterBuffTimeMagic
			{
				timeScaleValue = towerDefenseCharacterBuffTimeMagic.timeScaleValue,
				time = towerDefenseCharacterBuffTimeMagic.time,
				currentTime = towerDefenseCharacterBuffTimeMagic.currentTime
			};
		}
		if (type == typeof(TowerDefenseCharacterBuffJalaHit))
		{
			return new TowerDefenseCharacterBuffJalaHit();
		}
		if (type == typeof(TowerDefenseCharacterBuffNormallHit))
		{
			return new TowerDefenseCharacterBuffNormallHit();
		}
		if (type == typeof(TowerDefenseCharacterBuffPogo))
		{
			TowerDefenseCharacterBuffPogo towerDefenseCharacterBuffPogo = (TowerDefenseCharacterBuffPogo)this;
			return new TowerDefenseCharacterBuffPogo
			{
				time = towerDefenseCharacterBuffPogo.time,
				jumpSpeed = towerDefenseCharacterBuffPogo.jumpSpeed,
				pogoGravity = towerDefenseCharacterBuffPogo.pogoGravity,
				currentTime = towerDefenseCharacterBuffPogo.currentTime
			};
		}
		if (type == typeof(TowerDefenseCharacterBuffPoisoning))
		{
			TowerDefenseCharacterBuffPoisoning towerDefenseCharacterBuffPoisoning = (TowerDefenseCharacterBuffPoisoning)this;
			return new TowerDefenseCharacterBuffPoisoning
			{
				time = towerDefenseCharacterBuffPoisoning.time,
				currentTime = towerDefenseCharacterBuffPoisoning.currentTime,
				timer = towerDefenseCharacterBuffPoisoning.timer
			};
		}
		if (type == typeof(TowerDefenseCharacterBuffRedHeat))
		{
			TowerDefenseCharacterBuffRedHeat towerDefenseCharacterBuffRedHeat = (TowerDefenseCharacterBuffRedHeat)this;
			return new TowerDefenseCharacterBuffRedHeat
			{
				time = towerDefenseCharacterBuffRedHeat.time,
				currentTime = towerDefenseCharacterBuffRedHeat.currentTime
			};
		}
		if (type == typeof(TowerDefenseCharacterBuffSleep))
		{
			TowerDefenseCharacterBuffSleep towerDefenseCharacterBuffSleep = (TowerDefenseCharacterBuffSleep)this;
			return new TowerDefenseCharacterBuffSleep
			{
				time = towerDefenseCharacterBuffSleep.time,
				currentTime = towerDefenseCharacterBuffSleep.currentTime
			};
		}
		if (type == typeof(TowerDefenseCharacterBuffSquid))
		{
			TowerDefenseCharacterBuffSquid towerDefenseCharacterBuffSquid = (TowerDefenseCharacterBuffSquid)this;
			return new TowerDefenseCharacterBuffSquid
			{
				time = towerDefenseCharacterBuffSquid.time,
				currentTime = towerDefenseCharacterBuffSquid.currentTime
			};
		}
		if (type == typeof(TowerDefenseCharacterBuffTabooBean))
		{
			TowerDefenseCharacterBuffTabooBean towerDefenseCharacterBuffTabooBean = (TowerDefenseCharacterBuffTabooBean)this;
			return new TowerDefenseCharacterBuffTabooBean
			{
				time = towerDefenseCharacterBuffTabooBean.time,
				currentTime = towerDefenseCharacterBuffTabooBean.currentTime,
				blink = towerDefenseCharacterBuffTabooBean.blink,
				produceTimeScaleValue = towerDefenseCharacterBuffTabooBean.produceTimeScaleValue
			};
		}
		return null;
	}

	public virtual void _Init()
	{
	}

	public virtual void Enter()
	{
	}

	public virtual void EnterReadOnlyClient()
	{
	}

	public virtual bool Step(double delta)
	{
		return true;
	}

	public virtual void StepReadOnlyClient(double delta)
	{
	}

	public virtual void SyncPresentationReadOnlyClient()
	{
	}

	public virtual void Exit()
	{
	}

	public virtual void Remove()
	{
		Exit();
	}

	public virtual void ExitReadOnlyClient()
	{
	}

	public virtual void Cancel()
	{
		ExitReadOnlyClient();
	}

	public virtual void Refresh(TowerDefenseCharacterBuffConfig config)
	{
	}

	public virtual double SetAttackNum(double num)
	{
		return num;
	}

	public virtual void Destroy()
	{
	}

	public static TowerDefenseCharacterBuffConfig CreateBuffByKey(string buffKey)
	{
		TowerDefenseCharacterBuffConfig towerDefenseCharacterBuffConfig;
		switch (buffKey)
		{
		case "AttackSpeedDown":
			towerDefenseCharacterBuffConfig = new TowerDefenseCharacterBuffAttackSpeedDown();
			break;
		case "Frozen":
			towerDefenseCharacterBuffConfig = new TowerDefenseCharacterBuffFrozen();
			break;
		case "Burn":
			towerDefenseCharacterBuffConfig = new TowerDefenseCharacterBuffBurn();
			break;
		case "Hypnoses":
			towerDefenseCharacterBuffConfig = new TowerDefenseCharacterBuffHypnoses();
			break;
		case "IceSpeedDown":
			towerDefenseCharacterBuffConfig = new TowerDefenseCharacterBuffIceSpeedDown();
			break;
		case "Dizziness":
			towerDefenseCharacterBuffConfig = new TowerDefenseCharacterBuffDizziness();
			break;
		case "SirenHaste":
		case "KangarooRageHaste":
		case "CoffeePotHaste":
			towerDefenseCharacterBuffConfig = new TowerDefenseCharacterBuffOverallHaste
			{
				key = buffKey
			};
			break;
		case "SurfWaveHold":
		case "MagicBubble":
			towerDefenseCharacterBuffConfig = new TowerDefenseCharacterBuffMagicAnchor
			{
				key = buffKey
			};
			break;
		case "MagicImmobilize":
			towerDefenseCharacterBuffConfig = new TowerDefenseCharacterBuffMagicImmobilize();
			break;
		case "MagicRootHaste":
			towerDefenseCharacterBuffConfig = new TowerDefenseCharacterBuffMagicRootHaste();
			break;
		case "SealMagic":
			towerDefenseCharacterBuffConfig = new TowerDefenseCharacterBuffSealMagic();
			break;
		case "PoisonBottleHaste":
			towerDefenseCharacterBuffConfig = new TowerDefenseCharacterBuffPoisonBottleHaste();
			break;
		case "TimeMagic":
			towerDefenseCharacterBuffConfig = new TowerDefenseCharacterBuffTimeMagic();
			break;
		case "Butter":
			towerDefenseCharacterBuffConfig = new TowerDefenseCharacterBuffButter();
			break;
		case "ButterGene":
			towerDefenseCharacterBuffConfig = new TowerDefenseCharacterBuffButterGene();
			break;
		case "Cherry":
			towerDefenseCharacterBuffConfig = new TowerDefenseCharacterBuffCherry();
			break;
		case "Pogo":
			towerDefenseCharacterBuffConfig = new TowerDefenseCharacterBuffPogo();
			break;
		case "Sleep":
			towerDefenseCharacterBuffConfig = new TowerDefenseCharacterBuffSleep();
			break;
		case "FireHit":
			towerDefenseCharacterBuffConfig = new TowerDefenseCharacterBuffFireHit();
			break;
		case "JalaHit":
			towerDefenseCharacterBuffConfig = new TowerDefenseCharacterBuffJalaHit();
			break;
		case "RedHeat":
			towerDefenseCharacterBuffConfig = new TowerDefenseCharacterBuffRedHeat();
			break;
		case "Poisoning":
			towerDefenseCharacterBuffConfig = new TowerDefenseCharacterBuffPoisoning();
			break;
		case "Fluorescence":
			towerDefenseCharacterBuffConfig = new TowerDefenseCharacterBuffFluorescence();
			break;
		case "RuneFogHaste":
		case "RuneStormSlow":
		case "RuneFogDizzyImmune":
			towerDefenseCharacterBuffConfig = new TowerDefenseCharacterBuffRuneMagic
			{
				key = buffKey
			};
			break;
		case "Radiance":
			towerDefenseCharacterBuffConfig = new TowerDefenseCharacterBuffRadiance();
			break;
		case "NormalHit":
			towerDefenseCharacterBuffConfig = new TowerDefenseCharacterBuffNormallHit();
			break;
		case "Squid":
			towerDefenseCharacterBuffConfig = new TowerDefenseCharacterBuffSquid();
			break;
		case "TabooBean":
			towerDefenseCharacterBuffConfig = new TowerDefenseCharacterBuffTabooBean();
			break;
		case "Coffee":
			towerDefenseCharacterBuffConfig = new TowerDefenseCharacterBuffCoffee();
			break;
		case "EMP":
			towerDefenseCharacterBuffConfig = new TowerDefenseCharacterBuffEMP();
			break;
		case "EMSpeedDown":
			towerDefenseCharacterBuffConfig = new TowerDefenseCharacterBuffEMSpeedDown();
			break;
		default:
			return null;
		}
		towerDefenseCharacterBuffConfig._Init();
		return towerDefenseCharacterBuffConfig;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(16)
		{
			new MethodInfo(MethodName.CreateRuntimeInstance, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Resource"), exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.CreateBuiltInRuntimeRoot, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Resource"), exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._Init, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.Enter, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.EnterReadOnlyClient, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.Step, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.StepReadOnlyClient, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.SyncPresentationReadOnlyClient, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.Exit, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.Remove, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ExitReadOnlyClient, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.Cancel, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.Refresh, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "config", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Resource"), exported: false)
			}, null),
			new MethodInfo(MethodName.SetAttackNum, new PropertyInfo(Variant.Type.Float, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "num", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.Destroy, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.CreateBuffByKey, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Resource"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "buffKey", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.CreateRuntimeInstance && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<TowerDefenseCharacterBuffConfig>(CreateRuntimeInstance());
			return true;
		}
		if (method == MethodName.CreateBuiltInRuntimeRoot && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<TowerDefenseCharacterBuffConfig>(CreateBuiltInRuntimeRoot());
			return true;
		}
		if (method == MethodName._Init && args.Count == 0)
		{
			_Init();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Enter && args.Count == 0)
		{
			Enter();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EnterReadOnlyClient && args.Count == 0)
		{
			EnterReadOnlyClient();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Step && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<bool>(Step(VariantUtils.ConvertTo<double>(in args[0])));
			return true;
		}
		if (method == MethodName.StepReadOnlyClient && args.Count == 1)
		{
			StepReadOnlyClient(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SyncPresentationReadOnlyClient && args.Count == 0)
		{
			SyncPresentationReadOnlyClient();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Exit && args.Count == 0)
		{
			Exit();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Remove && args.Count == 0)
		{
			Remove();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ExitReadOnlyClient && args.Count == 0)
		{
			ExitReadOnlyClient();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Cancel && args.Count == 0)
		{
			Cancel();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Refresh && args.Count == 1)
		{
			Refresh(VariantUtils.ConvertTo<TowerDefenseCharacterBuffConfig>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetAttackNum && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<double>(SetAttackNum(VariantUtils.ConvertTo<double>(in args[0])));
			return true;
		}
		if (method == MethodName.Destroy && args.Count == 0)
		{
			Destroy();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CreateBuffByKey && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<TowerDefenseCharacterBuffConfig>(CreateBuffByKey(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.CreateBuffByKey && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<TowerDefenseCharacterBuffConfig>(CreateBuffByKey(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.CreateRuntimeInstance)
		{
			return true;
		}
		if (method == MethodName.CreateBuiltInRuntimeRoot)
		{
			return true;
		}
		if (method == MethodName._Init)
		{
			return true;
		}
		if (method == MethodName.Enter)
		{
			return true;
		}
		if (method == MethodName.EnterReadOnlyClient)
		{
			return true;
		}
		if (method == MethodName.Step)
		{
			return true;
		}
		if (method == MethodName.StepReadOnlyClient)
		{
			return true;
		}
		if (method == MethodName.SyncPresentationReadOnlyClient)
		{
			return true;
		}
		if (method == MethodName.Exit)
		{
			return true;
		}
		if (method == MethodName.Remove)
		{
			return true;
		}
		if (method == MethodName.ExitReadOnlyClient)
		{
			return true;
		}
		if (method == MethodName.Cancel)
		{
			return true;
		}
		if (method == MethodName.Refresh)
		{
			return true;
		}
		if (method == MethodName.SetAttackNum)
		{
			return true;
		}
		if (method == MethodName.Destroy)
		{
			return true;
		}
		if (method == MethodName.CreateBuffByKey)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.key)
		{
			key = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName.refresh)
		{
			refresh = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.canFliter)
		{
			canFliter = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.character)
		{
			character = VariantUtils.ConvertTo<TowerDefenseCharacter>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		bool from;
		if (name == PropertyName.AffectsHologram)
		{
			from = AffectsHologram;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.FrameMeshColorMultiplier)
		{
			value = VariantUtils.CreateFrom<Color>(FrameMeshColorMultiplier);
			return true;
		}
		if (name == PropertyName.MayModifyIncomingDamage)
		{
			from = MayModifyIncomingDamage;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.key)
		{
			value = VariantUtils.CreateFrom(in key);
			return true;
		}
		if (name == PropertyName.refresh)
		{
			value = VariantUtils.CreateFrom(in refresh);
			return true;
		}
		if (name == PropertyName.canFliter)
		{
			value = VariantUtils.CreateFrom(in canFliter);
			return true;
		}
		if (name == PropertyName.character)
		{
			value = VariantUtils.CreateFrom(in character);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, PropertyName.key, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Bool, PropertyName.refresh, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Bool, PropertyName.canFliter, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Bool, PropertyName.AffectsHologram, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName.character, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Color, PropertyName.FrameMeshColorMultiplier, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.MayModifyIncomingDamage, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.key, Variant.From(in key));
		info.AddProperty(PropertyName.refresh, Variant.From(in refresh));
		info.AddProperty(PropertyName.canFliter, Variant.From(in canFliter));
		info.AddProperty(PropertyName.character, Variant.From(in character));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.key, out var value))
		{
			key = value.As<string>();
		}
		if (info.TryGetProperty(PropertyName.refresh, out var value2))
		{
			refresh = value2.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.canFliter, out var value3))
		{
			canFliter = value3.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.character, out var value4))
		{
			character = value4.As<TowerDefenseCharacter>();
		}
	}
}

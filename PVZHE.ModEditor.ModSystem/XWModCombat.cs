using System;
using Godot;

namespace PVZHE.ModEditor.ModSystem;

internal static class XWModCombat
{
	internal readonly struct DamageScope : IDisposable
	{
		private readonly DamageOperation _operation;

		internal bool Rejected { get; }

		internal DamageScope(DamageOperation operation, bool rejected)
		{
			bool flag = rejected;
			_operation = operation;
			Rejected = flag;
		}

		public void Dispose()
		{
			_operation?.Complete();
		}
	}

	internal sealed class DamageOperation
	{
		internal readonly TowerDefenseCharacterInstance Instance;

		internal readonly XWModEntityHandle Target;

		internal readonly XWModEntityHandle Source;

		internal readonly XWModEventContext Context;

		private readonly TowerDefenseCharacter _source;

		private readonly IDisposable _dispatch;

		private readonly string _cause;

		private readonly string _projectile;

		private readonly XWModProjectileSource _attribution;

		private readonly Vector2? _targetPositionAtEntry;

		private readonly double _lifeBefore;

		private readonly double _armorBefore;

		private readonly double _requested;

		private readonly bool _wasDead;

		internal double Modified;

		internal double ShieldAbsorbed;

		internal DamageOperation(TowerDefenseCharacterInstance instance, double amount, string cause, TowerDefenseCharacter source, string projectile, XWModProjectileSource attribution)
		{
			_dispatch = XWModExtensions.EnterOperation();
			TowerDefenseCharacter character = instance.character;
			if (GodotObject.IsInstanceValid(character) && character.IsInsideTree())
			{
				Vector2 logicalGlobalPosition = character.GetLogicalGlobalPosition();
				if (logicalGlobalPosition.IsFinite())
				{
					_targetPositionAtEntry = logicalGlobalPosition;
				}
			}
			Instance = instance;
			Target = XWModCharacters.GetHandle(instance.character);
			Source = XWModCharacters.GetHandle(source);
			Context = XWModExtensions.CreateEventContext(XWModEventOrigin.Gameplay, XWModAuthority.Host, cause);
			_source = source;
			_cause = cause;
			_projectile = projectile;
			_attribution = attribution;
			_lifeBefore = instance.hitpoints;
			_armorBefore = ArmorDurability(instance);
			_wasDead = instance.die;
			_requested = (Modified = amount);
		}

		internal void Cancel()
		{
			_dispatch.Dispose();
		}

		internal void Complete()
		{
			try
			{
				Instance.ModDamageOperation = null;
				double num = Math.Max(0.0, _lifeBefore - Instance.hitpoints);
				double num2 = Math.Max(0.0, _armorBefore - ArmorDurability(Instance));
				if (XWModExtensions.HasHandlers(XWModCombatPoints.DamageApplied.Id))
				{
					XWModExtensions.Notify(XWModCombatPoints.DamageApplied, new XWModDamageApplied(Target, Source, _cause, _requested, Modified, num, num2, ShieldAbsorbed, num == 0.0 && num2 == 0.0 && ShieldAbsorbed == 0.0, !_wasDead && Instance.die)
					{
						Attribution = _attribution,
						ProjectileKey = _projectile,
						TargetPositionAtEntry = _targetPositionAtEntry
					}, Context);
				}
				Life(Instance, _lifeBefore, _cause, Context);
				if (_projectile != null)
				{
					XWModCharacters.ProjectileHit(_source, Instance.character, _projectile, num, Context, _attribution);
				}
			}
			finally
			{
				_dispatch.Dispose();
			}
		}
	}

	internal readonly struct ArmorScope : IDisposable
	{
		private readonly TowerDefenseArmorInstance _armor;

		private readonly double _before;

		private readonly bool _removed;

		private readonly string _cause;

		private readonly XWModEventContext _context;

		private readonly IDisposable _dispatch;

		internal ArmorScope(TowerDefenseArmorInstance armor, string cause, XWModEventOrigin origin)
		{
			_armor = armor;
			_before = armor.hitPoints;
			_removed = armor.isRemove;
			_cause = cause;
			_context = ((origin != XWModEventOrigin.Gameplay) ? null : armor.character?.instance?.ModDamageOperation?.Context);
			if ((object)_context == null)
			{
				_context = XWModExtensions.CreateEventContext(origin, (origin != XWModEventOrigin.Gameplay) ? XWModAuthority.Local : XWModAuthority.Host, cause);
			}
			_dispatch = XWModExtensions.EnterOperation();
			armor.ModArmorObservationDepth++;
		}

		public void Dispose()
		{
			if (_armor == null)
			{
				return;
			}
			try
			{
				_armor.ModArmorObservationDepth--;
				if (_before != _armor.hitPoints || _removed != _armor.isRemove || _context.Origin != XWModEventOrigin.Gameplay)
				{
					Armor(_armor, _before, _cause, _context);
				}
			}
			finally
			{
				_dispatch.Dispose();
			}
		}
	}

	internal readonly struct LifeRestoreScope : IDisposable
	{
		private readonly TowerDefenseCharacterInstance _instance;

		private readonly bool _previous;

		private readonly double _before;

		internal LifeRestoreScope(TowerDefenseCharacterInstance instance)
		{
			_instance = instance;
			_previous = instance.ModLifeRestoring;
			_before = instance.hitpoints;
			instance.ModLifeRestoring = true;
		}

		public void Dispose()
		{
			_instance.ModLifeRestoring = _previous;
			if (!_previous)
			{
				TowerDefenseCharacterInstance instance = _instance;
				double before = _before;
				TowerDefenseCharacter character = _instance.character;
				ObserveLife(instance, before, (character != null && character.IsRemoteNetworkReplica) ? XWModEventOrigin.NetworkRestore : XWModEventOrigin.SaveRestore);
			}
		}
	}

	internal static bool Host
	{
		get
		{
			if (Global.IsMultiplayerMode)
			{
				return MultiPlayerManager.IsHost;
			}
			return true;
		}
	}

	internal static IDisposable DeferEffects()
	{
		if (XWModRuntimeSession.Current == null || (!XWModExtensions.HasHandlers(XWModCombatPoints.EffectChanged.Id) && !XWModExtensions.HasHandlers(XWModCombatPoints.EffectObserved.Id)))
		{
			return null;
		}
		return XWModExtensions.EnterOperation();
	}

	internal static bool Gameplay(TowerDefenseCharacter character)
	{
		if (Host && GodotObject.IsInstanceValid(character) && character.inGame && !character.editorPreviewMode && !character.IsProgressRestoreInFlight)
		{
			return !character.IsRemoteNetworkReplica;
		}
		return false;
	}

	internal static DamageScope BeginDamage(TowerDefenseCharacterInstance instance, ref double amount, string cause, TowerDefenseCharacter source = null, string projectileKey = null, XWModProjectileSource attribution = default(XWModProjectileSource))
	{
		if (instance.ModLifeRestoring || !Gameplay(instance.character) || instance.ModDamageOperation != null || XWModRuntimeSession.Current == null)
		{
			return default(DamageScope);
		}
		if (!XWModExtensions.HasHandlers(XWModCombatPoints.Damage.Id) && !XWModExtensions.HasHandlers(XWModCombatPoints.DamageApplied.Id) && !XWModExtensions.HasHandlers(XWModCombatPoints.LifeChanged.Id) && !XWModExtensions.HasHandlers(XWModCombatPoints.ArmorChanged.Id) && !XWModExtensions.HasHandlers(XWModCombatPoints.EffectChanged.Id) && !XWModExtensions.HasHandlers(XWModCharacterPoints.Lifecycle.Id) && (projectileKey == null || !XWModExtensions.HasHandlers(XWModCharacterPoints.ProjectileHit.Id)))
		{
			return default(DamageScope);
		}
		DamageOperation damageOperation = new DamageOperation(instance, amount, cause, source, projectileKey, attribution);
		try
		{
			XWModDamageValue xWModDamageValue = XWModExtensions.Modify(XWModCombatPoints.Damage, new XWModDamageValue(damageOperation.Target, damageOperation.Source, cause, amount)
			{
				Attribution = attribution
			}, damageOperation.Context);
			amount = xWModDamageValue.Amount;
			damageOperation.Modified = amount;
			instance.ModDamageOperation = damageOperation;
			return new DamageScope(damageOperation, rejected: false);
		}
		catch (XWModHookCallbackException)
		{
			damageOperation.Cancel();
			return new DamageScope(null, rejected: true);
		}
		catch
		{
			damageOperation.Cancel();
			throw;
		}
	}

	private static double ArmorDurability(TowerDefenseCharacterInstance instance)
	{
		double num = 0.0;
		foreach (TowerDefenseArmorInstance armor in instance.armorList)
		{
			if (GodotObject.IsInstanceValid(armor) && !armor.isRemove)
			{
				num += Math.Max(0.0, armor.hitPoints);
			}
		}
		return num;
	}

	internal static XWModEventOrigin Origin(TowerDefenseCharacter character)
	{
		if (character == null || !character.IsRemoteNetworkReplica)
		{
			if (character == null || !character.IsProgressRestoreInFlight)
			{
				if (character != null && character.inGame && !character.editorPreviewMode)
				{
					return XWModEventOrigin.Gameplay;
				}
				return XWModEventOrigin.InitialBinding;
			}
			return XWModEventOrigin.SaveRestore;
		}
		return XWModEventOrigin.NetworkRestore;
	}

	internal static ArmorScope BeginArmor(TowerDefenseArmorInstance armor, string cause, XWModEventOrigin? origin = null)
	{
		XWModEventOrigin xWModEventOrigin = origin ?? Origin(armor.character);
		XWModEventPoint<XWModArmorChanged> xWModEventPoint = ((xWModEventOrigin == XWModEventOrigin.Gameplay) ? XWModCombatPoints.ArmorChanged : XWModCombatPoints.ArmorObserved);
		if (armor.ModArmorObservationDepth != 0 || (xWModEventOrigin == XWModEventOrigin.Gameplay && !Host) || !XWModExtensions.HasHandlers(xWModEventPoint.Id))
		{
			return default(ArmorScope);
		}
		return new ArmorScope(armor, cause, xWModEventOrigin);
	}

	internal static void Armor(TowerDefenseArmorInstance armor, double before, string cause, XWModEventContext context = null)
	{
		XWModEventOrigin xWModEventOrigin = context?.Origin ?? Origin(armor.character);
		XWModEventPoint<XWModArmorChanged> xWModEventPoint = ((xWModEventOrigin == XWModEventOrigin.Gameplay) ? XWModCombatPoints.ArmorChanged : XWModCombatPoints.ArmorObserved);
		if ((xWModEventOrigin != XWModEventOrigin.Gameplay || Host) && XWModExtensions.HasHandlers(xWModEventPoint.Id))
		{
			if ((object)context == null)
			{
				context = XWModExtensions.CreateEventContext(xWModEventOrigin, (xWModEventOrigin != XWModEventOrigin.Gameplay) ? XWModAuthority.Local : XWModAuthority.Host, cause);
			}
			XWModExtensions.Notify(xWModEventPoint, new XWModArmorChanged(XWModCharacters.GetHandle(armor.character), armor.slotConfig?.armorName ?? "", before, armor.hitPoints, armor.isRemove, cause), context);
		}
	}

	internal static bool ModifyHealing(TowerDefenseCharacterInstance instance, ref double amount, XWModEventContext context)
	{
		if (instance.ModLifeRestoring || !Gameplay(instance.character) || !XWModExtensions.HasHandlers(XWModCombatPoints.Healing.Id))
		{
			return true;
		}
		try
		{
			amount = XWModExtensions.Modify(XWModCombatPoints.Healing, new XWModDamageValue(XWModCharacters.GetHandle(instance.character), default(XWModEntityHandle), "Healing", amount), context).Amount;
			return true;
		}
		catch (XWModHookCallbackException)
		{
			return false;
		}
	}

	internal static void Life(TowerDefenseCharacterInstance instance, double before, string cause, XWModEventContext context = null)
	{
		if (!instance.ModLifeRestoring && Gameplay(instance.character) && before != instance.hitpoints && XWModExtensions.HasHandlers(XWModCombatPoints.LifeChanged.Id))
		{
			XWModExtensions.Notify(XWModCombatPoints.LifeChanged, new XWModLifeChanged(XWModCharacters.GetHandle(instance.character), before, instance.hitpoints, cause), context);
		}
	}

	internal static void ObserveLife(TowerDefenseCharacterInstance instance, double before, XWModEventOrigin origin)
	{
		if (XWModExtensions.HasHandlers(XWModCombatPoints.LifeObserved.Id))
		{
			XWModExtensions.Notify(XWModCombatPoints.LifeObserved, new XWModLifeChanged(XWModCharacters.GetHandle(instance.character), before, instance.hitpoints, "Restore"), XWModExtensions.CreateEventContext(origin, XWModAuthority.Local, "Restore"));
		}
	}

	internal static bool ModifyEffect(TowerDefenseCharacter character, TowerDefenseCharacterBuffConfig buff)
	{
		if (!Gameplay(character) || !XWModExtensions.HasHandlers(XWModCombatPoints.EffectParameters.Id))
		{
			return true;
		}
		XWModEffectParameters xWModEffectParameters = Parameters(character, buff);
		if (!xWModEffectParameters.SupportsDuration && !xWModEffectParameters.SupportsStrength)
		{
			return true;
		}
		try
		{
			XWModEffectParameters value = XWModExtensions.Modify(XWModCombatPoints.EffectParameters, xWModEffectParameters);
			ApplyParameters(buff, value);
			return true;
		}
		catch (XWModHookCallbackException)
		{
			return false;
		}
	}

	internal static bool AdmitEffect(TowerDefenseCharacter character, TowerDefenseCharacterBuffConfig buff)
	{
		if (!Gameplay(character) || !XWModExtensions.HasHandlers(XWModCombatPoints.EffectAdmission.Id))
		{
			return true;
		}
		try
		{
			return !XWModExtensions.Modify(XWModCombatPoints.EffectAdmission, new XWModEffectAdmission(XWModCharacters.GetHandle(character), buff.key)).Blocked;
		}
		catch (XWModHookCallbackException)
		{
			return false;
		}
	}

	internal static void Effect(TowerDefenseCharacter character, TowerDefenseCharacterBuffConfig buff, XWModEffectChange change, XWModEventOrigin origin)
	{
		XWModEventPoint<XWModEffectChanged> xWModEventPoint = ((origin == XWModEventOrigin.Gameplay) ? XWModCombatPoints.EffectChanged : XWModCombatPoints.EffectObserved);
		if ((origin != XWModEventOrigin.Gameplay || Host) && XWModExtensions.HasHandlers(xWModEventPoint.Id))
		{
			XWModEffectParameters xWModEffectParameters = Parameters(character, buff);
			XWModExtensions.Notify(xWModEventPoint, new XWModEffectChanged(xWModEffectParameters.Target, xWModEffectParameters.Key, change, xWModEffectParameters.Duration, xWModEffectParameters.Strength), XWModExtensions.CreateEventContext(origin, (origin != XWModEventOrigin.Gameplay) ? XWModAuthority.Local : XWModAuthority.Host, change.ToString()));
		}
	}

	internal static XWModEffectParameters Parameters(TowerDefenseCharacter character, TowerDefenseCharacterBuffConfig buff)
	{
		double? num = ((buff is TowerDefenseCharacterBuffAttackSpeedDown towerDefenseCharacterBuffAttackSpeedDown) ? new double?(towerDefenseCharacterBuffAttackSpeedDown.time) : ((buff is TowerDefenseCharacterBuffBurn towerDefenseCharacterBuffBurn) ? new double?(towerDefenseCharacterBuffBurn.time) : ((buff is TowerDefenseCharacterBuffButter towerDefenseCharacterBuffButter) ? new double?(towerDefenseCharacterBuffButter.time) : ((buff is TowerDefenseCharacterBuffButterGene towerDefenseCharacterBuffButterGene) ? new double?(towerDefenseCharacterBuffButterGene.time) : ((buff is TowerDefenseCharacterBuffCherry towerDefenseCharacterBuffCherry) ? new double?(towerDefenseCharacterBuffCherry.time) : ((buff is TowerDefenseCharacterBuffCoffee towerDefenseCharacterBuffCoffee) ? new double?(towerDefenseCharacterBuffCoffee.time) : ((buff is TowerDefenseCharacterBuffDizziness towerDefenseCharacterBuffDizziness) ? new double?(towerDefenseCharacterBuffDizziness.time) : ((buff is TowerDefenseCharacterBuffEMP towerDefenseCharacterBuffEMP) ? new double?(towerDefenseCharacterBuffEMP.time) : ((buff is TowerDefenseCharacterBuffEMSpeedDown towerDefenseCharacterBuffEMSpeedDown) ? new double?(towerDefenseCharacterBuffEMSpeedDown.time) : ((buff is TowerDefenseCharacterBuffFluorescence towerDefenseCharacterBuffFluorescence) ? new double?(towerDefenseCharacterBuffFluorescence.time) : ((buff is TowerDefenseCharacterBuffFrozen towerDefenseCharacterBuffFrozen) ? new double?(towerDefenseCharacterBuffFrozen.time) : ((buff is TowerDefenseCharacterBuffHypnoses towerDefenseCharacterBuffHypnoses) ? new double?(towerDefenseCharacterBuffHypnoses.time) : ((buff is TowerDefenseCharacterBuffIceSpeedDown towerDefenseCharacterBuffIceSpeedDown) ? new double?(towerDefenseCharacterBuffIceSpeedDown.time) : ((buff is TowerDefenseCharacterBuffMagicImmobilize towerDefenseCharacterBuffMagicImmobilize) ? new double?(towerDefenseCharacterBuffMagicImmobilize.time) : ((buff is TowerDefenseCharacterBuffMagicRootHaste towerDefenseCharacterBuffMagicRootHaste) ? new double?(towerDefenseCharacterBuffMagicRootHaste.time) : ((buff is TowerDefenseCharacterBuffPogo towerDefenseCharacterBuffPogo) ? new double?(towerDefenseCharacterBuffPogo.time) : ((buff is TowerDefenseCharacterBuffPoisoning towerDefenseCharacterBuffPoisoning) ? new double?(towerDefenseCharacterBuffPoisoning.time) : ((buff is TowerDefenseCharacterBuffRadiance towerDefenseCharacterBuffRadiance) ? new double?(towerDefenseCharacterBuffRadiance.time) : ((buff is TowerDefenseCharacterBuffRedHeat towerDefenseCharacterBuffRedHeat) ? new double?(towerDefenseCharacterBuffRedHeat.time) : ((buff is TowerDefenseCharacterBuffRuneMagic towerDefenseCharacterBuffRuneMagic) ? new double?(towerDefenseCharacterBuffRuneMagic.time) : ((buff is TowerDefenseCharacterBuffSealMagic towerDefenseCharacterBuffSealMagic) ? new double?(towerDefenseCharacterBuffSealMagic.time) : ((buff is TowerDefenseCharacterBuffSleep towerDefenseCharacterBuffSleep) ? new double?(towerDefenseCharacterBuffSleep.time) : ((buff is TowerDefenseCharacterBuffSquid towerDefenseCharacterBuffSquid) ? new double?(towerDefenseCharacterBuffSquid.time) : ((buff is TowerDefenseCharacterBuffTabooBean towerDefenseCharacterBuffTabooBean) ? new double?(towerDefenseCharacterBuffTabooBean.time) : ((!(buff is TowerDefenseCharacterBuffTimeMagic towerDefenseCharacterBuffTimeMagic)) ? ((double?)null) : new double?(towerDefenseCharacterBuffTimeMagic.time))))))))))))))))))))))))));
		double? duration = num;
		num = ((buff is TowerDefenseCharacterBuffAttackSpeedDown towerDefenseCharacterBuffAttackSpeedDown2) ? new double?(towerDefenseCharacterBuffAttackSpeedDown2.timeScaleValue) : ((buff is TowerDefenseCharacterBuffBurn towerDefenseCharacterBuffBurn2) ? new double?(towerDefenseCharacterBuffBurn2.dpsAttack) : ((buff is TowerDefenseCharacterBuffCoffee towerDefenseCharacterBuffCoffee2) ? new double?(towerDefenseCharacterBuffCoffee2.timeScaleValue) : ((buff is TowerDefenseCharacterBuffMagicRootHaste towerDefenseCharacterBuffMagicRootHaste2) ? new double?(towerDefenseCharacterBuffMagicRootHaste2.timeScaleValue) : ((buff is TowerDefenseCharacterBuffRuneMagic towerDefenseCharacterBuffRuneMagic2) ? new double?(towerDefenseCharacterBuffRuneMagic2.timeScaleValue) : ((!(buff is TowerDefenseCharacterBuffTimeMagic towerDefenseCharacterBuffTimeMagic2)) ? ((double?)null) : new double?(towerDefenseCharacterBuffTimeMagic2.timeScaleValue)))))));
		double? strength = num;
		return new XWModEffectParameters(XWModCharacters.GetHandle(character), buff.key, duration, strength, buff is TowerDefenseCharacterBuffHypnoses);
	}

	internal static double? Elapsed(TowerDefenseCharacterBuffConfig buff)
	{
		if (!(buff is TowerDefenseCharacterBuffAttackSpeedDown towerDefenseCharacterBuffAttackSpeedDown))
		{
			if (!(buff is TowerDefenseCharacterBuffBurn towerDefenseCharacterBuffBurn))
			{
				if (!(buff is TowerDefenseCharacterBuffButter towerDefenseCharacterBuffButter))
				{
					if (!(buff is TowerDefenseCharacterBuffButterGene towerDefenseCharacterBuffButterGene))
					{
						if (!(buff is TowerDefenseCharacterBuffCherry towerDefenseCharacterBuffCherry))
						{
							if (!(buff is TowerDefenseCharacterBuffCoffee towerDefenseCharacterBuffCoffee))
							{
								if (!(buff is TowerDefenseCharacterBuffDizziness towerDefenseCharacterBuffDizziness))
								{
									if (!(buff is TowerDefenseCharacterBuffEMP towerDefenseCharacterBuffEMP))
									{
										if (!(buff is TowerDefenseCharacterBuffEMSpeedDown towerDefenseCharacterBuffEMSpeedDown))
										{
											if (!(buff is TowerDefenseCharacterBuffFluorescence towerDefenseCharacterBuffFluorescence))
											{
												if (!(buff is TowerDefenseCharacterBuffFrozen towerDefenseCharacterBuffFrozen))
												{
													if (!(buff is TowerDefenseCharacterBuffHypnoses towerDefenseCharacterBuffHypnoses))
													{
														if (!(buff is TowerDefenseCharacterBuffIceSpeedDown towerDefenseCharacterBuffIceSpeedDown))
														{
															if (!(buff is TowerDefenseCharacterBuffMagicImmobilize towerDefenseCharacterBuffMagicImmobilize))
															{
																if (!(buff is TowerDefenseCharacterBuffMagicRootHaste towerDefenseCharacterBuffMagicRootHaste))
																{
																	if (!(buff is TowerDefenseCharacterBuffPogo towerDefenseCharacterBuffPogo))
																	{
																		if (!(buff is TowerDefenseCharacterBuffPoisoning towerDefenseCharacterBuffPoisoning))
																		{
																			if (!(buff is TowerDefenseCharacterBuffRadiance towerDefenseCharacterBuffRadiance))
																			{
																				if (!(buff is TowerDefenseCharacterBuffRedHeat towerDefenseCharacterBuffRedHeat))
																				{
																					if (!(buff is TowerDefenseCharacterBuffRuneMagic towerDefenseCharacterBuffRuneMagic))
																					{
																						if (!(buff is TowerDefenseCharacterBuffSealMagic towerDefenseCharacterBuffSealMagic))
																						{
																							if (!(buff is TowerDefenseCharacterBuffSleep towerDefenseCharacterBuffSleep))
																							{
																								if (!(buff is TowerDefenseCharacterBuffSquid towerDefenseCharacterBuffSquid))
																								{
																									if (!(buff is TowerDefenseCharacterBuffTabooBean towerDefenseCharacterBuffTabooBean))
																									{
																										if (buff is TowerDefenseCharacterBuffTimeMagic towerDefenseCharacterBuffTimeMagic)
																										{
																											return towerDefenseCharacterBuffTimeMagic.currentTime;
																										}
																										return null;
																									}
																									return towerDefenseCharacterBuffTabooBean.currentTime;
																								}
																								return towerDefenseCharacterBuffSquid.currentTime;
																							}
																							return towerDefenseCharacterBuffSleep.currentTime;
																						}
																						return towerDefenseCharacterBuffSealMagic.currentTime;
																					}
																					return towerDefenseCharacterBuffRuneMagic.currentTime;
																				}
																				return towerDefenseCharacterBuffRedHeat.currentTime;
																			}
																			return towerDefenseCharacterBuffRadiance.currentTime;
																		}
																		return towerDefenseCharacterBuffPoisoning.currentTime;
																	}
																	return towerDefenseCharacterBuffPogo.currentTime;
																}
																return towerDefenseCharacterBuffMagicRootHaste.currentTime;
															}
															return towerDefenseCharacterBuffMagicImmobilize.currentTime;
														}
														return towerDefenseCharacterBuffIceSpeedDown.currentTime;
													}
													return towerDefenseCharacterBuffHypnoses.currentTime;
												}
												return towerDefenseCharacterBuffFrozen.currentTime;
											}
											return towerDefenseCharacterBuffFluorescence.currentTime;
										}
										return towerDefenseCharacterBuffEMSpeedDown.currentTime;
									}
									return towerDefenseCharacterBuffEMP.currentTime;
								}
								return towerDefenseCharacterBuffDizziness.currentTime;
							}
							return towerDefenseCharacterBuffCoffee.currentTime;
						}
						return towerDefenseCharacterBuffCherry.currentTime;
					}
					return towerDefenseCharacterBuffButterGene.currentTime;
				}
				return towerDefenseCharacterBuffButter.currentTime;
			}
			return towerDefenseCharacterBuffBurn.currentTime;
		}
		return towerDefenseCharacterBuffAttackSpeedDown.currentTime;
	}

	internal static void ApplyParameters(TowerDefenseCharacterBuffConfig buff, XWModEffectParameters value)
	{
		double? duration = value.Duration;
		if (duration.HasValue)
		{
			double valueOrDefault = duration.GetValueOrDefault();
			if (!(buff is TowerDefenseCharacterBuffAttackSpeedDown towerDefenseCharacterBuffAttackSpeedDown))
			{
				if (!(buff is TowerDefenseCharacterBuffBurn towerDefenseCharacterBuffBurn))
				{
					if (!(buff is TowerDefenseCharacterBuffButter towerDefenseCharacterBuffButter))
					{
						if (!(buff is TowerDefenseCharacterBuffButterGene towerDefenseCharacterBuffButterGene))
						{
							if (!(buff is TowerDefenseCharacterBuffCherry towerDefenseCharacterBuffCherry))
							{
								if (!(buff is TowerDefenseCharacterBuffCoffee towerDefenseCharacterBuffCoffee))
								{
									if (!(buff is TowerDefenseCharacterBuffDizziness towerDefenseCharacterBuffDizziness))
									{
										if (!(buff is TowerDefenseCharacterBuffEMP towerDefenseCharacterBuffEMP))
										{
											if (!(buff is TowerDefenseCharacterBuffEMSpeedDown towerDefenseCharacterBuffEMSpeedDown))
											{
												if (!(buff is TowerDefenseCharacterBuffFluorescence towerDefenseCharacterBuffFluorescence))
												{
													if (!(buff is TowerDefenseCharacterBuffFrozen towerDefenseCharacterBuffFrozen))
													{
														if (!(buff is TowerDefenseCharacterBuffHypnoses towerDefenseCharacterBuffHypnoses))
														{
															if (!(buff is TowerDefenseCharacterBuffIceSpeedDown towerDefenseCharacterBuffIceSpeedDown))
															{
																if (!(buff is TowerDefenseCharacterBuffMagicImmobilize towerDefenseCharacterBuffMagicImmobilize))
																{
																	if (!(buff is TowerDefenseCharacterBuffMagicRootHaste towerDefenseCharacterBuffMagicRootHaste))
																	{
																		if (!(buff is TowerDefenseCharacterBuffPogo towerDefenseCharacterBuffPogo))
																		{
																			if (!(buff is TowerDefenseCharacterBuffPoisoning towerDefenseCharacterBuffPoisoning))
																			{
																				if (!(buff is TowerDefenseCharacterBuffRadiance towerDefenseCharacterBuffRadiance))
																				{
																					if (!(buff is TowerDefenseCharacterBuffRedHeat towerDefenseCharacterBuffRedHeat))
																					{
																						if (!(buff is TowerDefenseCharacterBuffRuneMagic towerDefenseCharacterBuffRuneMagic))
																						{
																							if (!(buff is TowerDefenseCharacterBuffSealMagic towerDefenseCharacterBuffSealMagic))
																							{
																								if (!(buff is TowerDefenseCharacterBuffSleep towerDefenseCharacterBuffSleep))
																								{
																									if (!(buff is TowerDefenseCharacterBuffSquid towerDefenseCharacterBuffSquid))
																									{
																										if (!(buff is TowerDefenseCharacterBuffTabooBean towerDefenseCharacterBuffTabooBean))
																										{
																											if (buff is TowerDefenseCharacterBuffTimeMagic towerDefenseCharacterBuffTimeMagic)
																											{
																												towerDefenseCharacterBuffTimeMagic.time = valueOrDefault;
																											}
																										}
																										else
																										{
																											towerDefenseCharacterBuffTabooBean.time = valueOrDefault;
																										}
																									}
																									else
																									{
																										towerDefenseCharacterBuffSquid.time = valueOrDefault;
																									}
																								}
																								else
																								{
																									towerDefenseCharacterBuffSleep.time = valueOrDefault;
																								}
																							}
																							else
																							{
																								towerDefenseCharacterBuffSealMagic.time = valueOrDefault;
																							}
																						}
																						else
																						{
																							towerDefenseCharacterBuffRuneMagic.time = valueOrDefault;
																						}
																					}
																					else
																					{
																						towerDefenseCharacterBuffRedHeat.time = valueOrDefault;
																					}
																				}
																				else
																				{
																					towerDefenseCharacterBuffRadiance.time = valueOrDefault;
																				}
																			}
																			else
																			{
																				towerDefenseCharacterBuffPoisoning.time = valueOrDefault;
																			}
																		}
																		else
																		{
																			towerDefenseCharacterBuffPogo.time = valueOrDefault;
																		}
																	}
																	else
																	{
																		towerDefenseCharacterBuffMagicRootHaste.time = valueOrDefault;
																	}
																}
																else
																{
																	towerDefenseCharacterBuffMagicImmobilize.time = valueOrDefault;
																}
															}
															else
															{
																towerDefenseCharacterBuffIceSpeedDown.time = valueOrDefault;
															}
														}
														else
														{
															towerDefenseCharacterBuffHypnoses.time = valueOrDefault;
														}
													}
													else
													{
														towerDefenseCharacterBuffFrozen.time = valueOrDefault;
													}
												}
												else
												{
													towerDefenseCharacterBuffFluorescence.time = valueOrDefault;
												}
											}
											else
											{
												towerDefenseCharacterBuffEMSpeedDown.time = valueOrDefault;
											}
										}
										else
										{
											towerDefenseCharacterBuffEMP.time = valueOrDefault;
										}
									}
									else
									{
										towerDefenseCharacterBuffDizziness.time = valueOrDefault;
									}
								}
								else
								{
									towerDefenseCharacterBuffCoffee.time = valueOrDefault;
								}
							}
							else
							{
								towerDefenseCharacterBuffCherry.time = valueOrDefault;
							}
						}
						else
						{
							towerDefenseCharacterBuffButterGene.time = valueOrDefault;
						}
					}
					else
					{
						towerDefenseCharacterBuffButter.time = valueOrDefault;
					}
				}
				else
				{
					towerDefenseCharacterBuffBurn.time = valueOrDefault;
				}
			}
			else
			{
				towerDefenseCharacterBuffAttackSpeedDown.time = valueOrDefault;
			}
		}
		duration = value.Strength;
		if (!duration.HasValue)
		{
			return;
		}
		double valueOrDefault2 = duration.GetValueOrDefault();
		if (!(buff is TowerDefenseCharacterBuffAttackSpeedDown towerDefenseCharacterBuffAttackSpeedDown2))
		{
			if (!(buff is TowerDefenseCharacterBuffBurn towerDefenseCharacterBuffBurn2))
			{
				if (!(buff is TowerDefenseCharacterBuffCoffee towerDefenseCharacterBuffCoffee2))
				{
					if (!(buff is TowerDefenseCharacterBuffMagicRootHaste towerDefenseCharacterBuffMagicRootHaste2))
					{
						if (!(buff is TowerDefenseCharacterBuffRuneMagic towerDefenseCharacterBuffRuneMagic2))
						{
							if (buff is TowerDefenseCharacterBuffTimeMagic towerDefenseCharacterBuffTimeMagic2)
							{
								towerDefenseCharacterBuffTimeMagic2.timeScaleValue = valueOrDefault2;
							}
						}
						else
						{
							towerDefenseCharacterBuffRuneMagic2.timeScaleValue = valueOrDefault2;
						}
					}
					else
					{
						towerDefenseCharacterBuffMagicRootHaste2.timeScaleValue = valueOrDefault2;
					}
				}
				else
				{
					towerDefenseCharacterBuffCoffee2.timeScaleValue = valueOrDefault2;
				}
			}
			else
			{
				towerDefenseCharacterBuffBurn2.dpsAttack = valueOrDefault2;
			}
		}
		else
		{
			towerDefenseCharacterBuffAttackSpeedDown2.timeScaleValue = valueOrDefault2;
		}
	}
}

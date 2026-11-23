using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Godot;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModPlantPlacementService : IDisposable
{
	private sealed record Registration(string Owner, XWModRuntimeSession Session, XWModPlantFootprintOptions Options, string CharacterKey, TowerDefenseEnum.PLANTGRIDTYPE SlotType)
	{
		internal bool SurfaceFailed { get; set; }

		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			RuntimeHelpers.EnsureSufficientExecutionStack();
			builder.Append("Owner = ");
			builder.Append((object?)Owner);
			builder.Append(", Session = ");
			builder.Append(Session);
			builder.Append(", Options = ");
			builder.Append(Options);
			builder.Append(", CharacterKey = ");
			builder.Append((object?)CharacterKey);
			builder.Append(", SlotType = ");
			builder.Append(SlotType.ToString());
			return true;
		}
	}

	private static readonly Dictionary<string, Registration> Registrations = new Dictionary<string, Registration>(StringComparer.Ordinal);

	private static readonly HashSet<TowerDefenseCellInstance> ReservedCells = new HashSet<TowerDefenseCellInstance>();

	private readonly XWModRuntimeContext _context;

	private readonly List<string> _keys = new List<string>();

	private bool _disposed;

	internal static bool IsReserved(TowerDefenseCellInstance cell)
	{
		if (GodotObject.IsInstanceValid(cell))
		{
			return ReservedCells.Contains(cell);
		}
		return false;
	}

	internal static bool Reserve(IEnumerable<TowerDefenseCellInstance> cells)
	{
		if (cells.Any(IsReserved))
		{
			return false;
		}
		foreach (TowerDefenseCellInstance cell in cells)
		{
			ReservedCells.Add(cell);
		}
		return true;
	}

	internal static void Release(IEnumerable<TowerDefenseCellInstance> cells)
	{
		foreach (TowerDefenseCellInstance cell in cells)
		{
			ReservedCells.Remove(cell);
		}
	}

	internal XWModPlantPlacementService(XWModRuntimeContext context)
	{
		_context = context;
	}

	public void Register(string packetKey, XWModPlantFootprintOptions options)
	{
		XWModExtensions.EnsureHostThread();
		ObjectDisposedException.ThrowIf(_disposed, this);
		XWModRuntimeSession current = XWModRuntimeSession.Current;
		if (current != null && current.Frozen)
		{
			throw new InvalidOperationException("Plant footprints must be registered during Initialize.");
		}
		if (options == null || options.Width < 2 || options.Width > 64 || !float.IsFinite(options.PreviewOffsetY))
		{
			throw new ArgumentOutOfRangeException("options");
		}
		if (!XWModRuntimeRegistry.TryGetEffectiveRegistration("Packet", packetKey, out var registration) || registration.OverridesBuiltIn || registration.IsOverride || !string.Equals(registration.OwnerMod, _context.ModId, StringComparison.OrdinalIgnoreCase) || !string.Equals(registration.ContentId.ProviderModId, _context.ModId, StringComparison.OrdinalIgnoreCase))
		{
			throw new InvalidOperationException("A footprint can only belong to the registering Mod's new packet.");
		}
		TowerDefensePacketConfig packetConfigReadOnly = TowerDefenseManager.GetPacketConfigReadOnly(packetKey);
		TowerDefenseCharacterConfig towerDefenseCharacterConfig = packetConfigReadOnly?.characterConfig;
		TowerDefensePlantConfig plant = towerDefenseCharacterConfig as TowerDefensePlantConfig;
		if (plant == null || !packetConfigReadOnly.plantUseCell || plant.extendGrid.Count != options.Width - 1 || Enumerable.Range(1, options.Width - 1).Any((int x) => !plant.extendGrid.Contains(new Vector2I(x, 0))) || !MatchesKind(plant, options) || plant.plantGridOverrideType == TowerDefenseEnum.PLANTGRIDTYPE.NOONE || plant.extendCoverDictionary.Count != 0 || packetConfigReadOnly.GetPlantCover().Count != 0)
		{
			throw new InvalidOperationException("Footprint configuration must describe a horizontal support or surround without cover recipes.");
		}
		if (!Registrations.TryAdd(packetKey, new Registration(_context.ModId, XWModRuntimeSession.Current, options, plant.name, plant.plantGridOverrideType)))
		{
			throw new InvalidOperationException("The packet already has a footprint.");
		}
		_keys.Add(packetKey);
	}

	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}
		_disposed = true;
		foreach (string key in _keys)
		{
			Registrations.Remove(key);
		}
		_keys.Clear();
	}

	private static bool TryGet(TowerDefensePacketConfig packet, out XWModPlantFootprintOptions options)
	{
		options = null;
		if (!TryGetRegistration(packet, out var registration))
		{
			return false;
		}
		options = registration.Options;
		return true;
	}

	private static bool TryGetRegistration(TowerDefensePacketConfig packet, out Registration registration)
	{
		registration = null;
		if (GodotObject.IsInstanceValid(packet) && Registrations.TryGetValue(packet.saveKey, out registration) && registration.Session == XWModRuntimeSession.Current)
		{
			if (registration.Session != null)
			{
				return registration.Session.IsOwnerActive(registration.Owner);
			}
			return true;
		}
		return false;
	}

	internal static bool TryGetFootprint(TowerDefenseCharacter character, out XWModPlantFootprintOptions options)
	{
		options = null;
		if (!GodotObject.IsInstanceValid(character) || character.isDestroy || character.die || !TryGetRegistration(character.packet, out var registration) || character.config?.name != registration.CharacterKey)
		{
			return false;
		}
		options = registration.Options;
		return true;
	}

	internal static void PositionPreviewSprite(AdobeAnimateSprite sprite, TowerDefensePacketConfig packet, Vector2I anchor)
	{
		if (TryGet(packet, out var options) && GodotObject.IsInstanceValid(sprite))
		{
			TowerDefenseManager instance = TowerDefenseManager.Instance;
			TowerDefenseMapControl mapControl = TowerDefenseManager.GetMapFeature().mapControl;
			TowerDefenseCellInstance mapCell = TowerDefenseManager.GetMapCell(anchor);
			Rect2 localBounds = AdobeAnimateDefinitionCache.GetOrBuild(sprite.flashAnimeData, sprite.atlasProfileOverride).LocalBounds;
			if (GodotObject.IsInstanceValid(mapCell) && !(localBounds.Size.X <= 0f))
			{
				Vector2 vector = new Vector2(instance.gridSize.X * (float)options.Width / localBounds.Size.X, mapControl.GlobalScale.Y);
				Vector2 mapCellPlantPos = TowerDefenseManager.GetMapCellPlantPos(anchor);
				mapCellPlantPos.X = instance.GetMapCellPos(anchor).X - (localBounds.Position.X + sprite.offset.X) * vector.X;
				mapCellPlantPos.Y += (26f + options.PreviewOffsetY - localBounds.End.Y - sprite.offset.Y - (float)mapCell.GetGroundHeight()) * vector.Y;
				Transform2D transform2D = new Transform2D(new Vector2(vector.X, 0f), new Vector2(0f, vector.Y), mapCellPlantPos);
				sprite.GlobalTransform = sprite.GetCanvasTransform().AffineInverse() * mapControl.GetCanvasTransform() * transform2D;
			}
		}
	}

	internal static bool TryGetSupportHeight(TowerDefenseCellInstance cell, TowerDefenseCharacter occupant, out double height)
	{
		return TryGetSupportHeight(cell, occupant, null, out height);
	}

	internal static double GetPlacementHeight(TowerDefenseCellInstance cell, TowerDefensePacketConfig packet, double terrainHeight)
	{
		if (!TryGetSupportHeight(cell, null, packet, out var height))
		{
			return terrainHeight;
		}
		return height;
	}

	private static bool TryGetSupportHeight(TowerDefenseCellInstance cell, TowerDefenseCharacter occupant, TowerDefensePacketConfig incoming, out double height)
	{
		height = 0.0;
		if (!GodotObject.IsInstanceValid(cell))
		{
			return false;
		}
		if (incoming != null && (!(incoming.characterConfig is TowerDefensePlantConfig) || IsRegistered(incoming)))
		{
			return false;
		}
		foreach (TowerDefenseCharacter character in cell.characterList)
		{
			if (!GodotObject.IsInstanceValid(character) || character == occupant || character.isDestroy || character.die || !TryGetRegistration(character.packet, out var registration) || registration.SurfaceFailed || registration.Options.SupportHeightProvider == null || character.config?.name != registration.CharacterKey || cell.gridPos.Y != character.gridPos.Y || cell.gridPos.X < character.gridPos.X || cell.gridPos.X >= character.gridPos.X + registration.Options.Width || (occupant != null && cell.characterSlotDictionary.GetValueOrDefault(character) != occupant && cell.characterSurround != occupant) || (incoming != null && !incoming.characterConfig.plantGridType.Contains(registration.SlotType) && !incoming.characterConfig.plantGridType.Contains(TowerDefenseEnum.PLANTGRIDTYPE.SURROUND)))
			{
				continue;
			}
			try
			{
				height = registration.Options.SupportHeightProvider(character, cell.gridPos, cell.GetGroundHeight());
				if (double.IsFinite(height))
				{
					return true;
				}
				throw new InvalidOperationException("Support height must be finite.");
			}
			catch (Exception ex)
			{
				registration.SurfaceFailed = true;
				GD.PushWarning("Mod " + registration.Owner + " support height provider failed: " + ex.Message);
			}
		}
		return false;
	}

	internal static void NotifySupportOccupants(TowerDefenseCellInstance cell, TowerDefenseCharacter support)
	{
		TowerDefenseCharacter[] array = new TowerDefenseCharacter[2]
		{
			cell.characterSlotDictionary.GetValueOrDefault(support),
			cell.characterSurround
		};
		foreach (TowerDefenseCharacter towerDefenseCharacter in array)
		{
			if (GodotObject.IsInstanceValid(towerDefenseCharacter) && towerDefenseCharacter != support)
			{
				towerDefenseCharacter.groundHeightComponent?.NotifyEnvironmentChanged();
			}
		}
	}

	internal static bool IsRegistered(TowerDefensePacketConfig packet)
	{
		if (GodotObject.IsInstanceValid(packet) && Registrations.TryGetValue(packet.saveKey, out var value))
		{
			return value.Session == XWModRuntimeSession.Current;
		}
		return false;
	}

	public static bool CanRunDeathEffect(TowerDefenseCharacter character)
	{
		if (IsRegistered(character?.packet) && GodotObject.IsInstanceValid(character) && character.inGame && !character.isShovel && !character.suppressDeathrattles && !character.skipDestroySet && GodotObject.IsInstanceValid(character.instance) && !character.instance.hologram)
		{
			if (!character.die && !character.instance.die)
			{
				return character.instance.hitpoints <= 0.0;
			}
			return true;
		}
		return false;
	}

	internal static bool IsProtected(TowerDefenseCharacter character)
	{
		if (GodotObject.IsInstanceValid(character) && GodotObject.IsInstanceValid(character.packet) && Registrations.TryGetValue(character.packet.saveKey, out var value) && value.Session == XWModRuntimeSession.Current)
		{
			return value.Options.ProtectFromReplacement;
		}
		return false;
	}

	internal static bool IsSupport(TowerDefenseCharacterConfig config)
	{
		if (GodotObject.IsInstanceValid(config))
		{
			return (config.physiqueTypeFlags & 6) != 0;
		}
		return false;
	}

	internal static bool IsSurround(TowerDefenseCharacterConfig config)
	{
		if (config is TowerDefensePlantConfig)
		{
			return config.plantGridType.Contains(TowerDefenseEnum.PLANTGRIDTYPE.SURROUND);
		}
		return false;
	}

	private static bool MatchesKind(TowerDefensePlantConfig plant, XWModPlantFootprintOptions options)
	{
		if (!options.Surround)
		{
			if (IsSupport(plant))
			{
				return !options.ReplaceSurrounds;
			}
			return false;
		}
		if (IsSurround(plant) && plant.plantGridOverrideType == TowerDefenseEnum.PLANTGRIDTYPE.SURROUND && !options.ReplaceSupports)
		{
			return options.SupportHeightProvider == null;
		}
		return false;
	}

	internal static bool BlocksSupport(TowerDefenseCellInstance cell, TowerDefensePacketConfig incoming)
	{
		if (IsSupport(incoming?.characterConfig))
		{
			return cell.characterList.Any(IsProtected);
		}
		return false;
	}

	public static bool TryResolveAnchor(Vector2I click, Vector2I mapSize, int width, out Vector2I anchor)
	{
		anchor = default(Vector2I);
		if (width < 2 || mapSize.X < width || click.X < 1 || click.X > mapSize.X || click.Y < 1 || click.Y > mapSize.Y)
		{
			return false;
		}
		anchor = new Vector2I(Math.Clamp(click.X - width / 2, 1, mapSize.X - width + 1), click.Y);
		return true;
	}

	public static bool TryResolveAnchor(Vector2I click, Vector2I mapSize, XWModPlantFootprintOptions options, out Vector2I anchor)
	{
		anchor = default(Vector2I);
		if (options == null || !TryResolveAnchor(click, mapSize, options.Width, out var anchor2))
		{
			return false;
		}
		if (options.StrictCenter && mapSize.X != options.Width && (click.X - options.Width / 2 < 1 || click.X - options.Width / 2 + options.Width - 1 > mapSize.X))
		{
			return false;
		}
		anchor = anchor2;
		return true;
	}

	internal static bool TryPrepare(TowerDefensePacketConfig packet, Vector2I click, out XWModPlantFootprintPlan plan)
	{
		return TryPrepareRow(packet, click, allowColumn: false, out plan);
	}

	internal static bool TryPrepareColumn(TowerDefensePacketConfig packet, int column, out XWModPlantFootprintBatch batch, int? limitPlantGridNum = null, ISet<int> allowedRows = null)
	{
		batch = null;
		TowerDefenseBattleFeatureMap mapFeature = TowerDefenseManager.GetMapFeature();
		if (!GodotObject.IsInstanceValid(mapFeature) || !mapFeature.isPlantColumn || Global.IsMultiplayerMode || !TryGet(packet, out var _) || column < 1 || column > mapFeature.config.gridNum.X)
		{
			return false;
		}
		List<XWModPlantFootprintPlan> list = new List<XWModPlantFootprintPlan>();
		try
		{
			ColumnPlantGridBudget columnPlantGridBudget = new ColumnPlantGridBudget(packet, limitPlantGridNum);
			for (int i = 1; i <= mapFeature.config.gridNum.Y; i++)
			{
				if ((allowedRows == null || allowedRows.Contains(i)) && TryPrepareRow(packet, new Vector2I(column, i), allowColumn: true, out var plan))
				{
					if (!columnPlantGridBudget.CanPlace(plan.Anchor))
					{
						plan.Dispose();
						continue;
					}
					list.Add(plan);
					columnPlantGridBudget.Reserve(plan.Anchor);
				}
			}
			if (list.Count == 0)
			{
				return false;
			}
			batch = new XWModPlantFootprintBatch(list);
			return true;
		}
		catch
		{
			foreach (XWModPlantFootprintPlan item in list)
			{
				item.Dispose();
			}
			throw;
		}
	}

	private static bool TryPrepareRow(TowerDefensePacketConfig packet, Vector2I click, bool allowColumn, out XWModPlantFootprintPlan plan)
	{
		plan = null;
		TowerDefenseManager instance = TowerDefenseManager.Instance;
		TowerDefenseBattleFeatureMap mapFeature = TowerDefenseManager.GetMapFeature();
		if (!TryGet(packet, out var options) || !GodotObject.IsInstanceValid(instance) || !GodotObject.IsInstanceValid(mapFeature) || Global.IsMultiplayerMode || (mapFeature.isPlantColumn && !allowColumn) || !TryResolveAnchor(click, instance.gridNum, options, out var anchor))
		{
			return false;
		}
		Registration registration = Registrations[packet.saveKey];
		TowerDefenseCharacterConfig characterConfig = packet.characterConfig;
		TowerDefensePlantConfig shape = characterConfig as TowerDefensePlantConfig;
		if (shape == null || shape.name != registration.CharacterKey || shape.plantGridOverrideType != registration.SlotType || !packet.plantUseCell || !MatchesKind(shape, options) || shape.extendGrid.Count != options.Width - 1 || shape.extendCoverDictionary.Count != 0 || packet.GetPlantCover().Count != 0 || Enumerable.Range(1, options.Width - 1).Any((int x) => !shape.extendGrid.Contains(new Vector2I(x, 0))))
		{
			return false;
		}
		TowerDefenseCellInstance[] cells = (from x in Enumerable.Range(0, options.Width)
			select TowerDefenseManager.GetMapCell(anchor + new Vector2I(x, 0))).ToArray();
		bool flag = allowColumn && instance.IsIZMMode() && packet.izmPlantAllCell;
		if (cells.Any((TowerDefenseCellInstance c) => !GodotObject.IsInstanceValid(c) || IsReserved(c)) || (!flag && mapFeature.stripeRow != -1 && anchor.X + options.Width - 1 > mapFeature.stripeRow))
		{
			return false;
		}
		if (!allowColumn && packet.IsLimitGridNum() && instance.currentControl?.levelConfig is TowerDefenseLevelConfig { limitGridPlantNum: >=0 } towerDefenseLevelConfig && mapFeature.GetCellPlantNum() + cells.Count((TowerDefenseCellInstance c) => !c.HasPlant()) > towerDefenseLevelConfig.limitGridPlantNum)
		{
			return false;
		}
		HashSet<TowerDefenseCharacter> hashSet = new HashSet<TowerDefenseCharacter>();
		TowerDefenseCellInstance[] array = cells;
		for (int num = 0; num < array.Length; num++)
		{
			foreach (TowerDefenseCharacter occupant in array[num].characterList)
			{
				if (!GodotObject.IsInstanceValid(occupant))
				{
					continue;
				}
				if (!options.Surround && IsProtected(occupant))
				{
					return false;
				}
				if (options.Surround ? IsSurround(occupant.config) : IsSupport(occupant.config))
				{
					if (IsProtected(occupant) || !(options.Surround ? options.ReplaceSurrounds : options.ReplaceSupports) || occupant.instance.hologram || occupant.instance.hypnoses != packet.GetHypnoses())
					{
						return false;
					}
					if (occupant.config is TowerDefensePlantConfig towerDefensePlantConfig && towerDefensePlantConfig.extendGrid.Prepend(Vector2I.Zero).Any((Vector2I offset) => !cells.Any((TowerDefenseCellInstance c) => c.gridPos == occupant.gridPos + offset)))
					{
						return false;
					}
					hashSet.Add(occupant);
				}
			}
		}
		XWModPlantFootprintPlan xWModPlantFootprintPlan = new XWModPlantFootprintPlan(packet, anchor, cells, hashSet, options);
		if (!xWModPlantFootprintPlan.Validate())
		{
			xWModPlantFootprintPlan.Dispose();
			return false;
		}
		plan = xWModPlantFootprintPlan;
		return true;
	}
}

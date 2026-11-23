using System;
using Godot;
using Godot.Collections;
using PVZHE.ModEditor.ModSystem;

public sealed class GameStateReplicator : NetworkReplicatorBase
{
	private const string ModStateKey = "__mod_state_v3";

	private const double SyncInterval = 0.5;

	private readonly IGameStateNetworkContext _gameContext;

	private double _syncTimer;

	private (Guid BattleGeneration, ulong Revision) _lastModStamp;

	public bool LastModRestoreSucceeded { get; private set; }

	public GameStateReplicator()
	{
	}

	public GameStateReplicator(IGameStateNetworkContext gameContext)
	{
		_gameContext = gameContext;
	}

	public override void Process(double delta)
	{
		if (base.Context != null && base.Context.IsMultiplayerActive && _gameContext != null && base.Context.IsHost)
		{
			_syncTimer += delta;
			if (_syncTimer >= 0.5)
			{
				_syncTimer = 0.0;
				BroadcastState();
			}
		}
	}

	public void BroadcastState()
	{
		if (base.Context == null || !base.Context.IsMultiplayerActive || _gameContext == null)
		{
			return;
		}
		Dictionary dictionary = new Dictionary();
		foreach (StringName key in _gameContext.Features.Keys)
		{
			Dictionary dictionary2 = _gameContext.Features[key].SyncSerialize();
			if (dictionary2.Count > 0)
			{
				Dictionary b = _gameContext.GameStateLastSync.GetValueOrDefault(key, new Dictionary()).AsGodotDictionary();
				if (!NetworkVariantComparer.DictionaryApproxEquals(dictionary2, b))
				{
					dictionary[key] = dictionary2;
					_gameContext.GameStateLastSync[key] = dictionary2.Duplicate(deep: true);
				}
			}
		}
		if (_gameContext.Process != null)
		{
			Dictionary dictionary3 = _gameContext.Process.SyncSerialize();
			if (dictionary3.Count > 0)
			{
				Dictionary b2 = _gameContext.GameStateLastSync.GetValueOrDefault("process", new Dictionary()).AsGodotDictionary();
				if (!NetworkVariantComparer.DictionaryApproxEquals(dictionary3, b2))
				{
					dictionary["process"] = dictionary3;
					_gameContext.GameStateLastSync["process"] = dictionary3.Duplicate(deep: true);
				}
			}
		}
		(Guid, ulong) battleStamp = XWModDataService.BattleStamp;
		(Guid, ulong) tuple = battleStamp;
		(Guid, ulong) lastModStamp = _lastModStamp;
		if (tuple.Item1 != lastModStamp.Item1 || tuple.Item2 != lastModStamp.Item2 || !_gameContext.GameStateLastSync.ContainsKey("__mod_state_v3"))
		{
			Dictionary dictionary4 = XWModDataService.CaptureBattle();
			Dictionary b3 = _gameContext.GameStateLastSync.GetValueOrDefault("__mod_state_v3", new Dictionary()).AsGodotDictionary();
			if (!NetworkVariantComparer.DictionaryApproxEquals(dictionary4, b3))
			{
				dictionary["__mod_state_v3"] = dictionary4;
			}
			_gameContext.GameStateLastSync["__mod_state_v3"] = dictionary4.Duplicate(deep: true);
			_lastModStamp = battleStamp;
		}
		if (dictionary.Count > 0)
		{
			_gameContext.SendGameState(Json.Stringify(dictionary));
		}
	}

	public void ApplyState(Dictionary state)
	{
		LastModRestoreSucceeded = false;
		if (_gameContext == null || state == null)
		{
			return;
		}
		foreach (Variant key in state.Keys)
		{
			StringName stringName = key.AsStringName();
			if (key.AsString() == "__mod_state_v3")
			{
				LastModRestoreSucceeded = state[key].VariantType == Variant.Type.Dictionary && XWModDataService.RestoreBattle(state[key].AsGodotDictionary(), out var _);
				continue;
			}
			if (key.AsString() == "process")
			{
				_gameContext.Process?.SyncDeserialize(state[key].AsGodotDictionary());
				continue;
			}
			if (!_gameContext.Features.ContainsKey(stringName))
			{
				_gameContext.EnsureFeature(stringName);
			}
			if (_gameContext.Features.ContainsKey(stringName))
			{
				_gameContext.Features[stringName].SyncDeserialize(state[key].AsGodotDictionary());
			}
		}
	}

	public Dictionary BuildFullState()
	{
		Dictionary dictionary = new Dictionary();
		dictionary["__mod_state_v3"] = XWModDataService.CaptureBattle();
		if (_gameContext == null)
		{
			return dictionary;
		}
		foreach (StringName key in _gameContext.Features.Keys)
		{
			TowerDefenseBattleFeature towerDefenseBattleFeature = _gameContext.Features[key];
			if (GodotObject.IsInstanceValid(towerDefenseBattleFeature))
			{
				Dictionary dictionary2 = towerDefenseBattleFeature.SyncSerialize();
				if (dictionary2.Count > 0)
				{
					dictionary[key] = dictionary2;
				}
			}
		}
		if (_gameContext.Process != null)
		{
			Dictionary dictionary3 = _gameContext.Process.SyncSerialize();
			if (dictionary3.Count > 0)
			{
				dictionary["process"] = dictionary3;
			}
		}
		return dictionary;
	}
}

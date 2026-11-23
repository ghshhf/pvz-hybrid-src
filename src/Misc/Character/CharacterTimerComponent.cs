using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

public sealed class CharacterTimerComponent : CharacterComponentRuntime
{
	public delegate void TimeoutEventHandler(string timerName);

	private static readonly StringName SyncRunningKey = new StringName("timerRunning");

	private static readonly StringName SyncWaitTimeKey = new StringName("timerWaitTime");

	private static readonly StringName SyncCurrentKey = new StringName("timerCurrent");

	private static readonly StringName SyncTimeScaleKey = new StringName("timeScale");

	public Dictionary timerDictionary = new Dictionary();

	private double _timeScale = 1.0;

	public readonly System.Collections.Generic.Dictionary<string, bool> timerRunning = new System.Collections.Generic.Dictionary<string, bool>();

	public readonly System.Collections.Generic.Dictionary<string, double> timerWaitTime = new System.Collections.Generic.Dictionary<string, double>();

	public readonly System.Collections.Generic.Dictionary<string, double> timerCurrent = new System.Collections.Generic.Dictionary<string, double>();

	private readonly List<string> _activeTimers = new List<string>();

	private readonly System.Collections.Generic.Dictionary<string, ulong> _pendingTimeouts = new System.Collections.Generic.Dictionary<string, ulong>();

	private ulong _timeoutSequence;

	private bool UsesOwnerTimeScale;

	private readonly Dictionary _syncPayload = new Dictionary();

	private readonly Dictionary _syncRunningData = new Dictionary();

	private readonly Dictionary _syncWaitTimeData = new Dictionary();

	private readonly Dictionary _syncCurrentData = new Dictionary();

	private bool _configured;

	private bool _syncPayloadInitialized;

	private ulong _syncStateRevision;

	private ulong _syncPayloadRevision;

	public double timeScale
	{
		get
		{
			return _timeScale;
		}
		set
		{
			if (_timeScale != value)
			{
				_timeScale = value;
				MarkSyncStateChanged();
			}
		}
	}

	private bool CanAdvance
	{
		get
		{
			if (timeScale > 0.0)
			{
				if (UsesOwnerTimeScale)
				{
					return base.Owner?.CanRunAbility() ?? false;
				}
				return true;
			}
			return false;
		}
	}

	private CharacterTimerComponentDefinition Definition => base.ComponentDefinition as CharacterTimerComponentDefinition;

	internal override bool WantsPhysicsProcess => _activeTimers.Count > _pendingTimeouts.Count;

	public event TimeoutEventHandler OnTimeout;

	protected override void OnBound()
	{
		TowerDefenseCharacter owner = base.Owner;
		bool usesOwnerTimeScale = ((owner is TowerDefensePlant || owner is TowerDefenseZombie) ? true : false);
		UsesOwnerTimeScale = usesOwnerTimeScale;
		ConfigureOnce();
	}

	protected override void OnDetaching(ComponentDetachReason reason)
	{
		_pendingTimeouts.Clear();
		ClearSyncPayload();
	}

	protected override void OnReleased()
	{
		_pendingTimeouts.Clear();
		_activeTimers.Clear();
		timerRunning.Clear();
		timerWaitTime.Clear();
		timerCurrent.Clear();
		timerDictionary.Clear();
		this.OnTimeout = null;
		ClearSyncPayload();
	}

	private void ConfigureOnce()
	{
		if (_configured || Definition == null)
		{
			return;
		}
		timerDictionary.Clear();
		if (Definition.timerDictionary != null)
		{
			foreach (Variant key in Definition.timerDictionary.Keys)
			{
				timerDictionary[key] = Definition.timerDictionary[key];
			}
		}
		timeScale = Definition.timeScale;
		ResetRuntimeTimersFromConfig();
		_configured = true;
	}

	private void ResetRuntimeTimersFromConfig()
	{
		_pendingTimeouts.Clear();
		timerRunning.Clear();
		timerWaitTime.Clear();
		timerCurrent.Clear();
		_activeTimers.Clear();
		foreach (Variant key2 in timerDictionary.Keys)
		{
			string key = key2.AsString();
			timerRunning[key] = false;
			timerWaitTime[key] = timerDictionary[key2].AsDouble();
			timerCurrent[key] = 0.0;
		}
		MarkSyncStateChanged();
	}

	internal override void PhysicsProcess(double delta, ulong physicsFrame)
	{
		if (!TowerDefenseManager._IsGameRunning() || _activeTimers.Count == 0 || !CanAdvance)
		{
			return;
		}
		double num = (UsesOwnerTimeScale ? (Math.Max(0.0, delta) * base.Owner.GetTimerTimeScale(base.Owner is TowerDefensePlant)) : delta) * timeScale;
		bool flag = _pendingTimeouts.Count > 0;
		int num2 = 0;
		while (num2 < _activeTimers.Count)
		{
			string key = _activeTimers[num2];
			bool value;
			double value2;
			double value3;
			if (flag && _pendingTimeouts.ContainsKey(key))
			{
				num2++;
			}
			else if (!timerRunning.TryGetValue(key, out value) || !value || !timerCurrent.TryGetValue(key, out value2) || !timerWaitTime.TryGetValue(key, out value3))
			{
				RemoveActiveTimerAt(num2);
				if (_activeTimers.Count == 0)
				{
					RefreshPhysicsProcessEligibility();
				}
			}
			else if (value2 < value3)
			{
				timerCurrent[key] = value2 + num;
				MarkSyncStateChanged();
				num2++;
			}
			else
			{
				QueueTimeout(key);
				num2++;
			}
		}
	}

	private void QueueTimeout(string key)
	{
		ulong sequence = ++_timeoutSequence;
		_pendingTimeouts[key] = sequence;
		RefreshPhysicsProcessEligibility();
		Callable.From(delegate
		{
			EmitTimeout(key, sequence);
		}).CallDeferred();
	}

	private void EmitTimeout(string key, ulong sequence)
	{
		if (!base.IsReleased && _pendingTimeouts.TryGetValue(key, out var value) && value == sequence)
		{
			_pendingTimeouts.Remove(key);
			if (!TowerDefenseManager._IsGameRunning() || !CanAdvance || !IsTimerRunning(key))
			{
				RefreshPhysicsProcessEligibility();
				return;
			}
			timerCurrent[key] = 0.0;
			timerRunning[key] = false;
			RemoveActiveTimer(key);
			MarkSyncStateChanged();
			RefreshPhysicsProcessEligibility();
			this.OnTimeout?.Invoke(key);
		}
	}

	private void RemoveActiveTimerAt(int index)
	{
		int index2 = _activeTimers.Count - 1;
		_activeTimers[index] = _activeTimers[index2];
		_activeTimers.RemoveAt(index2);
	}

	private bool RemoveActiveTimer(string timerName)
	{
		int num = _activeTimers.IndexOf(timerName);
		if (num < 0)
		{
			return false;
		}
		RemoveActiveTimerAt(num);
		return true;
	}

	private bool IsTimerRunning(string timerName)
	{
		bool value;
		return timerRunning.TryGetValue(timerName, out value) && value;
	}

	public void AddTimer(string timerName, Variant time = default(Variant))
	{
		if (!string.IsNullOrEmpty(timerName))
		{
			double num = ((time.VariantType == Variant.Type.Nil) ? 1.0 : time.AsDouble());
			bool num2 = RemoveActiveTimer(timerName);
			_pendingTimeouts.Remove(timerName);
			timerDictionary[timerName] = num;
			timerRunning[timerName] = false;
			timerWaitTime[timerName] = num;
			timerCurrent[timerName] = 0.0;
			MarkSyncStateChanged();
			if (num2)
			{
				RefreshPhysicsProcessEligibility();
			}
		}
	}

	public void Run(string timerName, double time = -1.0)
	{
		if (!base.IsReleased && timerDictionary.ContainsKey(timerName))
		{
			bool num = !WantsPhysicsProcess;
			_pendingTimeouts.Remove(timerName);
			if (!IsTimerRunning(timerName))
			{
				_activeTimers.Add(timerName);
			}
			timerWaitTime[timerName] = ((time == -1.0) ? timerDictionary[timerName].AsDouble() : time);
			timerCurrent[timerName] = 0.0;
			timerRunning[timerName] = true;
			MarkSyncStateChanged();
			if (num)
			{
				RefreshPhysicsProcessEligibility();
			}
		}
	}

	public void Stop(string timerName)
	{
		if (!base.IsReleased && timerDictionary.ContainsKey(timerName))
		{
			bool num = RemoveActiveTimer(timerName);
			_pendingTimeouts.Remove(timerName);
			timerCurrent[timerName] = 0.0;
			timerRunning[timerName] = false;
			MarkSyncStateChanged();
			if (num)
			{
				RefreshPhysicsProcessEligibility();
			}
		}
	}

	public bool IsRunning(string timerName)
	{
		if (!base.IsReleased)
		{
			return IsTimerRunning(timerName);
		}
		return false;
	}

	public override Dictionary ExportComponentSave()
	{
		Dictionary dictionary = new Dictionary();
		Dictionary dictionary2 = new Dictionary();
		Dictionary dictionary3 = new Dictionary();
		foreach (string key5 in timerRunning.Keys)
		{
			dictionary[key5] = timerRunning[key5];
			dictionary2[key5] = (timerWaitTime.TryGetValue(key5, out var value) ? value : 0.0);
			dictionary3[key5] = (timerCurrent.TryGetValue(key5, out var value2) ? value2 : 0.0);
		}
		return new Dictionary
		{
			["timerRunning"] = dictionary,
			["timerWaitTime"] = dictionary2,
			["timerCurrent"] = dictionary3,
			["timeScale"] = timeScale
		};
	}

	public override void ImportComponentSave(Dictionary data, TowerDefenseLevelSaveConfigCSharp owner)
	{
		if (data == null)
		{
			return;
		}
		ResetRuntimeTimersFromConfig();
		Dictionary dictionary = data.GetValueOrDefault("timerRunning", new Dictionary()).AsGodotDictionary();
		Dictionary dictionary2 = data.GetValueOrDefault("timerWaitTime", new Dictionary()).AsGodotDictionary();
		Dictionary dictionary3 = data.GetValueOrDefault("timerCurrent", new Dictionary()).AsGodotDictionary();
		timeScale = data.GetValueOrDefault("timeScale", 1.0).AsDouble();
		foreach (Variant key in dictionary.Keys)
		{
			string text = key.AsString();
			double num = dictionary2.GetValueOrDefault(key, 0.0).AsDouble();
			if (!timerDictionary.ContainsKey(text))
			{
				timerDictionary[text] = num;
			}
			timerRunning[text] = dictionary[key].AsBool();
			timerWaitTime[text] = num;
			timerCurrent[text] = dictionary3.GetValueOrDefault(key, 0.0).AsDouble();
		}
		_activeTimers.Clear();
		foreach (KeyValuePair<string, bool> item in timerRunning)
		{
			if (item.Value)
			{
				_activeTimers.Add(item.Key);
			}
		}
		MarkSyncStateChanged();
		RefreshPhysicsProcessEligibility();
	}

	public override Dictionary SyncSerialize()
	{
		if (_syncPayloadInitialized && _syncPayloadRevision == _syncStateRevision)
		{
			return _syncPayload;
		}
		RebuildSyncPayload();
		return _syncPayload;
	}

	public override void SyncDeserialize(Dictionary data)
	{
		ImportComponentSave(data, null);
	}

	private void RebuildSyncPayload()
	{
		_syncRunningData.Clear();
		_syncWaitTimeData.Clear();
		_syncCurrentData.Clear();
		foreach (KeyValuePair<string, bool> item in timerRunning)
		{
			double value;
			double num = (timerWaitTime.TryGetValue(item.Key, out value) ? value : 0.0);
			double value2;
			double num2 = (timerCurrent.TryGetValue(item.Key, out value2) ? value2 : 0.0);
			_syncRunningData[item.Key] = item.Value;
			_syncWaitTimeData[item.Key] = num;
			_syncCurrentData[item.Key] = num2;
		}
		_syncPayload.Clear();
		_syncPayload[SyncRunningKey] = _syncRunningData;
		_syncPayload[SyncWaitTimeKey] = _syncWaitTimeData;
		_syncPayload[SyncCurrentKey] = _syncCurrentData;
		_syncPayload[SyncTimeScaleKey] = timeScale;
		_syncPayloadRevision = _syncStateRevision;
		_syncPayloadInitialized = true;
	}

	private void ClearSyncPayload()
	{
		_syncPayload.Clear();
		_syncRunningData.Clear();
		_syncWaitTimeData.Clear();
		_syncCurrentData.Clear();
		_syncPayloadRevision = 0uL;
		_syncPayloadInitialized = false;
	}

	private void MarkSyncStateChanged()
	{
		_syncStateRevision++;
	}
}

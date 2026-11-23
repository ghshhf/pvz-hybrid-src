using System;
using Godot;

namespace PVZHE.ModEditor.ModSystem;

public sealed record XWModBattleSnapshot(Guid SessionId, string NetworkSessionId, bool Running, bool Paused, bool HostAuthority, double SimulationSeconds, Vector2I GridSize);

using System;

namespace PVZHE.ModEditor.ModSystem;

public sealed record XWModModeState(Guid Session, string LevelKey, string Mode, bool Running, bool Failed, bool PendingOperations);

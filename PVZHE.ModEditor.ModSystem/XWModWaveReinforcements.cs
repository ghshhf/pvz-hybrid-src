using System.Collections.Generic;

namespace PVZHE.ModEditor.ModSystem;

public sealed record XWModWaveReinforcements(int Wave, IReadOnlyList<XWModReinforcement> Entries);

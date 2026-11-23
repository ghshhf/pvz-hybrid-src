using System.Collections.Generic;

namespace PVZHE.ModEditor.ModSystem;

public sealed record XWModTargetSelection(XWModEntityHandle Source, IReadOnlyList<XWModEntityHandle> Candidates, XWModEntityHandle Selected);

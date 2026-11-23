using System.Collections.Generic;
using System.Linq;

namespace PVZHE.ModEditor.ModSystem;

internal sealed record XWOfficialModDescriptor(ModLoader.LoadedMod Owner, string Author, string ModId, string CurrentVersion, string DisplayName, int LoadOrder, string SubmissionId = null)
{
	internal bool IsCurrent(IReadOnlyList<ModLoader.LoadedMod> loaded)
	{
		if (Owner.Applied && loaded.Contains(Owner) && Owner.LoadedId == ModId && Owner.LoadedVersion == CurrentVersion && (SubmissionId == null || Owner.Manifest?.PostId == SubmissionId))
		{
			XWModRuntimeSession current = XWModRuntimeSession.Current;
			if (current == null)
			{
				return true;
			}
			return !current.Faults.ContainsKey(ModId);
		}
		return false;
	}
}

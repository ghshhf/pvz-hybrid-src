using System;

namespace PVZHE.ModEditor.ModSystem;

public readonly record struct XWModEntityHandle(Guid SessionId, long Id)
{
	public bool IsValid
	{
		get
		{
			if (SessionId != Guid.Empty)
			{
				return Id != 0;
			}
			return false;
		}
	}
}

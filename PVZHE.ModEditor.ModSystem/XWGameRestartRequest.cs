using System;

namespace PVZHE.ModEditor.ModSystem;

internal sealed class XWGameRestartRequest
{
	internal bool IsRequested { get; private set; }

	internal bool TryRequest(Func<string> save, Action restart, out string diagnostic)
	{
		diagnostic = "";
		if (IsRequested)
		{
			diagnostic = "游戏正在重启，请稍候。";
			return false;
		}
		IsRequested = true;
		try
		{
			diagnostic = save();
			if (!string.IsNullOrEmpty(diagnostic))
			{
				IsRequested = false;
				return false;
			}
			restart();
			return true;
		}
		catch (Exception ex)
		{
			IsRequested = false;
			diagnostic = "未能重启游戏：" + ex.GetBaseException().Message;
			return false;
		}
	}
}

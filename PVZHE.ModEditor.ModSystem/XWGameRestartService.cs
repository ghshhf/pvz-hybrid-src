using System;
using System.Linq;
using Godot;

namespace PVZHE.ModEditor.ModSystem;

internal static class XWGameRestartService
{
	private static readonly XWGameRestartRequest Request = new XWGameRestartRequest();

	internal static bool IsRestartRequested => Request.IsRequested;

	internal static bool CanRestart(out string diagnostic)
	{
		Action restart;
		return TryPrepare(out restart, out diagnostic);
	}

	internal static bool TryRestart(out string diagnostic)
	{
		if (OS.GetThreadCallerId() != OS.GetMainThreadId())
		{
			diagnostic = "请在游戏主线程请求重启。";
			return false;
		}
		if (!TryPrepare(out var restart, out diagnostic))
		{
			return false;
		}
		return Request.TryRequest(delegate
		{
			GameSaveManager instance = GameSaveManager.Instance;
			if (!GodotObject.IsInstanceValid(instance))
			{
				return "存档尚未就绪，请稍后重试。";
			}
			string diagnostic2;
			return (!instance.TrySave(out diagnostic2)) ? diagnostic2 : "";
		}, restart, out diagnostic);
	}

	private static bool TryPrepare(out Action restart, out string diagnostic)
	{
		restart = null;
		diagnostic = "";
		if (!OS.HasFeature("template"))
		{
			diagnostic = "当前为编辑器运行，请停止游戏后重新运行以应用更改。";
			return false;
		}
		try
		{
			if (OS.HasFeature("android"))
			{
				if (!Engine.HasSingleton("AndroidRuntime"))
				{
					throw new InvalidOperationException("Android 重启接口不可用。");
				}
				JavaObject activity = Engine.GetSingleton("AndroidRuntime").Call("getActivity").AsGodotObject() as JavaObject;
				if (activity == null || !activity.HasJavaMethod("onGodotRestartRequested"))
				{
					throw new InvalidOperationException("当前 Android 启动器不支持自动重启。");
				}
				JavaObject godot = CallJava(activity, "getGodot").AsGodotObject() as JavaObject;
				JavaObject javaObject = CallJava(activity, "getPackageManager").AsGodotObject() as JavaObject;
				string text = CallJava(activity, "getPackageName").AsString();
				if (godot == null || javaObject == null || CallJava(javaObject, "getLaunchIntentForPackage", text).AsGodotObject() == null)
				{
					throw new InvalidOperationException("无法取得游戏的 Android 启动入口。");
				}
				restart = delegate
				{
					CallJava(activity, "onGodotRestartRequested", godot);
				};
				return true;
			}
			if (OS.HasFeature("windows") || OS.HasFeature("linux") || OS.HasFeature("macos"))
			{
				MainLoop mainLoop = Engine.GetMainLoop();
				SceneTree tree = mainLoop as SceneTree;
				if (tree == null)
				{
					throw new InvalidOperationException("游戏尚未准备好重启。");
				}
				string[] cmdlineUserArgs = OS.GetCmdlineUserArgs();
				string[] arguments = ((cmdlineUserArgs.Length == 0) ? OS.GetCmdlineArgs() : OS.GetCmdlineArgs().Concat(new string[1] { "--" }).Concat(cmdlineUserArgs)
					.ToArray());
				restart = delegate
				{
					OS.SetRestartOnExit(restart: true, arguments);
					try
					{
						tree.Quit();
					}
					catch
					{
						OS.SetRestartOnExit(restart: false);
						throw;
					}
				};
				return true;
			}
			diagnostic = "当前平台暂不支持自动重启，请关闭后重新打开游戏。";
		}
		catch (Exception ex)
		{
			diagnostic = ex.GetBaseException().Message + " 请关闭后重新打开游戏。";
		}
		return false;
	}

	private static Variant CallJava(JavaObject target, string method, params Variant[] arguments)
	{
		if (!target.HasJavaMethod(method))
		{
			throw new InvalidOperationException("Android 接口缺少 " + method + "。");
		}
		Variant result = target.Call(method, arguments);
		JavaObject exception = JavaClassWrapper.GetException();
		if (exception != null)
		{
			throw new InvalidOperationException("Android 重启接口调用失败：" + exception.Call("toString").AsString());
		}
		return result;
	}
}

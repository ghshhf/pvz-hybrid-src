using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using PVZHE.ModEditor.Debugging;

namespace PVZHE.ModEditor.Blueprint;

public sealed class XWBlueprintRuntimeContext
{
	public XWBlueprintRuntimeLimits Limits { get; set; } = new XWBlueprintRuntimeLimits();

	public CancellationToken CancellationToken { get; set; }

	public Func<int, Task> NodeVisitedAsync { get; set; }

	public Func<XWBlueprintRuntimeTracePoint, Task> NodeLocationVisitedAsync { get; set; }

	public Func<double, Task> DelayAsync { get; set; }

	public Action<string> Output { get; set; }

	public Action<string> EventTriggered { get; set; }

	public string EntryEventName { get; set; } = string.Empty;

	public string ModProjectRoot { get; set; } = string.Empty;

	public string SelfClassName { get; set; } = "BlueprintSelf";

	public IReadOnlyDictionary<string, Variant> SelfProperties { get; set; }

	public IReadOnlyDictionary<int, Variant> BlueprintVariables { get; set; }

	public XWBPScriptData ScriptData { get; set; }

	public XWModDebugController DebugController { get; set; }

	public string BlueprintSourcePath { get; set; } = string.Empty;
}

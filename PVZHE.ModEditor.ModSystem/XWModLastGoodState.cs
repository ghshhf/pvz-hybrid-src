using System.Collections.Generic;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModLastGoodState
{
	public int SchemaVersion { get; set; } = 1;

	public XWModSelectionState Selection { get; set; } = new XWModSelectionState();

	public List<XWModBootPackage> Packages { get; set; } = new List<XWModBootPackage>();
}

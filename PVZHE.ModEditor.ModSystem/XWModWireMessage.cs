namespace PVZHE.ModEditor.ModSystem;

internal sealed class XWModWireMessage
{
	public string Owner { get; set; }

	public string Name { get; set; }

	public int Version { get; set; }

	public string Session { get; set; }

	public string RequestId { get; set; }

	public byte[] Payload { get; set; }
}

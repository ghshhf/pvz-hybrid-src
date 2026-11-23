namespace PVZHE.ModEditor.ModSystem;

public sealed record XWModDamageValue
{
	public XWModEntityHandle Target { get; }

	public XWModEntityHandle Source { get; }

	public string Cause { get; }

	public XWModProjectileSource Attribution { get; internal init; }

	public double Amount { get; set; }

	internal XWModDamageValue(XWModEntityHandle target, XWModEntityHandle source, string cause, double amount)
	{
		Target = target;
		Source = source;
		Cause = cause;
		Amount = amount;
	}
}

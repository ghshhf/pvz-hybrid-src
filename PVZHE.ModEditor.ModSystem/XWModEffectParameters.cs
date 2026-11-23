namespace PVZHE.ModEditor.ModSystem;

public sealed record XWModEffectParameters
{
	public XWModEntityHandle Target { get; }

	public string Key { get; }

	public bool SupportsDuration { get; }

	public bool SupportsStrength { get; }

	public bool AllowsPermanent { get; }

	public double? Duration { get; set; }

	public double? Strength { get; set; }

	internal XWModEffectParameters(XWModEntityHandle target, string key, double? duration, double? strength, bool permanent)
	{
		double? duration2 = duration;
		double? strength2 = strength;
		bool hasValue = duration.HasValue;
		bool hasValue2 = strength.HasValue;
		bool flag = permanent;
		Target = target;
		Key = key;
		Duration = duration2;
		Strength = strength2;
		SupportsDuration = hasValue;
		SupportsStrength = hasValue2;
		AllowsPermanent = flag;
	}
}

using System;

internal readonly struct AdobeAnimateGpuClockState
{
	public bool Enabled { get; }

	public float StartTime { get; }

	public float StartFrame { get; }

	public float FramesPerSecond { get; }

	public float ClipStart { get; }

	public float ClipEndExclusive { get; }

	public bool Loop { get; }

	public AdobeAnimateGpuClockState(bool enabled, float startTime, float startFrame, float framesPerSecond, float clipStart, float clipEndExclusive, bool loop)
	{
		Enabled = enabled;
		StartTime = startTime;
		StartFrame = startFrame;
		FramesPerSecond = framesPerSecond;
		ClipStart = clipStart;
		ClipEndExclusive = Math.Max(clipStart + 1f, clipEndExclusive);
		Loop = loop;
	}
}

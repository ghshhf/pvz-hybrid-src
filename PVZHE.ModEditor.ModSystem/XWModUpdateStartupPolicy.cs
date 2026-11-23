using System;

namespace PVZHE.ModEditor.ModSystem;

internal sealed class XWModUpdateStartupPolicy
{
	private readonly Func<long> _clock;

	private long? _deadline;

	private bool _collectionEnded;

	private bool _pending;

	private bool _shown;

	internal const long CollectionMilliseconds = 2000L;

	public XWModUpdateStartupPolicy(Func<long> clock = null)
	{
		_clock = clock ?? ((Func<long>)(() => Environment.TickCount64));
	}

	internal void EnterMainMenu()
	{
		long valueOrDefault = _deadline.GetValueOrDefault();
		if (!_deadline.HasValue)
		{
			valueOrDefault = _clock() + 2000;
			_deadline = valueOrDefault;
		}
	}

	internal void Advance(int count)
	{
		if (!_collectionEnded && _deadline.HasValue && _clock() >= _deadline.Value)
		{
			_collectionEnded = true;
			_pending = count > 0;
		}
		if (_collectionEnded && count == 0)
		{
			_pending = false;
		}
	}

	internal bool TryShowAutomatically(bool menuIdle, int count)
	{
		Advance(count);
		if (_shown || !_pending || !menuIdle || count == 0)
		{
			return false;
		}
		MarkShown();
		return true;
	}

	internal void MarkShown()
	{
		_shown = true;
		_pending = false;
	}
}

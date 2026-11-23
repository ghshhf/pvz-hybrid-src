using System;
using System.Collections.Generic;
using Godot;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModLifetime : IDisposable
{
	private readonly List<Action> _cleanup = new List<Action>();

	private bool _disposed;

	private bool _frozen;

	public string OwnerId { get; }

	public XWModLifetime(string ownerId)
	{
		OwnerId = ownerId ?? throw new ArgumentNullException("ownerId");
	}

	public T Track<T>(T value) where T : IDisposable
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		Track(((IDisposable)value).Dispose);
		return value;
	}

	public void Track(Action cleanup)
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("XWModLifetime");
		}
		if (_frozen)
		{
			throw new InvalidOperationException("Mod 生命周期结构已冻结，请使用预先建立的运行时子作用域。");
		}
		_cleanup.Add(cleanup ?? throw new ArgumentNullException("cleanup"));
	}

	public XWModLifetime CreateChildScope()
	{
		XWModLifetime xWModLifetime = new XWModLifetime(OwnerId);
		Track(xWModLifetime);
		return xWModLifetime;
	}

	public T TrackNode<T>(T node) where T : Node
	{
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		Track(delegate
		{
			if (GodotObject.IsInstanceValid(node))
			{
				node.QueueFree();
			}
		});
		return node;
	}

	public void Freeze()
	{
		_frozen = true;
	}

	public IReadOnlyList<Exception> Cleanup()
	{
		if (_disposed)
		{
			return Array.Empty<Exception>();
		}
		_disposed = true;
		List<Exception> list = new List<Exception>();
		for (int num = _cleanup.Count - 1; num >= 0; num--)
		{
			try
			{
				_cleanup[num]();
			}
			catch (Exception item)
			{
				list.Add(item);
			}
		}
		_cleanup.Clear();
		return list.AsReadOnly();
	}

	public void Dispose()
	{
		IReadOnlyList<Exception> readOnlyList = Cleanup();
		if (readOnlyList.Count != 0)
		{
			throw new AggregateException("Mod '" + OwnerId + "' 生命周期清理失败。", readOnlyList);
		}
	}
}

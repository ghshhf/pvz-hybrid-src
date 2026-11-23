using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModPresentationService : IDisposable
{
	private readonly string _owner;

	private readonly Dictionary<string, Dictionary<string, string>> _texts = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);

	private readonly List<XWModLifetime> _scopes = new List<XWModLifetime>();

	private readonly HashSet<string> _actions = new HashSet<string>(StringComparer.Ordinal);

	private bool _disposed;

	private string BindingsPath => ProjectSettings.GlobalizePath("user://ModInput/" + Uri.EscapeDataString(_owner) + ".cfg");

	internal XWModPresentationService(XWModRuntimeContext context)
	{
		_owner = context.ModId;
		XWModExtensions.LocalPresentationFault += Fault;
		XWModExtensions.OwnerEnding += Fault;
	}

	private void Fault(string owner)
	{
		if (string.Equals(owner, _owner, StringComparison.OrdinalIgnoreCase))
		{
			Dispose();
		}
	}

	public XWModLifetime CreateSceneScope(Node scene)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		if (!GodotObject.IsInstanceValid(scene) || !scene.IsInsideTree())
		{
			throw new ArgumentException("Scene must be inside the tree.", "scene");
		}
		XWModLifetime scope = new XWModLifetime(_owner);
		_scopes.Add(scope);
		scene.TreeExited += Exit;
		scope.Track(delegate
		{
			if (GodotObject.IsInstanceValid(scene))
			{
				scene.TreeExited -= Exit;
			}
			_scopes.Remove(scope);
		});
		return scope;
		void Exit()
		{
			XWModPresentation.CleanupLocal(scope);
		}
	}

	private void RequireScope(XWModLifetime scope)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		if (scope == null || !string.Equals(scope.OwnerId, _owner, StringComparison.OrdinalIgnoreCase))
		{
			throw new ArgumentException("Scene lifetime belongs to another Mod.");
		}
	}

	public AudioStreamPlayer PlayAudio(Node scene, XWModLifetime scope, AudioStream stream, float volumeDb = 0f)
	{
		RequireScope(scope);
		if (!GodotObject.IsInstanceValid(stream) || !float.IsFinite(volumeDb))
		{
			throw new ArgumentException("Invalid audio stream or volume.");
		}
		AudioStreamPlayer audioStreamPlayer = new AudioStreamPlayer
		{
			Stream = stream,
			VolumeDb = Mathf.Clamp(volumeDb, -80f, 12f)
		};
		scope.TrackNode(audioStreamPlayer);
		scene.AddChild(audioStreamPlayer, forceReadableName: false, Node.InternalMode.Disabled);
		audioStreamPlayer.Finished += audioStreamPlayer.QueueFree;
		audioStreamPlayer.Play();
		return audioStreamPlayer;
	}

	public void RegisterText(string locale, IReadOnlyDictionary<string, string> messages)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		if (string.IsNullOrWhiteSpace(locale) || messages == null)
		{
			throw new ArgumentException("Locale and text are required.");
		}
		_texts[locale.Replace('-', '_')] = new Dictionary<string, string>(messages, StringComparer.Ordinal);
	}

	public string Text(string key, string fallback = null)
	{
		string locale = TranslationServer.GetLocale();
		if (_texts.TryGetValue(locale, out var value) && value.TryGetValue(key, out var value2))
		{
			return value2;
		}
		string key2 = locale.Split('_')[0];
		if (_texts.TryGetValue(key2, out var value3) && value3.TryGetValue(key, out value2))
		{
			return value2;
		}
		if (_texts.TryGetValue("en", out var value4) && value4.TryGetValue(key, out value2))
		{
			return value2;
		}
		return fallback ?? key;
	}

	private string ActionName(string id)
	{
		if (string.IsNullOrWhiteSpace(id) || id.Length > 80 || id.Any((char c) => !char.IsAsciiLetterOrDigit(c) && c != '_' && c != '.'))
		{
			throw new ArgumentException("Invalid action ID.");
		}
		return "mod/" + _owner + "/" + id;
	}

	public string RegisterAction(string id, Key defaultKey)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		XWModRuntimeSession current = XWModRuntimeSession.Current;
		if (current != null && current.Frozen)
		{
			throw new InvalidOperationException("Register input actions during Mod initialization.");
		}
		string text = ActionName(id);
		if (_actions.Contains(text))
		{
			throw new InvalidOperationException("Action is already registered.");
		}
		if (InputMap.HasAction(text))
		{
			throw new InvalidOperationException("Action already belongs to another registration.");
		}
		ConfigFile configFile = new ConfigFile();
		Error error = configFile.Load(BindingsPath);
		if (error != Error.Ok && error != Error.FileNotFound)
		{
			throw new InvalidOperationException("Cannot read input bindings: " + error);
		}
		Key key = (Key)configFile.GetValue("keys", id, (long)defaultKey).AsInt64();
		InputMap.AddAction(text);
		if (key != Key.None)
		{
			InputMap.ActionAddEvent(text, new InputEventKey
			{
				PhysicalKeycode = key
			});
		}
		_actions.Add(text);
		return text;
	}

	public void SetBinding(string id, Key key)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		string text = ActionName(id);
		if (!_actions.Contains(text))
		{
			throw new InvalidOperationException("Register the action first.");
		}
		ConfigFile configFile = new ConfigFile();
		Error error = configFile.Load(BindingsPath);
		if (error != Error.Ok && error != Error.FileNotFound)
		{
			throw new InvalidOperationException("Input bindings are unreadable; not overwriting.");
		}
		configFile.SetValue("keys", id, (long)key);
		Directory.CreateDirectory(Path.GetDirectoryName(BindingsPath));
		string text2 = BindingsPath + ".tmp";
		if (configFile.Save(text2) != Error.Ok)
		{
			throw new IOException("Cannot save input binding.");
		}
		File.Move(text2, BindingsPath, overwrite: true);
		InputMap.ActionEraseEvents(text);
		if (key != Key.None)
		{
			InputMap.ActionAddEvent(text, new InputEventKey
			{
				PhysicalKeycode = key
			});
		}
	}

	public void BindAction(Node scene, XWModLifetime scope, string id, Action pressed, bool consume = true)
	{
		RequireScope(scope);
		string text = ActionName(id);
		if (!_actions.Contains(text))
		{
			throw new InvalidOperationException("Register the action first.");
		}
		XWModInputListener listener = new XWModInputListener
		{
			OwnerId = _owner,
			Session = XWModRuntimeSession.Current,
			ActionName = text,
			Pressed = (pressed ?? throw new ArgumentNullException("pressed")),
			Consume = consume,
			Fault = delegate(Exception ex)
			{
				GD.PushWarning("[Mod:" + _owner + "] local input failed: " + ex.Message);
				XWModPresentation.CleanupLocal(scope);
			}
		};
		scope.TrackNode(listener);
		scope.Track(delegate
		{
			listener.Active = false;
		});
		scene.AddChild(listener, forceReadableName: false, Node.InternalMode.Disabled);
	}

	public Button CreateTouchButton(Control parent, XWModLifetime scope, string id, string text)
	{
		RequireScope(scope);
		string action = ActionName(id);
		if (!_actions.Contains(action))
		{
			throw new InvalidOperationException("Register the action first.");
		}
		Button button = new Button
		{
			Text = text,
			FocusMode = Control.FocusModeEnum.None
		};
		scope.TrackNode(button);
		parent.AddChild(button, forceReadableName: false, Node.InternalMode.Disabled);
		button.ButtonDown += delegate
		{
			Input.ParseInputEvent(new InputEventAction
			{
				Action = action,
				Pressed = true
			});
		};
		button.ButtonUp += delegate
		{
			Input.ParseInputEvent(new InputEventAction
			{
				Action = action,
				Pressed = false
			});
		};
		scope.Track(delegate
		{
			Input.ActionRelease(action);
		});
		return button;
	}

	public Node2D SpawnEffect(Node scene, XWModLifetime scope, PackedScene effect, Vector2 position)
	{
		RequireScope(scope);
		if (!GodotObject.IsInstanceValid(effect) || !position.IsFinite())
		{
			throw new ArgumentException("Invalid effect.");
		}
		Node2D node2D = effect.Instantiate<Node2D>(PackedScene.GenEditState.Disabled);
		scope.TrackNode(node2D);
		scene.AddChild(node2D, forceReadableName: false, Node.InternalMode.Disabled);
		node2D.GlobalPosition = position;
		return node2D;
	}

	public Camera2D OverrideCamera(Node scene, XWModLifetime scope, Vector2 position, Vector2 zoom)
	{
		RequireScope(scope);
		if (!position.IsFinite() || !zoom.IsFinite() || zoom.X <= 0f || zoom.Y <= 0f)
		{
			throw new ArgumentException("Invalid camera transform.");
		}
		return XWModCameraStack.Push(scene, scope, position, zoom);
	}

	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}
		_disposed = true;
		XWModExtensions.LocalPresentationFault -= Fault;
		XWModExtensions.OwnerEnding -= Fault;
		XWModLifetime[] array = _scopes.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			XWModPresentation.CleanupLocal(array[i]);
		}
		_scopes.Clear();
		foreach (string action in _actions)
		{
			Input.ActionRelease(action);
			if (InputMap.HasAction(action))
			{
				InputMap.EraseAction(action);
			}
		}
		_actions.Clear();
		_texts.Clear();
	}
}

using System.Collections.Generic;
using System.Linq;
using Godot;

namespace PVZHE.ModEditor.ModSystem;

internal static class XWModCameraStack
{
	private sealed class StackState
	{
		public Camera2D Original;

		public readonly List<Camera2D> Cameras = new List<Camera2D>();
	}

	private static readonly Dictionary<ulong, StackState> States = new Dictionary<ulong, StackState>();

	internal static Camera2D Push(Node scene, XWModLifetime scope, Vector2 position, Vector2 zoom)
	{
		Viewport viewport = scene.GetViewport();
		ulong id = viewport.GetInstanceId();
		if (!States.TryGetValue(id, out var state))
		{
			Dictionary<ulong, StackState> states = States;
			ulong key = id;
			StackState obj = new StackState
			{
				Original = viewport.GetCamera2D()
			};
			StackState value = obj;
			state = obj;
			states[key] = value;
		}
		Camera2D camera = new Camera2D
		{
			Zoom = zoom
		};
		scene.AddChild(camera, forceReadableName: false, Node.InternalMode.Disabled);
		camera.GlobalPosition = position;
		state.Cameras.Add(camera);
		bool removed = false;
		scope.Track(Remove);
		camera.TreeExiting += Remove;
		camera.MakeCurrent();
		return camera;
		void Remove()
		{
			if (!removed)
			{
				removed = true;
				bool num = GodotObject.IsInstanceValid(camera) && camera.IsCurrent();
				state.Cameras.Remove(camera);
				if (GodotObject.IsInstanceValid(camera))
				{
					camera.TreeExiting -= Remove;
					camera.Enabled = false;
					camera.QueueFree();
				}
				if (num)
				{
					Camera2D camera2D = state.Cameras.LastOrDefault((Camera2D c) => GodotObject.IsInstanceValid(c) && !c.IsQueuedForDeletion()) ?? state.Original;
					if (GodotObject.IsInstanceValid(camera2D) && !camera2D.IsQueuedForDeletion())
					{
						camera2D.MakeCurrent();
					}
				}
				if (state.Cameras.Count == 0)
				{
					States.Remove(id);
				}
			}
		}
	}
}

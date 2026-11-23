using Godot;

namespace PVZHE.ModEditor.ModSystem;

public sealed record XWModUiSurface(Control Root, XWModLifetime Lifetime, string Kind, XWModEntityHandle Entity = default(XWModEntityHandle), string ContentKey = "", XWModCardHandle Card = default(XWModCardHandle));

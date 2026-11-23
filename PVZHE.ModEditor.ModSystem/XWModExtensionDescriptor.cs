using System;

namespace PVZHE.ModEditor.ModSystem;

public sealed record XWModExtensionDescriptor(string Id, XWModAuthority Authority, bool Modifiable, bool LocalPresentation, Type PayloadType);

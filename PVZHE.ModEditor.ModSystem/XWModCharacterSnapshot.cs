namespace PVZHE.ModEditor.ModSystem;

public sealed record XWModCharacterSnapshot(XWModEntityHandle Handle, string ContentKey, int Column, int Row, int Camp, double Hitpoints, bool Dead, bool Removing);

using Godot;

public struct ArmorDamageContext
{
	public TowerDefenseArmorInstance Armor;

	public readonly double IncomingDamage;

	public double Damage;

	public double AppliedDamage;

	public double RemainingDamage;

	public bool PlaySplatAudio;

	public Vector2 Velocity;

	public bool CreateDamagePart;

	public bool IgnoreLimit;

	public bool Cancel;

	public ArmorDamageContext(TowerDefenseArmorInstance armor, double damage, bool playSplatAudio, Vector2 velocity, bool createDamagePart, bool ignoreLimit)
	{
		Armor = armor;
		IncomingDamage = damage;
		Damage = damage;
		AppliedDamage = 0.0;
		RemainingDamage = damage;
		PlaySplatAudio = playSplatAudio;
		Velocity = velocity;
		CreateDamagePart = createDamagePart;
		IgnoreLimit = ignoreLimit;
		Cancel = false;
	}
}

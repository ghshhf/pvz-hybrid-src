public interface IBulletFieldStoredProjectileOwner
{
	string StoredProjectileSaveConfigName { get; }

	int GetStoredProjectileSaveOrder(BulletField field, int index);

	bool RestoreStoredProjectile(BulletField field, int index, int order);
}

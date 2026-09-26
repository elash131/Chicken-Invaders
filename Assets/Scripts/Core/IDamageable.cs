/// <summary>
/// A gameplay object that can receive projectile damage. The receiver owns health, death,
/// score and every other consequence; the projectile only delivers the hit once.
/// </summary>
public interface IDamageable
{
    void TakeDamage(int amount);
}

using UnityEngine;

/// <summary>
/// Attached to the boss's AttackHitbox. Deals damage to the player on contact.
/// </summary>
public class BossHitbox : MonoBehaviour
{
    private BossController boss;

    private void Awake()
    {
        boss = GetComponentInParent<BossController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only damage the player
        if (!other.CompareTag("Player")) return;

        // Call the damage method on the boss to get the damage value,
        // then apply it to the player.
        float damage = boss.GetCurrentAttackDamage();
        
        if (other.TryGetComponent(out PlayerCombatManager player))
        {
            player.TakeDamage(damage);
        }
    }
}
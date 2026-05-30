using UnityEngine;
using BreadcrumbAi;
using System;

public class PlayerCombatManager : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackRange = 1.4f;
    private float attackDamage = 25f;
    public float attackCooldown = 0.5f;
    public float attackRadius = 0.6f;
    
    [Header("Health")]
    [SerializeField] private float currentHealth;
    private float _lastMaxHealth; // для отслеживания изменений maxHealth
    
    // EVENTS
    public static event Action OnPlayerDeath;
    public static event Action<float, float> OnPlayerDamaged;  // (currentHealth, maxHealth)
    public static event Action<float, float> OnPlayerHealed;    // (currentHealth, maxHealth)
    
    private BasePlayerStats stats;
    private float nextAttackTime;
    private bool isDead;
    
    void Start()
    {
        stats = GetComponent<BasePlayerStats>();
        if (stats == null)
        {
            Debug.LogError("PlayerCombatManager requires BasePlayerStats on the same GameObject!");
        }
        else{
            RestoreHealth();
            attackDamage = stats.AttackDamage;
            _lastMaxHealth = stats.Health;
        }
    }
    
    public void RestoreHealth()
    {
        currentHealth = stats.Health;
        _lastMaxHealth = stats.Health;
        isDead = false;
        OnPlayerHealed?.Invoke(currentHealth, stats.Health);
        Debug.Log($"Health restored: {currentHealth}/{stats.Health}");
    }
    
    void Update()
    {
        if (isDead) return; // Dead men don't swing swords
        
        // Отслеживаем изменение maxHealth (после баффа рецепта)
        if (stats != null && stats.Health != _lastMaxHealth)
        {
            // При увеличении maxHealth - сразу восстанавливаем текущее хп до максимума
            if (stats.Health > _lastMaxHealth)
                currentHealth = stats.Health;
            // если maxHealth уменьшился - currentHealth не превышает новый максимум. По идее у нас нет дебаффов, но на всякий случай
            else
                currentHealth = Mathf.Min(currentHealth, stats.Health);
            _lastMaxHealth = stats.Health;
            OnPlayerHealed?.Invoke(currentHealth, stats.Health);
        }

        if (Input.GetMouseButtonDown(0) && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            TryAttack();
        }
    }
    
    void TryAttack()
    {
        Vector3 origin = transform.position + Vector3.up * 1f;
        RaycastHit hit;
        Debug.Log($"You be swinging fr");

        PlayerAnimationController arms = GetComponentInChildren<PlayerAnimationController>();
        if (arms != null) arms.PlayAttack();
        
        if (Physics.SphereCast(origin, attackRadius, transform.forward, out hit, attackRange))
        {
            Debug.Log($"Congrats, you hit something");

            Ai enemyAi = hit.collider.GetComponent<Ai>();
            if (enemyAi != null && enemyAi.lifeState == Ai.LIFE_STATE.IsAlive)
            {
                enemyAi.Health -= attackDamage;
                Debug.Log($"Player hit {hit.collider.name}! Enemy HP: {enemyAi.Health}");
            }

            BossController boss = hit.collider.GetComponentInParent<BossController>();
            if (boss != null)
            {
                boss.TakeDamage(attackDamage);
                Debug.Log($"Player hit the boss! Boss HP: {boss.currentHealth}");
            }
        }
    }
    
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        OnPlayerDamaged?.Invoke(currentHealth, stats.Health);
        
        Debug.Log($"Player took {damage} damage. HP: {currentHealth}/{stats.Health}");
        
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            isDead = true;
            OnPlayerDeath?.Invoke();
            Debug.Log($"Player died lol, HP: {currentHealth}/{stats.Health}");
        }
    }
    
    public void Heal(float amount)
    {
        if (isDead) return;
        
        currentHealth = Mathf.Min(currentHealth + amount, stats.Health);
        OnPlayerHealed?.Invoke(currentHealth, stats.Health);
        Debug.Log($"Player healed {amount}. HP: {currentHealth}/{stats.Health}");
    }
    
    // Public getters for other scripts
    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => stats.Health;
    public bool IsDead() => isDead;
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 origin = transform.position + Vector3.up * 1f;
        Gizmos.DrawWireSphere(origin, attackRadius);
        Gizmos.DrawWireSphere(origin + transform.forward * attackRange, attackRadius);
        Gizmos.DrawLine(origin, origin + transform.forward * attackRange);
    }
}
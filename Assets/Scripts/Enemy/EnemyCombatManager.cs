// using UnityEngine;
// using BreadcrumbAi;

// public class EnemyCombat : MonoBehaviour
// {
//     [Header("Combat Settings")]
//     public float meleeDamage = 5f;
//     public float meleeAttackRate = 1f;
//     public float rangedDamage = 8f;
//     public float rangedAttackRate = 2f;
//     public GameObject projectilePrefab;
//     public float projectileForce = 500f;
    
//     [Header("Death Settings")]
//     public float destroyDelay = 3f;
    
//     private Ai ai;
//     private float nextAttackTime;
//     private bool deathHandled;
//     private Transform player;
    
//     void Start()
//     {
//         ai = GetComponent<Ai>();
//         if (ai == null)
//         {
//             Debug.LogError("EnemyCombat requires Ai component on the same GameObject!");
//             return;
//         }
        
//         GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
//         if (playerObj != null)
//         {
//             player = playerObj.transform;
//         }
//     }
    
//     void Update()
//     {
//         if (ai.lifeState == Ai.LIFE_STATE.IsDead && !deathHandled)
//         {
//             HandleDeath();
//         }
//     }
    
//     void FixedUpdate()
//     {
//         if (ai.lifeState != Ai.LIFE_STATE.IsAlive || player == null) return;
        
//         if (ai.attackState == Ai.ATTACK_STATE.CanAttackPlayer && Time.time >= nextAttackTime)
//         {
//             if (ai._IsMelee)
//             {
//                 MeleeAttack();
//             }
//             else if (ai._IsRanged)
//             {
//                 RangedAttack();
//             }
//         }
//     }
    
//     void MeleeAttack()
//     {
//         nextAttackTime = Time.time + meleeAttackRate;
        
//         PlayerCombatManager playerCombat = player.GetComponent<PlayerCombatManager>();
//         if (playerCombat != null && !playerCombat.IsDead())
//         {
//             playerCombat.TakeDamage(meleeDamage);
//             Debug.Log($"{gameObject.name} melee hit player for {meleeDamage} damage!");
//         }
//     }
    
//     void RangedAttack()
//     {
//         nextAttackTime = Time.time + rangedAttackRate;
        
//         if (projectilePrefab != null)
//         {
//             Vector3 spawnPos = transform.position + transform.forward + Vector3.up;
//             GameObject projectile = Instantiate(projectilePrefab, spawnPos, transform.rotation);
            
//             Rigidbody rb = projectile.GetComponent<Rigidbody>();
//             if (rb != null)
//             {
//                 rb.AddForce(transform.forward * projectileForce);
//             }
//         }
//     }
    
//     void HandleDeath()
//     {
//         deathHandled = true;
        
//         Collider col = GetComponent<Collider>();
//         if (col != null) col.enabled = false;
        
//         Rigidbody rb = GetComponent<Rigidbody>();
//         if (rb != null) rb.isKinematic = true;
        
//         Debug.Log($"{gameObject.name} died!");
        
//         Destroy(gameObject, destroyDelay);
//     }
// }

using UnityEngine;
using BreadcrumbAi;

public class EnemyCombat : MonoBehaviour
{
    [Header("Combat Settings")]
    public float meleeDamage = 5f;
    public float meleeAttackRate = 1f;
    public float rangedDamage = 8f;
    public float rangedAttackRate = 2f;
    public GameObject projectilePrefab;
    public float projectileForce = 500f;
    
    [Header("Death Settings")]
    public float destroyDelay = 3f;
    
    [Header("Animation")]
    [Tooltip("Leave empty to auto-find on this GameObject")]
    public Animator animator;
    
    // Animator parameter hashes (faster than strings)
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int Attack = Animator.StringToHash("Attack");
    private static readonly int IsDead = Animator.StringToHash("IsDead");
    
    private Ai ai;
    private float nextAttackTime;
    private bool deathHandled;
    private Transform player;
    
    void Start()
    {
        ai = GetComponent<Ai>();
        if (ai == null)
        {
            Debug.LogError("EnemyCombat requires Ai component on the same GameObject!");
            return;
        }
        
        // Auto-find animator if not assigned
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }
    
    void Update()
    {
        UpdateAnimations();
        
        if (ai.lifeState == Ai.LIFE_STATE.IsDead && !deathHandled)
        {
            HandleDeath();
        }
    }
    
    void FixedUpdate()
    {
        if (ai.lifeState != Ai.LIFE_STATE.IsAlive) return;
        if (player == null) return;
        
        if (ai.attackState == Ai.ATTACK_STATE.CanAttackPlayer && Time.time >= nextAttackTime)
        {
            if (ai._IsMelee)
            {
                MeleeAttack();
            }
            else if (ai._IsRanged)
            {
                RangedAttack();
            }
        }
    }
    
    void UpdateAnimations()
    {
        if (animator == null) return;
        
        // Movement animation
        bool moving = ai.moveState == Ai.MOVEMENT_STATE.IsFollowingPlayer ||
                      ai.moveState == Ai.MOVEMENT_STATE.IsFollowingBreadcrumb ||
                      ai.moveState == Ai.MOVEMENT_STATE.IsFollowingAi ||
                      ai.moveState == Ai.MOVEMENT_STATE.IsFollowingAiTierTwo ||
                      ai.moveState == Ai.MOVEMENT_STATE.IsWandering ||
                      ai.moveState == Ai.MOVEMENT_STATE.IsPatrolling;
        
        animator.SetBool(IsMoving, moving);
        
        // Death animation
        if (ai.lifeState == Ai.LIFE_STATE.IsDead)
        {
            animator.SetBool(IsDead, true);
        }
    }
    
    void MeleeAttack()
    {
        nextAttackTime = Time.time + meleeAttackRate;
        
        // Trigger attack animation
        if (animator != null)
        {
            animator.SetTrigger(Attack);
        }
        
        PlayerCombatManager playerCombat = player.GetComponent<PlayerCombatManager>();
        if (playerCombat != null && !playerCombat.IsDead())
        {
            playerCombat.TakeDamage(meleeDamage);
            Debug.Log($"{gameObject.name} melee hit player for {meleeDamage} damage!");
        }
    }
    
    void RangedAttack()
    {
        nextAttackTime = Time.time + rangedAttackRate;
        
        if (animator != null)
        {
            animator.SetTrigger(Attack);
        }
        
        if (projectilePrefab != null)
        {
            Vector3 spawnPos = transform.position + transform.forward + Vector3.up;
            GameObject projectile = Instantiate(projectilePrefab, spawnPos, transform.rotation);
            
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(transform.forward * projectileForce);
            }
        }
    }
    
    void HandleDeath()
    {
        deathHandled = true;
        
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
        
        Debug.Log($"{gameObject.name} died!");
        
        Destroy(gameObject, destroyDelay);
    }
}
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Main controller for the dragon boss.
/// Manages states, animation triggers, and combat logic.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class BossController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Collider damageHitbox; // The boss's attack hitbox (enabled during active frames)
    
    [Header("WakingUp")]
    private float screamDuration = 3.33f; // How long the Scream animation lasts
    private float wakeUpTimer;
    private bool playerInRoom;

    [Header("Idle / Decision")]
    [SerializeField] private float idlePauseDuration = 0.8f;  // How long the boss pauses before choosing next action
    [SerializeField] [Range(0f, 1f)] private float attackChance = 0.6f;  // Likelihood of attacking vs taunting
    private float idleTimer;
    private int currentAttackType; // 0 = Bite (Basic Attack), 1 = Claw (Claw Attack)

    [Header("Repositioning")]
    [SerializeField] private float biteIdealRange = 2.5f;   // Distance boss wants for Bite attack
    [SerializeField] private float clawIdealRange = 5f;     // Distance boss wants for Claw attack
    [SerializeField] private float repositionStoppingDistance = 0.3f; // How close is "close enough"
    [SerializeField] private float turnSpeed = 120f; // Degrees per second, how fast boss turns

    [Header("Attacking")]
    [SerializeField] private float attackRecoveryDuration = 1f;  // Post-attack pause before choosing next action
    [SerializeField] private float biteDamage = 15f;
    [SerializeField] private float clawDamage = 20f;
    private float attackTimer;

    [Header("Taunting")]
    [SerializeField] private float tauntDuration = 3f; // How long the taunt lasts
    [SerializeField] [Range(0f, 1f)] private float sitChance = 0.4f; // Chance to sit (idle02) vs circle-walk
    [SerializeField] private float circleDistance = 4f; // How far from player to circle
    [SerializeField] private float circleSpeed = 1.5f; // Walking speed while circling
    [SerializeField] private Transform roomCenter; // Empty GameObject at the center of the room
    private float tauntTimer;
    private bool isSittingTaunt; // True = sitting (idle02), false = circling
    private float currentCircleAngle;

    [Header("Taking damage")]
    [SerializeField] private float maxHealth = 400f;
    [SerializeField] [Range(0f, 1f)] private float hitReactChance = 0.5f;   // Chance to flinch when hit
    [SerializeField] [Range(0f, 1f)] private float blockChance = 0.3f;      // Chance to block when hit
    [SerializeField] [Range(0f, 1f)] private float blockDamageReduction = 0.5f; // How much damage block prevents
    [HideInInspector] private float currentHealth;
    private bool isDead;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public BossState CurrentState => currentState;


    // Components
    private NavMeshAgent agent;
    private Animator animator;

    // Current state
    private BossState currentState;

    // Animator parameter hashes (cached for performance)
    private static readonly int OnWakeUpParam = Animator.StringToHash("OnWakeUp");
    private static readonly int OnAttackParam = Animator.StringToHash("OnAttack");
    private static readonly int OnHitReactParam = Animator.StringToHash("OnHitReact");
    private static readonly int OnBlockParam = Animator.StringToHash("OnBlock");
    private static readonly int OnDieParam = Animator.StringToHash("OnDie");
    private static readonly int IsSleepingParam = Animator.StringToHash("IsSleeping");
    private static readonly int IsWalkingParam = Animator.StringToHash("IsWalking");
    private static readonly int IsTauntingParam = Animator.StringToHash("IsTaunting");
    private static readonly int AttackTypeParam = Animator.StringToHash("AttackType");

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        agent.angularSpeed = turnSpeed;
    }

    private void Start()
    {
        TransitionToState(BossState.Sleeping); // Begin in sleeping state
        currentHealth = maxHealth;
    }

    private void Update()
    {
        Debug.Log($"Boss state: {currentState}");
        // Route to the logic for the current state
        switch (currentState)
        {
            case BossState.WakingUp:
                UpdateWakingUp();
                break;
            case BossState.Idle:
                UpdateIdle();
                break;
            case BossState.Repositioning:
                UpdateRepositioning();
                break;
            case BossState.Attacking:
                UpdateAttacking();
                break;
            case BossState.Taunting:
                UpdateTaunting();
                break;
            case BossState.HitReact:
                UpdateHitReact();
                break;
            case BossState.Blocking:
                UpdateBlocking();
                break;
            case BossState.Dying:
                UpdateDying();
                break;
        }
    }

    /// <summary>
    /// Central method for changing states. Handles cleanup of old state and setup of new state.
    /// </summary>
    private void TransitionToState(BossState newState)
    {
        // Exit logic for the previous state
        ExitState(currentState);

        // Set new state
        currentState = newState;

        // Enter logic for the new state
        EnterState(newState);
    }

    // ---------- State Enter/Exit ----------

    private void EnterState(BossState state)
    {
        switch (state)
        {
            case BossState.Sleeping:
                animator.SetBool(IsSleepingParam, true);
                agent.enabled = false; // No movement while sleeping
                break;

            case BossState.WakingUp:
                animator.SetBool(IsSleepingParam, false);
                animator.SetTrigger(OnWakeUpParam);
                wakeUpTimer = screamDuration;
                break;

            case BossState.Idle:
                idleTimer = idlePauseDuration;
                agent.enabled = false;  // Stand still while deciding
                break;

            case BossState.Repositioning:
                agent.enabled = true;
                animator.SetBool(IsWalkingParam, true);
                SetRepositionDestination(); // Set the NavMeshAgent's destination to the ideal position for the chosen attack
                break;

            case BossState.Attacking:
                agent.enabled = false; // Don't move while attacking
                attackTimer = attackRecoveryDuration;
                animator.SetTrigger(OnAttackParam);
                break;

            case BossState.Taunting:
                tauntTimer = tauntDuration;
                agent.enabled = true;
                 
                isSittingTaunt = Random.value <= sitChance; // Randomly sit or circle

                if (isSittingTaunt)
                {
                    agent.enabled = false; // Don't move while sitting
                    animator.SetBool(IsTauntingParam, true);
                }
                else
                {
                    animator.SetBool(IsWalkingParam, true);
                    agent.speed = circleSpeed;

                    Vector3 toBoss = transform.position - roomCenter.position;
                    currentCircleAngle = Mathf.Atan2(toBoss.x, toBoss.z) * Mathf.Rad2Deg;
                    SetCircleDestination();
                }
                break;

            case BossState.HitReact:
                agent.enabled = false;
                animator.SetTrigger(OnHitReactParam);
                break;

            case BossState.Blocking:
                agent.enabled = false;
                animator.SetTrigger(OnBlockParam);
                break;

            case BossState.Dying:
                agent.enabled = false;
                animator.SetTrigger(OnDieParam);
                break;
        }
    }

    private void ExitState(BossState state)
    {
        switch (state)
        {
            case BossState.Repositioning:
                animator.SetBool(IsWalkingParam, false);
                break;

            case BossState.Taunting:
                animator.SetBool(IsTauntingParam, false);
                animator.SetBool(IsWalkingParam, false);
                break;

            case BossState.Attacking:
                // Hitbox gets disabled elsewhere, but safety:
                if (damageHitbox != null)
                    damageHitbox.enabled = false;
                break;
        }
    }

    // ---------- State Update Methods ----------

    private void UpdateWakingUp()
    {
        wakeUpTimer -= Time.deltaTime;

        if (wakeUpTimer <= 0f)
        {
            TransitionToState(BossState.Idle);
        }
    }

    private void UpdateIdle()
    {
        idleTimer -= Time.deltaTime;

        if (idleTimer > 0f) return; // Still pausing

        // Choose next action
        if (Random.value <= attackChance)
        {
            ChooseAttack();
        }
        else
        {
            TransitionToState(BossState.Taunting);
        }
    }

    // Picks a random attack and transitions to Repositioning to get into range.
    private void ChooseAttack()
    {
        // Randomly pick: 0 = Bite, 1 = Claw
        int chosenAttack = Random.Range(0, 2);
        animator.SetInteger(AttackTypeParam, chosenAttack);

        // Store which attack was chosen (will be needed in Repositioning)
        currentAttackType = chosenAttack;

        TransitionToState(BossState.Repositioning);
    }

    private void UpdateRepositioning()
    {
        if (player == null)
        {
            TransitionToState(BossState.Idle);
            return;
        }

        // Check if we're close enough to the ideal position
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        float targetRange = GetIdealRangeForCurrentAttack();

        // Also check that the agent has reached its destination
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + repositionStoppingDistance)
        {
            // Face the player before attacking
            FacePlayer();
            TransitionToState(BossState.Attacking);
        }
    }

    // Sets the NavMeshAgent's destination to a point at the ideal range from the player.
    private void SetRepositionDestination()
    {
        if (player == null) return;

        float idealRange = GetIdealRangeForCurrentAttack();
        Vector3 directionToBoss = (transform.position - player.position).normalized;
        Vector3 targetPosition = player.position + directionToBoss * idealRange;

        // Ensure we use a valid NavMesh position
        if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, idealRange * 2f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            // Fallback: just go to player's position
            agent.SetDestination(player.position);
        }
    }

    // Returns the ideal engagement distance for the currently selected attack.
    private float GetIdealRangeForCurrentAttack()
    {
        return currentAttackType switch
        {
            0 => biteIdealRange,   // Bite
            1 => clawIdealRange,   // Claw
            _ => biteIdealRange
        };
    }

    // Rotates the boss to face the player.
    private void FacePlayer()
    {
        if (player == null) return;

        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0f; // Keep rotation on the horizontal plane

        if (direction == Vector3.zero) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * agent.angularSpeed);
    }

    private void UpdateAttacking()
    {
        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0f)
        {
            // Attack animation has played and recovery is over
            TransitionToState(BossState.Idle);
        }
    }

    // Called by Animation Events on attack clips to enable/disable the damage hitbox.

    public void EnableDamageHitbox()
    {
        if (damageHitbox != null)
            damageHitbox.enabled = true;
    }

    public void DisableDamageHitbox()
    {
        if (damageHitbox != null)
            damageHitbox.enabled = false;
    }

    // Returns the damage value for the current attack type.
    public float GetCurrentAttackDamage()
    {
        return currentAttackType switch
        {
            0 => biteDamage,
            1 => clawDamage,
            _ => biteDamage
        };
    }

    private void UpdateTaunting()
    {
        tauntTimer -= Time.deltaTime;

        if (isSittingTaunt)
        {
            // Sitting: just wait for timer
            if (tauntTimer <= 0f)
            {
                TransitionToState(BossState.Idle);
            }
        }
        else
        {
            // Circling: update destination when close to current one
            if (tauntTimer <= 0f)
            {
                TransitionToState(BossState.Idle);
                return;
            }

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.3f)
            {
                SetCircleDestination();
            }
        }
    }

    // Sets the NavMeshAgent's destination to a random point on a circle around the player.
    private void SetCircleDestination()
    {
        if (roomCenter == null) return;

        // Increment angle to walk clockwise around the circle
        currentCircleAngle += 60f; // 60 degrees per waypoint (6 points around the circle)

        Vector3 offset = new Vector3(
            Mathf.Sin(currentCircleAngle * Mathf.Deg2Rad),
            0f,
            Mathf.Cos(currentCircleAngle * Mathf.Deg2Rad)
        ) * circleDistance;

        Vector3 targetPosition = roomCenter.position + offset;

        if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, circleDistance * 2f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    private void UpdateHitReact()
    {
        // Wait for getHit animation to finish, then return to Idle
        if (IsAnimationFinished())
        {
            TransitionToState(BossState.Idle);
        }
    }

    private void UpdateBlocking()
    {
        // Wait for Defend animation to finish, then return to Idle
        if (IsAnimationFinished())
        {
            TransitionToState(BossState.Idle);
        }
    }

    // Checks if the currently playing animation is close to finishing.
    private bool IsAnimationFinished()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.normalizedTime >= 0.95f && !animator.IsInTransition(0);
    }

    private void UpdateDying()
    {
        if (IsAnimationFinished())
        {
            enabled = false; // Stops Update from running
        }
    }

    // ---------- Public Methods (for Player/Damage system to call) ----------

    /// <summary>
    /// Called by the player or damage system when the boss takes damage.
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        // Check if the boss can react (only in these states)
        bool canReact = currentState == BossState.Idle ||
                        currentState == BossState.Repositioning ||
                        currentState == BossState.Taunting;

        if (!canReact)
        {
            // Boss is attacking or already reacting — still take damage but no animation
            ApplyDamage(damage);
            return;
        }

        // Decide reaction: block first, then hit react, else just take it
        if (Random.value <= blockChance)
        {
            TransitionToState(BossState.Blocking);
            ApplyDamage(damage * (1f - blockDamageReduction));
        }
        else if (Random.value <= hitReactChance)
        {
            TransitionToState(BossState.HitReact);
            ApplyDamage(damage);
        }
        else
        {
            // No reaction animation, just take the hit
            ApplyDamage(damage);
        }
    }

    /// <summary>
    /// Reduces health and checks for death.
    /// </summary>
    private void ApplyDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0f && !isDead)
        {
            isDead = true;
            TransitionToState(BossState.Dying);
        }
    }

    /// <summary>
    /// Called by RoomTrigger when the player enters the room.
    /// </summary>
    public void OnPlayerEnterRoom()
    {
        playerInRoom = true;

        // If boss is sleeping, wake up
        if (currentState == BossState.Sleeping)
        {
            TransitionToState(BossState.WakingUp);
        }
    }

    /// <summary>
    /// Called by RoomTrigger when the player leaves the room.
    /// </summary>
    public void OnPlayerExitRoom()
    {
        playerInRoom = false;

        // If boss is awake and player left, go back to sleep
        if (currentState != BossState.Sleeping && currentState != BossState.Dying)
        {
            ReturnToSleep();
        }
    }

    /// <summary>
    /// Puts the boss back to sleep. Interrupts whatever it's doing.
    /// </summary>
    private void ReturnToSleep()
    {
        // Stop moving
        if (agent.enabled)
            agent.ResetPath();

        agent.enabled = false;

        // Reset all animation parameters
        animator.SetBool(IsWalkingParam, false);
        animator.SetBool(IsTauntingParam, false);
        animator.ResetTrigger(OnAttackParam);
        animator.ResetTrigger(OnHitReactParam);
        animator.ResetTrigger(OnBlockParam);

        // Disable damage hitbox if active
        if (damageHitbox != null)
            damageHitbox.gameObject.SetActive(false);

        TransitionToState(BossState.Sleeping);
        currentHealth = maxHealth;
    }

}
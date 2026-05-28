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

    [Header("Sleeping")]
    [SerializeField] private float wakeUpDistance = 13f; // How close the player must be to wake the boss
    
    [Header("WakingUp")]
    [SerializeField] private float screamDuration = 3.33f; // How long the Scream animation lasts
    private float wakeUpTimer;

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
        // Begin in sleeping state
        TransitionToState(BossState.Sleeping);
    }

    private void Update()
    {
        Debug.Log($"Boss state: {currentState}");
        // Route to the logic for the current state
        switch (currentState)
        {
            case BossState.Sleeping:
                UpdateSleeping();
                break;
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
                break;

            case BossState.Taunting:
                agent.enabled = true;
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

            case BossState.Blocking:
                // End damage reduction handled in UpdateBlocking
                break;

            case BossState.Idle:
                // Currently nothing
                break;
        }
    }

    // ---------- State Update Methods ----------

    private void UpdateSleeping()
    {
        // Only wake if player exists and is within range
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= wakeUpDistance)
        {
            TransitionToState(BossState.WakingUp);
        }
    }

    private void EnterWakingUp()
    {
        // Called manually from EnterState
        wakeUpTimer = screamDuration;
    }

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
        // TODO: Wait for animation to finish → TransitionToState(BossState.Idle)
    }

    private void UpdateTaunting()
    {
        // TODO: Circle player or sit (idle02) for duration → TransitionToState(BossState.Idle)
    }

    private void UpdateHitReact()
    {
        // TODO: Wait for getHit animation to finish → TransitionToState(BossState.Idle)
    }

    private void UpdateBlocking()
    {
        // TODO: Wait for Defend animation to finish → TransitionToState(BossState.Idle)
    }

    private void UpdateDying()
    {
        // Nothing — boss is dead, fight ends
    }

    // ---------- Public Methods (for Player/Damage system to call) ----------

    /// <summary>
    /// Called by the player or damage system when the boss takes damage.
    /// </summary>
    public void TakeDamage(float damage)
    {
        // TODO: Reduce health, check for death, trigger hit react or block
    }
}
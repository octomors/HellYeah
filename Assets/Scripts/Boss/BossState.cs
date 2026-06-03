/// <summary>
/// Defines all possible states for the boss dragon.
/// Used by BossController to manage behavior and animations.
/// </summary>
public enum BossState
{
    Sleeping,       // Initial state, plays Sleep animation, waits for player
    WakingUp,       // Plays Scream animation once, transitions to Idle
    Idle,           // Stands idle (idle01), chooses next action
    Repositioning,  // Walks toward player to get into attack range
    Attacking,      // Performing an attack
    Taunting,       // Deliberate window for player — circles or sits (idle02)
    HitReact,       // Plays getHit animation when damaged
    Blocking,       // Plays Defend animation, reduces incoming damage
    Dying           // Plays Die animation, fight ends
}
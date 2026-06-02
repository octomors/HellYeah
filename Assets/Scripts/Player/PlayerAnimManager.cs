using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public FirstPersonMovement movement;
    [SerializeField] private PlayerCombatManager combat;
    
    private Animator anim;
    
    // Animator parameter hashes (быстрее чем строки)
    private static readonly int IsWalking = Animator.StringToHash("IsWalking");
    private static readonly int IsDashing = Animator.StringToHash("IsDashing");
    private static readonly int Hit = Animator.StringToHash("Hit");
    
    void Awake()
    {
        anim = GetComponent<Animator>();
        
        // Авто-поиск ссылок если не назначены
        if (movement == null)
            movement = GetComponentInParent<FirstPersonMovement>();
        if (combat == null)
            combat = GetComponentInParent<PlayerCombatManager>();
        if (movement == null || combat == null)
            Debug.Log("Scripts are null");
    }
    
    void OnEnable()
    {
        // Подписываемся на события атаки (если есть) или будем проверять в Update
        PlayerCombatManager.OnPlayerDeath += HandleDeath;
    }
    
    void OnDisable()
    {
        PlayerCombatManager.OnPlayerDeath -= HandleDeath;
    }
    
    void Update()
    {
        if (movement == null) return;
        
        anim.SetBool(IsWalking, movement.IsWalking);
        anim.SetBool(IsDashing, movement.IsDashing);
    }
    
    // Вызывать этот метод из PlayerCombatManager при атаке
    public void PlayAttack()
    {
        anim.SetTrigger(Hit);
    }
    
    void HandleDeath()
    {
        // Если будет анимация смерти - добавить сюда
        anim.SetBool(IsWalking, false);
        anim.SetBool(IsDashing, false);
    }
}
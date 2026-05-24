using UnityEngine;
using UnityEngine.UI;
using BreadcrumbAi;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Vector3 offset = new Vector3(0, 2.5f, 0); // высота над врагом
    
    private Ai enemy;
    private float maxHealth;
    private Camera mainCamera;
    
    void Start()
    {
        mainCamera = Camera.main;
        
        // Ищем Ai на родительском объекте
        enemy = GetComponentInParent<Ai>();
        
        if (enemy != null)
        {
            maxHealth = enemy.Health; // запоминаем начальное HP как максимальное
        }
        
        if (healthSlider == null)
            healthSlider = GetComponentInChildren<Slider>();
    }
    
    void LateUpdate()
    {
        if (enemy == null || healthSlider == null) return;
        
        // Следуем за врагом
        transform.position = enemy.transform.position + offset;
        
        // Всегда лицом к камере (Billboard)
        if (mainCamera != null)
        {
            transform.forward = mainCamera.transform.forward;
        }
        
        // Обновляем значение слайдера
        float healthPercent = enemy.Health / maxHealth;
        healthSlider.value = Mathf.Clamp01(healthPercent);
        
        // Скрываем если враг мёртв или не атакует врага
        healthSlider.gameObject.SetActive(enemy.lifeState == Ai.LIFE_STATE.IsAlive && enemy.attackState == Ai.ATTACK_STATE.CanAttackPlayer || enemy.moveState == Ai.MOVEMENT_STATE.IsFollowingPlayer);
    }
}
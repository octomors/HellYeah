using TMPro; // Add this at the top
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
[RequireComponent(typeof(CanvasGroup))]
public class BossHPBar : MonoBehaviour
{
    [SerializeField] private BossController boss;
    [SerializeField] private float fadeSpeed = 2f;

    private Slider slider;
    private CanvasGroup canvasGroup;
    private TextMeshProUGUI bossName;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        canvasGroup = GetComponent<CanvasGroup>();
        bossName = GetComponentInChildren<TextMeshProUGUI>();
        canvasGroup.alpha = 0f;
    }

    private void Start()
    {
        if (boss != null)
        {
            slider.maxValue = boss.MaxHealth;
            slider.value = boss.CurrentHealth;
        }
    }

    private void Update()
    {
        if (boss == null) return;

        slider.value = boss.CurrentHealth;

        bool shouldShow = boss.CurrentState != BossState.Sleeping &&
                          boss.CurrentState != BossState.Dying &&
                          boss.CurrentHealth > 0f;

        float targetAlpha = shouldShow ? 1f : 0f;
        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
    }
}
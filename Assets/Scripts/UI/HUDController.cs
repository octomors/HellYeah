using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// HUD: здоровье + заряды рывка
public class HUDController : MonoBehaviour
{
    public static HUDController Instance { get; private set; }

    [Header("Здоровье")]
    public Slider healthSlider;
    public TextMeshProUGUI healthText;

    [Header("Контейнер для стамины")]
    public RectTransform dashContainer;

    [Header("Настройки прямоугольника одного дэша")]
    public float chargeWidth = 50f;
    public float chargeHeight = 14f;
    public float chargeSpacing = 8f;
    public Color chargeFullColor = new Color32(0xFF, 0xD7, 0x00, 0xFF);
    public Color chargeEmptyColor = new Color32(0x30, 0x30, 0x30, 0xFF);
    public Color chargingColor = new Color32(0xFF, 0xD7, 0x00, 0x90);
    public Sprite chargeSprite;

    private float _maxHealth = 100f;
    private float _currentHealth = 100f;
    private int _dashCharges = 0;
    private int _currentCharges = 0;
    private float _dashChargeRecoveryTime = 3f;
    private float _currentDashRecoveryTime = 0f;

    private List<Image> _fills = new List<Image>(); // жёлтые Fill
    private List<Image> _chargingBars = new List<Image>(); // анимация восстановления

    public float MaxHealth
    {
        get => _maxHealth;
        set { _maxHealth = value; RefreshHealth(); }
    }

    public float CurrentHealth
    {
        get => _currentHealth;
        set { _currentHealth = Mathf.Clamp(value, 0, _maxHealth); RefreshHealth(); }
    }

    // При изменении максимального числа зарядов - пересоздаём
    public int DashCharges
    {
        get => _dashCharges;
        set
        {
            if (_dashCharges == value) return;
            _dashCharges = value;
            RebuildChargeUI();
        }
    }

    public int CurrentCharges
    {
        get => _currentCharges;
        set { _currentCharges = Mathf.Clamp(value, 0, _dashCharges); RefreshDash(); }
    }

    public float DashChargeRecoveryTime
    {
        get => _dashChargeRecoveryTime;
        set => _dashChargeRecoveryTime = value;
    }

    public float CurrentDashRecoveryTime
    {
        get => _currentDashRecoveryTime;
        set { _currentDashRecoveryTime = value; RefreshChargeProgress(); }
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        RefreshHealth();
    }

    private void RefreshHealth()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = _maxHealth;
            healthSlider.value = _currentHealth;
        }
        if (healthText != null)
            healthText.text = $"{Mathf.CeilToInt(_currentHealth)} / {Mathf.CeilToInt(_maxHealth)}";
    }

    // Удаляет все прямоугольники и создаёт заново под текущее _dashCharges
    private void RebuildChargeUI()
    {
        if (dashContainer == null) return;

        // Удаляем старые
        foreach (Transform child in dashContainer)
            Destroy(child.gameObject);
        _fills.Clear();
        _chargingBars.Clear();

        // Настраиваем HorizontalLayoutGroup
        var layout = dashContainer.GetComponent<HorizontalLayoutGroup>();
        if (layout == null) layout = dashContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = chargeSpacing;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.childAlignment = TextAnchor.MiddleLeft;

        // Создаём прямоугольник для каждого заряда
        for (int i = 0; i < _dashCharges; i++)
        {
            // Корневой объект
            var chargeGO = new GameObject($"Charge{i + 1}", typeof(RectTransform));
            chargeGO.transform.SetParent(dashContainer, false);
            var chargeRT = chargeGO.GetComponent<RectTransform>();
            chargeRT.sizeDelta = new Vector2(chargeWidth, chargeHeight);

            // Background (тёмный фон)
            var bgGO = new GameObject("Background", typeof(Image));
            bgGO.transform.SetParent(chargeGO.transform, false);
            var bgRT = bgGO.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.sizeDelta = Vector2.zero;
            var bgImg = bgGO.GetComponent<Image>();
            bgImg.sprite = chargeSprite;
            bgImg.color = chargeEmptyColor;
            bgImg.type = Image.Type.Sliced;

            // Fill (жёлтый заряд)
            var fillGO = new GameObject("Fill", typeof(Image));
            fillGO.transform.SetParent(chargeGO.transform, false);
            var fillRT = fillGO.GetComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.sizeDelta = Vector2.zero;
            var fillImg = fillGO.GetComponent<Image>();
            fillImg.sprite = chargeSprite;
            fillImg.color = chargeFullColor;
            fillImg.type = Image.Type.Sliced;
            _fills.Add(fillImg);

            // ChargingBar (анимация восстановления)
            var barGO = new GameObject("ChargingBar", typeof(Image));
            barGO.transform.SetParent(chargeGO.transform, false);
            var barRT = barGO.GetComponent<RectTransform>();
            barRT.anchorMin = Vector2.zero;
            barRT.anchorMax = Vector2.one;
            barRT.sizeDelta = Vector2.zero;
            var barImg = barGO.GetComponent<Image>();
            barImg.sprite = chargeSprite;
            barImg.color = chargingColor;
            barImg.type = Image.Type.Filled;
            barImg.fillMethod = Image.FillMethod.Horizontal;
            barImg.fillOrigin = (int)Image.OriginHorizontal.Left;
            barImg.fillAmount = 0f;
            barGO.SetActive(false);
            _chargingBars.Add(barImg);
        }

        // Обновляем визуал под текущие значения
        _currentCharges = Mathf.Clamp(_currentCharges, 0, _dashCharges);
        RefreshDash();
    }

    // Обновление зарядов
    private void RefreshDash()
    {
        for (int i = 0; i < _fills.Count; i++)
        {
            if (_fills[i] == null) continue;
            // Fill виден только если заряд доступен
            _fills[i].gameObject.SetActive(i < _currentCharges);
        }
        RefreshChargeProgress();
    }

    // Анимация восстановления
    private void RefreshChargeProgress()
    {
        // Скрываем все ChargingBar
        foreach (var bar in _chargingBars)
            if (bar != null) bar.gameObject.SetActive(false);

        // Все заряды полные — ничего не показываем
        if (_currentCharges >= _dashCharges) return;
        if (_currentCharges >= _chargingBars.Count) return;
        if (_chargingBars[_currentCharges] == null) return;

        float progress = _dashChargeRecoveryTime > 0 ? _currentDashRecoveryTime / _dashChargeRecoveryTime : 0f;

        var activeBar = _chargingBars[_currentCharges];
        activeBar.gameObject.SetActive(true);
        activeBar.fillAmount = progress;
    }
}
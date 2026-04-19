using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class IngredientUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text amountText;
    public Button button;

    public Ingredient Ingredient { get; private set; }
    private int currentAmount;
    public event Action<IngredientUI> OnClicked;

    private void Awake()
    {
        if (button != null)
            button.onClick.AddListener(HandleClick);
    }

    private void HandleClick()
    {
        // Вызываем событие клика
        OnClicked?.Invoke(this);
    }

    public void Setup(Ingredient ingredient, int amount)
    {
        Ingredient = ingredient;
        currentAmount = amount;
        
        iconImage.sprite = ingredient.icon;
        nameText.text = ingredient.ingredientName;
        UpdateAmountDisplay();
    }

    public void UpdateAmount(int newAmount)
    {
        currentAmount = newAmount;
        UpdateAmountDisplay();
        
        // Если количество стало 0, можно либо скрыть, либо показать "0"
        if (currentAmount <= 0)
        {
            // Вариант 1: Скрыть ингредиент
            // gameObject.SetActive(false);
            
            // Вариант 2: Показать 0 и сделать неактивным
            button.interactable = false;
        }
    }

    private void UpdateAmountDisplay()
    {
        amountText.text = $"x{currentAmount}";
    }
}
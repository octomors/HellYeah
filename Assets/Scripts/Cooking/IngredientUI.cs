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
        
        if (currentAmount <= 0)
        {
            button.interactable = false;
        }
    }

    private void UpdateAmountDisplay()
    {
        amountText.text = $"x{currentAmount}";
    }
}
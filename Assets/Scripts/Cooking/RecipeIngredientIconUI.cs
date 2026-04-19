using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecipeIngredientIconUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text amountText;
    
    public Ingredient Ingredient { get; private set; }
    public int RequiredAmount { get; private set; }
    
    [Header("Colors")]
    [SerializeField] private Color availableColor = Color.white;
    [SerializeField] private Color unavailableColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    public void Setup(Ingredient ingredient, int amount)
    {
        Ingredient = ingredient;
        RequiredAmount = amount;
        
        Debug.Log($"RecipeIngredientIconUI.Setup called: {ingredient.ingredientName} x{amount}");
        
        if (iconImage != null)
        {
            if (ingredient.icon != null)
            {
                iconImage.sprite = ingredient.icon;
                iconImage.color = availableColor;
                Debug.Log($"Sprite set to: {ingredient.icon.name}");
            }
            else
            {
                Debug.LogError($"Ingredient {ingredient.ingredientName} has no icon assigned!");
            }
        }
        else
        {
            Debug.LogError("iconImage is not assigned in RecipeIngredientIconUI!");
        }
        
        if (amountText != null)
        {
            amountText.text = amount.ToString();
            Debug.Log($"Amount set to: {amount}");
        }
        else
        {
            Debug.LogError("amountText is not assigned in RecipeIngredientIconUI!");
        }
    }
    
    public void SetAvailable(bool available)
    {
        if (iconImage != null)
        {
            iconImage.color = available ? availableColor : unavailableColor;
        }
        
        if (amountText != null)
        {
            amountText.color = available ? Color.white : Color.red;
        }
    }
}
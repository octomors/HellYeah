using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class RecipeUI : MonoBehaviour
{
    [Header("Main Elements")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text buffText;
    [SerializeField] private Button button;
    
    [Header("Ingredients")]
    [SerializeField] private Transform ingredientsContainer;
    [SerializeField] private GameObject ingredientIconPrefab;
    
    public Recipe Recipe { get; private set; }
    public event Action<RecipeUI> OnClicked;
    
    private List<RecipeIngredientIconUI> ingredientIcons = new List<RecipeIngredientIconUI>();

    private void Awake()
    {
        if (button != null)
            button.onClick.AddListener(() => OnClicked?.Invoke(this));
    }

    public void Setup(Recipe recipe)
    {
        Recipe = recipe;
        
        Debug.Log($"Setting up recipe: {recipe.recipeName}");
        Debug.Log($"Ingredients count: {recipe.ingredients.Count}");
        
        if (iconImage != null)
            iconImage.sprite = recipe.recipeIcon;
        if (nameText != null)
            nameText.text = recipe.recipeName;
        if (buffText != null)
            buffText.text = recipe.buffDescription;
        
        CreateIngredientIcons();
    }
    
    private void CreateIngredientIcons()
    {
        // Очищаем старые
        foreach (Transform child in ingredientsContainer)
        {
            Destroy(child.gameObject);
        }
        ingredientIcons.Clear();
        
        if (Recipe == null || Recipe.ingredients == null) 
        {
            Debug.LogError("Recipe or ingredients list is null!");
            return;
        }
        
        // Создаём новые
        foreach (var requirement in Recipe.ingredients)
        {
            if (requirement.ingredient == null)
            {
                Debug.LogError("Ingredient requirement has null ingredient!");
                continue;
            }
            
            Debug.Log($"Creating icon for: {requirement.ingredient.ingredientName} x{requirement.amount}");
            
            GameObject iconObj = Instantiate(ingredientIconPrefab, ingredientsContainer);
            RecipeIngredientIconUI iconUI = iconObj.GetComponent<RecipeIngredientIconUI>();
            
            if (iconUI != null)
            {
                iconUI.Setup(requirement.ingredient, requirement.amount);
                ingredientIcons.Add(iconUI);
            }
            else
            {
                Debug.LogError("RecipeIngredientIconUI component not found on prefab!");
            }
        }
        
        UpdateIngredientsAvailability();
    }
    
    public void UpdateIngredientsAvailability()
    {
        if (InventoryManager.Instance == null) return;
        
        foreach (var icon in ingredientIcons)
        {
            if (icon != null && icon.Ingredient != null)
            {
                bool hasEnough = InventoryManager.Instance.HasIngredient(
                    icon.Ingredient, 
                    icon.RequiredAmount
                );
                icon.SetAvailable(hasEnough);
            }
        }
    }
}
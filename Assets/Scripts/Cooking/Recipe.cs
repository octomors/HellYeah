using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewRecipe", menuName = "Cooking/Recipe")]
public class Recipe : ScriptableObject
{
    public string recipeName;
    public Sprite recipeIcon;
    [TextArea(3, 5)]
    public string description;
    public List<IngredientRequirement> ingredients;
    [TextArea(2, 4)]
    public string buffDescription;
    public CookingResult cookingResult;
}

[System.Serializable]
public class IngredientRequirement
{
    public Ingredient ingredient;
    public int amount;
}
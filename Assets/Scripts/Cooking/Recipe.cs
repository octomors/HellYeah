using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewRecipe", menuName = "Cooking/Recipe")]
public class Recipe : ScriptableObject
{
    public string recipeName;
    public Sprite recipeIcon;
    [TextArea(3, 5)]
    public string description;
    public List<IngredientRequirement> ingredients; // Список ингредиентов
    [TextArea(2, 4)]
    public string buffDescription; // Описание баффа
    public CookingResult cookingResult; // Ссылка на результат приготовления
}

[System.Serializable]
public class IngredientRequirement
{
    public Ingredient ingredient;
    public int amount;
}
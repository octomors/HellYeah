using UnityEngine;

[CreateAssetMenu(fileName = "NewIngredient", menuName = "Cooking/Ingredient")]
public class Ingredient : ScriptableObject
{
    public string ingredientName; // Название, например "Крысиный хвост"
    public Sprite icon;           // Иконка для UI
    public int initialAmount;     // Стартовое количество
    public bool isRare;           // Редкий ли ингредиент
}
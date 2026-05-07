using UnityEngine;

[CreateAssetMenu(fileName = "NewIngredient", menuName = "Cooking/Ingredient")]
public class Ingredient : ScriptableObject
{
    public string ingredientName;
    public Sprite icon;
    [TextArea(2, 4)]
    public string description;
    public ItemType itemType = ItemType.Ingredient;

    [Tooltip("Можно ли хранить несколько штук в одном слоте")]
    public bool stackable = true;

    [Tooltip("Максимальное количество в одном слоте")]
    public int maxStack = 99;
}

public enum ItemType
{
    Ingredient, //ингредиент для готовки
    Misc //прочее, если будем добавлять что-то еще - вписать сюда тип
}
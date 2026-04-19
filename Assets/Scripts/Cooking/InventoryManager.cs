using UnityEngine;
using System.Collections.Generic;
using System;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance; // Синглтон для доступа из любого места
    public Dictionary<Ingredient, int> ingredients = new Dictionary<Ingredient, int>();
    public event Action OnInventoryChanged; // Событие для обновления UI

    [Header("Test Settings")]
    [SerializeField] private bool addTestIngredients = true;
    [SerializeField] private List<Ingredient> testIngredients = new List<Ingredient>();
    [SerializeField] private int testAmount = 10;

    private void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else 
        {
            Destroy(gameObject);
            return;
        }
        
        // Добавляем тестовые ингредиенты
        if (addTestIngredients)
        {
            AddTestIngredients();
        }
    }
    
    private void AddTestIngredients()
    {
        // Способ 1: Загрузить ВСЕ ингредиенты из папки Resources
        Ingredient[] allIngredients = Resources.LoadAll<Ingredient>("Ingredients");
        
        foreach (Ingredient ing in allIngredients)
        {
            ingredients[ing] = testAmount;
            Debug.Log($"Добавлен тестовый ингредиент: {ing.ingredientName} x{testAmount}");
        }
        
        /* Способ 2: Использовать список из инспектора
        foreach (Ingredient ing in testIngredients)
        {
            if (ing != null)
            {
                ingredients[ing] = testAmount;
                Debug.Log($"Добавлен ингредиент из списка: {ing.ingredientName} x{testAmount}");
            }
        }
        */
        
        Debug.Log($"Всего добавлено {ingredients.Count} ингредиентов");
        
        // Вызываем событие обновления
        OnInventoryChanged?.Invoke();
    }

    public void AddIngredient(Ingredient ingredient, int amount)
    {
        if (ingredients.ContainsKey(ingredient))
            ingredients[ingredient] += amount;
        else
            ingredients.Add(ingredient, amount);
            
        OnInventoryChanged?.Invoke(); // Оповещаем UI об изменении
    }

    public bool RemoveIngredient(Ingredient ingredient, int amount)
    {
        if (!ingredients.ContainsKey(ingredient) || ingredients[ingredient] < amount)
            return false;
            
        ingredients[ingredient] -= amount;
        
        // Вызываем событие обновления
        OnInventoryChanged?.Invoke();
        return true;
    }

    public bool HasIngredient(Ingredient ingredient, int amount)
    {
        if (ingredients.ContainsKey(ingredient))
        {
            return ingredients[ingredient] >= amount;
        }
        return false;
    }
}
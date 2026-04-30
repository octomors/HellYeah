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
        
        if (addTestIngredients)
        {
            AddTestIngredients();
        }
    }
    
    private void AddTestIngredients()
    {
        // Загружаем все ингредиенты из папки Resources
        Ingredient[] allIngredients = Resources.LoadAll<Ingredient>("Ingredients");
        
        foreach (Ingredient ing in allIngredients)
        {
            ingredients[ing] = testAmount;
        }
        
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
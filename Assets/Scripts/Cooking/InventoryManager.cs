using UnityEngine;
using System.Collections.Generic;
using System;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance; // Синглтон для доступа из любого места
    public Dictionary<Ingredient, int> ingredients = new Dictionary<Ingredient, int>();
    public event Action OnInventoryChanged; // Событие для обновления UI
    public IReadOnlyDictionary<Ingredient, int> Ingredients => ingredients;

    private void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SaveService.Load(null, null, this);
        }
        else 
        {
            Destroy(gameObject);
            return;
        }
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

    public void ClearInventory()
    {
        ingredients.Clear();
        OnInventoryChanged?.Invoke();
    }

    public void ReplaceInventory(Dictionary<Ingredient, int> newInventory)
    {
        ingredients.Clear();
        if (newInventory != null)
        {
            foreach (KeyValuePair<Ingredient, int> entry in newInventory)
            {
                if (entry.Key == null || entry.Value <= 0) continue;
                ingredients[entry.Key] = entry.Value;
            }
        }

        OnInventoryChanged?.Invoke();
    }
}
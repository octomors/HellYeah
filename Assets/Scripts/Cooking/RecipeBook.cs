using System;
using System.Collections.Generic;
using UnityEngine;

public class RecipeBook : MonoBehaviour
{
    public static RecipeBook Instance { get; private set; }

    private readonly HashSet<Recipe> knownRecipes = new HashSet<Recipe>();

    public IReadOnlyCollection<Recipe> KnownRecipes => knownRecipes;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public bool IsKnown(Recipe recipe)
    {
        if (recipe == null) return false;
        return knownRecipes.Contains(recipe);
    }

    public bool AddRecipe(Recipe recipe)
    {
        if (recipe == null) return false;
        return knownRecipes.Add(recipe);
    }

    public bool RemoveRecipe(Recipe recipe)
    {
        if (recipe == null) return false;
        return knownRecipes.Remove(recipe);
    }

    [Serializable]
    private class RecipeBookData
    {
        public List<string> recipeNames = new List<string>();
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

public class RecipeBook : MonoBehaviour
{
    public static RecipeBook Instance { get; private set; }

    private readonly HashSet<Recipe> knownRecipes = new HashSet<Recipe>();
    [SerializeField] private List<Recipe> startingRecipes = new List<Recipe>();

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

    private void Start()
    {
        if (startingRecipes == null || startingRecipes.Count == 0)
            return;

        foreach (Recipe recipe in startingRecipes)
        {
            if (recipe == null || IsKnown(recipe)) continue;
            knownRecipes.Add(recipe);
        }
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

    public void ClearKnownRecipes()
    {
        knownRecipes.Clear();
    }

    public void ReplaceKnownRecipes(IEnumerable<Recipe> recipes)
    {
        knownRecipes.Clear();
        if (recipes == null) return;

        foreach (Recipe recipe in recipes)
        {
            if (recipe == null) continue;
            knownRecipes.Add(recipe);
        }
    }

    [Serializable]
    private class RecipeBookData
    {
        public List<string> recipeNames = new List<string>();
    }
}

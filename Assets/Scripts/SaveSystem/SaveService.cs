using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SaveService
{
    public static void Load(BasePlayerStats playerStats, RecipeBook recipeBook)
    {
        SavePaths.EnsureSaveDirectory();

        if (playerStats != null)
        {
            if (!TryLoadStats(playerStats))
            {
                SaveStatsData(playerStats);
            }
        }

        if (recipeBook != null)
        {
            if (TryLoadRecipeBook(out RecipeBookData recipeData))
            {
                ApplyRecipeData(recipeBook, recipeData);
            }
            else
            {
                SaveRecipeBookData(BuildRecipeBookData(recipeBook));
            }
        }
    }

    public static void Save(BasePlayerStats playerStats, RecipeBook recipeBook)
    {
        SavePaths.EnsureSaveDirectory();

        if (playerStats != null)
        {
            SaveStatsData(playerStats);
        }

        if (recipeBook != null)
        {
            SaveRecipeBookData(BuildRecipeBookData(recipeBook));
        }
    }

    public static void ResetFiles()
    {
        SavePaths.EnsureSaveDirectory();

        if (File.Exists(SavePaths.StatsFilePath))
        {
            File.Delete(SavePaths.StatsFilePath);
        }

        if (File.Exists(SavePaths.RecipeBookFilePath))
        {
            File.Delete(SavePaths.RecipeBookFilePath);
        }
    }


    private static RecipeBookData BuildRecipeBookData(RecipeBook recipeBook)
    {
        RecipeBookData data = new RecipeBookData();
        foreach (Recipe recipe in recipeBook.KnownRecipes)
        {
            if (recipe == null) continue;
            string recipeId = string.IsNullOrWhiteSpace(recipe.recipeName) ? recipe.name : recipe.recipeName;
            if (!string.IsNullOrWhiteSpace(recipeId))
            {
                data.recipeNames.Add(recipeId);
            }
        }

        return data;
    }

    private static void ApplyRecipeData(RecipeBook recipeBook, RecipeBookData data)
    {
        Dictionary<string, Recipe> lookup = BuildRecipeLookup();
        if (lookup.Count == 0)
        {
            return;
        }

        List<Recipe> resolvedRecipes = new List<Recipe>();

        if (data != null && data.recipeNames != null)
        {
            foreach (string recipeName in data.recipeNames)
            {
                if (string.IsNullOrWhiteSpace(recipeName)) continue;
                if (lookup.TryGetValue(recipeName, out Recipe recipe))
                {
                    resolvedRecipes.Add(recipe);
                }
            }
        }

        recipeBook.ReplaceKnownRecipes(resolvedRecipes);
    }

    private static Dictionary<string, Recipe> BuildRecipeLookup()
    {
        Dictionary<string, Recipe> lookup = new Dictionary<string, Recipe>(StringComparer.Ordinal);
        Recipe[] recipes = Resources.LoadAll<Recipe>(string.Empty);

        foreach (Recipe recipe in recipes)
        {
            if (recipe == null) continue;

            if (!string.IsNullOrWhiteSpace(recipe.recipeName))
            {
                lookup[recipe.recipeName] = recipe;
            }

            if (!string.IsNullOrWhiteSpace(recipe.name))
            {
                lookup[recipe.name] = recipe;
            }
        }

        return lookup;
    }

    private static bool TryLoadStats(BasePlayerStats playerStats)
    {
        if (!File.Exists(SavePaths.StatsFilePath)) return false;

        try
        {
            string json = File.ReadAllText(SavePaths.StatsFilePath);
            if (string.IsNullOrWhiteSpace(json)) return false;

            JsonUtility.FromJsonOverwrite(json, playerStats);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Failed to read save file {SavePaths.StatsFilePath}: {ex.Message}");
            return false;
        }
    }

    private static bool TryLoadRecipeBook(out RecipeBookData data)
    {
        data = null;
        if (!File.Exists(SavePaths.RecipeBookFilePath)) return false;

        try
        {
            string json = File.ReadAllText(SavePaths.RecipeBookFilePath);
            if (string.IsNullOrWhiteSpace(json)) return false;

            data = JsonUtility.FromJson<RecipeBookData>(json);
            return data != null;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Failed to read save file {SavePaths.RecipeBookFilePath}: {ex.Message}");
            return false;
        }
    }

    private static void SaveStatsData(BasePlayerStats playerStats)
    {
        SaveJson(SavePaths.StatsFilePath, playerStats);
    }

    private static void SaveRecipeBookData(RecipeBookData data)
    {
        SaveJson(SavePaths.RecipeBookFilePath, data);
    }

    private static void SaveJson<T>(string filePath, T data)
    {
        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Failed to write save file {filePath}: {ex.Message}");
        }
    }
}

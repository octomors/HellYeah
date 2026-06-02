using System.Collections.Generic;
using UnityEngine;

public class RecipePickupInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private List<Recipe> possibleRecipes = new List<Recipe>();
    [Range(0f, 100f)]
    [SerializeField] private float spawnChancePercent = 10f;

    private Recipe chosen;

    private void Awake()
    {
        float roll = Random.Range(0f, 100f);
        if (roll > spawnChancePercent)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (possibleRecipes == null || possibleRecipes.Count == 0)
        {
            RecipeBook recipeBook = RecipeBook.Instance;
            if (recipeBook != null)
            {
                Recipe[] allRecipes = Resources.LoadAll<Recipe>("Recipes");
                possibleRecipes = new List<Recipe>();
                for (int i = 0; i < allRecipes.Length; i++)
                {
                    Recipe recipe = allRecipes[i];
                    if (recipe == null) continue;
                    if (!recipeBook.IsKnown(recipe))
                        possibleRecipes.Add(recipe);
                }
            }
        }

        if (possibleRecipes != null && possibleRecipes.Count > 0)
            chosen = possibleRecipes[Random.Range(0, possibleRecipes.Count)];

        if (chosen == null)
            Destroy(gameObject);
    }


    public void Interact()
    {
        RecipeBook recipeBook = RecipeBook.Instance;
        if (recipeBook == null)
        {
            Debug.LogWarning("[RecipePickup] RecipeBook not found in scene.");
            return;
        }

        bool added = recipeBook.AddRecipe(chosen);
        if (added)
            Debug.Log($"[RecipePickup] Learned recipe '{chosen.recipeName}'.");
        else
            Debug.Log($"[RecipePickup] Recipe '{chosen.recipeName}' already known.");

        Destroy(gameObject);
    }

    public string GetInteractText()
    {
        if (chosen == null)
            return "Изучить рецепт [E]";

        return $"Изучить рецепт {chosen.recipeName} [E]";
    }

    public Outline GetOutline()
    {
        return null;
    }

    public Transform GetTransform()
    {
        return transform;
    }
}

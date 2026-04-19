using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class CookingUIManager : MonoBehaviour
{
    [Header("References")]
    public GameObject ingredientPrefab;
    public GameObject recipePrefab;
    public Transform ingredientsContent;
    public Transform recipesContent;
    public TMP_Text potContentsText;
    public Button cookButton;
    public RectTransform cauldronImage;
    public CanvasGroup cookingScreenCanvasGroup;
    
    [Header("Cooking Results")]
    [SerializeField] private CookingResult suspiciousDishResult;
    
    [Header("Result Panel")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private CanvasGroup resultCanvasGroup;
    [SerializeField] private Image resultDishIcon;
    [SerializeField] private TMP_Text resultDishName;
    [SerializeField] private TMP_Text resultBuffsText;
    [SerializeField] private Button resultCloseButton;
    
    [Header("Recipe Details Panel")]
    [SerializeField] private GameObject recipeDetailsPanel;
    [SerializeField] private CanvasGroup recipeDetailsCanvasGroup;
    [SerializeField] private Image detailsRecipeIcon;
    [SerializeField] private TMP_Text detailsRecipeName;
    [SerializeField] private TMP_Text detailsDescriptionText;
    [SerializeField] private TMP_Text detailsBuffsText;
    [SerializeField] private Transform detailsIngredientsContainer;
    [SerializeField] private GameObject detailsIngredientIconPrefab;
    [SerializeField] private Button detailsCookButton;
    [SerializeField] private Button detailsCloseButton;
    [SerializeField] private Button detailsBackgroundButton;
    
    [Header("Pot Controls")]
    [SerializeField] private Button clearPotButton;

    [Header("Animation")]
    [SerializeField] private GameObject flyingIngredientPrefab; // префаб FlyingIngredient
    [SerializeField] private Transform flyingObjectsParent;     // родитель для летящих объектов (обычно Canvas)
    [SerializeField] private RectTransform cauldronTarget;      // точка назначения — центр котла
    
    [Header("Right Panel UI Elements")]
    [SerializeField] private CanvasGroup cookButtonCanvasGroup;
    [SerializeField] private CanvasGroup potContentsCanvasGroup;
    [SerializeField] private CanvasGroup clearPotButtonCanvasGroup;

    [Header("Cooking Animation")]
    [SerializeField] private GameObject fireObject;
    [SerializeField] private CanvasGroup fireCanvasGroup;
    [SerializeField] private GameObject steamObject;
    [SerializeField] private CanvasGroup steamCanvasGroup;
    [SerializeField] private float animationDuration = 3f;    // Длительность приготовления
    [SerializeField] private CanvasGroup leftPanelCanvasGroup;

    private Dictionary<Ingredient, int> currentPotIngredients = new Dictionary<Ingredient, int>();
    private Dictionary<Ingredient, IngredientUI> ingredientUIMap = new Dictionary<Ingredient, IngredientUI>();
    private List<RecipeUI> recipeUIs = new List<RecipeUI>();
    private Recipe currentDisplayedRecipe;
    private List<RecipeIngredientIconUI> detailsIngredientIcons = new List<RecipeIngredientIconUI>();

    private void Start()
    {
        UpdateIngredientUI();
        PopulateRecipeBook();
        
        cookButton.onClick.AddListener(OnCookButtonClicked);
        
        if (clearPotButton != null)
            clearPotButton.onClick.AddListener(ClearPot);
        
        if (resultCloseButton != null)
            resultCloseButton.onClick.AddListener(HideResultPanel);
            
        if (detailsCloseButton != null)
            detailsCloseButton.onClick.AddListener(HideRecipeDetails);
        if (detailsBackgroundButton != null)
            detailsBackgroundButton.onClick.AddListener(HideRecipeDetails);
        if (detailsCookButton != null)
            detailsCookButton.onClick.AddListener(OnCookFromDetails);
        
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged += UpdateIngredientUI;
            InventoryManager.Instance.OnInventoryChanged += UpdateAllRecipesAvailability;
        }
        
        if (resultPanel != null)
            resultPanel.SetActive(false);
        if (recipeDetailsPanel != null)
            recipeDetailsPanel.SetActive(false);
    }

    // ========== ИНГРЕДИЕНТЫ ==========
    
    public void UpdateIngredientUI()
    {
        foreach (Transform child in ingredientsContent) 
            Destroy(child.gameObject);
        ingredientUIMap.Clear();

        foreach (var item in InventoryManager.Instance.ingredients)
        {
            if (item.Value <= 0) continue;
            
            GameObject obj = Instantiate(ingredientPrefab, ingredientsContent);
            IngredientUI ui = obj.GetComponent<IngredientUI>();
            ui.Setup(item.Key, item.Value);
            
            ingredientUIMap[item.Key] = ui;
            ui.OnClicked += (clickedUI) => AddIngredientToPot(clickedUI.Ingredient);
        }
    }

    public void AddIngredientToPot(Ingredient ingredient)
    {
        if (!InventoryManager.Instance.ingredients.ContainsKey(ingredient) || 
            InventoryManager.Instance.ingredients[ingredient] <= 0)
        {
            Debug.Log("Нет такого ингредиента!");
            return;
        }

        // Получаем позицию UI-элемента, по которому кликнули
        IngredientUI clickedUI = ingredientUIMap[ingredient];
        RectTransform clickedRect = clickedUI.GetComponent<RectTransform>();
        Vector3 startPos = clickedRect.position;
        Vector3 endPos = cauldronTarget.position;

        // Удаляем из инвентаря
        if (!InventoryManager.Instance.RemoveIngredient(ingredient, 1)) 
            return;

        // Обновляем UI ингредиента (количество)
        int newAmount = InventoryManager.Instance.ingredients[ingredient];
        clickedUI.UpdateAmount(newAmount);
        if (newAmount <= 0)
            ingredientUIMap.Remove(ingredient);

        // Запускаем анимацию полёта
        GameObject flyingObj = Instantiate(flyingIngredientPrefab, flyingObjectsParent);
        FlyingIngredient flying = flyingObj.GetComponent<FlyingIngredient>();
        
        flying.Setup(ingredient.icon, startPos, endPos, () =>
        {
            // По завершении полёта добавляем ингредиент в котёл
            if (currentPotIngredients.ContainsKey(ingredient))
                currentPotIngredients[ingredient]++;
            else
                currentPotIngredients.Add(ingredient, 1);
            
            UpdatePotContentsText();
        });
    }

    void UpdatePotContentsText()
    {
        if (currentPotIngredients.Count == 0)
        {
            potContentsText.text = "Котёл пуст";
            return;
        }
        
        string text = "В котле:\n";
        foreach (var item in currentPotIngredients)
        {
            text += $"{item.Key.ingredientName} x{item.Value}\n";
        }
        potContentsText.text = text;
    }

    // Метод для кнопки "Очистить котёл" — возвращает ингредиенты
    public void ClearPot()
    {
        // Возвращаем ингредиенты в инвентарь
        foreach (var item in currentPotIngredients)
        {
            InventoryManager.Instance.AddIngredient(item.Key, item.Value);
        }
        
        currentPotIngredients.Clear();
        UpdatePotContentsText();
        Debug.Log("Котёл очищен, ингредиенты возвращены в инвентарь");
    }

    // Метод для готовки — ингредиенты расходуются
    private void ConsumePotIngredients()
    {
        // Просто очищаем котёл, ингредиенты уже удалены из инвентаря при добавлении
        currentPotIngredients.Clear();
        UpdatePotContentsText();
        Debug.Log("Ингредиенты израсходованы на приготовление");
    }

    // ========== РЕЦЕПТЫ ==========
    
    void PopulateRecipeBook()
    {
        foreach (Transform child in recipesContent) 
            Destroy(child.gameObject);
        recipeUIs.Clear();
        
        Recipe[] recipes = Resources.LoadAll<Recipe>("Recipes");
        foreach (Recipe recipe in recipes)
        {
            GameObject obj = Instantiate(recipePrefab, recipesContent);
            RecipeUI ui = obj.GetComponent<RecipeUI>();
            ui.Setup(recipe);
            ui.OnClicked += (clickedUI) => ShowRecipeDetails(clickedUI.Recipe);
            
            recipeUIs.Add(ui);
        }
    }

    private void ShowRecipeDetails(Recipe recipe)
    {
        if (recipeDetailsPanel == null) return;
        
        currentDisplayedRecipe = recipe;
        
        if (detailsRecipeIcon != null && recipe.recipeIcon != null)
            detailsRecipeIcon.sprite = recipe.recipeIcon;
        
        if (detailsRecipeName != null)
            detailsRecipeName.text = recipe.recipeName;
        
        if (detailsDescriptionText != null)
            detailsDescriptionText.text = recipe.description;
        
        if (detailsBuffsText != null)
            detailsBuffsText.text = recipe.buffDescription;
        
        CreateDetailsIngredientIcons(recipe);
        UpdateDetailsCookButton();
        
        recipeDetailsPanel.SetActive(true);
        
        if (recipeDetailsCanvasGroup != null)
        {
            recipeDetailsCanvasGroup.alpha = 0f;
            recipeDetailsCanvasGroup.DOFade(1f, 0.3f).SetUpdate(true);
        }
        
        if (cookingScreenCanvasGroup != null)
            cookingScreenCanvasGroup.DOFade(0.5f, 0.2f);
    }

    private void CreateDetailsIngredientIcons(Recipe recipe)
    {
        foreach (Transform child in detailsIngredientsContainer)
            Destroy(child.gameObject);
        detailsIngredientIcons.Clear();
        
        if (recipe == null) return;
        
        foreach (var requirement in recipe.ingredients)
        {
            GameObject iconObj = Instantiate(detailsIngredientIconPrefab, detailsIngredientsContainer);
            RecipeIngredientIconUI iconUI = iconObj.GetComponent<RecipeIngredientIconUI>();
            
            if (iconUI != null)
            {
                iconUI.Setup(requirement.ingredient, requirement.amount);
                detailsIngredientIcons.Add(iconUI);
            }
        }
        
        UpdateDetailsIngredientsAvailability();
    }

    private void UpdateDetailsIngredientsAvailability()
    {
        if (InventoryManager.Instance == null) return;
        
        foreach (var icon in detailsIngredientIcons)
        {
            if (icon != null && icon.Ingredient != null)
            {
                bool hasEnough = InventoryManager.Instance.HasIngredient(icon.Ingredient, icon.RequiredAmount);
                icon.SetAvailable(hasEnough);
            }
        }
    }

    private void UpdateDetailsCookButton()
    {
        if (detailsCookButton == null || currentDisplayedRecipe == null) return;
        
        bool canCook = CanCookRecipe(currentDisplayedRecipe);
        detailsCookButton.interactable = canCook;
        
        TMP_Text buttonText = detailsCookButton.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
            buttonText.text = canCook ? "Добавить ингредиенты в котёл" : "Не хватает ингредиентов";
    }

    private bool CanCookRecipe(Recipe recipe)
    {
        if (InventoryManager.Instance == null) return false;
        
        foreach (var req in recipe.ingredients)
        {
            if (!InventoryManager.Instance.HasIngredient(req.ingredient, req.amount))
                return false;
        }
        return true;
    }

    private void HideRecipeDetails()
    {
        if (recipeDetailsCanvasGroup != null)
        {
            recipeDetailsCanvasGroup.DOFade(0f, 0.2f).OnComplete(() =>
            {
                recipeDetailsPanel.SetActive(false);
            });
        }
        else
        {
            recipeDetailsPanel.SetActive(false);
        }
        
        if (cookingScreenCanvasGroup != null)
            cookingScreenCanvasGroup.DOFade(1f, 0.2f);
    }

    private void OnCookFromDetails()
    {
        if (currentDisplayedRecipe == null) return;
        
        if (!CanCookRecipe(currentDisplayedRecipe))
        {
            Debug.Log("Не хватает ингредиентов!");
            return;
        }
        
        AddAllIngredientsToPot(currentDisplayedRecipe);
        HideRecipeDetails();
    }

    private void AddAllIngredientsToPot(Recipe recipe)
{
    foreach (var req in recipe.ingredients)
    {
        for (int i = 0; i < req.amount; i++)
        {
            // Мгновенно добавляем в котёл (без анимации)
            if (InventoryManager.Instance.HasIngredient(req.ingredient, 1))
            {
                InventoryManager.Instance.RemoveIngredient(req.ingredient, 1);
                if (currentPotIngredients.ContainsKey(req.ingredient))
                    currentPotIngredients[req.ingredient]++;
                else
                    currentPotIngredients.Add(req.ingredient, 1);
            }
        }
    }
    UpdateIngredientUI(); // обновим UI ингредиентов
    UpdatePotContentsText();
}

    private void UpdateAllRecipesAvailability()
    {
        foreach (var recipeUI in recipeUIs)
        {
            recipeUI.UpdateIngredientsAvailability();
        }
        
        if (recipeDetailsPanel != null && recipeDetailsPanel.activeSelf)
        {
            UpdateDetailsIngredientsAvailability();
            UpdateDetailsCookButton();
        }
    }

    // ========== ГОТОВКА ==========
    
    private Recipe CheckExactRecipe()
    {
        Recipe[] recipes = Resources.LoadAll<Recipe>("Recipes");
        
        foreach (Recipe recipe in recipes)
        {
            if (currentPotIngredients.Count != recipe.ingredients.Count)
                continue;
            
            bool exactMatch = true;
            
            foreach (var req in recipe.ingredients)
            {
                if (!currentPotIngredients.ContainsKey(req.ingredient))
                {
                    exactMatch = false;
                    break;
                }
                
                if (currentPotIngredients[req.ingredient] != req.amount)
                {
                    exactMatch = false;
                    break;
                }
            }
            
            if (exactMatch)
            {
                Debug.Log($"Найден точный рецепт: {recipe.recipeName}");
                return recipe;
            }
        }
        
        Debug.Log("Точный рецепт не найден");
        return null;
    }

    void OnCookButtonClicked()
    {
        if (currentPotIngredients.Count == 0)
        {
            Debug.Log("Котёл пуст! Добавьте ингредиенты.");
            return;
        }
        
        Recipe exactRecipe = CheckExactRecipe();
        
        if (exactRecipe != null)
        {
            StartCoroutine(CookSequenceFromRecipe(exactRecipe));
        }
        else
        {
            StartCoroutine(CookSequenceFromResult(suspiciousDishResult));
        }
    }

    private IEnumerator CookSequenceFromRecipe(Recipe recipe)
    {
        yield return StartCoroutine(PlayCookingAnimation());
        ConsumePotIngredients();
        ShowResultPanelFromRecipe(recipe);
    }

    private IEnumerator CookSequenceFromResult(CookingResult result)
    {
        yield return StartCoroutine(PlayCookingAnimation());
        ConsumePotIngredients();
        ShowResultPanelFromResult(result);
    }

    private IEnumerator PlayCookingAnimation()
    {
        if (leftPanelCanvasGroup != null)
            leftPanelCanvasGroup.interactable = false; 

        cookButton.interactable = false;
        
        // 1. Включаем объекты огня и пара
        fireObject.SetActive(true);
        steamObject.SetActive(true);
        fireCanvasGroup.alpha = 0f;
        steamCanvasGroup.alpha = 0f;
        
        // 2. Создаём Sequence
        Sequence seq = DOTween.Sequence();
        
        // Скрываем UI элементы в правой панели (кнопка, текст)
        seq.Join(cookButtonCanvasGroup.DOFade(0f, 0.3f));
        seq.Join(potContentsCanvasGroup.DOFade(0f, 0.3f));
        seq.Join(clearPotButtonCanvasGroup.DOFade(0f, 0.3f));
        // Если есть clearPotButton, то же самое
        
        // Появление огня и пара
        seq.Join(fireCanvasGroup.DOFade(1f, 0.4f).SetDelay(0.1f));
        seq.Join(fireObject.transform.DOScale(1.2f, 0.5f).From(0.8f).SetEase(Ease.OutBack));
        
        seq.Join(steamCanvasGroup.DOFade(1f, 0.5f).SetDelay(0.2f));
        seq.Join(steamObject.transform.DOScale(1.1f, 0.6f).From(0.9f).SetEase(Ease.OutSine));
        seq.Join(steamObject.transform.DOBlendableMoveBy(Vector3.up * 20f, 0.6f).SetLoops(-1, LoopType.Yoyo));
        
        // Тряска котла
        if (cauldronImage != null)
            seq.Join(cauldronImage.DOShakeAnchorPos(3f, 5f, 20, 90f, false, true));
        
        // Ждём 3 секунды (вся анимация длится animationDuration)
        yield return new WaitForSeconds(animationDuration);
        
        // 3. Завершение: убираем огонь и пар
        seq = DOTween.Sequence();
        
        seq.Join(fireCanvasGroup.DOFade(0f, 0.3f));
        seq.Join(steamCanvasGroup.DOFade(0f, 0.3f));
        
        // Возвращаем UI элементы
        seq.Join(cookButtonCanvasGroup.DOFade(1f, 0.3f));
        seq.Join(potContentsCanvasGroup.DOFade(1f, 0.3f));
        seq.Join(clearPotButtonCanvasGroup.DOFade(1f, 0.3f));
        
        // По окончании выключаем объекты
        seq.OnComplete(() =>
        {
            fireObject.SetActive(false);
            steamObject.SetActive(false);
            steamObject.transform.DOKill();
        });
        
        yield return seq.WaitForCompletion();
        
        cookButton.interactable = true;
        leftPanelCanvasGroup.interactable = true;
    }

    private void ShowResultPanelFromRecipe(Recipe recipe)
    {
        if (resultPanel == null) return;
        
        if (resultDishIcon != null && recipe.recipeIcon != null)
            resultDishIcon.sprite = recipe.recipeIcon;
        
        if (resultDishName != null)
        {
            resultDishName.text = recipe.recipeName;
            resultDishName.color = Color.white;
        }
        
        if (resultBuffsText != null)
        {
            resultBuffsText.text = recipe.buffDescription;
            resultBuffsText.color = Color.green;
        }
        
        resultPanel.SetActive(true);
        
        if (resultCanvasGroup != null)
        {
            resultCanvasGroup.alpha = 0f;
            resultCanvasGroup.DOFade(1f, 0.5f).SetUpdate(true);
        }
        
        if (cookingScreenCanvasGroup != null)
            cookingScreenCanvasGroup.DOFade(0.5f, 0.3f);
    }

    private void ShowResultPanelFromResult(CookingResult result)
    {
        if (resultPanel == null)
        {
            Debug.LogError("resultPanel не назначен в инспекторе!");
            return;
        }
        
        if (result == null)
        {
            Debug.LogError("CookingResult is null!");
            return;
        }
        
        // Заполняем иконку
        if (resultDishIcon != null)
        {
            if (result.resultIcon != null)
            {
                resultDishIcon.sprite = result.resultIcon;
            }
            else
            {
                Debug.LogWarning("У CookingResult нет иконки! Используется заглушка.");
                resultDishIcon.color = Color.gray;
            }
        }
        else
        {
            Debug.LogError("resultDishIcon не назначен в инспекторе!");
        }
        
        // Заполняем название
        if (resultDishName != null)
        {
            resultDishName.text = !string.IsNullOrEmpty(result.resultName) ? result.resultName : "Неизвестное блюдо";
            resultDishName.color = result.isSuccess ? Color.white : Color.red;
        }
        else
        {
            Debug.LogError("resultDishName не назначен в инспекторе!");
        }
        
        // Заполняем описание баффов
        if (resultBuffsText != null)
        {
            resultBuffsText.text = !string.IsNullOrEmpty(result.buffDescription) ? result.buffDescription : "Нет эффектов";
            resultBuffsText.color = result.isSuccess ? Color.green : Color.grey;
        }
        else
        {
            Debug.LogError("resultBuffsText не назначен в инспекторе!");
        }
        
        // Показываем панель
        resultPanel.SetActive(true);
        
        if (resultCanvasGroup != null)
        {
            resultCanvasGroup.alpha = 0f;
            resultCanvasGroup.DOFade(1f, 0.5f).SetUpdate(true);
        }
        
        if (cookingScreenCanvasGroup != null)
        {
            cookingScreenCanvasGroup.DOFade(0.5f, 0.3f);
        }
    }

    private void HideResultPanel()
    {
        if (resultCanvasGroup != null)
        {
            resultCanvasGroup.DOFade(0f, 0.3f).OnComplete(() =>
            {
                resultPanel.SetActive(false);
            });
        }
        else
        {
            resultPanel.SetActive(false);
        }
        
        if (cookingScreenCanvasGroup != null)
            cookingScreenCanvasGroup.DOFade(1f, 0.3f);
    }
}
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
    public CanvasGroup cookingScreenCanvasGroup;
    
    [Header("Result Panel")]
    [SerializeField] private CookingResult suspiciousDishResult;
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

    [Header("Animation")]
    [SerializeField] private GameObject flyingIngredientPrefab;
    [SerializeField] private Transform flyingObjectsParent;
    [SerializeField] private RectTransform cauldronTarget;
    
    [Header("Right Panel UI Elements")]
    [SerializeField] private Button clearPotButton;
    [SerializeField] private CanvasGroup cookButtonCanvasGroup;
    [SerializeField] private CanvasGroup potContentsCanvasGroup;
    [SerializeField] private CanvasGroup clearPotButtonCanvasGroup;
    public Button exitCookingButton;

    [Header("Cooking Animation")]
    [SerializeField] private ParticleSystem cookingFire;
    [SerializeField] private ParticleSystem cookingSteam;
    [SerializeField] private AudioSource cookingSteamAudio;
    [SerializeField] private float animationDuration = 3f; //длительность приготовления
    [SerializeField] private CanvasGroup leftPanelCanvasGroup;

    [Header("Управление камерой")]
    public CookingCameraController cookingCameraController;

    private Dictionary<Ingredient, int> currentPotIngredients = new Dictionary<Ingredient, int>();
    private Dictionary<Ingredient, IngredientUI> ingredientUIMap = new Dictionary<Ingredient, IngredientUI>();
    private List<RecipeUI> recipeUIs = new List<RecipeUI>();
    private Recipe currentDisplayedRecipe;
    private List<RecipeIngredientIconUI> detailsIngredientIcons = new List<RecipeIngredientIconUI>();

    private bool _isCooking = false;
    private bool _fireEverLit = false;
    private bool _initialized = false; //защита от повторной инициализации

    private void Start()
    {
        Initialize();
        gameObject.SetActive(false);
    }

    // Инициализация — вызывается один раз
    private void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        PopulateRecipeBook();

        if (cookButton != null)
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

        if (exitCookingButton != null)
            exitCookingButton.onClick.AddListener(CloseCookingScreen);
    }

    // Обновление ингредиентов
    public void UpdateIngredientUI()
    {
        if (ingredientsContent == null || ingredientPrefab == null) return;

        if (InventoryManager.Instance == null) return;

        foreach (Transform child in ingredientsContent)
            Destroy(child.gameObject);
        ingredientUIMap.Clear();

        foreach (var item in InventoryManager.Instance.ingredients)
        {
            if (item.Value <= 0) continue;

            GameObject obj = Instantiate(ingredientPrefab, ingredientsContent);
            IngredientUI ui = obj.GetComponent<IngredientUI>();

            if (ui == null)
            {
                Debug.LogError($"[CookingUI] Prefab '{ingredientPrefab.name}' не содержит компонент IngredientUI! Проверь префаб.");
                continue;
            }

            ui.Setup(item.Key, item.Value);
            ingredientUIMap[item.Key] = ui;
            ui.OnClicked += (clickedUI) => AddIngredientToPot(clickedUI.Ingredient);
        }
    }

    // Глобальное обновление всех элементов интерфейса, связанных с доступностью ингредиентов, когда что-то меняется в инвентаре
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

    // Добавление ингредиента в котел с анимацией
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

    // Обновляем надпись того что находится в котле
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

    // Метод для кнопки "Очистить котёл" - возвращает ингредиенты
    public void ClearPot()
    {
        // Возвращаем ингредиенты в инвентарь
        foreach (var item in currentPotIngredients)
        {
            InventoryManager.Instance.AddIngredient(item.Key, item.Value);
        }
        
        currentPotIngredients.Clear();
        UpdatePotContentsText();
    }

    // Метод для готовки - ингредиенты расходуются
    private void ConsumePotIngredients()
    {
        currentPotIngredients.Clear();
        UpdatePotContentsText();
    }
    
    // Обновляем книгу рецептов
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

    // Показ деталей определенного рецепта
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

    // Создаем иконки для ингредиентов в рецепте
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

    // Метод обновляет цвет иконок в списке ингредиентов (на панели деталей рецепта)
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

    // Обновляем кнопку приготовления в зависимости от того хватает ли ингредиентов для рецепта
    private void UpdateDetailsCookButton()
    {
        if (detailsCookButton == null || currentDisplayedRecipe == null) return;
        
        bool canCook = CanCookRecipe(currentDisplayedRecipe);
        detailsCookButton.interactable = canCook;
        
        TMP_Text buttonText = detailsCookButton.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
            buttonText.text = canCook ? "Добавить ингредиенты в котёл" : "Не хватает ингредиентов";
    }

    // Можно ли приготовить рецепт? Если все ингредиенты есть - возвращает true. Если хотя бы одного не хватает - возвращает false
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

    // Закрыть окно детального описания рецепта
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

    // Добавляет ингредиенты из детального описания рецепта в котел
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
        UpdateIngredientUI();
        UpdatePotContentsText();
    }

    // Возвращает рецепт с точно такими же ингредиентами и их количеством как в котле, если такой существует
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
                return recipe;
            }
        }
        return null;
    }

    // Обработчик кнопки "Приготовить"
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

    // В случае если рецепт существует 
    private IEnumerator CookSequenceFromRecipe(Recipe recipe)
    {
        yield return StartCoroutine(PlayCookingAnimation());
        ConsumePotIngredients();
        ShowResultPanelFromRecipe(recipe);
    }

    // В случае если рецепт не существует (положены рандомные ингредиенты в котел)
    private IEnumerator CookSequenceFromResult(CookingResult result)
    {
        yield return StartCoroutine(PlayCookingAnimation());
        ConsumePotIngredients();
        ShowResultPanelFromResult(result);
    }

    // Анимация готовки
    private IEnumerator PlayCookingAnimation()
    {
        _isCooking = true;

        // Выключаем кнопку выхода - чтобы нельзя было выйти с экрана готовки во время приготовления
        if (exitCookingButton != null) exitCookingButton.gameObject.SetActive(false); 

        cookButton.interactable = false;
        if (leftPanelCanvasGroup != null) leftPanelCanvasGroup.interactable = false;
        // Скрываем кнопки правой панели
        if (cookButtonCanvasGroup != null) cookButtonCanvasGroup.DOFade(0f, 0.3f);
        if (potContentsCanvasGroup != null) potContentsCanvasGroup.DOFade(0f, 0.3f);
        if (clearPotButtonCanvasGroup != null) clearPotButtonCanvasGroup.DOFade(0f, 0.3f);

        // Зажигаем огонь под котлом
        if (cookingFire != null && !_fireEverLit)
        {
            cookingFire.gameObject.SetActive(true);
            cookingFire.Play();
            _fireEverLit = true;
        }

        // Запускаем пар из котла
        if (cookingSteam != null)
        {
            cookingSteam.gameObject.SetActive(true);
            cookingSteam.Play();
        }
        if (cookingSteamAudio != null) cookingSteamAudio.Play();

        // Ждём пока готовится
        yield return new WaitForSeconds(animationDuration);

        // Гасим пар 
        if (cookingSteamAudio != null) cookingSteamAudio.Stop();
        if (cookingSteam != null)
        {
            cookingSteam.Stop();
            yield return new WaitForSeconds(1.5f); // ждём пока догорят частицы
            cookingSteam.gameObject.SetActive(false);
        }

        // Возвращаем кнопки
        if (cookButtonCanvasGroup != null) cookButtonCanvasGroup.DOFade(1f, 0.3f);
        if (potContentsCanvasGroup != null) potContentsCanvasGroup.DOFade(1f, 0.3f);
        if (clearPotButtonCanvasGroup != null) clearPotButtonCanvasGroup.DOFade(1f, 0.3f);

        cookButton.interactable = true;
        if (leftPanelCanvasGroup != null) leftPanelCanvasGroup.interactable = true;

        if (exitCookingButton != null) exitCookingButton.gameObject.SetActive(true);

        _isCooking = false;
    }

    // Показывает результат готовки (если рецепт правильный)
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

    // Показывает результат готовки (если рецепт неправильный)
    private void ShowResultPanelFromResult(CookingResult result)
    {
        if (resultPanel == null) return;
        
        if (result == null) return;
        
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
        
        if (resultDishName != null)
        {
            resultDishName.text = !string.IsNullOrEmpty(result.resultName) ? result.resultName : "Неизвестное блюдо";
            resultDishName.color = result.isSuccess ? Color.white : Color.red;
        }
        
        if (resultBuffsText != null)
        {
            resultBuffsText.text = !string.IsNullOrEmpty(result.buffDescription) ? result.buffDescription : "Нет эффектов";
            resultBuffsText.color = result.isSuccess ? Color.green : Color.grey;
        }
        
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

    // Закрывает экран с результатом готовки
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

    // Открывает экран готовки. Вызывается из CampfireInteractable после перехода камеры
    public void OpenCookingScreen()
    {
        // На случай если объект был неактивен при старте
        Initialize();

        gameObject.SetActive(true);

        if (cookingScreenCanvasGroup != null)
            cookingScreenCanvasGroup.alpha = 1f;

        ClearPot();
        UpdateIngredientUI();
        PopulateRecipeBook();
    }
    
    // Закрывает экран готовки и возвращает камеру к игроку.
    public void CloseCookingScreen()
    {
        if (_isCooking) return;
        
        gameObject.SetActive(false);
        
        if (cookingCameraController != null)
            cookingCameraController.ExitCookingMode();
    }

    // При нажатии на esc выходит с экрана готовки
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && gameObject.activeSelf && !_isCooking)
        {
            CloseCookingScreen();
        }
    }
}
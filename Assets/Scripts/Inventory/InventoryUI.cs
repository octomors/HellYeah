using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Окно инвентаря открывается по I/Tab, при наведении на слот показывает DetailPanel
public class InventoryUI : MonoBehaviour
{
    [Header("Окно инвентаря")]
    public GameObject inventoryPanel;

    [Header("Сетка слотов")]
    public Transform slotsContainer;
    public GameObject slotPrefab;
    public int totalSlots = 30; //максимальное количество слотов в инвентаре

    [Header("Панель деталей")]
    public GameObject detailPanel;
    public Image detailIcon;
    public TextMeshProUGUI detailName;
    public TextMeshProUGUI detailAmount;
    public TextMeshProUGUI detailDescription;

    [Header("Имя объекта игрока")]
    public string playerRootName = "Player(Clone)";

    private bool _isOpen = false; //состояние окна
    private List<InventorySlotUI> _slotUIs = new List<InventorySlotUI>(); //список всех созданных слотов
    private MonoBehaviour[] _playerScripts; //массив MonoBehaviour'ов на игроке, которые надо отключать при открытии инвентаря (чтобы игрок не двигался и не взаимодействовал)

    private static InventoryUI _instance;

    // Гарантирует, что существует только один экземпляр этого UI
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(transform.root.gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(transform.root.gameObject); //чтобы окно инвентаря не исчезало при перезагрузке сцены
    }

    // При загрузке новой сцены сбрасываем кеш ссылок на скрипты игрока, потому что старый игрок уничтожен, а новый создан
    // В следующий раз при открытии инвентаря они найдутся заново
    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        _playerScripts = null;
    }

    private void Start()
    {
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);

        if (detailPanel != null)
            detailPanel.SetActive(false);

        CreateSlots();

        // Когда что-то добавляется или убирается, автоматически вызывается RefreshSlots, и окно обновляется
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged += RefreshSlots;
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= RefreshSlots;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.Tab))
        {
            if (_isOpen) Close();
            else Open();
        }

        if (Input.GetKeyDown(KeyCode.Escape) && _isOpen)
            Close();
    }

    // Включает панель
    public void Open()
    {
        _isOpen = true;
        inventoryPanel.SetActive(true);
        RefreshSlots();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SetPlayerScripts(false);
    }

    public void Close()
    {
        _isOpen = false;
        inventoryPanel.SetActive(false);

        if (detailPanel != null)
            detailPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SetPlayerScripts(true);
    }

    // Создаёт экземпляры префаба слота внутри контейнера
    private void CreateSlots()
    {
        for (int i = 0; i < totalSlots; i++)
        {
            var obj = Instantiate(slotPrefab, slotsContainer);
            var slotUI = obj.GetComponent<InventorySlotUI>();

            if (slotUI == null)
            {
                Debug.LogError($"[InventoryUI] Prefab '{slotPrefab.name}' не содержит InventorySlotUI!");
                continue;
            }

            // Подписываем колбэки деталей
            slotUI.OnHovered = ShowDetail;
            slotUI.OnUnhovered = HideDetail;

            slotUI.SetEmpty();
            _slotUIs.Add(slotUI);
        }
    }

    // Собирает из InventoryManager только те ингредиенты, у которых количество > 0
    private void RefreshSlots()
    {
        if (InventoryManager.Instance == null) return;

        var items = new List<KeyValuePair<Ingredient, int>>();
        foreach (var pair in InventoryManager.Instance.ingredients)
            if (pair.Value > 0)
                items.Add(pair);

        for (int i = 0; i < _slotUIs.Count; i++)
        {
            if (i < items.Count) //если есть ингредиент для этого слота, заполняет его
                _slotUIs[i].Setup(items[i].Key, items[i].Value);
            else
                _slotUIs[i].SetEmpty();
        }
    }

    // Включает панель деталей при наведении на заполненный слот
    private void ShowDetail(Ingredient ingredient, int amount)
    {
        if (detailPanel == null || ingredient == null) return;

        detailPanel.SetActive(true);

        if (detailIcon != null)
        {
            detailIcon.sprite = ingredient.icon;
            detailIcon.enabled = ingredient.icon != null;
        }

        if (detailName != null)
            detailName.text = ingredient.ingredientName;

        if (detailAmount != null)
            detailAmount.text = $"Количество: {amount}";

        if (detailDescription != null)
            detailDescription.text = ingredient.description;
    }

    private void HideDetail()
    {
        if (detailPanel != null)
            detailPanel.SetActive(false);
    }

    // Ищет объект игрока по имени playerRootName, проходит по всем MonoBehaviour на игроке
    // Когда вызывается с enable = false, эти скрипты отключаются
    private void SetPlayerScripts(bool enable)
    {
        if (_playerScripts == null || _playerScripts.Length == 0)
        {
            var player = GameObject.Find(playerRootName);
            if (player == null) return;

            var all = player.GetComponentsInChildren<MonoBehaviour>();
            var list = new List<MonoBehaviour>();
            foreach (var s in all)
            {
                string n = s.GetType().Name;
                if (n == "FirstPersonMovement" || n == "Jump" || n == "Crouch" || n == "PlayerInteractor")
                    list.Add(s);
            }
            _playerScripts = list.ToArray();
        }

        foreach (var s in _playerScripts)
            if (s != null) s.enabled = enable;
    }
}
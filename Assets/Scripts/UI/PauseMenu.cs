using UnityEngine;
using UnityEngine.UI;

// Меню паузы. Открывается по Esc, останавливает игру
public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject pausePanel;
    public Button continueButton;
    public Button quitButton;

    [Header("Ссылки на другие UI (чтобы не открываться поверх них)")]
    public InventoryUI inventoryUI;
    public CookingUIManager cookingUIManager;

    private bool _isPaused = false;

    public static event System.Action<bool> OnPauseChanged;

    private static PauseMenu _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(transform.root.gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(transform.root.gameObject);
    }

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
        // При переходе на новую сцену снимаем паузу и сбрасываем ссылки
        Time.timeScale = 1f;
        _isPaused = false;
        pausePanel.SetActive(false);
    }

    private void Start()
    {
        pausePanel.SetActive(false);

        continueButton.onClick.AddListener(Resume);
        quitButton.onClick.AddListener(QuitGame);
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        // Не открываем если активен инвентарь или готовка
        if (IsAnyUIOpen()) return;

        if (_isPaused) Resume();
        else Pause();
    }

    private bool IsAnyUIOpen()
    {
        if (inventoryUI != null && inventoryUI.IsOpen) return true;
        if (cookingUIManager != null && cookingUIManager.gameObject.activeSelf) return true;
        return false;
    }

    public void Pause()
    {
        _isPaused = true;
        pausePanel.SetActive(true);
        HUDController.Instance?.Hide();
        if (UIManager.Instance != null) UIManager.Instance.HideTextHint();
        
        Time.timeScale = 0f; //замораживает всю игру

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        OnPauseChanged?.Invoke(true);
    }

    public void Resume()
    {
        _isPaused = false;
        pausePanel.SetActive(false);
        HUDController.Instance?.Show();
        
        Time.timeScale = 1f; //возобновляет игру

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
        OnPauseChanged?.Invoke(false);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f; //сбрасываем на случай если выходим в главное меню
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }

    // На случай если сцена выгружается пока пауза активна
    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}
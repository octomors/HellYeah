using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject interactPanel;
    public TextMeshProUGUI interactText;

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

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// Показывает подсказку для взаимодействия с объектом. Например, "Нажмите E, чтобы открыть дверь".
    /// </summary>
    /// <param name="text"></param>
    public void ShowTextHint(string text)
    {
        if (interactPanel == null || interactText == null)
            return;

        interactPanel.SetActive(true);
        interactText.text = text;
    }

    /// <summary>
    /// Скрывает подсказку для взаимодействия с объектом.
    /// </summary>
    public void HideTextHint()
    {
        if (interactPanel == null)
            return;

        interactPanel.SetActive(false);
    }
}
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject interactPanel;
    public TextMeshProUGUI interactText;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;
    }

    /// <summary>
    /// Показывает подсказку для взаимодействия с объектом. Например, "Нажмите E, чтобы открыть дверь".
    /// </summary>
    /// <param name="text"></param>
    public void ShowTextHint(string text)
    {
        interactPanel.SetActive(true);
        interactText.text = text;
    }

    /// <summary>
    /// Скрывает подсказку для взаимодействия с объектом.
    /// </summary>
    public void HideTextHint()
    {
        interactPanel.SetActive(false);
    }
}
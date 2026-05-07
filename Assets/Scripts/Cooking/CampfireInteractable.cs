using UnityEngine;

public class CampfireInteractable : MonoBehaviour, IInteractable
{
    public CookingCameraController cameraController;
    public CookingUIManager cookingUIManager;
    [SerializeField] private Outline _outline;

    // IInteractable: вызывается когда игрок нажимает E
    public void Interact()
    {
        // Скрываем подсказку взаимодействия перед открытием UI
        if (UIManager.Instance != null)
            UIManager.Instance.HideTextHint();

        cameraController.EnterCookingMode(() =>
        {
            cookingUIManager.OpenCookingScreen();
        });
    }

    // IInteractable: текст подсказки над объектом
    public string GetInteractText()
    {
        return "Готовить [E]";
    }

    // IInteractable: обводка при наведении
    public Outline GetOutline()
    {
        return _outline;
    }
    public Transform GetTransform()
    {
        return transform;
    }
}
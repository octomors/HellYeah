using UnityEngine;

public class DungeonDoor : MonoBehaviour, IInteractable
{
    public string sceneToLoad;
    public string spawnPointName;

    public void Interact()
    {
        UIManager.Instance.ShowTextHint("Загрузка...");
    }

    public string GetInteractText()
    {
        return "Нажмите E, чтобы войти";
    }

    public Transform GetTransform()
    {
        return transform;
    }
}

using UnityEngine;

public class CampDoor : MonoBehaviour, IInteractable
{
    public string sceneToLoad;
    public string spawnPointName;

    private Outline outline;

    private void Awake()
    {
        outline = GetComponent<Outline>();
        if (outline != null)
            outline.enabled = false;
    }

    public void Interact()
    {
        UIManager.Instance.ShowTextHint("Загрузка...");
        SceneLoader.Instance.LoadScene(sceneToLoad, spawnPointName);
    }

    public string GetInteractText()
    {
        return "Нажмите E, чтобы выйти";
    }

    public Outline GetOutline()
    {
        return outline;
    }
    public Transform GetTransform()
    {
        return transform;
    }
}

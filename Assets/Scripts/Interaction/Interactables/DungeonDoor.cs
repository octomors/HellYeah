using UnityEngine;

public class DungeonDoor : MonoBehaviour, IInteractable
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
        Debug.Log($"Loading scene '{sceneToLoad}' with spawn point '{spawnPointName}'");
        SceneLoader.Instance.LoadScene(sceneToLoad, spawnPointName);
    }

    public string GetInteractText()
    {
        return "Нажмите E, чтобы войти";
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

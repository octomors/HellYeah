using UnityEngine;

public enum DoorType
{
    StartRun,
    NextFloor
}

public class DungeonDoor : MonoBehaviour, IInteractable
{
    [SerializeField] private DoorType doorType;
    [SerializeField] private string interactText = "Нажмите E, чтобы войти";

    private IDoorHandler handler;
    private Outline outline;

    private void Awake()
    {
        outline = GetComponent<Outline>();
        if (outline != null)
            outline.enabled = false;
    }

    public void Interact()
    {
        if (handler == null)
        {
            GameManager gameManager = FindAnyObjectByType<GameManager>();
            if (gameManager != null)
            {
                Init(gameManager);
            }
        }

        if (handler == null)
        {
            Debug.LogWarning($"Door '{name}' has no handler initialized.");
            return;
        }

        handler.HandleDoor(doorType);
    }

    public void Init(IDoorHandler handler)
    {
        this.handler = handler;
    }

    public void SetDoorType(DoorType type)
    {
        doorType = type;
    }

    public string GetInteractText()
    {
        return interactText;
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

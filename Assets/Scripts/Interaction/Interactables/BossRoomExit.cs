using UnityEngine;

public class BossRoomExit : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        GameManager.Instance.EndRun();
    }

    public string GetInteractText()
    {
        return "Нажмите E чтобы вернуться в лагерь";
    }

    public Outline GetOutline()
    {
        return null;
    }

    public Transform GetTransform()
    {
        return transform;
    }
}

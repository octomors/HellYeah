using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    public float interactDistance = 3f;
    public LayerMask interactLayer;
    public Camera playerCamera;

    private IInteractable currentInteractable;

    void Update()
    {
        CheckInteraction();

        if (currentInteractable != null && Input.GetKeyDown(KeyCode.E))
        {
            currentInteractable.Interact();
        }
    }

    void CheckInteraction()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance, interactLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                SetCurrentInteractable(interactable);
                return;
            }
        }

        ClearInteractable();
    }

    void SetCurrentInteractable(IInteractable newInteractable)
    {
        if (currentInteractable == newInteractable) return;

        ClearInteractable();
        currentInteractable = newInteractable;

        UIManager.Instance.ShowTextHint(currentInteractable.GetInteractText());

        Outline outline = currentInteractable.GetOutline();
        if (outline != null)
            outline.enabled = true;
    }

    void ClearInteractable()
    {
        if (currentInteractable == null) return;

        UIManager.Instance.HideTextHint();

        Outline outline = currentInteractable.GetOutline();
        if (outline != null)
            outline.enabled = false;

        currentInteractable = null;
    }
}
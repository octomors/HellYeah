using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    public float interactDistance = 3f;
    public LayerMask interactLayer;
    private Camera playerCamera;

    private IInteractable currentInteractable;
    private MonoBehaviour currentInteractableBehaviour;

    void Awake()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }

    void Update()
    {
        if (currentInteractableBehaviour == null && currentInteractable != null)
            ClearInteractable();

        CheckInteraction();

        if (currentInteractable != null && Input.GetKeyDown(KeyCode.E))
        {
            currentInteractable.Interact();
            ClearInteractable();
        }
    }

    void CheckInteraction()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
                return;
        }

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
        currentInteractableBehaviour = newInteractable as MonoBehaviour;

        if (UIManager.Instance != null)
            UIManager.Instance.ShowTextHint(currentInteractable.GetInteractText());

        Outline outline = currentInteractable.GetOutline();
        if (outline != null)
            outline.enabled = true;

    }

    void ClearInteractable()
    {
        if (currentInteractable == null) return;

        if (UIManager.Instance != null)
            UIManager.Instance.HideTextHint();

        if (currentInteractableBehaviour != null)
        {
            Outline outline = currentInteractable.GetOutline();
            if (outline != null)
                outline.enabled = false;
        }

        currentInteractable = null;
        currentInteractableBehaviour = null;
    }
}
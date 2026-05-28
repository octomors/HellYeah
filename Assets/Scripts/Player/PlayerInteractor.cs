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
            Debug.Log($"[PlayerInteractor] Awake camera: {(playerCamera != null ? playerCamera.name : "<null>")}");
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
            Debug.Log($"[PlayerInteractor] Rebind camera: {(playerCamera != null ? playerCamera.name : "<null>")}");
            if (playerCamera == null)
                return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance, interactLayer))
        {
            Debug.Log($"[PlayerInteractor] Ray hit '{hit.collider.name}' (layer {hit.collider.gameObject.layer}).");
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                SetCurrentInteractable(interactable);
                return;
            }
            else
            {
                Debug.Log($"[PlayerInteractor] Hit has no IInteractable. Root: '{hit.collider.transform.root.name}'.");
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

        Debug.Log($"[PlayerInteractor] Set interactable: '{currentInteractable.GetTransform().name}'.");
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

        Debug.Log($"[PlayerInteractor] Cleared interactable: '{currentInteractable.GetTransform().name}'.");
        currentInteractable = null;
        currentInteractableBehaviour = null;
    }
}
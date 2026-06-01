using UnityEngine;

public class FirstPersonLook : MonoBehaviour
{
    [SerializeField]
    Transform character;
    public float sensitivity = 2;
    public float smoothing = 1.5f;

    Vector2 velocity;
    Vector2 frameVelocity;


    void Reset()
    {
        // Get the character from the FirstPersonMovement in parents.
        character = GetComponentInParent<FirstPersonMovement>().transform;
    }

    void Start()
    {
        // Lock the mouse cursor to the game screen.
        Cursor.lockState = CursorLockMode.Locked;
        InitializeLookFromTransforms();
    }

    void Update()
    {
        if (Time.timeScale == 0f) return; //пауза - не двигаем камеру
        // Get smooth velocity.
        Vector2 mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        Vector2 rawFrameVelocity = Vector2.Scale(mouseDelta, Vector2.one * sensitivity);
        frameVelocity = Vector2.Lerp(frameVelocity, rawFrameVelocity, 1 / smoothing);
        velocity += frameVelocity;
        velocity.y = Mathf.Clamp(velocity.y, -90, 90);

        // Rotate camera up-down and controller left-right from velocity.
        transform.localRotation = Quaternion.AngleAxis(-velocity.y, Vector3.right);
        character.localRotation = Quaternion.AngleAxis(velocity.x, Vector3.up);
    }

    void InitializeLookFromTransforms()
    {
        if (character == null)
        {
            var movement = GetComponentInParent<FirstPersonMovement>();
            if (movement != null)
            {
                character = movement.transform;
            }
        }

        if (character == null) return;

        // Derive the internal look state from current prefab rotation.
        float initialYaw = NormalizeAngle(character.localEulerAngles.y);
        float initialPitch = NormalizeAngle(transform.localEulerAngles.x);

        velocity = new Vector2(initialYaw, -initialPitch);
        frameVelocity = Vector2.zero;
    }

    static float NormalizeAngle(float angle)
    {
        return Mathf.DeltaAngle(0f, angle);
    }
}

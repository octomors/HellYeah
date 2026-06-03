using System.Collections.Generic;
using UnityEngine;

public class FirstPersonMovement : MonoBehaviour
{
    public bool IsDashing { get; private set; } = false;
    public bool IsWalking { get; private set; }
    public KeyCode DashKey = KeyCode.LeftShift;
    
    public BasePlayerStats basePlayerStats;

    Rigidbody rb;
    GroundCheck groundCheck;
    /// <summary> Functions to override movement speed. Will use the last added override. </summary>
    public List<System.Func<float>> speedOverrides = new List<System.Func<float>>();

    // Dash state
    //bool IsDashing = false;
    float dashTimer = 0f;
    int currentDashCharges;
    float dashRecoveryTimer = 0f;
    Vector2 cachedDashInput = Vector2.zero;
    bool dashRequested = false;

    void Awake()
    {
        // Get the rigidbody on this.
        rb = GetComponent<Rigidbody>();
        groundCheck = GetComponent<GroundCheck>();
        // Get PlayerStats from the same GameObject.
        if (basePlayerStats == null)
        {
            basePlayerStats = FindAnyObjectByType<BasePlayerStats>();
        }
        currentDashCharges = basePlayerStats.DashCharges;

        // Инициализируем HUD
        if (HUDController.Instance != null)
        {
            currentDashCharges = basePlayerStats.DashCharges;
        }
    }

    void Start()
    {
        if (HUDController.Instance != null)
        {
            HUDController.Instance.DashCharges = basePlayerStats.DashCharges;
            HUDController.Instance.CurrentCharges = currentDashCharges;
            HUDController.Instance.DashChargeRecoveryTime = basePlayerStats.DashChargeRecoveryTime;
        }
        else
        {
            // HUD ещё не готов - попробуем позже
            StartCoroutine(InitHUDDelayed());
        }
    }

    private System.Collections.IEnumerator InitHUDDelayed()
    {
        // Ждём один кадр - к этому моменту все Awake уже отработали
        yield return null;

        if (HUDController.Instance != null)
        {
            HUDController.Instance.DashCharges = basePlayerStats.DashCharges;
            HUDController.Instance.CurrentCharges = currentDashCharges;
            HUDController.Instance.DashChargeRecoveryTime = basePlayerStats.DashChargeRecoveryTime;
        }
    }

    void Update()
    {
        // Проверяем изменились ли статы (например после баффов)
        if (HUDController.Instance != null)
        {
            if (HUDController.Instance.DashCharges != basePlayerStats.DashCharges) HUDController.Instance.DashCharges = basePlayerStats.DashCharges;
            if (HUDController.Instance.DashChargeRecoveryTime != basePlayerStats.DashChargeRecoveryTime) HUDController.Instance.DashChargeRecoveryTime = basePlayerStats.DashChargeRecoveryTime;
        } 

        if (!IsDashing && Input.GetKeyDown(DashKey) && groundCheck.isGrounded && currentDashCharges > 0)
        {
            dashRequested = true;
        }
    }

    void FixedUpdate()
    {
        if (dashRequested && !IsDashing)
        {
            dashRequested = false;
            IsDashing = true;
            currentDashCharges--;
            dashTimer = basePlayerStats.DashTime;

            // Обновляем HUD
            if (HUDController.Instance != null)
                HUDController.Instance.CurrentCharges = currentDashCharges;

            cachedDashInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

            if (cachedDashInput == Vector2.zero)
            {
                cachedDashInput = Vector2.up;
            }
        }

        // If currently dashing, maintain dash velocity regardless of input
        if (IsDashing)
        {
            HandleDash();
        }
        else
        {
            HandleNormalMovement();
        }

        RecoverDashCharges();
    }

    private void HandleDash()
    {
        dashTimer -= Time.fixedDeltaTime;
        if (dashTimer <= 0f)
        {
            IsDashing = false;
            cachedDashInput = Vector2.zero;
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            return;
        }

        // Apply movement.
        rb.linearVelocity =
        transform.rotation * new Vector3(cachedDashInput.x * basePlayerStats.DashSpeed, rb.linearVelocity.y, cachedDashInput.y * basePlayerStats.DashSpeed);
    }

    private void HandleNormalMovement()
    {
        float targetMovingSpeed = basePlayerStats.Speed;

        if (speedOverrides.Count > 0)
        {
            targetMovingSpeed = speedOverrides[speedOverrides.Count - 1]();
        }

        // Get targetVelocity from input.
        Vector2 targetVelocity =
        new Vector2(Input.GetAxis("Horizontal") * targetMovingSpeed, Input.GetAxis("Vertical") * targetMovingSpeed);

        IsWalking = targetVelocity.magnitude > 0.1f;

        // Apply movement.
        rb.linearVelocity =
        transform.rotation * new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.y);
    }
    
    private void RecoverDashCharges()
    {
        if (currentDashCharges == basePlayerStats.DashCharges)
        {
            return;
        }
        
        if(dashRecoveryTimer >= basePlayerStats.DashChargeRecoveryTime)
        {
            currentDashCharges++;
            dashRecoveryTimer = 0;

            if (HUDController.Instance != null)
            {
                HUDController.Instance.CurrentCharges = currentDashCharges;
                HUDController.Instance.CurrentDashRecoveryTime = 0f;
            }
        }
        else
        {
            dashRecoveryTimer += Time.fixedDeltaTime;

            if (HUDController.Instance != null)
                HUDController.Instance.CurrentDashRecoveryTime = dashRecoveryTimer;
        }
    }
}
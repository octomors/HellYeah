using System.Collections.Generic;
using UnityEngine;

public class FirstPersonMovement : MonoBehaviour
{
    public bool IsDashing { get; private set; }
    public KeyCode DashKey = KeyCode.LeftShift;
    
    public BasePlayerStats basePlayerStats;

    Rigidbody rb;
    GroundCheck groundCheck;
    /// <summary> Functions to override movement speed. Will use the last added override. </summary>
    public List<System.Func<float>> speedOverrides = new List<System.Func<float>>();

    // Dash state
    bool isDashing = false;
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
            basePlayerStats = GetComponent<BasePlayerStats>();
        }
        currentDashCharges = basePlayerStats.DashCharges;
    }

    void Update()
    {
        if (!isDashing && Input.GetKeyDown(DashKey) && groundCheck.isGrounded && currentDashCharges > 0)
        {
            dashRequested = true;
        }
    }

    void FixedUpdate()
    {
        if (dashRequested && !isDashing)
        {
            dashRequested = false;
            isDashing = true;
            currentDashCharges--;
            dashTimer = basePlayerStats.DashTime;
            cachedDashInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

            if (cachedDashInput == Vector2.zero)
            {
                cachedDashInput = Vector2.up;
            }
        }

        // If currently dashing, maintain dash velocity regardless of input
        if (isDashing)
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
            isDashing = false;
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
        }
        else
        {
            dashRecoveryTimer += Time.fixedDeltaTime;
        }
    }
}
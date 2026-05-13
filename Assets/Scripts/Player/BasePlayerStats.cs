using UnityEngine;
using System;

[DisallowMultipleComponent]
public class BasePlayerStats : MonoBehaviour
{
    [Header("Movement")]
    public float Speed;
    public float JumpStrength;
    public float CrouchSpeed;

    [Header("Dash")]
    public int DashCharges;
    public float DashChargeRecoveryTime;
    public float DashSpeed;
    public float DashTime;

    [Header("Combat")]
    public float Health = 100f;
   
    [Header("Limits")]
    public float MaxSpeed;
    public float MaxJumpStrength;
    public float MaxCrouchSpeed;
    public int MaxDashCharges;
    public float MinDashChargeRecoveryTime;
    public float MaxHealth = 400f;
}
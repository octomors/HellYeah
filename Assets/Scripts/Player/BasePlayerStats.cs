using UnityEngine;
using System;

[DisallowMultipleComponent]
public class BasePlayerStats : MonoBehaviour
{
    [Header("Movement")]
        [SerializeField] private float speed;
        [SerializeField] private float jumpStrength;
        [SerializeField] private float crouchSpeed;

    [Header("Dash")]
        [SerializeField] private int dashCharges;
        [SerializeField] private float dashChargeRecoveryTime;
        [SerializeField] private float dashSpeed;
        [SerializeField] private float dashTime;

    [Header("Combat")]
        [SerializeField] private float health;
        [SerializeField] private float attackDamage;
   
    [Header("Limits")]
        [SerializeField] private float maxSpeed;
        [SerializeField] private float maxJumpStrength;
        [SerializeField] private float maxCrouchSpeed;
        [SerializeField] private int maxDashCharges;
        [SerializeField] private float maxDashChargeRecoveryTime;
        [SerializeField] private float maxDashSpeed;
        [SerializeField] private float maxDashTime;
        [SerializeField] private float maxHealth;
        [SerializeField] private float maxAttackDamage;

        public float Speed
        {
            get => speed;
            set => speed = value > maxSpeed ? maxSpeed : value;
        }

        public float JumpStrength
        {
            get => jumpStrength;
            set => jumpStrength = value > maxJumpStrength ? maxJumpStrength : value;
        }

        public float CrouchSpeed
        {
            get => crouchSpeed;
            set => crouchSpeed = value > maxCrouchSpeed ? maxCrouchSpeed : value;
        }

        public int DashCharges
        {
            get => dashCharges;
            set => dashCharges = value > maxDashCharges ? maxDashCharges : value;
        }

        public float DashChargeRecoveryTime
        {
            get => dashChargeRecoveryTime;
            set => dashChargeRecoveryTime = value > maxDashChargeRecoveryTime ? maxDashChargeRecoveryTime : value;
        }

        public float DashSpeed
        {
            get => dashSpeed;
            set => dashSpeed = value > maxDashSpeed ? maxDashSpeed : value;
        }

        public float DashTime
        {
            get => dashTime;
            set => dashTime = value > maxDashTime ? maxDashTime : value;
        }

        public float Health
        {
            get => health;
            set => health = value > maxHealth ? maxHealth : value;
        }

        public float AttackDamage
        {
            get => attackDamage;
            set => attackDamage = value > maxAttackDamage ? maxAttackDamage : value;
        }
}
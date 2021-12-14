using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class CharacterHealth : NetworkBehaviour
{
    // If the character is hit by a projectile with the same team id, it will not take damage.
    public int teamId;

    // Callback when the health is changed.
    public delegate void OnHealthChanged(float newHealth);
    public OnHealthChanged onHealthChangedCallback;

    NetworkVariable<float> health = new NetworkVariable<float>(100.0f);
    public float Health
    {
        get
        {
            return health.Value;
        }
        set
        {
            if (IsServer)
            {
                health.Value = value;
            }
        }
    }

    public float maxHealth = 100.0f;

    private void Awake()
    {
        health.OnValueChanged += HealthChanged;
    }

    private void Start()
    {
        if (IsServer)
        {
            health.Value = maxHealth;
        }
    }

    private void HealthChanged(float previousValue, float newValue)
    {
        onHealthChangedCallback?.Invoke(newValue);
    }
}

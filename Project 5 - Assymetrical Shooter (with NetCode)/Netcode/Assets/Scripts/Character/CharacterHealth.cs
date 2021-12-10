using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CharacterHealth : NetworkBehaviour
{
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

    private void Start()
    {
        if (IsServer)
        {
            health.Value = maxHealth;
        }
    }
}

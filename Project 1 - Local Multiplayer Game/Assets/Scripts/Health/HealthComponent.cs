/**
 * HealthComponent.cs
 * Description: This script contains info about a GameObject's health. When the health reaches 0, the root GameObject is deleted.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    [SerializeField] float MaxHealth = 100.0f;

    float currentHealth;
    public float CurrentHealth 
    { 
        get 
        { 
            return currentHealth; 
        } 
        set 
        {
            currentHealth = value < 0 ? 0 : (value > MaxHealth ? MaxHealth : value);
        } 
    }

    private void Awake()
    {
        CurrentHealth = MaxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        // If the health of the game object goes 0 or lower, destroy the game object.
        if(CurrentHealth <= 0.0f)
        {
            Destroy(transform.root.gameObject);
        }
    }
}

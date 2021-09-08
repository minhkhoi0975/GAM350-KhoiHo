/**
 * HealthComponent.cs
 * Description: This script contains info about a GameObject's health. When the health reaches 0, the GameObject is deleted.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    [SerializeField] float MaxHealth = 100.0f;

    float CurrentHealth;

    private void Awake()
    {
        CurrentHealth = MaxHealth;
    }

    // Take damage from an instigator.
    public void TakeDamage(float DamageAmount, int InstigatorInputIndex = -1)
    {
        CurrentHealth -= DamageAmount;
        if(CurrentHealth <= 0)
        {
            // Check if the instigator and the game object associated with this component are characters.
            // If both of them are, increase the score of the instigator.
            if (InstigatorInputIndex >= 0 && InstigatorInputIndex < GameManager.MAX_NUMBER_OF_PLAYERS && gameObject.CompareTag("Character"))
            {
                GameManager.Instance.PlayerScores[InstigatorInputIndex]++;
            }
            Destroy(transform.gameObject);
        }
    }
}

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
    [SerializeField] float maxHealth = 100.0f;
    float currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    // Take damage from an instigator.
    // The instigator is the game object that is responsible for the damage (e.g. the character that shoots a bullet).
    // If the instigator is a playable character, then InstigatorInputIndex is the input index of the player, otherwise it should be -1.
    public void TakeDamage(float DamageAmount, int InstigatorInputIndex = -1)
    {
        currentHealth -= DamageAmount;
        if(currentHealth <= 0)
        {
            // Check if the instigator is a playable character and the victim is a character.
            if (InstigatorInputIndex >= 0 && InstigatorInputIndex < GameManager.MAX_NUMBER_OF_PLAYERS && gameObject.CompareTag("Character"))
            {
                // Instigator kills the other player? Get penalty.
                if (gameObject.GetComponent<PlayerCharacterInput>())
                {
                    GameManager.Instance.playerScores[InstigatorInputIndex] -= 5;
                }
                // Instigator kills NPC? Increase score.
                else
                {
                    GameManager.Instance.playerScores[InstigatorInputIndex]++;
                }
            }
            Destroy(transform.gameObject);
        }
    }
}

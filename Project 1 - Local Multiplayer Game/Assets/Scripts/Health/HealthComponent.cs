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

    float CurrentHealth;

    private void Awake()
    {
        CurrentHealth = MaxHealth;
    }

    public void TakeDamage(float DamageAmount, GameObject Instigator)
    {
        CurrentHealth -= DamageAmount;
        if(CurrentHealth <= 0)
        {
            if (Instigator)
            {
                // Check if the instigator and the game object associated with this component are characters.
                if (Instigator.CompareTag("Character") && gameObject.CompareTag("Character"))
                {
                    // Get the control ID of the character, then update the score.
                    CharacterController CharacterController = Instigator.GetComponent<CharacterController>();
                    if (CharacterController && GameManager.Instance)
                    {
                        if (CharacterController.ControlID == 1)
                        {
                            GameManager.Instance.Player1Score++;
                        }
                        else
                        {
                            GameManager.Instance.Player2Score++;
                        }
                    }
                }
            }

            Destroy(transform.root.gameObject);
        }
    }
}

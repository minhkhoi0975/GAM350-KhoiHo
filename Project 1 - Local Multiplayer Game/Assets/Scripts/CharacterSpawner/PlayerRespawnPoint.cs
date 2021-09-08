/**
 * PlayerRespawnPoint.cs
 * Description: This script is used for respawning playble character of a particle input index.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRespawnPoint : MonoBehaviour
{
    [SerializeField] GameObject characterPrefab;
    [SerializeField] Material characterBodyMaterial; // The material for the character's body.

    public int inputIndex = 1;                       // Each player has a unique input index. The index of the first player is 1.

    GameObject currentCharacterInstance;

    private void Awake()
    {
        // Hide the dummy.
        transform.GetChild(0).gameObject.SetActive(false);
    }

    private void Start()
    {
        Respawn();
    }

    private void Update()
    {
        // Respawn key is the same as firing key.
        if (Input.GetButtonDown("Fire" + inputIndex))
        {
            Respawn();
        }
    }

    void Respawn()
    {
        // Don't respawn if the current player character is still alive.
        if (!currentCharacterInstance)
        {
            // Create a character.
            currentCharacterInstance = Instantiate(characterPrefab, transform.position, transform.rotation);

            // Set the control ID of the character.
            PlayerCharacterInput playerCharacterInputComponent = currentCharacterInstance.GetComponent<PlayerCharacterInput>();
            if (playerCharacterInputComponent)
            {
                playerCharacterInputComponent.inputIndex = this.inputIndex;
            }

            // Set the material of the character's body.
            Renderer characterBodyRenderer = currentCharacterInstance.GetComponent<Renderer>();
            if (characterBodyRenderer)
            {
                characterBodyRenderer.material = characterBodyMaterial;
            }
        }
    }
}

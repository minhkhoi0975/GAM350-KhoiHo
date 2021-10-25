/**
 * PlayerInput.cs
 * Description: This script handles the input from the player.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NetworkSync))]
[RequireComponent(typeof(CharacterMovement))]
public class PlayerInput : MonoBehaviour
{
    public NetworkSync networkSync;              // Reference to the network sync component.

    public CharacterMovement characterMovement;  // Reference to the character movement component.

    float horizontalAxis, verticalAxis;

    private void Awake()
    {
        if(!networkSync)
        {
            networkSync = GetComponent<NetworkSync>();
        }

        if(!characterMovement)
        {
            characterMovement = GetComponent<CharacterMovement>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Get the input from the player.
        horizontalAxis = Input.GetAxisRaw("Horizontal");
        verticalAxis = Input.GetAxisRaw("Vertical");
    }

    private void FixedUpdate()
    {
        if (networkSync.owned)
        {
            Vector3 worldMovementDirection = new Vector3(horizontalAxis, 0.0f, verticalAxis);
            characterMovement.Move(worldMovementDirection);
        }
    }
}

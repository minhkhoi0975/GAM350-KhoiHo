/**
 * PlayerInput.cs
 * Description: This script handles the input from the player.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterMovement))]
[RequireComponent(typeof(CharacterCameraMovement))]
public class PlayerInput : MonoBehaviour
{
    public NetworkSync networkSync;  // Reference to NetworkSync component.

    public CharacterMovement characterMovement;  // Reference to the character movement component.

    public CharacterCameraMovement characterCameraMovement;  // Reference to the character camera movement component.

    float horizontalAxis, verticalAxis;

    float mouseXAxis, mouseYAxis;

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

        if(!characterCameraMovement)
        {
            characterCameraMovement = GetComponent<CharacterCameraMovement>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Client only controls their character, not other clients'.
        if (!networkSync.owned)
            return;

        // Get movement input.
        horizontalAxis = Input.GetAxisRaw("Horizontal");
        verticalAxis = Input.GetAxisRaw("Vertical");

        // Get camera input.
        mouseXAxis = Input.GetAxis("Mouse X");
        mouseYAxis = Input.GetAxis("Mouse Y");

        // Move camera.
        characterCameraMovement.RotateCamera(mouseXAxis, mouseYAxis);

        // Jump.
        if(Input.GetButtonDown("Jump"))
        {
            characterMovement.Jump();
        }
    }

    private void FixedUpdate()
    {
        // Client only controls their character, not other clients'.
        if (!networkSync.owned)
            return;

        Vector3 worldMovementDirection = new Vector3(horizontalAxis, 0.0f, verticalAxis);
        characterMovement.Move(worldMovementDirection);
    }
}

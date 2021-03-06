/**
 * PlayerInput.cs
 * Description: This script handles the input from a shooter.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterMovement))]
[RequireComponent(typeof(ShooterCameraMovement))]
[RequireComponent(typeof(CharacterCombat))]
[RequireComponent(typeof(InputLock))]
public class ShooterInput : MonoBehaviour
{
    // References to components.
    public NetworkSync networkSync;  
    public CharacterMovement characterMovement;
    public ShooterCameraMovement characterCameraMovement;
    public CharacterCombat characterCombat;

    // Used for locking input.
    public InputLock inputLock;

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
            characterCameraMovement = GetComponent<ShooterCameraMovement>();
        }

        if(!characterCombat)
        {
            characterCombat = GetComponent<CharacterCombat>();
        }

        if(!inputLock)
        {
            inputLock = GetComponent<InputLock>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (inputLock.isLocked)
            return;

        // Client only controls their character, not other clients'.
        if (networkSync && networkSync.enabled && !networkSync.owned)
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

        // Fire a projectile.
        if(Input.GetButtonDown("Fire1"))
        {
            characterCombat.FireProjectile();
        }
    }

    private void FixedUpdate()
    {
        if (inputLock.isLocked)
            return;

        // Client only controls their character, not other clients'.
        if (networkSync && networkSync.enabled && !networkSync.owned)
            return;

        // Move character.
        Vector3 worldMovementDirection = new Vector3(horizontalAxis, 0.0f, verticalAxis);
        characterMovement.Move(worldMovementDirection);
    }
}

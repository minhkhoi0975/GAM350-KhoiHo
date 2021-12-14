/**
 * CharacterMovement.cs
 * Description: This script handles the movement of a character.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CharacterMovement : NetworkBehaviour
{
    // Reference to the rigid body of the character.
    [SerializeField] Rigidbody rigidBody;

    // Reference to the character's foot.
    [SerializeField] CharacterFoot characterFoot;

    // Reference to the character's camera.
    [SerializeField] Camera characterCamera;

    // Walking.
    [Header("Walking")]
    public float movementSpeed = 50.0f;

    // Jumping and falling.
    [Header("Jumping and Falling")]
    public float jumpForce = 1500.0f;
    public float gravity = -100f;

    // Looking.
    [Header("Looking")]
    public float sensitivityX = 20.0f; 
    public float sensitivityY = 20.0f;
    float cameraPitch = 0.0f;

    void Awake()
    {
        if (!rigidBody)
        {
            rigidBody = GetComponent<Rigidbody>();
        }
        if (!rigidBody)
        {
            rigidBody = GetComponentInChildren<Rigidbody>(true);
        }

        if (!characterFoot)
        {
            characterFoot = GetComponent<CharacterFoot>();
        }
        if (!characterFoot)
        {
            characterFoot = GetComponentInChildren<CharacterFoot>(true);
        }

        if (!characterCamera)
        {
            characterCamera = GetComponentInChildren<Camera>(true);
        }
    }

    private void FixedUpdate()
    {
        if (IsOwner || IsServer)
        {
            Fall();
        }
    }

    // Move the character.
    public void Move(Vector3 relativeMovementDirection)
    {
        if (relativeMovementDirection.magnitude >= 0.1f)
        {
            // Prevent fast diagonal movement.
            if (relativeMovementDirection.magnitude != 1.0f)
            {
                relativeMovementDirection = relativeMovementDirection.normalized;
            }

            // Normalize the movement direction to make sure that the character does not move more quickly or slowly than usual.
            relativeMovementDirection = relativeMovementDirection.normalized;

            // Convert relative direction into world direction.
            Vector3 worldMovementDirection = transform.TransformVector(relativeMovementDirection).normalized;

            // If the character is on a slope, project worldMovementDirection on slope surface.
            if (characterFoot.IsOnSlope)
            {
                worldMovementDirection = Vector3.ProjectOnPlane(worldMovementDirection, characterFoot.GroundInfo.normal).normalized;
            }

            // Move the character.
            rigidBody.AddForce(worldMovementDirection * movementSpeed * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }
    }

    // Make the character jump.
    public void Jump()
    {
        if (characterFoot.IsGrounded)
        {
            rigidBody.AddForce(Vector3.up * jumpForce);
        }
    }

    // Make the character fall.
    public void Fall()
    {
        if (!characterFoot.IsGrounded)
        {
            rigidBody.AddForce(Vector3.up * gravity, ForceMode.Acceleration);
        }
    }

    // Rotate the camera.
    public void RotateCamera(float mouseX, float mouseY)
    {
        // Move the camera up/down.
        cameraPitch -= mouseY * sensitivityY/Screen.height * Time.deltaTime;

        // Clamp the camera pitch between -90 degees and 90 degrees.
        cameraPitch = Mathf.Clamp(cameraPitch, -90.0f, 90.0f);

        characterCamera.transform.localRotation = Quaternion.Euler(cameraPitch, 0.0f, 0.0f);


        // Move the camera left/right
        float cameraYaw = transform.rotation.eulerAngles.y + mouseX * sensitivityX/Screen.width * Time.deltaTime;
        transform.rotation = Quaternion.Euler(0.0f, cameraYaw, 0.0f);
    }

    [ServerRpc]
    public void RotateCameraServerRpc(float mouseX, float mouseY)
    {
        RotateCamera(mouseX, mouseY);
    }
}
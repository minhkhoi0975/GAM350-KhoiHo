/**
 * CharacterMovement.cs
 * Description: This script handles the movement of a character.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    // Reference to the rigid body of the character.
    [SerializeField] Rigidbody rigidBody;

    // Reference to the character's foot.
    [SerializeField] CharacterFoot characterFoot;

    // Walking.
    public float movementSpeed = 50.0f;

    // Jumping and falling.
    public float jumpForce = 1500.0f;
    public float gravity = -100f;

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
    }

    private void FixedUpdate()
    {
        Fall();
    }

    // Change the movement speed of the character.
    public void SetMovementSpeed(float newMovementSpeed)
    {
        movementSpeed = newMovementSpeed;
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
    void Fall()
    {
        if(!characterFoot.IsGrounded)
        {
            rigidBody.AddForce(Vector3.up * gravity, ForceMode.Acceleration);
        }
    }
}
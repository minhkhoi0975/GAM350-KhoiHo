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
    public Rigidbody rigidBody;           // Reference to the rigid body of the character.

    public float movementSpeed = 50.0f;

    void Awake()
    {
        if(!rigidBody)
        {
            rigidBody = GetComponent<Rigidbody>();
        }
        if(!rigidBody)
        {
            rigidBody = GetComponentInChildren<Rigidbody>(true);
        }
    }

    // Change the movement speed of the character.
    public void ChangeMovementSpeed(float newMovementSpeed)
    {
        movementSpeed = newMovementSpeed;
    }

    // Move the character.
    public void Move(Vector3 worldMovementDirection)
    {
        if (worldMovementDirection.magnitude >= 0.1f)
        {
            // Normalize the movement direction to make sure that the character does not move more quickly or slowly than usual.
            worldMovementDirection = worldMovementDirection.normalized;

            // Move the character.
            rigidBody.AddForce(worldMovementDirection * movementSpeed * Time.fixedDeltaTime, ForceMode.VelocityChange);

            // Rotate the character to match the movement direction.
            float newWorldRotationY = Mathf.Atan2(worldMovementDirection.x, worldMovementDirection.z) * Mathf.Rad2Deg;
            rigidBody.rotation = Quaternion.Lerp(rigidBody.rotation, Quaternion.Euler(0.0f, newWorldRotationY, 0.0f), 0.3f);
        }
    }
}
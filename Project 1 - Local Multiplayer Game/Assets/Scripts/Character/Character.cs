/**
 * Character.cs
 * Description: This script handles the behaviors of a character (movement and firing).
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Character : MonoBehaviour
{
    [SerializeField] Rigidbody rigidBodyComponent;

    // Movement
    [SerializeField] float moveSpeed = 30.0f;
    [SerializeField] float turnSpeed = 20.0f;

    // Firing
    [SerializeField] GameObject projectilePrefab;

    // Start is called before the first frame update
    void Awake()
    {
        if(rigidBodyComponent == null)
        {
            rigidBodyComponent = GetComponent<Rigidbody>();
        }
    }

    public void MoveCharacter(float vertical, float horizontal)
    {
        // Get the move direction relative to the player.
        Vector3 relativeMoveDirection = new Vector3(horizontal, 0.0f, vertical).normalized;

        if (relativeMoveDirection.magnitude >= 0.1f) // Prevent the character from rotating when it's idle.
        {
            // Smoothly rotate the character.
            float rotationAngleInDegrees = Mathf.Atan2(relativeMoveDirection.x, relativeMoveDirection.z) * Mathf.Rad2Deg;
            rigidBodyComponent.rotation = Quaternion.Lerp(rigidBodyComponent.rotation, Quaternion.Euler(0.0f, rotationAngleInDegrees, 0.0f), Time.fixedDeltaTime * turnSpeed);

            // Convert the relative move direction to world move direction.
            Vector3 worldMoveDirection = Quaternion.Euler(0.0f, rotationAngleInDegrees, 0.0f) * Vector3.forward;

            // Move the player.
            rigidBodyComponent.AddForce(worldMoveDirection * moveSpeed * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }
    }

    public void FireProjectile()
    {
        if (projectilePrefab)
        {
            // Spawn a projectile.
            GameObject projectile = Instantiate(projectilePrefab, transform.position + transform.forward * 1.5f, rigidBodyComponent.rotation);

            // Set the instigator of the projectile to be this character.
            Projectile projectileCompponent = projectile.GetComponent<Projectile>();
            if (projectileCompponent)
            {
                projectileCompponent.Instigator = this.gameObject;
            }
        }
    }
}
/**
 * CharacterController.cs
 * Description: This script handles the inputs from the player.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class CharacterController : MonoBehaviour
{
    [SerializeField] private byte controlID = 1;  // The ID number that determines how this character is controlled (1 = WASD, 2 = Arrows).
    public byte ControlID 
    { 
        get 
        { 
            return controlID; 
        } 
        set
        {
            controlID = (byte)(value < 1 ? 1 : (value > 2 ? 2 : value));
        }
    }

    [SerializeField] private float MoveForceMagnitude = 50.0f;
    [SerializeField] private float TurnRate = 10.0f;

    [SerializeField] private GameObject ProjectilePrefab; // The prefab of projectiles fired by the character.

    private Rigidbody RigidBodyComponent;                  // The rigid body of the character.
    private float Vertical, Horizontal; // Use for moving the character vertically/horizontally.

    private void Start()
    {
        RigidBodyComponent = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        HandleInput();
    }

    private void FixedUpdate()
    {
        MoveCharacter();
    }

    void HandleInput()
    {
        // WASD control.
        if (ControlID == 1)
        {
            // Movement.
            Vertical = Input.GetAxis("Vertical1");
            Horizontal = Input.GetAxis("Horizontal1");

            // Fire a projectile.
            if(Input.GetButtonDown("Fire1"))
            {
                FireProjectile();
            }
        }

        // Arrow control.
        else if(ControlID == 2)
        {
            // Movement.
            Vertical = Input.GetAxis("Vertical2");
            Horizontal = Input.GetAxis("Horizontal2");

            // Fire a projectile.
            if (Input.GetButtonDown("Fire2"))
            {
                FireProjectile();
            }
        }
    }

    void MoveCharacter()
    {  
        // Calculate the move direction relative to the player.
        Vector3 RelativeMoveDirection = new Vector3(Horizontal, 0.0f, Vertical).normalized;

        if (RelativeMoveDirection.magnitude >= 0.1f)
        {
            // Rotate the character.
            float RotationAngleInDegrees = Mathf.Atan2(RelativeMoveDirection.x, RelativeMoveDirection.z) * Mathf.Rad2Deg;
            RigidBodyComponent.rotation = Quaternion.Lerp(RigidBodyComponent.rotation, Quaternion.Euler(0.0f, RotationAngleInDegrees, 0.0f), Time.fixedDeltaTime * TurnRate);

            // Move the character.
            Vector3 WorldMoveDirection = Quaternion.Euler(0.0f, RotationAngleInDegrees, 0.0f) * Vector3.forward;
            RigidBodyComponent.AddForce(WorldMoveDirection * MoveForceMagnitude * Time.deltaTime, ForceMode.VelocityChange);
        }
    }

    void FireProjectile()
    {
        // Spawn a projectile.
        GameObject Projectile = Instantiate(ProjectilePrefab, transform.position + transform.forward * 1.5f, RigidBodyComponent.rotation);

        // Set the instigator of the projectile to be this character.
        ProjectileComponent ProjectileCompponent = Projectile.GetComponent<ProjectileComponent>();
        if(ProjectileCompponent)
        {
            ProjectileCompponent.SetInstigator(this.gameObject);
        }
    }
}
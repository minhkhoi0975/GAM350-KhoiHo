/**
 * ProjectileComponent.cs
 * Description: This script handles the movement and damage of a projectile.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    Rigidbody rigidBodyComponent;

    [SerializeField] float moveSpeed = 20.0f;    // How fast the projectile moves.
    [SerializeField] float ageInSeconds = 5;     // How long before this projectile is automatically destroyed.   

    public float damage = 20.0f;                 // The damage of the projectile.
    GameObject instigator;                       // The game object responsible for the damage (e.g. the character that shot this projectile).
    public GameObject Instigator
    {
        get
        {
            return instigator;
        }
        set
        {
            instigator = value;

            // If the instigator is a player character, also set InstigatorInputIndex.
            if (instigator && Instigator.GetComponent<PlayerCharacterInput>())
            {
                instigatorInputIndex = Instigator.GetComponent<PlayerCharacterInput>().inputIndex;
            }
            else
            {
                instigatorInputIndex = -1;
            }
        }
    }
    int instigatorInputIndex = -1;                // If the instigator is a player character, set this field to the input index of the player, otherise set it to -1.
    public int InstigatorInputIndex
    {
        get
        {
            return instigatorInputIndex;
        }
    }
                    
    float CurrentAgeInSeconds = 0;               // How long the projectile has existed.
    bool IsColliding = false;                    // Is this projectile colliding another game object? Used to prevent OnCollisionEnter() from being called multipled times.

    private void Awake()
    {
        rigidBodyComponent = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAge();
    }

    private void FixedUpdate()
    {
        UpdatePosition();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (IsColliding)
        {
            return;
        }
        else
        {
            IsColliding = true;
        }
        
        // Avoid harming the instigator.
        if (collision.gameObject != instigator)
        {
            // Check if the hit game object has health component. If it does, reduce health.
            HealthComponent HealthComponent = collision.gameObject.GetComponent<HealthComponent>();
            if (HealthComponent)
            {
                HealthComponent.TakeDamage(damage, instigatorInputIndex);
            }
        }

        // Destroy the projectile.
        Destroy(gameObject);
    }

    // Update the position of the projectile.
    void UpdatePosition()
    {
        //transform.Translate(Vector3.forward * MoveSpeed * Time.deltaTime);
        rigidBodyComponent.MovePosition(rigidBodyComponent.position + transform.forward * moveSpeed * Time.fixedDeltaTime);
    }

    // Update the age of the projectile.
    void UpdateAge()
    {
        CurrentAgeInSeconds += Time.deltaTime;
        if(CurrentAgeInSeconds >= ageInSeconds)
        {
            Destroy(gameObject);
        }
    }
}

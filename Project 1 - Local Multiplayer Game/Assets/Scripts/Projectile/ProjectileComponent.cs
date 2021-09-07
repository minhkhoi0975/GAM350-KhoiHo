/**
 * ProjectileComponent.cs
 * Description: This script handles the movement and damage of a projectile.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ProjectileComponent : MonoBehaviour
{
    [SerializeField] float MoveSpeed = 20.0f;         // The speed of the projectile.
    [SerializeField] float Age = 5;                   // How long before this projectile is automatically destroyed.   

    public float Damage = 20.0f;                      // The damage of the projectile.
    [HideInInspector]public GameObject Instigator;    // The game object responsible for the damage (i.e. the character that shot this projectile).

    Rigidbody RigidBodyComponent;                     // The rigid body of the projectile.
    float CurrentAge = 0;                             // How long the projectile has existed.

    bool bIsColliding = false;                        // Is this projectile colliding another game object? Used to prevent OnCollisionEnter() from being called multipled times.

    private void Start()
    {
        RigidBodyComponent = GetComponent<Rigidbody>();
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
        if (bIsColliding)
            return;
        else
            bIsColliding = true;
        
        // Avoid harming the instigator.
        if (collision.gameObject != Instigator)
        {
            // Check if the hit game object has health component. If it does, reduce health.
            HealthComponent HealthComponent = collision.gameObject.GetComponent<HealthComponent>();
            if (HealthComponent)
            {
                HealthComponent.TakeDamage(Damage, Instigator);
            }
        }

        // Destroy the projectile.
        Destroy(gameObject);
    }

    public void SetInstigator(GameObject Instigator)
    {
        this.Instigator = Instigator;
    }

    // Update the position of the projectile.
    void UpdatePosition()
    {
        //transform.Translate(Vector3.forward * MoveSpeed * Time.deltaTime);
        RigidBodyComponent.MovePosition(RigidBodyComponent.position + transform.forward * MoveSpeed * Time.fixedDeltaTime);
    }

    // Update the age of the projectile.
    void UpdateAge()
    {
        CurrentAge += Time.deltaTime;
        if(CurrentAge >= Age)
        {
            Destroy(gameObject);
        }
    }
}

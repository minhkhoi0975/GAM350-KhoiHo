using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    // Reference to the rigid body.
    [SerializeField] Rigidbody rigidBody;

    // The movement speed of the projectile.
    [SerializeField] float movementSpeed = 5.0f;

    // The maximuim age of the projectile. When the projectile reaches this age, it is automatically destroyed.
    [SerializeField] float age = 5.0f;
    float currentAge = 0.0f;

    // Handle collision on client side.
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Environment"))
        {
            DestroyProjectile();
        }
    }

    private void Awake()
    {
        if(!rigidBody)
        {
            rigidBody = GetComponent<Rigidbody>();
        }
    }

    private void Update()
    {
        currentAge += Time.deltaTime;
        if(currentAge >= age)
        {
            DestroyProjectile();
        }
    }

    private void FixedUpdate()
    {
        if(movementSpeed > 0.0f)
        {
            rigidBody.velocity = transform.forward * movementSpeed;
        }
    }

    public void DestroyProjectile()
    {
        Destroy(gameObject);
    }
}

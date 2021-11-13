using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    // Reference to the network sync component.
    [SerializeField] NetworkSync networkSync;

    // Reference to the rigid body.
    [SerializeField] Rigidbody rigidBody;

    // The movement speed of the projectile.
    [SerializeField] float movementSpeed = 5.0f;

    // The movement direction of the projectile.
    Vector3 movementDirection;

    // The maximuim age of the projectile. When the projectile reaches this age, it is automatically destroyed.
    [SerializeField] float age = 5.0f;
    float currentAge = 0.0f;

    private void Awake()
    {
        if(!networkSync)
        {
            networkSync = GetComponent<NetworkSync>();
        }

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
            networkSync.Destroy();
        }
    }

    private void FixedUpdate()
    {
        if (!networkSync.owned)
            return;

        if(movementSpeed > 0.0f && movementDirection.magnitude > 0.0f)
        {
            movementDirection = movementDirection.normalized;
            rigidBody.velocity = movementDirection * movementSpeed;
        }
    }
}

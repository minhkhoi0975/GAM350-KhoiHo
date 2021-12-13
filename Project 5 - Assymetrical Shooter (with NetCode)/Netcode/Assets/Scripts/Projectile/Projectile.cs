using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : NetworkBehaviour
{
    // Reference to the rigid body.
    [SerializeField] Rigidbody rigidBody;

    // The movement speed of the projectile.
    [SerializeField] float movementSpeed = 5.0f;

    // The maximuim age of the projectile. When the projectile reaches this age, it is automatically destroyed.
    [SerializeField] float age = 5.0f;
    float currentAge = 0.0f;

    // The damage of the projectile.
    NetworkVariable<float> damage = new NetworkVariable<float>(30.0f);
    public float Damage
    {
        get
        {
            return damage.Value;
        }
        set
        {
            damage.Value = value;
        }
    }

    // Handle collision on client side.
    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer)
            return;

        if (other.attachedRigidbody && other.attachedRigidbody.CompareTag("Character"))
        {
            other.attachedRigidbody.GetComponent<CharacterHealth>().Health -= damage.Value;

            if (other.attachedRigidbody.GetComponent<CharacterHealth>().Health <= 0)
            {
                Destroy(other.attachedRigidbody.gameObject);
            }
        }

        DestroyProjectile();
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
        if (!IsServer)
            return;

        Destroy(gameObject);
    }
}

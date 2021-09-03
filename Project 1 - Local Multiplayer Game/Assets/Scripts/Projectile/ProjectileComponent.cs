using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileComponent : MonoBehaviour
{
    
    [SerializeField] float MoveSpeed = 20.0f;         // The speed of the projectile.
    [SerializeField] float Age = 5;                   // How long before this projectile is automatically destroyed.   

    public float Damage = 20.0f;                      // The damage of the projectile.
    public GameObject Instigator;                     // The game object responsible for the damage (i.e. the character that shot this projectile).

    float CurrentAge = 0;                             // How long the projectile has existed.

    // Update is called once per frame
    void Update()
    {
        UpdatePosition();
        UpdateAge();
    }

    void OnCollisionEnter(Collision collision)
    {
        // Avoid harming the instigator.
        if (collision.gameObject != Instigator)
        {
            // Check if the hit game object has health component. If it does, reduce health.
            HealthComponent HealthComponent = collision.gameObject.GetComponent<HealthComponent>();
            if (HealthComponent)
            {
                HealthComponent.CurrentHealth -= Damage;
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
        transform.Translate(Vector3.forward * MoveSpeed * Time.deltaTime);
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

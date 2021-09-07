using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AIController : MonoBehaviour
{
    [SerializeField] GameObject ProjectilePrefab;
    [SerializeField] float FireDelay = 3.0f;        // Time interval between two consecutive shots.
    bool ReadyToFire = true;

    NavMeshAgent NavMeshAgent;  // The Nav Mesh Agent component of this AI.
    GameObject TargetPlayerCharacter; // The player character this AI tries to attack.

    private void Start()
    {
        NavMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        ExecuteAI();
    }

    void ExecuteAI()
    {
        FindClosestPlayerCharacter();
        
        if(TargetPlayerCharacter)
        {
            NavMeshAgent.SetDestination(TargetPlayerCharacter.transform.position);

            if (IsAimingGood(TargetPlayerCharacter) && ReadyToFire)
            {
                StartCoroutine("FireProjectile");
            }
        }
    }

    void FindClosestPlayerCharacter()
    {
        GameObject ClosestPlayerCharacter = null;
        float ClosestDistance = 0.0f;                 // The distance from this AI character to ClosestPlayerCharacter.

        // Get all game objects whose tag is "Character".
        GameObject[] GameObjects = GameObject.FindGameObjectsWithTag("Character");
        for (int i = 0; i < GameObjects.Length; i++)
        {
            // Only select the game objects that have PlayerController component.
            if (GameObjects[i].GetComponent<PlayerController>())
            {
                // Assign the first result to be the closest player character.
                if(!ClosestPlayerCharacter)
                {
                    ClosestPlayerCharacter = GameObjects[i];
                    ClosestDistance = Vector3.Distance(transform.position, GameObjects[i].transform.position);
                }
                // Update CloestPlayerCharacter by comparing the distance of current element and ClosestDistance.
                else
                {
                    float Distance = Vector3.Distance(transform.position, GameObjects[i].transform.position);
                    if(Distance < ClosestDistance)
                    {
                        ClosestPlayerCharacter = GameObjects[i];
                        ClosestDistance = Distance;
                    }
                }
            }
        }

        TargetPlayerCharacter = ClosestPlayerCharacter;
    }

    // Check whether this AI character faces the target.
    bool IsAimingGood(GameObject Target, float MinimumDotProduct = 0.9f)
    {
        if(Vector3.Dot(transform.forward, (Target.transform.position - this.transform.position).normalized) >= MinimumDotProduct)
        {
            return true;
        }

        return false;
    }

    IEnumerator FireProjectile()
    {

        // Spawn a projectile.
        GameObject Projectile = Instantiate(ProjectilePrefab, transform.position + transform.forward * 1.5f, transform.rotation);

        // Set the instigator of the projectile to be this character.
        ProjectileComponent ProjectileCompponent = Projectile.GetComponent<ProjectileComponent>();
        if (ProjectileCompponent)
        {
            ProjectileCompponent.SetInstigator(this.gameObject);
        }

        // Delay.
        ReadyToFire = false;
        yield return new WaitForSeconds(FireDelay);
        ReadyToFire = true;
    }
}

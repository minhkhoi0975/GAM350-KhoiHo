/**
 * AICharacterInput.cs
 * Description: This script handles the inputs for AI character.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Character))]
[RequireComponent(typeof(NavMeshAgent))]
public class AICharacterInput : MonoBehaviour
{
    [SerializeField] Character characterComponent;
    [SerializeField] NavMeshAgent navMeshAgentComponent;

    [SerializeField] float fireDelayInSeconds = 3.0f;  // Time interval between two consecutive shots.
    bool readyToFire = true;                           // Used for preventing the AI from shooting thousands of projectiles within a short time.

    GameObject targetPlayerCharacter;                  // The player character this AI tries to attack.

    // Start is called before the first frame update
    void Start()
    {
        if (characterComponent == null)
        {
            characterComponent = GetComponent<Character>();
        }

        if(navMeshAgentComponent == null)
        {
            navMeshAgentComponent = GetComponent<NavMeshAgent>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        ExecuteAI();
    }

    void ExecuteAI()
    {
        // Find the player character to attack.
        FindClosestPlayerCharacter();

        if (targetPlayerCharacter)
        {
            // Move to the player character.
            navMeshAgentComponent.SetDestination(targetPlayerCharacter.transform.position);

            // If the firing conditions are good, fire a projectile.
            if (IsAimingGood() && CanSeePlayer() && readyToFire)
            {
                StartCoroutine("FireProjectile");
            }
        }
    }

    void FindClosestPlayerCharacter()
    {
        GameObject closestPlayerCharacter = null;
        float closestDistance = 0.0f;                 // The distance from this AI character to ClosestPlayerCharacter.

        // Get all PlayerCharacterInput components.
        PlayerCharacterInput[] playerCharacterInputComponents = FindObjectsOfType<PlayerCharacterInput>();
        for (int i = 0; i < playerCharacterInputComponents.Length; i++)
        {
            // Assign the first result to be the closest player character.
            if (!closestPlayerCharacter)
            {
                closestPlayerCharacter = playerCharacterInputComponents[i].gameObject;
                closestDistance = Vector3.Distance(transform.position, playerCharacterInputComponents[i].transform.position);
            }
            // Update CloestPlayerCharacter by comparing the current element and ClosestDistance.
            else
            {
                float distance = Vector3.Distance(transform.position, playerCharacterInputComponents[i].transform.position);
                if (distance < closestDistance)
                {
                    closestPlayerCharacter = playerCharacterInputComponents[i].gameObject;
                    closestDistance = distance;
                }
            }
        }

        targetPlayerCharacter = closestPlayerCharacter;
    }

    bool IsAimingGood()
    {
        // Calculate the dot product of the AI character's forward vector and the ideal aiming vector.
        float AimingDotProduct = Vector3.Dot(transform.forward, (targetPlayerCharacter.transform.position - this.transform.position).normalized);

        // Check if the dot product is equal to or greater than the minimum dot product.
        return AimingDotProduct >= 0.85f;
    }

    // Return true if there is no obstacle between the two characters.
    bool CanSeePlayer()
    {
        RaycastHit hitInfo;
        bool hit = Physics.Raycast(transform.position, transform.forward, out hitInfo, Mathf.Infinity);

        if (!hit || hitInfo.rigidbody.transform.gameObject == targetPlayerCharacter)
        {
            return true;
        }

        return false;
    }

    IEnumerator FireProjectile()
    {
        // Fire projectile.
        characterComponent.FireProjectile();

        // Delay.
        readyToFire = false;
        yield return new WaitForSeconds(fireDelayInSeconds);
        readyToFire = true;
    }
}

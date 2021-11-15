/**
 * AICharacterInput.cs
 * Description: This script handles the inputs for AI character.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CharacterCombat))]
[RequireComponent(typeof(NavMeshAgent))]
public class AIController : MonoBehaviour
{
    // Reference to NetworkSync component.
    [SerializeField] NetworkSync networkSync;

    [SerializeField] CharacterCombat characterCombat;

    [SerializeField] NavMeshAgent navMeshAgent;

    [SerializeField] float fireDelayInSeconds = 3.0f;  // Time interval between two consecutive shots.
    bool readyToFire = true;                           // Used for preventing the AI from shooting thousands of projectiles within a short time.

    GameObject targetPlayerCharacter;                  // The player character this AI tries to attack.

    // Start is called before the first frame update
    void Start()
    {
        if(!networkSync)
        {
            networkSync = GetComponent<NetworkSync>();
        }

        if(!characterCombat)
        {
            characterCombat = GetComponent<CharacterCombat>();
        }

        if (!navMeshAgent)
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // AI behavior is only executed on owning client.
        if (networkSync && networkSync.enabled && !networkSync.owned)
            return;

        ExecuteAI();
    }

    void ExecuteAI()
    {
        // Find the player character to attack.
        FindClosestPlayerCharacter();

        if (targetPlayerCharacter)
        {
            // Move to the player character.
            navMeshAgent.SetDestination(targetPlayerCharacter.transform.position);

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
        PlayerInput[] playerCharacterInputComponents = FindObjectsOfType<PlayerInput>();
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
        if (!targetPlayerCharacter)
            return false;

        // Calculate the dot product of the AI character's forward vector and the ideal aiming vector.
        float AimingDotProduct = Vector3.Dot(transform.forward, (targetPlayerCharacter.transform.position - this.transform.position).normalized);

        // Check if the dot product is equal to or greater than the minimum dot product.
        return AimingDotProduct >= 0.85f;
    }

    // Return true if there is no obstacle between the two characters.
    bool CanSeePlayer()
    {
        if (!targetPlayerCharacter)
            return false;

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
        characterCombat.FireProjectile();

        // Delay.
        readyToFire = false;
        yield return new WaitForSeconds(fireDelayInSeconds);
        readyToFire = true;
    }
}
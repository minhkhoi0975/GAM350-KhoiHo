/**
 * AIMovement.cs
 * Description: This script handles the movement of an NPC.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

[RequireComponent(typeof(NavMeshAgent))]
public class AIMovement : NetworkBehaviour
{
    [SerializeField] NavMeshAgent navMeshAgent;

    // Start is called before the first frame update
    void Start()
    {
        if(!navMeshAgent)
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
        }
    }

    public void SetMovementSpeed(float movementSpeed)
    {
        navMeshAgent.speed = movementSpeed;
    }
}
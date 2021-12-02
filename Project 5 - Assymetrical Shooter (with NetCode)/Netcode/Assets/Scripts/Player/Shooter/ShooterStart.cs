/**
 * ShooterStart.cs
 * Description: This script move the shooter to a random position after the shooter is spawned.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ShooterStart : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if(IsServer)
        {
            GameObject[] startPositions = GameObject.FindGameObjectsWithTag("PlayerStartPosition");

            if (startPositions.Length > 0)
            {
                GameObject startPosition = startPositions[Random.Range(0, startPositions.Length)];

                transform.position = startPosition.transform.position;
                transform.rotation = startPosition.transform.rotation;
            }
        }
    }
}

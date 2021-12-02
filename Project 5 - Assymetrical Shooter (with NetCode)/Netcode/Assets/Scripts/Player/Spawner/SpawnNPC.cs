/**
 * SpawnNPC.cs
 * Description: This script allows the spawner to spawn a NPC.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpawnNPC : MonoBehaviour
{
    // The prefab of NPC to be spawned.
    [SerializeField] GameObject prefab;

    public void Spawn(Vector3 position, Quaternion rotation)
    {
        // Try finding the nearest point on navmesh.
        NavMeshHit hit;
        if (!NavMesh.SamplePosition(position, out hit, 1.0f, NavMesh.AllAreas))
            return;

        // Spawn NPC.
        Instantiate(prefab, hit.position, rotation, null);
    }
}

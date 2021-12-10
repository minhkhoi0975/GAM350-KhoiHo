/**
 * SpawnNPC.cs
 * Description: This script allows the spawner to spawn a NPC.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

public class SpawnNPC : NetworkBehaviour
{
    // The prefab of NPC to be spawned.
    [SerializeField] GameObject prefab;

    [ServerRpc]
    public void SpawnServerRpc(Vector3 position, Quaternion rotation)
    {
        Spawn(position, rotation);
    }

    public void Spawn(Vector3 position, Quaternion rotation)
    {
        // Try finding the nearest point on navmesh.
        NavMeshHit hit;
        if (!NavMesh.SamplePosition(position, out hit, 1.0f, NavMesh.AllAreas))
            return;

        // Spawn NPC.
        GameObject npc = Instantiate(prefab, hit.position, rotation, null);
        npc.GetComponent<NetworkObject>().Spawn(true);
    }
}

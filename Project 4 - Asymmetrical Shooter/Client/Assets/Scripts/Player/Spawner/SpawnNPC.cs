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
    // Reference to the client net.
    public ASClient client;

    // The prefab of NPC to be spawned.
    GameObject prefab;

    private void Awake()
    {
        prefab = (GameObject)Resources.Load("NPC");
    }

    private void Start()
    {
        if(!client)
        {
            client = FindObjectOfType<ASClient>();
        }
    }

    public void Spawn(Vector3 position, Quaternion rotation)
    {
        // Try finding the nearest point on navmesh.
        NavMeshHit hit;
        if (!NavMesh.SamplePosition(position, out hit, 1.0f, NavMesh.AllAreas))
            return;

        // Spawn in offline mode.
        if(!client || !client.enabled)
        {
            Instantiate(prefab, hit.position, rotation, null);
        }

        // Spawn in online mode.
        else
        {
            client.clientNet.CallRPC("SpawnNPC", UCNetwork.MessageReceiver.ServerOnly, -1, hit.position, rotation);
        }
    }
}

/**
 * SpawnNPC.cs
 * Description: This script allows the spawner to spawn a NPC.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnNPC : MonoBehaviour
{
    // Reference to the client net.
    public TagClient client;

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
            client = FindObjectOfType<TagClient>();
        }
    }

    public void Spawn(Vector3 position)
    {
        // Spawn in offline mode.
        if(!client || !client.enabled)
        {
            Instantiate(prefab, position, Quaternion.identity, null);
        }

        // Spawn in online mode.
        else
        {
            client.clientNet.CallRPC("SpawnNPC", UCNetwork.MessageReceiver.ServerOnly, -1, client.myPlayerId, position);
        }
    }
}

/**
 * NetworkSync.cs
 * Descripption: This script synchronizes the transform of a network game object from a server to clients.
 * Programmer: Khoi Ho
 * Credits: Professor Carrigg for his NetworkSync script in UCNetwork.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkTransformSync : NetworkBehaviour
{
    // Callback when the client receives the transform from the server.
    public delegate void TransformSynchronized(Vector3 position, Quaternion rotation);
    public TransformSynchronized transformSynchronizedCallback;

    // How often is the transform synchronized?
    // If the value is 0.1, then the transform is synchronized 1/0.1 = 10 times a second.
    float broadcastFrequency = 0.1f;

    // How long before the server synchronizes the transfrom.
    float timeToSend = 0.0f;

    // Update is called once per frame
    void Update()
    {
        if (IsServer)
        {
            timeToSend -= broadcastFrequency;
            if (timeToSend <= 0)
            {
                SynchronizeTransformClientRpc(transform.position, transform.rotation);
            }
        }
    }

    [ClientRpc]
    void SynchronizeTransformClientRpc(Vector3 position, Quaternion rotation)
    {
        transformSynchronizedCallback?.Invoke(position, rotation);

        transform.position = position;
        transform.rotation = rotation;      
    }
}

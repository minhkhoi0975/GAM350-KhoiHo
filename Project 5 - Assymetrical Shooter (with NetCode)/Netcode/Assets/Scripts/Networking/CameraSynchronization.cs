/**
 * NetworkSync.cs
 * Descripption: This script synchronizes the transform of a network game object from a server to clients. I created this script because the NetworkTransform component does not support client-side prediction.
 * Programmer: Khoi Ho
 * Credits: Professor Carrigg for his NetworkSync script in UCNetwork.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class CameraSynchronization : NetworkBehaviour
{
    public NetworkVariable<SynchronizedTransform> synchronizedTranform;

    // Callback when the client receives the transform from the server.
    public delegate void TransformSynchronized(Vector3 position, Quaternion rotation);
    public TransformSynchronized transformSynchronizedCallback;

    // How long before the server synchronizes the transfrom.
    float timeToSend = 0.0f;

    // Update is called once per frame
    void Update()
    {
        if (IsServer)
        {
            synchronizedTranform.Value = new SynchronizedTransform
            {
                synchronizedPosition = transform.localPosition,
                synchronizedRotation = transform.localRotation
            };
        }
        else
        {
            transform.localPosition = synchronizedTranform.Value.synchronizedPosition;
            transform.localRotation = synchronizedTranform.Value.synchronizedRotation;
        }
    }
}

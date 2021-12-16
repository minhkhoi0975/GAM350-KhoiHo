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

public struct SynchronizedTransform: INetworkSerializable
{
    public Vector3 synchronizedPosition;
    public Quaternion synchronizedRotation;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref synchronizedPosition);
        serializer.SerializeValue(ref synchronizedRotation);
    }
}

public class NetworkTransformSync : NetworkBehaviour
{
    public NetworkVariable<SynchronizedTransform> synchronizedTranform;

    // Callback when the client receives the transform from the server.
    public delegate void TransformSynchronized(Vector3 position, Quaternion rotation);
    public TransformSynchronized transformSynchronizedCallback;

    // How often is the transform synchronized?
    // If the value is 0.1, then the transform is synchronized 1/0.1 = 10 times a second.
    float broadcastFrequency = 0.1f;
    public float BroadcastFrequency
    {
        get
        {
            return broadcastFrequency;
        }
    }

    // How long before the server synchronizes the transfrom.
    float timeToSend = 0.0f;

    private void Awake()
    {
        if (IsServer)
        {
            // Broadcast frequency must match the tick rate of the network manager.
            broadcastFrequency = 1 / NetworkManager.Singleton.NetworkTickSystem.TickRate;
        }
    }

    private void Start()
    {
        // Server does not need to synchronize.
        if (!IsServer)
        {
            synchronizedTranform.OnValueChanged += SynchronizeTransformOnClient;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (IsServer)
        {
            timeToSend -= Time.deltaTime;
            if (timeToSend <= 0)
            {
                synchronizedTranform.Value = new SynchronizedTransform
                {
                    synchronizedPosition = transform.localPosition,
                    synchronizedRotation = transform.localRotation
                };

                timeToSend = broadcastFrequency;
            }
        }
    }

    private void SynchronizeTransformOnClient(SynchronizedTransform previousValue, SynchronizedTransform newValue)
    {
        if (IsServer || IsHost)
            return;
     
        transform.localPosition = newValue.synchronizedPosition;
        transform.localRotation = newValue.synchronizedRotation;

        transformSynchronizedCallback?.Invoke(newValue.synchronizedPosition, newValue.synchronizedRotation);

        //Debug.Log(name + " Server position: " + newValue.synchronizedPosition + " " + newValue.synchronizedRotation.eulerAngles);
        //Debug.Log(name + " Client position: " + transform.localPosition + " " + transform.localRotation.eulerAngles);     
    }
}

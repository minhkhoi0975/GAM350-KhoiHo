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
    public float sendTime;                      // When is the transform sent to the client?
    public Vector3 synchronizedPosition;
    public Quaternion synchronizedRotation;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref sendTime);
        serializer.SerializeValue(ref synchronizedPosition);
        serializer.SerializeValue(ref synchronizedRotation);
    }
}

public class NetworkTransformSync : NetworkBehaviour
{
    public const int MAX_HISTORY_COUNT = 20;
    [HideInInspector] public List<SynchronizedTransform> transformRecord = new List<SynchronizedTransform>();


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
                    sendTime = NetworkManager.ServerTime.TimeAsFloat,
                    synchronizedPosition = transform.localPosition,
                    synchronizedRotation = transform.localRotation
                };

                timeToSend = broadcastFrequency;
            }
        }
        else
        {
            if (transformRecord.Count > 1)
            {
                float currentTime = NetworkManager.LocalTime.TimeAsFloat;
                float lastSendTime = transformRecord[transformRecord.Count - 1].sendTime;

                // Interpolation.
                if (currentTime - lastSendTime < broadcastFrequency * 2)
                {
                    SynchronizedTransform mostRecentTransform = transformRecord[transformRecord.Count - 1];
                    SynchronizedTransform secondRecentTransform = transformRecord[transformRecord.Count - 2];

                    float t = (mostRecentTransform.sendTime - secondRecentTransform.sendTime) / broadcastFrequency;

                    transform.localPosition = Vector3.Lerp(transform.localPosition, mostRecentTransform.synchronizedPosition, t);
                    transform.localRotation = Quaternion.Slerp(transform.localRotation, mostRecentTransform.synchronizedRotation, t);
                }

                // Extrapolation.
                else
                {
                    SynchronizedTransform mostRecentTransform = transformRecord[transformRecord.Count - 1];
                    SynchronizedTransform secondRecentTransform = transformRecord[transformRecord.Count - 2];

                    Vector3 velocity = (mostRecentTransform.synchronizedPosition - secondRecentTransform.synchronizedPosition) / (broadcastFrequency * 2);

                    float f = broadcastFrequency;
                    transform.localRotation = Quaternion.Slerp(transform.localRotation, mostRecentTransform.synchronizedRotation, f);
                    transform.localPosition = Vector3.Lerp(transform.localPosition, mostRecentTransform.synchronizedPosition + velocity, f);
                }
            }
        }
    }

    private void SynchronizeTransformOnClient(SynchronizedTransform previousValue, SynchronizedTransform newValue)
    {
        if (IsServer || IsHost)
            return;

        // Obsolete transforms are skipped.
        if (transformRecord.Count > 0 && newValue.sendTime < transformRecord[transformRecord.Count - 1].sendTime)
            return;

        transformRecord.Add(newValue);
        if (transformRecord.Count > MAX_HISTORY_COUNT)
        {
            transformRecord.RemoveAt(0);
        }

        if (transformRecord.Count == 1)
        {
            transform.localPosition = newValue.synchronizedPosition;
            transform.localRotation = newValue.synchronizedRotation;
        }

        /*
        // Calculate the time interval between when the transform is sent and when the transform is received.
        float receiveTime = NetworkManager.LocalTime.TimeAsFloat;
        float receiveTimeInterval = receiveTime - newValue.sendTime;

        Debug.Log("Time from server to client: " + receiveTimeInterval);

        // If the receive time interval is lower than the broadcast frequency, perform interpolation.
        //if (receiveTimeInterval < broadcastFrequency)
        
        {
            if (transformRecord.Count >= 2)
            {
                SynchronizedTransform mostRecentTransform = transformRecord[transformRecord.Count - 1];
                SynchronizedTransform secondRecentTransform = transformRecord[transformRecord.Count - 2];

                // float t = (mostRecentTransform.sendTime - secondRecentTransform.sendTime) / broadcastFrequency;

                float t = receiveTimeInterval / (mostRecentTransform.sendTime - secondRecentTransform.sendTime);

                transform.localPosition = Vector3.Lerp(transform.localPosition, newValue.synchronizedPosition, t);
                transform.localRotation = Quaternion.Slerp(transform.localRotation, newValue.synchronizedRotation, t);
            }
            else
            {
                transform.localPosition = newValue.synchronizedPosition;
                transform.localRotation = newValue.synchronizedRotation;
            }
        }

        transformSynchronizedCallback?.Invoke(newValue.synchronizedPosition, newValue.synchronizedRotation);    
        */
    }
}

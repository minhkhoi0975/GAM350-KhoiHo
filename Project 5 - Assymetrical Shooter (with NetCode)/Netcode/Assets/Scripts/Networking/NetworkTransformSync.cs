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
    NetworkVariable<Vector3> synchronizedPosition;
    public Vector3 SynchronizedPosition
    {
        get
        {
            return synchronizedPosition.Value;
        }
        set
        {
            synchronizedPosition.Value = value;
        }
    }

    NetworkVariable<Quaternion> synchronizedRotation;
    public Quaternion SynchronizedRotation
    {
        get
        {
            return synchronizedRotation.Value;
        }
        set
        {
            synchronizedRotation.Value = value;
        }
    }


    // Callback when the client receives the transform from the server.
    public delegate void TransformSynchronized(Vector3 position, Quaternion rotation);
    public TransformSynchronized transformSynchronizedCallback;

    // How often is the transform synchronized?
    // If the value is 0.1, then the transform is synchronized 1/0.1 = 10 times a second.
    [SerializeField] float broadcastFrequency = 0.1f;
    public float BroadcastFrequency
    {
        get
        {
            return broadcastFrequency;
        }
    }

    // How long before the server synchronizes the transfrom.
    float timeToSend = 0.0f;

    // Update is called once per frame
    void Update()
    {
        if (IsServer)
        {
            timeToSend -= Time.deltaTime;
            if (timeToSend <= 0)
            {
                synchronizedPosition.Value = transform.position;
                synchronizedRotation.Value = transform.rotation;
                SynchronizeTransformClientRpc();
                timeToSend = broadcastFrequency;
            }
        }
        /*
        else
        {
            Debug.Log(Time.deltaTime);

            transform.position = synchronizedPosition.Value;
            transform.rotation = synchronizedRotation.Value;

            transformSynchronizedCallback?.Invoke(synchronizedPosition.Value, synchronizedRotation.Value);
        }
        */
    }

    [ClientRpc(Delivery = RpcDelivery.Unreliable)]
    void SynchronizeTransformClientRpc()
    {
        // Debug.Log("Client transform of " + gameObject.name + ": " + transform.position + " " + transform.rotation);
        // Debug.Log("Server transform of " + gameObject.name + ": " + synchronizedPosition.Value + " " + synchronizedRotation.Value);

        transform.position = synchronizedPosition.Value;
        transform.rotation = synchronizedRotation.Value;     
    
        transformSynchronizedCallback?.Invoke(synchronizedPosition.Value, synchronizedRotation.Value);
    }
}

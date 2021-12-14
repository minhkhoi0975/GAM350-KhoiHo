/**
 * NetworkInterpolatedTransform.cs
 * Description: This script makes the movement of a network object look smooth on the client side.
 * Programmer: Khoi Ho
 * Credit(s): Professor Carrigg for the NetworkInterpolatedTransform script in UCNetwork.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public struct TransformRecord
{
    public float timeStamp;
    public Vector3 position;
    public Quaternion rotation;
}

[RequireComponent(typeof(NetworkTransformSync))]
public class NetworkInterpolatedTransform : NetworkBehaviour
{
    // Reference to the NetworkTransformSync component.
    [SerializeField] NetworkTransformSync networkTransformSync;

    // "Playback" information.
    List<TransformRecord> transformRecords = new List<TransformRecord>();

    // How many elements can transformRecords store?
    public static int TRANSFORM_RECORD_MAX_COUNT = 20;

    bool first = true;
    float interpolationBackTime = 0.2f;
    float extrapolationTime = 0.1f;

    private void Awake()
    {
        if (!networkTransformSync)
        {
            networkTransformSync = GetComponent<NetworkTransformSync>();
        }

        networkTransformSync.transformSynchronizedCallback += OnTransformSynchronized;

        interpolationBackTime = networkTransformSync.BroadcastFrequency * 2;
        extrapolationTime = networkTransformSync.BroadcastFrequency;
    }

    private void Start()
    {
        if (IsServer)
        {
            this.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (IsServer)
            return;

        float currentTime = NetworkManager.Singleton.ServerTime.TimeAsFloat;
        float interpolationTime = currentTime - interpolationBackTime;

        // Interpolation
        if (transformRecords.Count > 0 && currentTime - transformRecords[transformRecords.Count - 1].timeStamp > interpolationTime)
        {
            for (int i = 0; i < transformRecords.Count; i++)
            {
                if (transformRecords[i].timeStamp <= interpolationTime || transformRecords[i].timeStamp == transformRecords.Count - 1)
                {
                    // The state one slot newer (<100ms) than the best playback state
                    TransformRecord rhs = transformRecords[Mathf.Max(i - 1, 0)];
                    // The best playback state (closest to 100 ms old (default time))
                    TransformRecord lhs = transformRecords[i];

                    // Use the time between the two slots to determine if interpolation is necessary
                    float length = rhs.timeStamp - lhs.timeStamp;
                    float t = 0.0F;
                    // As the time difference gets closer to 100 ms t gets closer to 1 in 
                    // which case rhs is only used
                    if (length > 0.0001)
                        t = (float)((interpolationTime - lhs.timeStamp) / length);

                    // if t=0 => lhs is used directly
                    transform.position = Vector3.Lerp(lhs.position, rhs.position, t);
                    transform.rotation = Quaternion.Slerp(lhs.rotation, rhs.rotation, t);

                    return;
                }
            }
        }

        // Extrapolation
        else
        {
            TransformRecord latestTransformRecord = transformRecords[0];
            Vector3 velocity = new Vector3();

            if (transformRecords.Count > 1)
            {
                velocity = (latestTransformRecord.position - transformRecords[1].position) / (float)interpolationTime;
            }

            float f = extrapolationTime;
            transform.rotation = Quaternion.Slerp(transform.rotation, latestTransformRecord.rotation, f);
            transform.position = Vector3.Lerp(transform.position, latestTransformRecord.position + velocity, f);
        }
    }

    // Called when the client receives the transform from the server.
    public void OnTransformSynchronized(Vector3 position, Quaternion rotation)
    {
        TransformRecord newTransformRecord = new TransformRecord();

        newTransformRecord.timeStamp = NetworkManager.Singleton.ServerTime.TimeAsFloat;
        newTransformRecord.position = position;
        newTransformRecord.rotation = rotation;

        if (first)
        {
            first = false;
            transform.position = newTransformRecord.position;
            transform.rotation = newTransformRecord.rotation;
        }

        // Remove old transform records (which are in the rightmost of transformRecords).
        if (transformRecords.Count >= TRANSFORM_RECORD_MAX_COUNT)
        {        
            transformRecords.RemoveRange(transformRecords.Count - 1, transformRecords.Count - TRANSFORM_RECORD_MAX_COUNT + 1);
        }

        // Add the new transform record in the leftmost of transformRecords.
        transformRecords.Insert(0, newTransformRecord);
    }
}

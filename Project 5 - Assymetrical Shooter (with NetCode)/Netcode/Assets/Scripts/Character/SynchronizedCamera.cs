/**
 * SynchronizedCamera.cs
 * Description: This script synchronizes the transform of a camera from the server to the clients.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SynchronizedCamera : NetworkBehaviour
{
    // Reference to the camera.
    [SerializeField] Camera camera;

    // Synchronized transform.
    NetworkVariable<Vector3> synchronizedPosition;
    NetworkVariable<Quaternion> synchronizedRotation;

    // Update is called once per frame
    void Update()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            synchronizedPosition.Value = camera.transform.position;
            synchronizedRotation.Value = camera.transform.rotation;
        }
        else
        {
            camera.transform.position = synchronizedPosition.Value;
            camera.transform.rotation = synchronizedRotation.Value;
        }
    }
}

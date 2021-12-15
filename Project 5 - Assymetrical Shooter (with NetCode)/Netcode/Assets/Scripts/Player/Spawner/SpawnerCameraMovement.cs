/**
 * SpawnerCameraInput.cs
 * Description: This script handles the movement of the camera.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class SpawnerCameraMovement : NetworkBehaviour
{
    public float moveSpeed = 10.0f;
    public float rotationSpeed = 3.0f;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            if (Camera.main)
            {
                Camera.main.GetComponent<AudioListener>().enabled = false;
                Camera.main.enabled = false;
            }
            GetComponent<Camera>().enabled = true;
            GetComponent<AudioListener>().enabled = true;
        }
        else
        {
            GetComponent<Camera>().enabled = false;
            GetComponent<AudioListener>().enabled = false;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner && IsClient && !IsHost)
        {
            NetworkManager.Singleton.GetComponent<ASClient>().DisconnectFromServer();
            SceneManager.LoadScene("NetCode");
        }
    }

    // Move the camera.
    public void Move(float horizontalAxis, float verticalAxis)
    {
        transform.position += transform.right * moveSpeed * Time.deltaTime * horizontalAxis;
        transform.position += transform.forward * moveSpeed * Time.deltaTime * verticalAxis;       
    }

    // Rotate the camera.
    public void Rotate(float mouseXAxis, float mouseYAxis)
    {
        float newRotationX = transform.localEulerAngles.y + mouseXAxis * rotationSpeed;
        float newRotationY = transform.localEulerAngles.x - mouseYAxis * rotationSpeed;
        transform.localEulerAngles = new Vector3(newRotationY, newRotationX, 0f);
    }
}

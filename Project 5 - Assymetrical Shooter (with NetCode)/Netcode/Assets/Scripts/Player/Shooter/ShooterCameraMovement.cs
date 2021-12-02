/**
 * CharacterCameraMovement.cs
 * Description: This script handles the movement of the character's camera.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ShooterCameraMovement : NetworkBehaviour
{
    // Reference to the character's camera.
    public Camera characterCamera;

    // Sensitivity of the camera.
    public float sensitivityX = 20.0f, sensitivityY = 20.0f;

    float cameraPitch = 0.0f;

    private void Awake()
    {
        if (!characterCamera)
        {
            characterCamera = GetComponentInChildren<Camera>(true);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            if(Camera.current)
            {
                Camera.current.enabled = false;
            }
            characterCamera.enabled = true;

            Debug.Log("I'm owner of camera.");
        }
        else
        {
            characterCamera.enabled = false;
            characterCamera.GetComponent<AudioListener>().enabled = true;
        }
    }

    public void RotateCamera(float mouseX, float mouseY)
    {
        RotateCameraServerRpc(mouseX, mouseY);
    }

    [ServerRpc]
    public void RotateCameraServerRpc(float mouseX, float mouseY)
    {
        RotateCameraImplementation(mouseX, mouseY);
    }

    void RotateCameraImplementation(float mouseX, float mouseY)
    {
        // Move the camera up/down.

        cameraPitch -= mouseY * sensitivityY * Time.deltaTime;

        // Clamp the camera pitch between -90 degees and 90 degrees.
        cameraPitch = Mathf.Clamp(cameraPitch, -90.0f, 90.0f);

        characterCamera.transform.localRotation = Quaternion.Euler(cameraPitch, 0.0f, 0.0f);


        // Move the camera left/right

        float cameraYaw = transform.rotation.eulerAngles.y + mouseX * sensitivityX * Time.deltaTime;
        transform.rotation = Quaternion.Euler(0.0f, cameraYaw, 0.0f);
    }
}

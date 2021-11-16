/**
 * CharacterCameraMovement.cs
 * Description: This script handles the movement of the character's camera.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCameraMovement : MonoBehaviour
{
    // Reference to the character's camera.
    public Camera camera;

    // Sensitivity of the camera.
    public float sensitivityX = 20.0f, sensitivityY = 20.0f;

    float cameraPitch = 0.0f;

    private void Awake()
    {
        if (!camera)
        {
            camera = GetComponentInChildren<Camera>(true);
        }
    }

    public void RotateCamera(float mouseX, float mouseY)
    {
        // Move the camera up/down.

        cameraPitch -= mouseY * sensitivityY * Time.deltaTime;

        // Clamp the camera pitch between -90 degees and 90 degrees.
        cameraPitch = Mathf.Clamp(cameraPitch, -90.0f, 90.0f);

        camera.transform.localRotation = Quaternion.Euler(cameraPitch, 0.0f, 0.0f);


        // Move the camera left/right

        float cameraYaw = transform.rotation.eulerAngles.y + mouseX * sensitivityX * Time.deltaTime;
        transform.rotation = Quaternion.Euler(0.0f, cameraYaw, 0.0f);
    }
}

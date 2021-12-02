/**
 * CameraMovement.cs
 * Description: This script handles the movement of a following camera.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowingCameraMovement : MonoBehaviour
{
    public GameObject focusedGameObject; // The game object the camera looks at.

    public Vector3 cameraOffset = new Vector3(0.0f, 187.6f, -30f);

    // Update is called once per frame
    void LateUpdate()
    {
        MoveCamera();
    }

    void MoveCamera()
    {
        if (focusedGameObject)
        {
            // Keep the same distance from the camera to the character.
            this.transform.position = focusedGameObject.transform.position + cameraOffset;
        }
    }
}
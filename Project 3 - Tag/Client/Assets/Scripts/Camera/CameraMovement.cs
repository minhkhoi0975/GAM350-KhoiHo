/**
 * CameraMovement.cs
 * Description: This script handles the movement of a camera.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] GameObject focusedGameObject; // The game object the camera looks at.
    public GameObject FocusedGameObject
    {
        get
        {
            return focusedGameObject;
        }
        set
        {
            focusedGameObject = value;
            cameraOffset = this.transform.position - focusedGameObject.transform.position;
        }
    }

    private Vector3 cameraOffset;

    private void Awake()
    {
        if (focusedGameObject)
        {
            cameraOffset = this.transform.position - focusedGameObject.transform.position;
        }
    }

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


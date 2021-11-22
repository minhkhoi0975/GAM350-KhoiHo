/**
 * SpawnerCameraInput.cs
 * Description: This script handles the movement of the camera.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerCameraMovement : MonoBehaviour
{
    public float moveSpeed = 10.0f;
    public float rotationSpeed = 3.0f;

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

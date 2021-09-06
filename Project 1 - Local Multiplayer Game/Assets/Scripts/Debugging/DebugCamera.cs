using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugCamera : MonoBehaviour
{
    public float moveSpeed = 20.0f;
    public float rotationSpeed = 3.0f;

    private bool canBeRotated = false;

    // Update is called once per frame
    void Update()
    {
        // Move the camera to the left/to the right/forward/backward.
        if (Input.GetKey(KeyCode.A))
        {
            transform.position -= transform.right * moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.position += transform.right * moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.W))
        {
            transform.position += transform.forward * moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.position -= transform.forward * moveSpeed * Time.deltaTime;
        }

        // Hold the left mouse button to rotate the camera.
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            canBeRotated = true;
        }
        if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            canBeRotated = false;
        }

        // Rotate the camera when the left mouse button is held.
        if (canBeRotated)
        {
            float newRotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * rotationSpeed;
            float newRotationY = transform.localEulerAngles.x - Input.GetAxis("Mouse Y") * rotationSpeed;
            transform.localEulerAngles = new Vector3(newRotationY, newRotationX, 0f);
        }
    }
}

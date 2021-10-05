/**
 * CameraControl.cs
 * Description: This script allows the player to move the camera.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraControl : MonoBehaviour
{
    public float moveSpeedX = 20.0f, moveSpeedY = 20.0f;
    public float zoomSpeed = 500.0f;

    private Camera camera;

    private void Start()
    {
        if(!camera)
        {
            camera = GetComponent<Camera>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Move camera vertically/horizontally.
        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");
        transform.Translate(horizontal * moveSpeedX * Time.deltaTime, vertical * moveSpeedY * Time.deltaTime, 0.0f);

        // Zoom in/zoom out.
        float mouseScrollWheel = Input.GetAxis("Mouse ScrollWheel");
        camera.orthographicSize -= mouseScrollWheel * zoomSpeed * Time.deltaTime;
    }
}

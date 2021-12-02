/**
 * SpawnerInput.cs
 * Description: This script handles spawners' input.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(SpawnerCameraMovement))]
[RequireComponent(typeof(SpawnNPC))]
[RequireComponent(typeof(InputLock))]

public class SpawnerInput : MonoBehaviour
{
    // Reference to the camera.
    public Camera spawnerCamera;

    // Reference to SpawnerCameraMovement component.
    public SpawnerCameraMovement cameraMovement;

    // Reference to SpawnNPC component.
    public SpawnNPC spawner;

    // Used for locking input.
    public InputLock inputLock;

    private void Awake()
    {
        if(!spawnerCamera)
        {
            spawnerCamera = GetComponent<Camera>();
        }

        if(!cameraMovement)
        {
            cameraMovement = GetComponent<SpawnerCameraMovement>();
        }

        if(!spawner)
        {
            spawner = GetComponent<SpawnNPC>();
        }

        if(!inputLock)
        {
            inputLock = GetComponent<InputLock>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (inputLock.isLocked)
            return;

        // Move camera.
        cameraMovement.Move(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        // Rotate camera.
        if(Input.GetButton("Fire2"))
        {
            cameraMovement.Rotate(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        }

        // Spawn an NPC.
        if(Input.GetButtonDown("Fire1"))
        {
            // Raycast from camera to check if the spawn clicks on a floor.
            RaycastHit hitInfo;
            if (Physics.Raycast(spawnerCamera.ScreenPointToRay(Input.mousePosition), out hitInfo))
            {
                // Set NPC's Y-rotation to be the same as camera's.
                Quaternion rotation = Quaternion.Euler(0.0f, spawnerCamera.transform.rotation.eulerAngles.y, 0.0f);

                if(hitInfo.collider.CompareTag("Environment"))
                {
                    spawner.Spawn(hitInfo.point, rotation);
                }
                else
                {
                    Debug.Log("The ray does not hit the floor.");
                }
            }
            else
            {
                Debug.Log("The ray does not hit anything.");
            }
        }
    }
}

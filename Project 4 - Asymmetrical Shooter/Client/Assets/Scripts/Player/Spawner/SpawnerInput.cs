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
public class SpawnerInput : MonoBehaviour
{
    // Reference to the camera.
    public Camera camera;

    // Reference to SpawnerCameraMovement component.
    public SpawnerCameraMovement cameraMovement;

    // Reference to SpawnNPC component.
    public SpawnNPC spawner;

    private void Awake()
    {
        if(!camera)
        {
            camera = GetComponent<Camera>();
        }

        if(!cameraMovement)
        {
            cameraMovement = GetComponent<SpawnerCameraMovement>();
        }

        if(!spawner)
        {
            spawner = GetComponent<SpawnNPC>();
        }
    }

    // Update is called once per frame
    void Update()
    {
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
            Debug.Log("Spawned an NPC.");

            // Raycast from camera to check if the spawn clicks on a floor.
            RaycastHit hitInfo;
            if (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out hitInfo))
            {
                if(hitInfo.collider.CompareTag("Environment"))
                {
                    spawner.Spawn(hitInfo.point);
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

/**
 * ShooterMinimap.cs
 * Descrpiption: When a shooter is spawned, display the HUD for this shooter.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ShooterHUD : NetworkBehaviour
{
    // Minimap camera prefab.
    [SerializeField] GameObject minimapCameraPrefab;

    // Reference to the minimap camera.
    GameObject minimapCamera;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // Create a minimap camera for this character.
            minimapCamera = Instantiate(minimapCameraPrefab, new Vector3(0.0f, transform.position.y + 500.0f, 0.0f), Quaternion.Euler(90.0f, 0.0f, 0.0f), null);
            minimapCamera.GetComponent<FollowingCameraMovement>().focusedGameObject = gameObject;

            // Set callback when health changes.
            GetComponent<CharacterHealth>().onHealthChangedCallback = HUDLogic.Singleton.SetHealth;

            // Display the HUD.
            HUDLogic.Singleton.DisplayHUD(true);

            Debug.Log("Shooter's HUD enabled");
        }
    }

    public override void OnNetworkDespawn()
    {
        // Destroy the minimap camera.
        if (minimapCamera)
        {
            Destroy(minimapCamera);
        }
    }
}

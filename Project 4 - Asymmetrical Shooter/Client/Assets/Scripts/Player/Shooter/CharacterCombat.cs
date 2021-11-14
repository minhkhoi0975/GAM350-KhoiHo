/**
 * CharacterCombat.cs
 * Description: This script handles character's combat.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCombat : MonoBehaviour
{
    // Reference to the network sync component of the character.
    [SerializeField] NetworkSync networkSync;

    // Reference to the character's camera.
    [SerializeField] Camera camera;

    // Muzzle transform stores information about where the projectile will be spawned.
    [SerializeField] Transform muzzleTransform;

    // Projectile prefab.
    GameObject projectilePrefab;

    private void Awake()
    {
        if(!networkSync)
        {
            networkSync = GetComponent<NetworkSync>();
        }

        if(!camera)
        {
            camera = GetComponentInChildren<Camera>(true);
        }

        projectilePrefab = Resources.Load("Projectile") as GameObject;
    }

    public void FireProjectile()
    {
        // Offline mode.
        if (!networkSync || !networkSync.enabled)
        {
            GameObject projectile = Instantiate(projectilePrefab, muzzleTransform.position, muzzleTransform.rotation);
        }

        // Online mode.
        else
        {
            networkSync.clientNet.CallRPC("SpawnProjectile", UCNetwork.MessageReceiver.ServerOnly, -1, networkSync.GetId(), muzzleTransform.position, camera.transform.rotation);
        }
    }
}

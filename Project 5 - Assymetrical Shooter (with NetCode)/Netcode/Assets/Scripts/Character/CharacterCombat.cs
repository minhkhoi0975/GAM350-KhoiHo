/**
 * CharacterCombat.cs
 * Description: This script handles character's combat.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CharacterCombat : NetworkBehaviour
{
    // Reference to the character's camera.
    [SerializeField] Camera characterCamera;
    public Camera CharacterCamera
    {
        get
        {
            return characterCamera;
        }
    }

    // Where will the projectile be spawned?
    [SerializeField] Transform muzzleTransform;

    // Projectile prefab.
    [SerializeField] GameObject projectilePrefab;

    private void Awake()
    {
        if(!characterCamera)
        {
            characterCamera = GetComponentInChildren<Camera>(true);
        }
    }

    public void FireProjectile()
    {
        FireProjectileServerRpc();
    }

    [ServerRpc]
    public void FireProjectileServerRpc()
    {
        if (projectilePrefab)
        {
            GameObject projectile = Instantiate(projectilePrefab, muzzleTransform.position, muzzleTransform.rotation);
            projectile.GetComponent<NetworkObject>().Spawn(true);
        }
    }
}

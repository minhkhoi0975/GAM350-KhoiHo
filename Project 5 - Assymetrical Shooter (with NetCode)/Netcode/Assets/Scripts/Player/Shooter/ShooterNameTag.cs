/**
 * PlayerName.cs
 * Description: This script makes the name tag follow the player character.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class ShooterNameTag : NetworkBehaviour
{
    // Reference to the character game object.
    public GameObject characterGameObject;

    // Reference to the name text.
    public TextMesh playerNameText;

    public override void OnNetworkSpawn()
    {
        // If the client is the owner of this character, hide the name tag.
        if (IsOwner)
        {
            enabled = false;
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // Get current active camera.
        Camera activeCamera = Camera.current;

        // Follow the player.
        if (characterGameObject && activeCamera)
        {
            playerNameText.transform.LookAt(activeCamera.transform.position);
            playerNameText.transform.Rotate(0.0f, 180.0f, 0.0f);
        }
    }

    [ClientRpc]
    public void SetNameTagClientRpc(string name)
    {
        SetNameTag(name);
    }

    public void SetNameTag(string name)
    {
        if (playerNameText)
        {
            playerNameText.text = "[" + name + "]";
        }
    }
}

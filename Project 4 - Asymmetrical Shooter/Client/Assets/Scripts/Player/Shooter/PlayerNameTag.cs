/**
 * PlayerName.cs
 * Description: This script makes the name tag follow the player character.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNameTag : MonoBehaviour
{
    // Reference to the character game object.
    public GameObject characterGameObject;

    // Reference to the name text.
    public TextMesh playerNameText;

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

    // Change the name tag of the character.
    public void SetNameTag(string name)
    {
        Debug.Log("Name tag changed.");

        if (playerNameText)
        {
            playerNameText.text = name;
        }
    }
}

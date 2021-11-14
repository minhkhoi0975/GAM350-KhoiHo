/**
 * PlayerName.cs
 * Description: This script makes the tag follow the player character.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerName : MonoBehaviour
{
    // Reference to the character game object.
    public GameObject characterGameObject;

    // Reference to the name text.
    public TextMesh playerNameText;

    // Offset
    public Vector3 offset = new Vector3(0.0f, 20.0f, -2.38f);

    // Start is called before the first frame update
    void Start()
    {
          
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (characterGameObject)
        {
            playerNameText.transform.position = characterGameObject.transform.position + offset;
            playerNameText.transform.rotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
        }
    }

    // Change the name of the character.
    public void SetName(string name)
    {
        Debug.Log("Name changed.");

        if (playerNameText)
        {
            playerNameText.text = name;
        }
    }
}

/**
 * TestInputLock.cs
 * Description: This script is used for testing input lock.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TagClient))]
public class TestInputLock : MonoBehaviour
{
    // Reference to client component.
    public TagClient client;

    public void Awake()
    {
        if(!client)
        {
            client = GetComponent<TagClient>();
        }
    }

    public void Update()
    {
        if(Input.GetButtonDown("ToggleChatBox"))
        {
            if(client.inputLock)
            {
                client.inputLock.isLocked = !client.inputLock.isLocked;

                Debug.Log(client.inputLock.isLocked ? "Input disabled." : "Input enabled.");
            }
        }
    }
}

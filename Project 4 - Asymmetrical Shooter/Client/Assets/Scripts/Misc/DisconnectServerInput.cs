/**
 * DisconnectServerInput.cs
 * Description: This script defines an input to disconnect a client from the server.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ClientNetwork))]
public class DisconnectServerInput : MonoBehaviour
{
    public ClientNetwork client;

    private void Awake()
    {
        if (!client)
        {
            client = GetComponent<ClientNetwork>();
        }
    }

    private void Update()
    {
        if (Input.GetButtonDown("Cancel") && client.IsConnected())
        {
            client.Disconnect("The client wants to exit the game.");
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;
using System;

public class ASClient : MonoBehaviour
{
    private void Start()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    public void StartClient(string address, int port)
    {
        // Set the ip address and the port of the game session.
        NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = address;
        NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectPort = port;
        NetworkManager.Singleton.GetComponent<UNetTransport>().ServerListenPort = port;
        
        NetworkManager.Singleton.StartClient();
    }

    // Callback when a client is disconnected from the game.
    private void OnClientDisconnected(ulong clientId)
    {
        // If the disconnected client is this client, reload the scene.
        if (!NetworkManager.Singleton.IsConnectedClient && !NetworkManager.Singleton.IsHost && !NetworkManager.Singleton.IsServer)
        {
            Debug.Log("You have disconnected from the server.");
            DisconnectFromServer();
        }
    }

    // Disconnect this client from the server.
    public void DisconnectFromServer()
    {
        if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene("NetCode");
        }
    }
}
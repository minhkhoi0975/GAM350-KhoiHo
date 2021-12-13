using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;
using System;

public class ASClient : MonoBehaviour
{
    static ASClient singleton;
    public ASClient Singleton
    {
        get
        {
            return singleton;
        }
    }

    [SerializeField] Text playerNameText;

    private void Awake()
    {
        // When the client loads a new level, the NetworkManager game object is duplicated.
        // Need those lines to avoid duplication.
        if (!singleton)
        {
            singleton = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

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

        // When the client connects to the server, send the player's name to the server.
        NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(playerNameText.text);

        NetworkManager.Singleton.StartClient();
        StartCoroutine(WaitingForConnection());
    }

    IEnumerator WaitingForConnection()
    {
        float requestConnectionTime = Time.realtimeSinceStartup;

        while (!NetworkManager.Singleton.IsConnectedClient)
        {
            // If the waiting time is too long, automatically disconnect the client.
            if (Time.realtimeSinceStartup - requestConnectionTime >= 10.0f)
            {
                DisconnectFromServer();
                yield break;
            }
        }

        yield return null;
    }

    // Callback when a client is disconnected from the game.
    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log("You have disconnected from the server.");
        DisconnectFromServer();
    }

    // Disconnect this client from the server.
    public void DisconnectFromServer()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene("NetCode");
        }
    }
}

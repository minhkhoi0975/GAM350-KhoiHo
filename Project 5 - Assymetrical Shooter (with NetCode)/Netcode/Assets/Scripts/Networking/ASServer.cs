using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;
using System;

public class ASServer : MonoBehaviour
{
    // Info about a player.
    public class PlayerData
    {
        public bool canPlay = false;
        public int team = 0;
    }

    // List of all players in the game.
    Dictionary<ulong, PlayerData> players = new Dictionary<ulong, PlayerData>();

    // Shooter prefab.
    [SerializeField] GameObject shooterPrefab;

    // List of start transforms for the shooters.
    [SerializeField] List<Transform> shooterStartTransforms;

    private void Start()
    {
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;

        NetworkManager.Singleton.ConnectionApprovalCallback += OnClientConnecting;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    // Start the server.
    public void StartServer(string address, int port, bool listenServer = true)
    {
        // Set the ip address and the port of the game session.
        NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = address;
        NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectPort = port;
        NetworkManager.Singleton.GetComponent<UNetTransport>().ServerListenPort = port;

        if(listenServer == true)
        {
            NetworkManager.Singleton.StartHost();
        }
        else
        {
            NetworkManager.Singleton.StartServer();
        }
    }

    // ----------
    // CALLBACKS
    // ----------

    // Called when the server has started.
    private void OnServerStarted()
    {
        Debug.Log("Server started.");
    }

    // Check whether the connection should be approved.
    private void OnClientConnecting(byte[] connectionData, ulong clientId, NetworkManager.ConnectionApprovedDelegate callback)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log("Player " + clientId + " is connecting to the game.");

            PlayerData newPlayer = new PlayerData();
            newPlayer.canPlay = false;
            newPlayer.team = 0;

            players.Add(clientId, newPlayer);

            callback(false, null, true, null, null);
        }
    }

    // Called when a client has connected to the server.
    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log("Player " + clientId + " has successfully connected to the game.");

            players[clientId].canPlay = true;
            players[clientId].team = GetTeamForNewPlayer();

            // If the player is a shooter, spawn a character for the player.
            if (players[clientId].team == 1)
            {
                // Get random spawn position.
                Transform shooterStartTransform = shooterStartTransforms[UnityEngine.Random.Range(0, shooterStartTransforms.Count)];
                Vector3 startPosition = shooterStartTransform.position;
                Quaternion startRotation = shooterStartTransform.rotation;

                // Spawn the shooter.
                GameObject shooter = Instantiate(shooterPrefab, startPosition, startRotation, null);
                shooter.GetComponent<NetworkObject>().SpawnWithOwnership(clientId, true);
            }
        }
    }

    // Get team for the new player.
    int GetTeamForNewPlayer()
    {
        int team1Count = 0;
        int team2Count = 0;

        foreach (KeyValuePair<ulong, PlayerData> player in players)
        {
            if (player.Value.team == 1)
            {
                team1Count++;
            }
            else
            {
                team2Count++;
            }
        }

        return team1Count <= team2Count ? 1 : 2;
    }

    // Called when a client has disconnected from the server.
    private void OnClientDisconnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log("Player " + clientId + " has disconnected from the game.");

            if (players.ContainsKey(clientId))
            {
                players.Remove(clientId);
            }
        }
    }
}

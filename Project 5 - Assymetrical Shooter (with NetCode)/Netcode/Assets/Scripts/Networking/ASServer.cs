using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;
using System;

public class ASServer : MonoBehaviour
{
    // Info about a player.
    public class PlayerData
    {
        public string name = "";
        public bool canPlay = false;
        public int team = 0;
    }

    // List of all players in the game.
    Dictionary<ulong, PlayerData> players = new Dictionary<ulong, PlayerData>();

    // Shooter prefab.
    [SerializeField] GameObject shooterPrefab;

    // List of start transforms for the shooters.
    [SerializeField] List<Transform> shooterStartTransforms;

    // Reference to the player name text. Need this to set the name for player who is the listen server.
    [SerializeField] Text playerNameText;

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

        if (listenServer == true)
        {
            NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(playerNameText.text);
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
            Debug.Log("A player with clientId = " + clientId + " is connecting to the game.");

            PlayerData newPlayer = new PlayerData();
            newPlayer.name = System.Text.Encoding.ASCII.GetString(connectionData);
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
            Debug.Log(players[clientId].name + " (clientId = " + clientId + ") has successfully connected to the game.");

            players[clientId].canPlay = true;
            players[clientId].team = GetTeamForNewPlayer();

            // Let the new player see shooters' name tags.
            LetNewPlayerSeeGameTags(clientId);

            // If the player is a shooter, spawn a character for the player.
            // if (players[clientId].team == 1)
            {
                SpawnShooter(clientId);
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

    // Let the new player see shooters' name tags.
    void LetNewPlayerSeeGameTags(ulong clientId)
    {
        foreach (KeyValuePair<ulong, PlayerData> otherPlayer in players)
        {
            if (otherPlayer.Key != clientId /*&&otherPlayer.Value.team == 1*/)
            {
                NetworkClient networkClient = NetworkManager.Singleton.ConnectedClients[otherPlayer.Key];
                foreach (NetworkObject networkObject in networkClient.OwnedObjects)
                {
                    networkObject.GetComponent<ShooterNameTag>()?.SetNameTagClientRpc(otherPlayer.Value.name);
                }
            }
        }
    }

    // Spawn a character for a shooter.
    void SpawnShooter(ulong clientId)
    {
        // Get a random spawn position.
        Transform shooterStartTransform = shooterStartTransforms[UnityEngine.Random.Range(0, shooterStartTransforms.Count)];
        Vector3 startPosition = shooterStartTransform.position;
        Quaternion startRotation = shooterStartTransform.rotation;

        // Spawn the shooter.
        GameObject shooter = Instantiate(shooterPrefab, startPosition, startRotation, null);
        shooter.GetComponent<NetworkObject>().SpawnWithOwnership(clientId, true);
        shooter.GetComponent<ShooterNameTag>().SetNameTag(players[clientId].name);
        shooter.GetComponent<ShooterNameTag>().SetNameTagClientRpc(players[clientId].name);
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

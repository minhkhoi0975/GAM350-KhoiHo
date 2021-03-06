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
    static ASServer singleton;
    public static ASServer Singleton
    {
        get
        {
            return singleton;
        }
    }

    // Info about a player.
    public class PlayerData
    {
        public string name = "";
        public bool canPlay = false;
        public int team = 0;
    }

    // Reference to the player name text. Need this to set the name for player who is the listen server.
    [SerializeField] Text playerNameText;

    // List of all players in the game.
    public Dictionary<ulong, PlayerData> players = new Dictionary<ulong, PlayerData>();

    [Header("Shooter")]

    // Shooter prefab.
    [SerializeField] GameObject shooterPrefab;

    // List of start transforms for the shooters.
    [SerializeField] List<Transform> shooterStartTransforms;

    [Header("Spawner")]

    // Spawner camera prefab.
    [SerializeField] GameObject spawnerCameraPrefab;

    // Initial transform of the spawner camera.
    [SerializeField] Vector3 initialCameraPosition;
    [SerializeField] Quaternion initialCameraRotation;

    // The maximum number of NPCs that can appear at the same time = Number of shooters * maxNpcShooterRatio.
    public int maxNpcShooterRatio = 5;

    [Header("Chat System")]
    [SerializeField] GameObject ChatManagerPrefab;

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

        // Create a chat manager network object.
        GameObject chatManager = Instantiate(ChatManagerPrefab);
        chatManager.GetComponent<NetworkObject>().Spawn();
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

            // Host joins a random team.
            if (clientId == 0)
            {
                players[clientId].team = UnityEngine.Random.Range(1, 3);
            }
            else
            {
                players[clientId].team = GetTeamForNewPlayer();
            }

            Debug.Log(players[clientId].name + " is assigned to team " + (players[clientId].team == 1 ? " Shooter" : " Spawner") + ".");

            // Let the new player see shooters' name tags.
            LetNewPlayerSeeGameTags(clientId);

            // If the player is a shooter, spawn a character for the player.
            if (players[clientId].team == 1)
            {
                SpawnShooter(clientId);
            }
            else if (players[clientId].team == 2)
            {
                SpawnSpawnerCamera(clientId);
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
            else if (player.Value.team == 2)
            {
                team2Count++;
            }
        }

        Debug.Log("Team 1 count: " + team1Count);
        Debug.Log("Team 2 count: " + team2Count);
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
    public void SpawnShooter(ulong clientId)
    {
        if (!players.ContainsKey(clientId))
            return;

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

    // Spawn a camera for a spawner.
    public void SpawnSpawnerCamera(ulong clientId)
    {
        if (!players.ContainsKey(clientId))
            return;

        // Spawn the camera.
        GameObject spawnerCamera = Instantiate(spawnerCameraPrefab, initialCameraPosition, initialCameraRotation, null);
        spawnerCamera.GetComponent<NetworkObject>().SpawnWithOwnership(clientId, true);
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

    // Shut down the server
    public void ShutDownServer()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            // Clear all player data.
            players.Clear();

            // Shut down the server.
            NetworkManager.Singleton.Shutdown();

            // Load the main menu.
            SceneManager.LoadScene("NetCode");
        }
    }

    // Get the number of players in a team.
    public int GetNumPlayers(int team)
    {
        int count = 0;

        foreach (PlayerData player in players.Values)
        {
            if (player.team == team)
            {
                count++;
            }
        }

        return count;
    }
}

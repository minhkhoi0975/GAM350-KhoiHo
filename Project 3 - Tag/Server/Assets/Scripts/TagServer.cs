using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LitJson;

public class TagServer : MonoBehaviour
{
    public ServerNetwork serverNet;

    public int portNumber = 603;

    // Data about a player/client.
    public class Player
    {
        // Data about connection.
        public long clientId;
        public bool isConnected;   // Is the client connected to the server?

        // Data about character.
        public int playerId = 0;
        public bool isHunter = false;

        // Data about character game object.
        public int gameObjectNetworkId = -1;
        public Vector3 position;
        public Quaternion rotation;
    }

    // List of all players in the game.
    List<Player> players = new List<Player>();

    // The player ID of the client that has just connected to the server.
    static int lastPlayerId = 0;

    // The current hunter.
    Player currentHunter;

    // If a player has become the Hunter, the player cannot catch a prey for a couple of seconds.
    public float hunterCooldown = 10.0f;
    bool canHunterGoHunting = false;

    // The movement speeds of the hunter and the preys.
    public float hunterMovementSpeed = 450.0f;
    public float preyMovementSpeed = 150.0f;

    enum GameState
    {
        Waiting,        // The current number of players has not reached minimumNumberOfPlayers.
        Playing         // The game has already started.
    }
    GameState currentGameState = GameState.Waiting;

    public int minimumNumberOfPlayers = 4; // How many players have to enter the game before the game starts?

    // Initialize the server
    void Awake()
    {
        // Initialization of the server network
        ServerNetwork.port = portNumber;
        if (serverNet == null)
        {
            serverNet = GetComponent<ServerNetwork>();
        }
        if (serverNet == null)
        {
            serverNet = (ServerNetwork)gameObject.AddComponent(typeof(ServerNetwork));
            Debug.Log("ServerNetwork component added.");
        }

        //serverNet.EnableLogging("rpcLog.txt");
    }

    // CALLBACK FUNCTIONS
    //  The following functions will be called by the ServerNetwork script while the game is running:

    // A client has just requested to connect to the server
    void ConnectionRequest(ServerNetwork.ConnectionRequestInfo data)
    {
        Debug.Log("Connection request from " + data.username);

        // Set the info of the client.
        Player newPlayer = new Player();
        newPlayer.clientId = data.id;
        newPlayer.isConnected = false;
        newPlayer.isHunter = false;

        // Add the client to the list.
        players.Add(newPlayer);

        // Approve the connection
        serverNet.ConnectionApproved(data.id);
    }

    // A client has finished connecting to the server
    void OnClientConnected(long aClientId)
    {
        Player newPlayer = GetPlayerByClientId(aClientId);
        if(newPlayer == null)
        {
            Debug.Log("OnClientConnected: Unable to find unknown player for client " + aClientId);
            return;
        }

        newPlayer.isConnected = true;
        newPlayer.playerId = lastPlayerId++;
        serverNet.CallRPC("SetPlayerId", aClientId, -1, newPlayer.playerId);

        // A client has connected, send the data about other connected clients
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].clientId != aClientId)
            {
                // Let this client know about other clients.
                serverNet.CallRPC("NewPlayerConnected", aClientId, -1, players[i].playerId);

                // Let other clients know about this client.
                serverNet.CallRPC("NewPlayerConnected", players[i].clientId, -1, newPlayer.playerId);
            }
        }
    }

    // A client has disconnected
    void OnClientDisconnected(long aClientId)
    {
        Player disconnectedPlayer = GetPlayerByClientId(aClientId);
        if (disconnectedPlayer == null)
            return;

        serverNet.CallRPC("ClientDisconnected", UCNetwork.MessageReceiver.AllClients, -1, disconnectedPlayer.playerId);
        players.Remove(disconnectedPlayer);
    }

    // Get the player for the given client id
    Player GetPlayerByClientId(long aClientId)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].clientId == aClientId)
            {
                return players[i];
            }
        }

        // If we can't find the player for this client, who are they? kick them
        Debug.Log("Unable to get player for unknown client " + aClientId);
        serverNet.Kick(aClientId);
        return null;
    }

    // A network object has been instantiated by a client
    void OnInstantiateNetworkObject(ServerNetwork.IntantiateObjectData aObjectData)
    {
        
    }

    // A client has been added to a new area
    void OnAddArea(ServerNetwork.AreaChangeInfo aInfo)
    {

    }

    // An object has been added to a new area
    void AddedObjectToArea(int aNetworkId)
    {
        /*
        Player newPlayer = FindPlayerWithGameObjectNetworkId(aNetworkId);
        if (newPlayer != null)
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].clientId != newPlayer.clientId)
                {
                    if (players[i] == currentHunter)
                    {
                        serverNet.CallRPC("SetMaterial", newPlayer.clientId, players[i].gameObjectNetworkId, true);
                    }
                }
            }
        }
        */
    }

    // Initialization data should be sent to a network object
    void InitializeNetworkObject(ServerNetwork.InitializationInfo aInfo)
    {

    }

    // A game object has been destroyed
    void OnDestroyNetworkObject(int aObjectId)
    {

    }

    private void Update()
    {
    }

    public void NetObjectUpdated(int aNetId)
    {
        // Update the position of the player game object.
        UpdateGameObjectTransform(aNetId);

        // Check whether the hunter catches a prey.
        Player prey;
        if (canHunterGoHunting && HunterCatchesPrey(out prey))
        {
            SetHunter(prey.playerId);
        }
    }

    // Does the hunter catch a prey?
    public bool HunterCatchesPrey(out Player prey)
    {
        if (currentHunter == null)
        {
            prey = null;
            return false;
        }
            
        foreach(Player player in players)
        {
            // The diameter of a capsule is 1.0f, so we use 1.0f here.
            if (player != currentHunter && Vector3.Distance(player.position, currentHunter.position) <= 1.0f)
            {
                prey = player;

                Debug.Log(currentHunter.playerId + " has just caught " + prey.playerId);
                return true;
            }
        }

        prey = null;
        return false;
    }

    // Change the hunter.
    public void SetHunter(int playerId)
    {
        // The current hunter is no longer a hunter.
        if (currentHunter != null)
        {
            currentHunter.isHunter = false;
            serverNet.CallRPC("SetMovementSpeed", UCNetwork.MessageReceiver.AllClients, currentHunter.gameObjectNetworkId, preyMovementSpeed);
            serverNet.CallRPC("SetMaterial", UCNetwork.MessageReceiver.AllClients, currentHunter.gameObjectNetworkId, false);
        }

        // Make the player that matches the ID become the next hunter.
        foreach(Player player in players)
        {
            if(player.playerId == playerId)
            {
                player.isHunter = true;
                currentHunter = player;
                serverNet.CallRPC("SetMovementSpeed", UCNetwork.MessageReceiver.AllClients, currentHunter.gameObjectNetworkId, hunterMovementSpeed);
                serverNet.CallRPC("SetMaterial", UCNetwork.MessageReceiver.AllClients, currentHunter.gameObjectNetworkId, true);
                break;
            }
        }

        // Make the new hunter wait for a couple of seconds before they can start hunting.
        StartCoroutine(HunterCoolDown());

        Debug.Log("Player " + playerId + " has become the hunter.");
        serverNet.CallRPC("SetHunter", UCNetwork.MessageReceiver.AllClients, -1, playerId);
    }

    // The hunter must wait for a couple of seconds before they can hunt for a prey.
    IEnumerator HunterCoolDown()
    {
        canHunterGoHunting = false;
        yield return new WaitForSeconds(hunterCooldown);
        canHunterGoHunting = true;
    }

    // RPC when a client's character has been spawned.
    public void PlayerGameObjectSpawned(int playerId, int gameObjectNetworkId)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if(players[i].playerId == playerId)
            {
                players[i].gameObjectNetworkId = gameObjectNetworkId;
                Debug.Log("Player " + playerId + " created a game object with network ID " + gameObjectNetworkId);
                break;
            }      
        }

        // Check whether the game can start.
        if (currentGameState == GameState.Waiting && CanStartGame())
        {
            StartGame();
            Debug.Log("The game has started.");
        }
    }

    // Can the hunt begin?
    public bool CanStartGame()
    {
        int connectedPlayerCount = 0;
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].isConnected && players[i].gameObjectNetworkId != -1)
            {
                connectedPlayerCount++;
            }
        }

        Debug.Log("Number of connected players: " + connectedPlayerCount);
        if (connectedPlayerCount >= minimumNumberOfPlayers)
        {
            Debug.Log("The game can start.");
            return true;
        }
        else
        {
            Debug.Log("The game needs more players.");
            return false;
        }
    }

    // Start the game.
    public void StartGame()
    {
        // Change the state of the game.
        currentGameState = GameState.Playing;

        // Select a random client to be the hunter.
        int randomIndex = UnityEngine.Random.Range(0, players.Count);
        SetHunter(players[randomIndex].playerId);
    }

    // Find the player whose game object matches the network ID.
    public Player FindPlayerWithGameObjectNetworkId(int gameObjectNetworkId)
    {
        foreach (Player player in players)
        {
            if (player.gameObjectNetworkId == gameObjectNetworkId)
            {
                return player;
            }
        }

        return null;
    }

    // Update the transform of a player game object.
    public void UpdateGameObjectTransform(int networkId)
    {  
        Player player = FindPlayerWithGameObjectNetworkId(networkId);

        // If the player is found, update the position and rotation of the game object.
        if (player != null)
        {
            ServerNetwork.NetworkObject networkObject = serverNet.GetNetObjById(networkId);
            player.position = networkObject.position;
            player.rotation = networkObject.rotation;

            Debug.Log("Gameobject " + networkId + " Position: " + player.position + " Rotation: " + player.rotation);
        }
    }
}

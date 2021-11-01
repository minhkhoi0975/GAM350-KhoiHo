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
        public string name = "";
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

    // Reference to game rules.
    public GameRules gameRules;

    // Return true when the cooldown reaches 0.
    bool canHunterGoHunting = false;

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

        // Initialization of the game rules.
        if(gameRules == null)
        {
            gameRules = new GameRules();
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
        if (newPlayer == null)
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
                // Let this client know about other clients' data.
                serverNet.CallRPC("NewPlayerConnected", aClientId, -1, players[i].playerId);
                serverNet.CallRPC("PlayerNameChanged", aClientId, -1, players[i].playerId, players[i].name);

                // Let other clients know about this client's data.
                serverNet.CallRPC("NewPlayerConnected", players[i].clientId, -1, newPlayer.playerId);
                serverNet.CallRPC("PlayerNameChanged", players[i].clientId, -1, newPlayer.playerId, newPlayer.name);
            }
        }
    }

    // A client has disconnected
    void OnClientDisconnected(long aClientId)
    {
        Player disconnectedPlayer = GetPlayerByClientId(aClientId);
        if (disconnectedPlayer == null)
            return;

        // Destroy the character of the disconnected client.
        serverNet.Destroy(disconnectedPlayer.gameObjectNetworkId);

        // Tell the clients that a player has left the game.
        serverNet.CallRPC("ClientDisconnected", UCNetwork.MessageReceiver.AllClients, -1, disconnectedPlayer.playerId);

        players.Remove(disconnectedPlayer);

        // If the disconnected player is the hunter, choose a random player to be the hunter.
        if (disconnectedPlayer == currentHunter)
        {
            StartGame();
        }

        Debug.Log("A client has disconnected from the game.");
    }

    // RPC from the client to set the name of thier player
    public void SetName(int playerId, string aName)
    {
        Player player = GetPlayerByPlayerId(playerId);
        if (player == null)
        {
            // If we can't find the player for this client, who are they? kick them
            Debug.Log("Unable to get player for unknown client " + serverNet.SendingClientId);
            serverNet.Kick(serverNet.SendingClientId);
            return;
        }

        player.name = aName;

        serverNet.CallRPC("PlayerNameChanged", UCNetwork.MessageReceiver.AllClients, -1, player.playerId, player.name);
        serverNet.CallRPC("SetName", UCNetwork.MessageReceiver.AllClients, player.gameObjectNetworkId, player.name);

        Debug.Log(player.playerId + " has set their name to " + player.name);
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

        foreach (Player player in players)
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

            // Reset the field of view of the client.
            serverNet.CallRPC("SetFieldOfView", currentHunter.clientId, -1, gameRules.preyFieldOfView);

            // Reset the movement speed of this character.
            serverNet.CallRPC("SetMovementSpeed", currentHunter.clientId, currentHunter.gameObjectNetworkId, gameRules.preyMovementSpeed);

            // Reset the appearance of this character.
            serverNet.CallRPC("SetMaterial", UCNetwork.MessageReceiver.AllClients, currentHunter.gameObjectNetworkId, false);
        }

        Player newHunter = GetPlayerByPlayerId(playerId);
        if (newHunter != null)
        {
            newHunter.isHunter = true;
            currentHunter = newHunter;

            // Set the field of view of the new hunter.
            serverNet.CallRPC("SetFieldOfView", currentHunter.clientId, -1, gameRules.hunterFieldOfView);

            // The new hunter cannot move until the cooldown reaches 0.
            serverNet.CallRPC("SetMovementSpeed", currentHunter.clientId, currentHunter.gameObjectNetworkId, 0.0f);

            // Change the appearance of the new hunter.
            serverNet.CallRPC("SetMaterial", UCNetwork.MessageReceiver.AllClients, currentHunter.gameObjectNetworkId, true);
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
        yield return new WaitForSeconds(gameRules.hunterCooldown);
        serverNet.CallRPC("SetMovementSpeed", currentHunter.clientId, currentHunter.gameObjectNetworkId, gameRules.hunterMovementSpeed);
        canHunterGoHunting = true;
    }

    // Called when a client successfully instantiates their network game object.
    public void PlayerGameObjectSpawned(int playerId, int gameObjectNetworkId)
    {
        // Let the server know the network id of the game object the client takes control.
        Player newPlayer = GetPlayerByPlayerId(playerId);
        if (newPlayer != null)
        {
            newPlayer.gameObjectNetworkId = gameObjectNetworkId;
            Debug.Log("Player " + playerId + " created a game object with network ID " + gameObjectNetworkId);

            // Set the movement speed of the character.
            serverNet.CallRPC("SetMovementSpeed", newPlayer.clientId, gameObjectNetworkId, gameRules.preyMovementSpeed);

            // Set the FOV of the client.
            serverNet.CallRPC("SetFieldOfView", newPlayer.clientId, -1, gameRules.preyFieldOfView);

            // Let the new player know other clients' name.
            foreach (Player otherPlayer in players)
            {
                if (otherPlayer != newPlayer)
                {
                    serverNet.CallRPC("SetName", newPlayer.clientId, otherPlayer.gameObjectNetworkId, otherPlayer.name);
                }
            }

            // Let the new player know who the hunter is.
            if (currentHunter != null)
            {
                serverNet.CallRPC("SetMaterial", newPlayer.clientId, currentHunter.gameObjectNetworkId, true);
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
        // Count the current number of players.
        int connectedPlayerCount = 0;
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].isConnected && players[i].gameObjectNetworkId != -1)
            {
                connectedPlayerCount++;
            }
        }

        return connectedPlayerCount >= minimumNumberOfPlayers;
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

    // Update the transform of a player game object.
    public void UpdateGameObjectTransform(int aNetworkId)
    {
        Player player = GetPlayerByGameObjectNetworkId(aNetworkId);

        // If the player is found, update the position and rotation of the game object.
        if (player != null)
        {
            ServerNetwork.NetworkObject networkObject = serverNet.GetNetObjById(aNetworkId);
            player.position = networkObject.position;
            player.rotation = networkObject.rotation;
        }
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

    // Get the player with the given player ID.
    public Player GetPlayerByPlayerId(int aPlayerId)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].playerId == aPlayerId)
            {
                return players[i];
            }
        }

        Debug.Log("Unable to get player with playerId" + aPlayerId);
        return null;
    }

    // Get the player whose game object matches the network ID.
    public Player GetPlayerByGameObjectNetworkId(int aNetworkId)
    {
        foreach (Player player in players)
        {
            if (player.gameObjectNetworkId == aNetworkId)
            {
                return player;
            }
        }

        Debug.Log("Unable to get player whose game object's network id is " + aNetworkId);
        return null;
    }
}
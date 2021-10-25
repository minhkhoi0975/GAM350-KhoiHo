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

    public GameObject playerObject;

    Dictionary<int, GameObject> playerGameObjects = new Dictionary<int, GameObject>();

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
        public int gameObjectNetworkId;
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
    public float hunterCooldown = 3.0f;
    float currentHunterCooldown = 0.0f;

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
        Player newPlayer = null;

        foreach (Player player in players)
        {
            if (player.clientId == aClientId)
            {
                player.isConnected = true;
                player.playerId = lastPlayerId++;
                serverNet.CallRPC("SetPlayerId", aClientId, -1, player.playerId);

                newPlayer = player;
            }
        }

        if (newPlayer == null)
        {
            Debug.Log("OnClientConnected: Unable to find unknown player for client " + aClientId);
            return;
        }

        // A client has connected, send the data about other connected clients
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].clientId != aClientId)
            {
                // Tell the other client that this player has connected
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
        // Get the network object information, store in a dictionary
        ServerNetwork.NetworkObject obj = serverNet.GetNetObjById(aObjectData.netObjId);
        playerGameObjects[aObjectData.netObjId] = Instantiate(playerObject, obj.position, obj.rotation);

        if (CanGameStart())
        {
            int randomPlayerId = UnityEngine.Random.Range(0, players.Count);
            ChangeHunter(randomPlayerId);
        }
    }

    // A client has been added to a new area
    void OnAddArea(ServerNetwork.AreaChangeInfo aInfo)
    {

    }

    // An object has been added to a new area
    void AddedObjectToArea(int aNetworkId)
    {

    }

    // Initialization data should be sent to a network object
    void InitializeNetworkObject(ServerNetwork.InitializationInfo aInfo)
    {

    }

    // A game object has been destroyed
    void OnDestroyNetworkObject(int aObjectId)
    {

    }

    // Set the name of a client.
    public void SetName(string aName)
    {
        Player player = GetPlayerByClientId(serverNet.SendingClientId);
        if (player == null)
        {
            // If we can't find the player for this client, who are they? kick them
            Debug.Log("Unable to get player for unknown client " + serverNet.SendingClientId);
            serverNet.Kick(serverNet.SendingClientId);
        }

        player.name = aName;
        serverNet.CallRPC("PlayerNameChanged", UCNetwork.MessageReceiver.AllClients, -1, player.playerId, player.name);
    }

    private void Update()
    {
        // Dictionary<int, ServerNetwork.NetworkObject> allObjs = serverNet.GetAllObjects();

        // Update the player cooldown.
        if(hunterCooldown > 0.0f)
        {
            hunterCooldown -= Time.deltaTime;
        }

        // Check whether the hunter catches a prey.
        Player prey;
        if (hunterCooldown <= 0.0f && HunterCatchesPrey(out prey))
        {
            ChangeHunter(prey.playerId);
        }
    }

    private void FixedUpdate()
    {

    }

    public void NetObjectUpdated(int aNetId)
    {
        Debug.Log("Object has been updated:" + aNetId);

        ServerNetwork.NetworkObject obj = serverNet.GetNetObjById(aNetId);
        playerGameObjects[aNetId].transform.position = obj.position;
        playerGameObjects[aNetId].transform.localRotation = obj.rotation;

        /*
        //...
        long ownerClientId = serverNet.GetOwnerClientId(aNetId);
        serverNet.CallRPC("IsHunter", ownerClientId, aNetId, false);
        //...
        //serverNet.CallRPC("IsHunter", otherClientId, otherNetId, true);
        */

        // Update the position of the player game object.
        UpdateGameObjectTransform(aNetId);
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
                return true;
            }
        }

        prey = null;
        return false;
    }

    // Change the hunter.
    public void ChangeHunter(int playerId)
    {
        // The current hunter is no longer a hunter.
        if (currentHunter != null)
        {
            currentHunter.isHunter = false;
        }

        // Make the player that matches the ID become the next hunter.
        foreach(Player player in players)
        {
            if(player.playerId == playerId)
            {
                player.isHunter = true;
                currentHunter = player;
                break;
            }
        }

        // Set cooldown for the new hunter.
        currentHunterCooldown = hunterCooldown;

        Debug.Log("Player " + playerId + " has become a hunter.");
        serverNet.CallRPC("ChangeHunter", UCNetwork.MessageReceiver.AllClients, -1, playerId);
    }

    // Can the game start?
    public bool CanGameStart()
    {
        int connectedPlayerCount = 0;
        foreach(Player player in players)
        {
            if(player.isConnected)
            {
                connectedPlayerCount++;
            }
        }

        Debug.Log("Number of connected players: " + connectedPlayerCount);
        if (connectedPlayerCount > 1)
        {
            Debug.Log("The game can start.");
            return true;
        }
        else
        {
            Debug.Log("The game needs 1 more player.");
            return false;
        }
    }

    // Set the game object network ID of a player.
    public void SetGameObjectNetworkId(int playerId, int gameObjectNetworkId)
    {
        foreach(Player player in players)
        {
            if(player.playerId == playerId)
            {
                player.gameObjectNetworkId = gameObjectNetworkId;
            }
            return;
        }
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
            ServerNetwork.NetworkObject obj = serverNet.GetNetObjById(networkId);
            player.position = obj.position;
            player.rotation = obj.rotation;

            Debug.Log("Gameobject " + networkId + " Position: " + player.position + " Rotation: " + player.rotation);
        }
    }
}

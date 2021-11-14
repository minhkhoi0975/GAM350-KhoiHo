﻿/**
 * TagServer.cs
 * Description: This script handles the logic of the game on the server side.
 * Programmer: Khoi Ho
 * Credit(s): Professor Carrigg (for providing the example source code).
 */

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
        public int teamId = 0;     // 1 = shooter, 2 = spawner

        // Data about character game object.
        public int shooterObjNetId = -1;     // If the client is a shooter, this is the network id of their character game object.
        public Vector3 position;
        public Quaternion rotation;
    }

    // List of all players in the game.
    List<Player> players = new List<Player>();

    // The player ID of the client that has just connected to the server.
    static int lastPlayerId = 0;

    // Reference to game rules.
    public GameRules gameRules;

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
        newPlayer.teamId = 0;

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
        newPlayer.teamId = GetTeamIdForNewPlayer();
        serverNet.CallRPC("SetTeamId", aClientId, -1, newPlayer.teamId);

        // A client has connected, send the data about other connected clients
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].clientId != aClientId)
            {
                // Let this client know about other clients' data.
                serverNet.CallRPC("NewPlayerConnected", aClientId, -1, players[i].playerId, players[i].teamId);
                serverNet.CallRPC("PlayerNameChanged", aClientId, -1, players[i].playerId, players[i].name);

                // Let other clients know about this client's data.
                serverNet.CallRPC("NewPlayerConnected", players[i].clientId, -1, newPlayer.playerId, newPlayer.teamId);
                serverNet.CallRPC("PlayerNameChanged", players[i].clientId, -1, newPlayer.playerId, newPlayer.name);
            }
        }
    }

    public int GetTeamIdForNewPlayer()
    {
        int shooterCount = 0;
        int spawnerCount = 0;

        foreach(Player player in players)
        {
            if(player.teamId == 1)
            {
                shooterCount++;
            }
            else if(player.teamId == 2)
            {
                spawnerCount++;
            }
        }

        return shooterCount <= spawnerCount ? 1 : 2;
    }

    // A client has disconnected from the game.
    void OnClientDisconnected(long aClientId)
    {
        Player disconnectedPlayer = GetPlayerByClientId(aClientId);
        if (disconnectedPlayer == null)
            return;

        // Tell the clients that a player has left the game.
        serverNet.CallRPC("ClientDisconnected", UCNetwork.MessageReceiver.AllClients, -1, disconnectedPlayer.playerId);

        // Remove the disconnected player from the list of players.
        players.Remove(disconnectedPlayer);

        Debug.Log("A client has disconnected from the game.");
    }

    // A network object has been instantiated by a client
    void OnInstantiateNetworkObject(ServerNetwork.IntantiateObjectData aObjectData)
    {
        Debug.Log("Network object " + aObjectData.netObjId + " has been instantiated.");
    }

    // A client has been added to a new area
    void OnAddArea(ServerNetwork.AreaChangeInfo aInfo)
    {
        Debug.Log(aInfo.networkId + " has been added to area " + aInfo.areaId);
    }

    // An object has been added to a new area
    void AddedObjectToArea(int aNetworkId)
    {
        Debug.Log("Network object " + aNetworkId + " has been added to a new area.");
    }

    // Initialization data should be sent to a network object
    void InitializeNetworkObject(ServerNetwork.InitializationInfo aInfo)
    {
        Debug.Log("Network object " + aInfo.netObjId + " has been initialized.");
    }

    // A game object has been destroyed
    void OnDestroyNetworkObject(int aObjectId)
    {
        Debug.Log("Network object " + aObjectId + " has been destroyed.");
    }

    // RPC from the client to set the name of their player
    public void SetName(int aPlayerId, string aName)
    {
        Player player = GetPlayerByPlayerId(aPlayerId);
        if (player == null)
        {
            // If we can't find the player for this client, who are they? kick them
            Debug.Log("Unable to get player for unknown client " + serverNet.SendingClientId);
            serverNet.Kick(serverNet.SendingClientId);
            return;
        }

        player.name = aName;

        serverNet.CallRPC("PlayerNameChanged", UCNetwork.MessageReceiver.AllClients, -1, player.playerId, player.name);
        serverNet.CallRPC("SetName", UCNetwork.MessageReceiver.AllClients, player.shooterObjNetId, player.name);

        Debug.Log(player.playerId + " has set their name to " + player.name);
    }

    public void NetObjectUpdated(int aNetId)
    {
        // Update the position of the player game object.
        UpdateGameObjectTransform(aNetId);
    }

    // Called when a new client successfully instantiates their character game object.
    public void PlayerGameObjectSpawned(int aPlayerId, int aNetObjId)
    {
        // Let the server know the network id of the game object the client takes control.
        Player newPlayer = GetPlayerByPlayerId(aPlayerId);
        if (newPlayer != null)
        {
            newPlayer.shooterObjNetId = aNetObjId;
            Debug.Log("Player " + aPlayerId + " created a game object with network ID " + aNetObjId);

            // Set the movement speed of the character.
            serverNet.CallRPC("SetMovementSpeed", newPlayer.clientId, aNetObjId, gameRules.preyMovementSpeed);

            // Let the new player know other clients' name.
            foreach (Player otherPlayer in players)
            {
                if (otherPlayer != newPlayer)
                {
                    serverNet.CallRPC("SetName", newPlayer.clientId, otherPlayer.shooterObjNetId, otherPlayer.name);
                }
            }
        }
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

    // Spawn a projectile.
    public void SpawnProjectile(int gameObjNetId, Vector3 position, Vector3 direction)
    {
        Player player = GetPlayerByGameObjectNetworkId(gameObjNetId);
        if (player == null)
            return;

        serverNet.CallRPC("SpawnProjectileOnline", player.clientId, gameObjNetId, position, direction);
    }

    // Spawn an NPC.
    public void SpawnNPC(int playerId, Vector3 position)
    {
        Player player = GetPlayerByPlayerId(playerId);
        if (player == null)
            return;

        if (player.teamId == 2)
        {
            serverNet.CallRPC("SpawnNPC", players[playerId].clientId, -1, position);
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
            if (player.shooterObjNetId == aNetworkId)
            {
                return player;
            }
        }

        Debug.Log("Unable to get player whose game object's network id is " + aNetworkId);
        return null;
    }
}
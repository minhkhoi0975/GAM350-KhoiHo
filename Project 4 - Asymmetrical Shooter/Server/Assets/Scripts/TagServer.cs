/**
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

    // List of all players in the game.
    List<PlayerData> players = new List<PlayerData>();

    // The ID for the client that has just connected to the server.
    static int newPlayerId = 0;

    // Reference to game rules.
    public GameRules gameRules;

    // All information about hitboxes are stored here.
    [SerializeField] HitboxData hitboxData;

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
        if (gameRules == null)
        {
            gameRules = new GameRules();
        }

        //serverNet.EnableLogging("rpcLog.txt");
    }

    // ---------------------------
    // CALLBACK FUNCTIONS
    // ---------------------------

    // A client has just requested to connect to the server
    void ConnectionRequest(ServerNetwork.ConnectionRequestInfo data)
    {
        Debug.Log("Connection request from " + data.username);

        // Set the info of the client.
        PlayerData newPlayer = new PlayerData();
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
        PlayerData newPlayer = GetPlayerByClientId(aClientId);
        if (newPlayer == null)
        {
            Debug.Log("OnClientConnected: Unable to find unknown player for client " + aClientId);
            return;
        }

        newPlayer.isConnected = true;

        // Set player ID for the new player.
        newPlayer.playerId = newPlayerId++;
        serverNet.CallRPC("SetPlayerId", aClientId, -1, newPlayer.playerId);

        // Set team ID for the new player.
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

        foreach (PlayerData player in players)
        {
            if (player.teamId == 1)
            {
                shooterCount++;
            }
            else if (player.teamId == 2)
            {
                spawnerCount++;
            }
        }

        return shooterCount <= spawnerCount ? 1 : 2;
    }

    // A client has disconnected from the game.
    void OnClientDisconnected(long aClientId)
    {
        PlayerData disconnectedPlayer = GetPlayerByClientId(aClientId);
        if (disconnectedPlayer == null)
            return;

        // Make sure that the shooter's character object is completely removed, in case the client fails to do so.
        if (disconnectedPlayer.shooterObjNetId != -1)
        {
            ServerNetwork.NetworkObject shooter = serverNet.GetNetObjById(disconnectedPlayer.shooterObjNetId);
            if (shooter != null)
            {
                serverNet.Destroy(shooter.networkId);
            }

            serverNet.Destroy(disconnectedPlayer.shooterObjNetId);
        }

        // Tell the clients that a player has left the game.
        serverNet.CallRPC("ClientDisconnected", UCNetwork.MessageReceiver.AllClients, -1, disconnectedPlayer.playerId);

        // Remove the disconnected player from the list of players.
        players.Remove(disconnectedPlayer);

        Debug.Log("A client has disconnected from the game.");
    }

    // A network object has been instantiated by a client
    void OnInstantiateNetworkObject(ServerNetwork.IntantiateObjectData aObjectData)
    {
        InstantiateHitbox(aObjectData);

        Debug.Log("Network object " + aObjectData.netObjId + " has been instantiated.");
    }

    // Instantiate a hitbox on server side.
    void InstantiateHitbox(ServerNetwork.IntantiateObjectData aObjectData)
    {
        ServerNetwork.NetworkObject networkObject = serverNet.GetNetObjById(aObjectData.netObjId);

        // If the instantiated network object is a projectile, create a hitbox for the projectile.
        if (networkObject.prefabName == "Projectile")
        {
            hitboxData.ProjectileHitboxes[networkObject.networkId] = new Projectile(Instantiate(hitboxData.ProjectileHitboxPrefab, networkObject.position, networkObject.rotation), gameRules.projectileDamage);
        }

        // If the instantiated network object is a character (either a shooter or an NPC), create a hitbox for the character.
        else if (networkObject.prefabName == "Shooter")
        {
            hitboxData.ShooterHitboxes[networkObject.networkId] = new Character(Instantiate(hitboxData.CharacterHitboxPrefab, networkObject.position, networkObject.rotation), gameRules.shooterHealth);
        }
        else if(networkObject.prefabName == "NPC")
        {
            hitboxData.NPCHitboxes[networkObject.networkId] = new Character(Instantiate(hitboxData.CharacterHitboxPrefab, networkObject.position, networkObject.rotation), gameRules.npcHealth);
        }
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
        DestroyHitbox(aObjectId);

        Debug.Log("Network object " + aObjectId + " has been destroyed.");
    }

    // Destroy a hitbox if it exists.
    void DestroyHitbox(int aNetId)
    {
        ServerNetwork.NetworkObject networkObject = serverNet.GetNetObjById(aNetId);

        if (hitboxData.ProjectileHitboxes.ContainsKey(aNetId))
        {
            Destroy(hitboxData.ProjectileHitboxes[aNetId]);
            hitboxData.ProjectileHitboxes.Remove(aNetId);
        }
        else if (hitboxData.ShooterHitboxes.ContainsKey(aNetId))
        {
            Destroy(hitboxData.ShooterHitboxes[aNetId]);
            hitboxData.ShooterHitboxes.Remove(aNetId);
        }
        else if (hitboxData.NPCHitboxes.ContainsKey(aNetId))
        {
            Destroy(hitboxData.NPCHitboxes[aNetId]);
            hitboxData.NPCHitboxes.Remove(aNetId);
        }
    }

    // Called when a network object is updated.
    public void NetObjectUpdated(int aNetId)
    {
        // Update the transform of a hitbox.
        UpdateHitboxTransform(aNetId);

        // Check collision of a projectile.
        CheckProjectileCollision(aNetId);
    }

    // Update the transform of a hitbox.
    void UpdateHitboxTransform(int aNetId)
    {
        ServerNetwork.NetworkObject networkObject = serverNet.GetNetObjById(aNetId);

        if (hitboxData.ProjectileHitboxes.ContainsKey(aNetId))
        {
            hitboxData.ProjectileHitboxes[aNetId].gameObject.transform.position = networkObject.position;
            hitboxData.ProjectileHitboxes[aNetId].gameObject.transform.rotation = networkObject.rotation;
        }
        else if (hitboxData.ShooterHitboxes.ContainsKey(aNetId))
        {
            hitboxData.ShooterHitboxes[aNetId].gameObject.transform.position = networkObject.position;
            hitboxData.ShooterHitboxes[aNetId].gameObject.transform.rotation = networkObject.rotation;
        }
        else if (hitboxData.NPCHitboxes.ContainsKey(aNetId))
        {
            hitboxData.NPCHitboxes[aNetId].gameObject.transform.position = networkObject.position;
            hitboxData.NPCHitboxes[aNetId].gameObject.transform.rotation = networkObject.rotation;
        }
    }

    // Check collision of a projectile.
    void CheckProjectileCollision(int aProjectileNetId)
    {
        if (!hitboxData.ProjectileHitboxes.ContainsKey(aProjectileNetId) || !hitboxData.ProjectileHitboxes[aProjectileNetId].gameObject)
            return;

        SphereCollider projectileCollider = hitboxData.ProjectileHitboxes[aProjectileNetId].gameObject.GetComponent<SphereCollider>();
        
        // Get all characters that collide with the projectile.
        // The layer mask of characters is 6.
        List<Collider> projectileOverlap = new List<Collider>(Physics.OverlapSphere(projectileCollider.transform.position, projectileCollider.radius, 1 << 6));
        if (projectileOverlap.Count == 0)
            return;

        // Check if the projectile hits a shooter.
        foreach (KeyValuePair<int, Character> shooterHitbox in hitboxData.ShooterHitboxes)
        {
            if (projectileOverlap.Contains(shooterHitbox.Value.gameObject.GetComponent<Collider>()))
            {
                // Reduce health of the shooter.
                shooterHitbox.Value.health -= gameRules.projectileDamage;

                // If the shooter dies, kick them out of server.
                if(shooterHitbox.Value.health <= 0)
                {
                    PlayerData kickedPlayer = GetPlayerByGameObjectNetworkId(shooterHitbox.Key);
                    serverNet.Kick(kickedPlayer.clientId);
                }

                // Remove the projectile hitbox.
                Destroy(hitboxData.ProjectileHitboxes[aProjectileNetId]);
                hitboxData.ProjectileHitboxes.Remove(aProjectileNetId);

                // Destroy the projectile.
                serverNet.Destroy(aProjectileNetId);

                return;
            }
        }

        // Check if the projectile hits an NPC.
        foreach (KeyValuePair<int, Character> npcHitBox in hitboxData.NPCHitboxes)
        {
            if (projectileOverlap.Contains(npcHitBox.Value.gameObject.GetComponent<Collider>()))
            {
                // Reduce health of the npc.
                npcHitBox.Value.health -= gameRules.projectileDamage;

                // If the NPC dies, destroy the NPC.
                if (npcHitBox.Value.health <= 0)
                {
                    serverNet.Destroy(npcHitBox.Key);
                    DestroyHitbox(npcHitBox.Key);
                }

                // Remove the projectile hitbox.
                Destroy(hitboxData.ProjectileHitboxes[aProjectileNetId]);
                hitboxData.ProjectileHitboxes.Remove(aProjectileNetId);

                // Destroy the projectile.
                serverNet.Destroy(aProjectileNetId);

                return;
            }
        }
    }

    // -----------------------
    // RPC FUNCTIONS
    // -----------------------

    // RPC from a client to set their name.
    public void SetName(int aPlayerId, string aName)
    {
        PlayerData player = GetPlayerByPlayerId(aPlayerId);
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

    // RPC when a shooter successfully instantiates their character game object.
    public void ShooterGameObjectSpawned(int aPlayerId, int aNetObjId)
    {
        // Let the server know the network id of the game object the client takes control.
        PlayerData newPlayer = GetPlayerByPlayerId(aPlayerId);
        if (newPlayer != null)
        {
            newPlayer.shooterObjNetId = aNetObjId;
            Debug.Log("Player " + aPlayerId + " created a game object with network ID " + aNetObjId);

            // Set the movement speed of the shooter.
            serverNet.CallRPC("SetMovementSpeed", newPlayer.clientId, aNetObjId, gameRules.preyMovementSpeed);

            // Let the new player know other clients' name.
            foreach (PlayerData otherPlayer in players)
            {
                if (otherPlayer != newPlayer)
                {
                    serverNet.CallRPC("SetName", newPlayer.clientId, otherPlayer.shooterObjNetId, otherPlayer.name);
                }
            }
        }
    }

    // RPC to spawn a projectile.
    public void SpawnProjectile(int characterNetworkId, Vector3 position, Quaternion rotation)
    {
        // Check if the RPC is from a shooter.
        PlayerData player = GetPlayerByGameObjectNetworkId(characterNetworkId);
        if (player != null && player.teamId == 1)
        {
            ServerNetwork.NetworkObject projectile = serverNet.InstantiateNetworkObject("Projectile", position, rotation, player.clientId, "");
            serverNet.AddObjectToArea(projectile.networkId, 1);
        }

        // Check if the RPC is from an NPC.
        else if(hitboxData.NPCHitboxes.ContainsKey(characterNetworkId))
        {
            ServerNetwork.NetworkObject projectile = serverNet.InstantiateNetworkObject("Projectile", position, rotation, serverNet.SendingClientId, "");
            serverNet.AddObjectToArea(projectile.networkId, 1);
        }
    }

    // RPC to spawn an NPC.
    public void SpawnNPC(int playerId, Vector3 position)
    {
        PlayerData player = GetPlayerByPlayerId(playerId);
        if (player == null)
            return;

        // The player can only spawn an NPC if:
        // + The player is a spawner.
        // + Number of NPCs < Number of Shooters * NPC Count Multiplier.
        if (player.teamId == 2 && hitboxData.NPCHitboxes.Count < hitboxData.ShooterHitboxes.Count * gameRules.npcCountMultiplier)
        {
            ServerNetwork.NetworkObject npc = serverNet.InstantiateNetworkObject("NPC", position, Quaternion.identity, player.clientId, "");
            serverNet.AddObjectToArea(npc.networkId, 1);
        }
    }

    // ---------------------
    // UTILITY FUNCTIONS
    // ---------------------

    // Get the player for the given client id
    PlayerData GetPlayerByClientId(long aClientId)
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
    public PlayerData GetPlayerByPlayerId(int aPlayerId)
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
    public PlayerData GetPlayerByGameObjectNetworkId(int aNetworkId)
    {
        foreach (PlayerData player in players)
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
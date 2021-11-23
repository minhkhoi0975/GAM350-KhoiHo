/**
 * TagServer.cs
 * Description: This script handles the logic of the game on the server side.
 * Programmer: Khoi Ho
 * Credit(s): Professor Carrigg (for providing the example source code).
 */

using System.Collections.Generic;
using UnityEngine;

public class ASServer : MonoBehaviour
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

        // If the disconnected client is a shooter, destroy their character.
        if (disconnectedPlayer.shooterObjNetId != -1)
        {
            ServerNetwork.NetworkObject shooter = serverNet.GetNetObjById(disconnectedPlayer.shooterObjNetId);
            if (shooter != null)
            {
                serverNet.Destroy(shooter.networkId);
            }
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
        Debug.Log("Network object " + aObjectData.netObjId + " has been instantiated.");
    }

    // A client has been added to a new area
    void OnAddArea(ServerNetwork.AreaChangeInfo aInfo)
    {
        Debug.Log("Client " + aInfo.id + " has been added to area " + aInfo.areaId);

        // Update the name tags of the shooters.
        foreach (PlayerData player in players)
        {
            if (player.teamId == 1 && player.clientId != aInfo.id)
            {
                serverNet.CallRPC("SetNameTag", aInfo.id, player.shooterObjNetId, "[" + player.name + "]");
            }
        }

        // Tell the new client about the movement speed of the NPCs.
        foreach (ServerNetwork.NetworkObject npc in serverNet.GetNetObjsByPrefab("NPC"))
        {
            if (npc != null)
            {
                serverNet.CallRPC("SetMovementSpeed", aInfo.id, npc.networkId, gameRules.npcMovementSpeed);
            }
        }
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
        // Destroy a hitbox if it exists.
        DestroyHitbox(aObjectId);

        Debug.Log("Network object " + aObjectId + " has been destroyed.");
    }

    void DestroyHitbox(int aNetId)
    {
        ServerNetwork.NetworkObject networkObject = serverNet.GetNetObjById(aNetId);
        if (networkObject == null)
            return;

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

    void UpdateHitboxTransform(int aNetId)
    {
        ServerNetwork.NetworkObject networkObject = serverNet.GetNetObjById(aNetId);

        if (hitboxData.ProjectileHitboxes.ContainsKey(aNetId))
        {
            hitboxData.ProjectileHitboxes[aNetId].hitboxGameObject.transform.position = networkObject.position;
            hitboxData.ProjectileHitboxes[aNetId].hitboxGameObject.transform.rotation = networkObject.rotation;
        }
        else if (hitboxData.ShooterHitboxes.ContainsKey(aNetId))
        {
            hitboxData.ShooterHitboxes[aNetId].hitboxGameObject.transform.position = networkObject.position;
            hitboxData.ShooterHitboxes[aNetId].hitboxGameObject.transform.rotation = networkObject.rotation;
        }
        else if (hitboxData.NPCHitboxes.ContainsKey(aNetId))
        {
            hitboxData.NPCHitboxes[aNetId].hitboxGameObject.transform.position = networkObject.position;
            hitboxData.NPCHitboxes[aNetId].hitboxGameObject.transform.rotation = networkObject.rotation;
        }
    }

    void CheckProjectileCollision(int aProjectileNetId)
    {
        if (!hitboxData.ProjectileHitboxes.ContainsKey(aProjectileNetId) || !hitboxData.ProjectileHitboxes[aProjectileNetId].hitboxGameObject)
            return;

        SphereCollider projectileCollider = hitboxData.ProjectileHitboxes[aProjectileNetId].hitboxGameObject.GetComponent<SphereCollider>();

        // Get all characters that collide with the projectile.
        // The layer mask of characters is 6.
        List<Collider> projectileOverlap = new List<Collider>(Physics.OverlapSphere(projectileCollider.transform.position, projectileCollider.radius, 1 << 6));
        if (projectileOverlap.Count == 0)
            return;

        // Get the client id of the projectile's instigator.
        int projectileInstigatorNetId = hitboxData.ProjectileHitboxes[aProjectileNetId].instigatorNetworkId;

        // This variable is used to prevent the projectile from colliding the character that fires it.
        bool projectileHitsTarget = false;

        // Check if the projectile hits a shooter.
        foreach (KeyValuePair<int, CharacterHitbox> shooterHitbox in hitboxData.ShooterHitboxes)
        {
            Collider shooterCollider = shooterHitbox.Value.hitboxGameObject.GetComponent<Collider>();

            if (projectileOverlap.Contains(shooterCollider) && projectileInstigatorNetId != shooterHitbox.Key)
            {
                // Get the client id of the shooter that takes damage.
                PlayerData hitPlayer = GetPlayerByShooterObjNetId(shooterHitbox.Key);

                // Reduce health of the shooter.
                shooterHitbox.Value.health -= gameRules.projectileDamage;
                serverNet.CallRPC("SetHealth", hitPlayer.clientId, -1, shooterHitbox.Value.health);

                // If the shooter dies, kick them out of server.
                if (shooterHitbox.Value.health <= 0)
                {
                    serverNet.Kick(hitPlayer.clientId);
                }

                projectileHitsTarget = true;
                break;
            }
        }

        // Check if the projectile hits an NPC.
        foreach (KeyValuePair<int, CharacterHitbox> npcHitbox in hitboxData.NPCHitboxes)
        {
            Collider npcCollider = npcHitbox.Value.hitboxGameObject.GetComponent<Collider>();

            if (projectileOverlap.Contains(npcCollider) && projectileInstigatorNetId != npcHitbox.Key)
            {
                // Reduce health of the npc.
                npcHitbox.Value.health -= gameRules.projectileDamage;

                // If the NPC dies, destroy the NPC.
                if (npcHitbox.Value.health <= 0)
                {
                    serverNet.Destroy(npcHitbox.Key);
                    DestroyHitbox(npcHitbox.Key);
                }

                projectileHitsTarget = true;
                break;
            }
        }

        if (projectileHitsTarget)
        {
            // Remove the projectile hitbox.
            Destroy(hitboxData.ProjectileHitboxes[aProjectileNetId]);
            hitboxData.ProjectileHitboxes.Remove(aProjectileNetId);

            // Destroy the projectile.
            serverNet.Destroy(aProjectileNetId);
        }
    }

    // -----------------------
    // RPC FUNCTIONS
    // -----------------------

    // RPC from a client to set their name.
    public void SetName(string aName)
    {
        PlayerData player = GetPlayerByClientId(serverNet.SendingClientId);
        if (player == null)
        {
            // If we can't find the player for this client, who are they? kick them
            Debug.Log("Unable to get player for unknown client " + serverNet.SendingClientId);
            serverNet.Kick(serverNet.SendingClientId);
            return;
        }

        player.name = aName;

        serverNet.CallRPC("PlayerNameChanged", UCNetwork.MessageReceiver.AllClients, -1, player.playerId, player.name);

        Debug.Log(player.playerId + " has set their name to " + player.name);
    }

    // RPC to spawn a shooter.
    public void SpawnShooter(Vector3 position, Quaternion rotation)
    {
        PlayerData player = GetPlayerByClientId(serverNet.SendingClientId);
        if (player == null || player.teamId != 1)
            return;

        // Instantiate a character for the shooter.
        ServerNetwork.NetworkObject shooter = serverNet.InstantiateNetworkObject("Shooter", position, rotation, serverNet.SendingClientId, "");
        if (shooter == null)
        {
            // Kick the player out of the game as their character cannot be spawned.
            serverNet.Kick(player.clientId);
            return;
        }

        player.shooterObjNetId = shooter.networkId;

        // Instantiate a hitbox for the shooter on the server side.
        hitboxData.ShooterHitboxes[shooter.networkId] = new CharacterHitbox(Instantiate(hitboxData.CharacterHitboxPrefab, shooter.position, shooter.rotation), gameRules.shooterHealth);

        serverNet.AddObjectToArea(shooter.networkId, 1);

        // Set the name tag of the character.
        serverNet.CallRPC("SetNameTag", UCNetwork.MessageReceiver.AllClients, player.shooterObjNetId, "[" + player.name + "]");

        // Set the movement speed of the character.
        serverNet.CallRPC("SetMovementSpeed", player.clientId, player.shooterObjNetId, gameRules.shooterMovementSpeed);

        // Tell the client that their character has been successfully spawned.
        serverNet.CallRPC("ShooterSpawned", player.clientId, -1, shooter.networkId);
    }

    // RPC to spawn a projectile.
    public void SpawnProjectile(int characterNetworkId, Vector3 position, Quaternion rotation)
    {
        // Instantiate a projectile game object.
        ServerNetwork.NetworkObject projectile = serverNet.InstantiateNetworkObject("Projectile", position, rotation, serverNet.SendingClientId, "");
        if (projectile == null)
            return;

        // Instantiate a hitbox for the projectile on server side.
        hitboxData.ProjectileHitboxes[projectile.networkId] = new ProjectileHitbox(Instantiate(hitboxData.ProjectileHitboxPrefab, projectile.position, projectile.rotation), gameRules.projectileDamage, characterNetworkId);

        serverNet.AddObjectToArea(projectile.networkId, 1);
    }

    // RPC to spawn an NPC.
    public void SpawnNPC(Vector3 position, Quaternion rotation)
    {
        PlayerData player = GetPlayerByClientId(serverNet.SendingClientId);
        if (player == null)
            return;

        // The player can only spawn an NPC if:
        // + The player is a spawner.
        // + The current NPC-shooter ratio does not exceeed maxNpcShooterRatio.
        if (player.teamId == 2 && hitboxData.NPCHitboxes.Count < hitboxData.ShooterHitboxes.Count * gameRules.maxNpcShooterRatio)
        {
            // Instantiate an NPC.
            ServerNetwork.NetworkObject npc = serverNet.InstantiateNetworkObject("NPC", position, rotation, player.clientId, "");
            if (npc == null)
                return;

            // Instantiate the hitbox of the NPC on the client side.
            hitboxData.NPCHitboxes[npc.networkId] = new CharacterHitbox(Instantiate(hitboxData.CharacterHitboxPrefab, position, rotation), gameRules.npcHealth);

            serverNet.AddObjectToArea(npc.networkId, 1);

            // Set the movement speed of the NPC.
            // The RPC is sent to all clients so that the movement speed of the NPCs is maintained after their ownership is transferred to another client.
            serverNet.CallRPC("SetMovementSpeed", UCNetwork.MessageReceiver.AllClients, npc.networkId, gameRules.npcMovementSpeed);
        }
    }

    // RPC to send a message to clients.
    // All clients: messageType=1. Team members only: messageType=2.
    public void SendChatMessage(string message, int messageType)
    {
        PlayerData player = GetPlayerByClientId(serverNet.SendingClientId);
        if (player == null)
            return;

        // Send the message the all clients.
        if (messageType == 1)
        {
            string messageToSend = player.name + ": " + message;
            serverNet.CallRPC("ReceiveChatMessage", UCNetwork.MessageReceiver.AllClients, -1, messageToSend);
        }

        // Send the message to team members only.
        else if (messageType == 2)
        {
            string messageToSend = player.name + " [TEAM]: " + message;
            foreach (PlayerData player2 in players)
            {
                if (player2.teamId == player.teamId)
                {
                    serverNet.CallRPC("ReceiveChatMessage", player2.clientId, -1, messageToSend);
                }
            }
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
        serverNet.Kick(aClientId);
        return null;
    }

    // Get the player whose game object matches the network ID.
    public PlayerData GetPlayerByShooterObjNetId(int aNetworkId)
    {
        if (aNetworkId <= 0)
            return null;

        foreach (PlayerData player in players)
        {
            if (player.shooterObjNetId == aNetworkId)
            {
                return player;
            }
        }

        return null;
    }
}
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LitJson;

/*
This server runs a simple tactics game, as outlined for the "Tactics Client" assignment for GAM-350
Author: David Carrigg
*/
public class TacticsServer : MonoBehaviour
{
    // Singleton instance of this class
    public static TacticsServer instance;

    // Instance of the server network
    public ServerNetwork serverNet;

    // New Hampshire pride
    public int portNumber = 603;

    // The three character types a client can choose
    enum PlayerClass
    {
        Warrior = 1,
        Rogue = 2,
        Wizard = 3
    }

    // Data about a specific client/player
    class Player
    {
        public long clientId; // Unique client id
        public bool isConnected; // Is this client connected, false if they have been removed from the game

        public int playerId; // Assigned player id
        public int teamId; // Team id

        // Data that the client is allowed to set
        public string name;
        public bool isReady;
        public PlayerClass playerClass;

        // Position on the board
        public int maxMoves;
        public int movesRemaining;
        public int xPosition;
        public int yPosition;

        // Attack data
        public int attackRange;

        // Health
        public int maxHealth;
        public int health;
    }
    List<Player> players = new List<Player>();
    static int lastPlayerId = 0;

    // What "state" is the game in
    enum GameState
    {
        Login,
        Starting,
        Playing
    }
    GameState currentState = GameState.Login;

    // The state of the board
    // Each location should have a player id in it, 0 if it is empty, or -1 if it is blocked
    int[,] boardState;
    int activePlayerIndex = -1;

    // Min and max sizes the map can be
    int maxMapSize = 15;
    int minMapSize = 8;
    // Size of the map
    int mapXSize = 0;
    int mapYSize = 0;

    // How long to wait before starting the game
    int gameStartWaitTime = 2;
    float maxTurnTime = 10;
    float currentTurnTime = 0;

    // Use this for initialization
    void Awake()
    {
        instance = this;

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
    }

    // A client has just requested to connect to the server
    void ConnectionRequest(ServerNetwork.ConnectionRequestInfo data)
    {
        Debug.Log("Connection request from IP " + data.connection.RemoteEndPoint.Address);

        // Approve connections as long as we are in the login state
        if (currentState == GameState.Login)
        {
            Player newPlayer = new Player();
            newPlayer.clientId = data.id;
            newPlayer.isConnected = false;
            newPlayer.name = "";
            newPlayer.isReady = false;
            newPlayer.playerClass = 0;
            newPlayer.teamId = 0;
            newPlayer.xPosition = 0;
            newPlayer.yPosition = 0;
            players.Add(newPlayer);

            serverNet.ConnectionApproved(data.id);
        }
        else
        {
            serverNet.ConnectionDenied(data.id);
        }
    }

    // Get the next appropriate team id
    int GetTeamId()
    {
        int team1 = 0;
        int team2 = 0;
        foreach (Player p in players)
        {
            if (p.teamId == 1)
            {
                team1++;
            }
            else if (p.teamId == 2)
            {
                team2++;
            }
        }
        return (team1 < team2) ? 1 : 2;
    }

    void OnClientConnected(long aClientId)
    {
        Player newPlayer = null;

        // Set the isConnected to true on the player
        foreach (Player p in players)
        {
            if (p.clientId == aClientId)
            {
                p.isConnected = true;
                p.playerId = ++lastPlayerId;
                serverNet.CallRPC("SetPlayerId", aClientId, -1, p.playerId);
                int teamId = GetTeamId();
                serverNet.CallRPC("SetTeam", aClientId, -1, teamId);
                p.teamId = teamId;

                newPlayer = p;
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
                serverNet.CallRPC("NewPlayerConnected", aClientId, -1, players[i].playerId, players[i].teamId);
                if (players[i].name != "")
                {
                    serverNet.CallRPC("PlayerNameChanged", aClientId, -1, players[i].playerId, players[i].name);
                }
                if (players[i].isReady)
                {
                    serverNet.CallRPC("PlayerIsReady", aClientId, -1, players[i].playerId, players[i].isReady);
                }
                if (players[i].playerClass != 0)
                {
                    serverNet.CallRPC("PlayerClassChanged", aClientId, -1, players[i].playerId, (int)(players[i].playerClass));
                }

                // Tell the other client that this player has connected
                serverNet.CallRPC("NewPlayerConnected", players[i].clientId, -1, newPlayer.playerId, newPlayer.teamId);
            }
        }
    }

    // A client has disconnected
    void OnClientDisconnected(long aClientId)
    {
        // Set the isConnected to false on the player
        Player p = GetPlayerByClientId(serverNet.SendingClientId);
        if (p == null) return;

        p.isConnected = false;

        // If we are in login, remove the player
        if (currentState == GameState.Login)
        {
            players.Remove(p);
        }
        else if (currentState == GameState.Playing)
        {
            // If we are in game mode, mark the player as dead
            // TODO
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

    // RPC from the client to set the name of thier player
    public void SetName(string aName)
    {
        Player p = GetPlayerByClientId(serverNet.SendingClientId);
        if (p == null)
        {
            // If we can't find the player for this client, who are they? kick them
            Debug.Log("Unable to get player for unknown client " + serverNet.SendingClientId);
            serverNet.Kick(serverNet.SendingClientId);
        }

        p.name = aName;
        serverNet.CallRPC("PlayerNameChanged", UCNetwork.MessageReceiver.AllClients, -1, p.playerId, p.name);
    }

    // RPC from the client to set the character type of the client
    public void SetCharacterType(int aType)
    {
        Player p = GetPlayerByClientId(serverNet.SendingClientId);
        if (p == null)
        {
            return;
        }

        // Check to make sure the class is valid
        foreach (PlayerClass pClass in Enum.GetValues(typeof(PlayerClass)))
        {
            if (pClass == (PlayerClass)aType)
            {
                p.playerClass = (PlayerClass)aType;
                // Tell everyone the player has changed their class
                serverNet.CallRPC("PlayerClassChanged", UCNetwork.MessageReceiver.AllClients, -1, p.playerId, (int)(p.playerClass));
                return;
            }
        }

        // If we are here, they sent an invalid class
        Debug.Log("Client " + serverNet.SendingClientId + " sent an invalid character type. Peace!");
        serverNet.Kick(serverNet.SendingClientId);
    }

    // RPC from the client to tell the server they are ready to play
    public void Ready(bool isReady)
    {
        Player p = GetPlayerByClientId(serverNet.SendingClientId);
        if (p == null)
        {
            return;
        }

        if (p.name == "" || p.playerClass == 0)
        {
            serverNet.CallRPC("PlayerIsReady", UCNetwork.MessageReceiver.AllClients, -1, p.playerId, false);
            return;
        }

        p.isReady = isReady;
        serverNet.CallRPC("PlayerIsReady", UCNetwork.MessageReceiver.AllClients, -1, p.playerId, p.isReady);

        // Are all of the players ready?
        int numPlayersReady = 0;
        foreach (Player player in players)
        {
            if (player.isReady)
            {
                numPlayersReady++;
            }
        }
        if (numPlayersReady == players.Count - 1 && players.Count > 5)
        {
            if (currentState == GameState.Login)
            {
                currentState = GameState.Starting;

                // Tell everyone the game is about to start
                serverNet.CallRPC("GameStart", UCNetwork.MessageReceiver.AllClients, -1, gameStartWaitTime);
                StartCoroutine("StartGame");
            }
        }
    }

    // Start the game after some time passes seconds
    IEnumerator StartGame()
    {
        yield return new WaitForSeconds(gameStartWaitTime);

        currentState = GameState.Playing;

        // Tell everyone the map size
        mapXSize = UnityEngine.Random.Range(minMapSize, maxMapSize);
        mapYSize = UnityEngine.Random.Range(minMapSize, maxMapSize);
        boardState = new int[mapXSize, mapYSize];

        serverNet.CallRPC("SetMapSize", UCNetwork.MessageReceiver.AllClients, -1, mapXSize, mapYSize);

        // Set some number of "blocked" spaces
        for (int i = 0; i < mapXSize; i++)
        {
            for (int j = 0; j < mapYSize; j++)
            {
                if (UnityEngine.Random.Range(0, 100) < 5)
                {
                    boardState[i, j] = -1;
                    serverNet.CallRPC("SetBlockedSpace", UCNetwork.MessageReceiver.AllClients, -1, i, j);
                }
                else
                {
                    boardState[i, j] = 0;
                }
            }
        }

        // Set the initial positions of every player
        foreach (Player p in players)
        {
            // Set the initial values for the player based on their class
            if (p.playerClass == PlayerClass.Warrior)
            {
                p.maxHealth = 100;
                p.maxMoves = 2;
                p.attackRange = 1;
            }
            else if (p.playerClass == PlayerClass.Rogue)
            {
                p.maxHealth = 70;
                p.maxMoves = 5;
                p.attackRange = 1;
            }
            else if (p.playerClass == PlayerClass.Wizard)
            {
                p.maxHealth = 30;
                p.maxMoves = 4;
                p.attackRange = 6;
            }
            p.health = p.maxHealth;
            serverNet.CallRPC("UpdateHealth", UCNetwork.MessageReceiver.AllClients, -1, p.playerId, p.health);

            // Loop while we haven't found a spot for this player
            while (true)
            {
                int x = UnityEngine.Random.Range(0, mapXSize);
                int y = UnityEngine.Random.Range(0, mapYSize);
                if (boardState[x, y] == 0)
                {
                    boardState[x, y] = p.playerId;
                    p.xPosition = x;
                    p.yPosition = y;
                    serverNet.CallRPC("SetPlayerPosition", UCNetwork.MessageReceiver.AllClients, -1, p.playerId, x, y);
                    break;
                }
            }
        }

        // Tell a player that they are first to act
        activePlayerIndex = 0;
        currentTurnTime = maxTurnTime;
        players[activePlayerIndex].movesRemaining = players[activePlayerIndex].maxMoves;
        serverNet.CallRPC("StartTurn", UCNetwork.MessageReceiver.AllClients, -1, players[activePlayerIndex].playerId);
    }

    bool VerifyPlayerTurn(Player aPlayer)
    {
        return players.IndexOf(aPlayer) == activePlayerIndex;
    }

    // Movement request from a client
    public void RequestMove(int x, int y)
    {
        Player p = GetPlayerByClientId(serverNet.SendingClientId);
        if (p == null)
        {
            return;
        }
        
        // Verify that it is this players turn
        if (!VerifyPlayerTurn(p))
        {
            ChangeHealth(p, -10);
            return;
        }

        // Verify the move is adjacent to thier current position
        bool validMove = false;
        if (x == p.xPosition && (y == p.yPosition - 1 || y == p.yPosition + 1))
        {
            validMove = true;
        }
        if (y == p.yPosition && (x == p.xPosition - 1 || x == p.xPosition + 1))
        {
            validMove = true;
        }
        if (!validMove)
        {
            ChangeHealth(p, -10);
            return;
        }

        // Verify position is not off the grid
        if (x < 0 || x >= mapXSize || y <0 || y >= mapYSize)
        {
            ChangeHealth(p, -10);
            return;
        }

        // Verify that you have movement remaining
        if (p.movesRemaining <= 0)
        {
            ChangeHealth(p, -10);
            return;
        }

        // Verify that this isn't a blocked spaced
        if (boardState[x,y] != 0)
        {
            ChangeHealth(p, -10);
            return;
        }

        // Decrement your movement
        p.movesRemaining--;

        // Update the player position
        boardState[p.xPosition, p.yPosition] = 0;
        boardState[x, y] = p.playerId;
        p.xPosition = x;
        p.yPosition = y;

        // Tell everyone else that the player position has changed
        currentTurnTime = maxTurnTime;
        serverNet.CallRPC("SetPlayerPosition", UCNetwork.MessageReceiver.AllClients, -1, p.playerId, x, y);

        // Advance to the next player's turn if this player is out of moves
        if (p.movesRemaining == 0)
        {
            AdvanceTurn();
        }
    }

    // Request from a client to make an attack
    public void RequestAttack(int x, int y)
    {
        Player p = GetPlayerByClientId(serverNet.SendingClientId);
        if (p == null)
        {
            return;
        }
        // Verify that it is this players turn
        if (!VerifyPlayerTurn(p))
        {
            ChangeHealth(p, -10);
            return;
        }

        // Verify the attack is within range
        if (Mathf.Abs(p.xPosition - x) + Mathf.Abs(p.yPosition - y) > p.attackRange)
        {
            ChangeHealth(p, -10);
            return;
        }

        // Verify that you haven't moved
        if (p.maxMoves != p.movesRemaining)
        {
            ChangeHealth(p, -10);
            return;
        }

        // Verify that this is a valid space to attack
        if (boardState[x,y] <= 0)
        {
            ChangeHealth(p, -10);
            return;
        }
        
        // Tell everyone that an attack has happened
        serverNet.CallRPC("AttackMade", UCNetwork.MessageReceiver.AllClients, -1, p.playerId, x, y);

        // Deal damage to the player at the given location
        int damage = -10;
        if (p.playerClass == PlayerClass.Warrior)
        {
            damage *= 3;
        }
        foreach(Player target in players)
        {
            if (target.playerId == boardState[x, y])
            {
                ChangeHealth(target, damage);
            }
        }
        
        // Advance to the next player's turn
        AdvanceTurn();
    }

    // Request from a client to send a chat message
    public void SendChat(string message)
    {
        Player p = GetPlayerByClientId(serverNet.SendingClientId);
        if (p != null)
        {
            serverNet.CallRPC("DisplayChatMessage", UCNetwork.MessageReceiver.AllClients, -1, p.name + ": " + message);
        }
    }

    // Request from a client to send a chat message
    public void SendTeamChat(string message)
    {
        Player p = GetPlayerByClientId(serverNet.SendingClientId);
        if (p != null)
        {
            foreach (Player p2 in players)
            {
                if (p.teamId == p2.teamId)
                {
                    serverNet.CallRPC("DisplayChatMessage", p2.clientId, -1, "(team) " + p.name + ": " + message);
                }
            }
        }
    }

    // Request from a client to pass the turn
    public void PassTurn()
    {
        if (serverNet.SendingClientId == players[activePlayerIndex].clientId)
        {
            AdvanceTurn();
        }
        else
        {
            ChangeHealth(serverNet.SendingClientId, -10);
        }
    }

    // Change the health of a player, based on their client id
    void ChangeHealth(long aClientId, int aAmount)
    {
        Player p = GetPlayerByClientId(aClientId);
        if (p != null)
        {
            ChangeHealth(p, aAmount);
        }
    }

    // Change the health of a player
    void ChangeHealth(Player aPlayer, int aAmount)
    {
        aPlayer.health += aAmount;
        serverNet.CallRPC("UpdateHealth", UCNetwork.MessageReceiver.AllClients, -1, aPlayer.playerId, aPlayer.health);

        if (aPlayer.health <= 0 && VerifyPlayerTurn(aPlayer))
        {
            AdvanceTurn();
        }
    }

    // Advance to the next player's turn
    void AdvanceTurn()
    {
        // Find the next player. Lowest player id which is greater than the current, if none, lowest player id
        int currentPlayer = activePlayerIndex;

        // Advance the player
        activePlayerIndex++;
        if (activePlayerIndex >= players.Count)
        {
            activePlayerIndex = 0;
        }
        // Advance past dead players
        while (players[activePlayerIndex].health <= 0)
        {
            activePlayerIndex++;
            if (activePlayerIndex >= players.Count)
            {
                activePlayerIndex = 0;
            }
            if (activePlayerIndex == currentPlayer)
            {
                break;
            }
        }
        if (activePlayerIndex != currentPlayer)
        {
            currentTurnTime = maxTurnTime;
            players[activePlayerIndex].movesRemaining = players[activePlayerIndex].maxMoves;
            serverNet.CallRPC("StartTurn", UCNetwork.MessageReceiver.AllClients, -1, players[activePlayerIndex].playerId);
        }
    }

    private void Update()
    {
        // Timeout for turns
        if (currentTurnTime > 0)
        {
            currentTurnTime -= Time.deltaTime;
            if (currentTurnTime <= 0)
            {
                AdvanceTurn();
            }
        }
    }
}

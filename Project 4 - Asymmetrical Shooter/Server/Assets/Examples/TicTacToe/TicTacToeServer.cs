using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LitJson;

public class TicTacToeServer : MonoBehaviour
{
    public ServerNetwork serverNet;

    public int portNumber = 603;
    
    // Stores information about a player in the game
    class Player
    {
        public long clientId;
        public string playerName;
        public bool isReady;
        public bool isConnected;
    }

    // List of all players
    List<Player> players = new List<Player>();

    // Whose turn it is
    int currentActivePlayer;
    
    // State of the board
    enum BoardState
    {
        Empty,
        X,
        O
    }
    BoardState[,] board = new BoardState[3,3];

    // Use this for initialization
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

    // A client has just requested to connect to the server
    void ConnectionRequest(ServerNetwork.ConnectionRequestInfo data)
    {
        Debug.Log("Connection request from " + data.username);

        // Allow the first two players to connect
        if (players.Count < 2)
        {
            Player newPlayer = new Player();
            newPlayer.clientId = data.id;
            newPlayer.playerName = data.username;
            newPlayer.isConnected = false;
            newPlayer.isReady = false;
            players.Add(newPlayer);

            serverNet.ConnectionApproved(data.id);
        }
        else
        {
            serverNet.ConnectionDenied(data.id);
        }
    }

    void OnClientConnected(long aClientId)
    {
        // Set the isConnected to true on the player
        foreach (Player p in players)
        {
            if (p.clientId == aClientId)
            {
                p.isConnected = true;
            }
        }

        // Send the client the current state of the board
        for (int i=0; i<board.GetLongLength(0); i++)
        {
            for (int j=0; j<board.GetLongLength(1); j++)
            {
                serverNet.CallRPC("UpdateGameState", aClientId, -1, i, j, (int)board[i, j]);
            }
        }
    }

    // RPC from the client, to be called when the player is ready
    public void PlayerIsReady()
    {
        // Who called this RPC: serverNet.SendingClientId
        Debug.Log("Player is ready");

        // Set the isReady to true on the player
        foreach (Player p in players)
        {
            if (p.clientId == serverNet.SendingClientId)
            {
                p.isReady = true;
                break;
            }
        }

        // Are all of the players ready?
        bool allPlayersReady = true;
        foreach (Player p in players)
        {
            if (!p.isReady)
            {
                allPlayersReady = false;
            }
        }
        if (allPlayersReady)
        {
            // Tell the first player it's their turn
            currentActivePlayer = 0;
            serverNet.CallRPC("StartTurn", players[0].clientId, -1);
        }
    }

    // RPC from the client to take a turn
    public void TakeTurn(int xPos, int yPos)
    {
        // Is it your turn?
        if (players[currentActivePlayer].clientId != serverNet.SendingClientId)
        {
            Debug.Log("A client has attempted to take a turn when it was not their turn");
            return;
        }

        // Is that spot open?
        if (board[xPos, yPos] != BoardState.Empty)
        {
            Debug.Log("A client has chosen a non-empty space to place their piece");
            return;
        }

        // Check if this player is the X or the O
        if (players[0].clientId == serverNet.SendingClientId)
        {
            board[xPos, yPos] = BoardState.X;
        }
        else
        {
            board[xPos, yPos] = BoardState.O;
        }

        // Check if a player has won the game
        // This is where logic would be written to check if a player has won the game

        // Tell the clients that the game has been updated
        serverNet.CallRPC("UpdateGameState", UCNetwork.MessageReceiver.AllClients, -1, xPos, yPos, (int)board[xPos, yPos]);

        // Tell the clients whose turn it is
        currentActivePlayer = currentActivePlayer == 1 ? 0 : 1;
        serverNet.CallRPC("StartTurn", players[currentActivePlayer].clientId, -1);
    }

    void OnClientDisconnected(long aClientId)
    {
        // Set the isConnected to true on the player
        foreach (Player p in players)
        {
            if (p.clientId == aClientId)
            {
                p.isConnected = false;
                p.isReady = false;
            }
        }
    }
}

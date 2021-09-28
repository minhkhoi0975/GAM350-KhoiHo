using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TacticsClient : MonoBehaviour
{
    public ClientNetwork clientNet;

    // Are we in the process of logging into a server
    private bool loginInProcess = false;

    public GameObject loginScreen;

    // Represents a player
    class Player
    {
        public string name = "";
        public int playerId = 0;
        public int characterClass = 0;
        public int health = 0;
        public int team = 0;
        public int xPos;
        public int yPos;
        public bool isReady;
        public GameObject playerObject = null;
    }
    Dictionary<int, Player> players;

    // My player id
    int myPlayerId;

    // Info about the map
    int[,] boardState;
    int mapXSize = 0;
    int mapYSize = 0;

    // Use this for initialization
    void Awake()
    {
        // Make sure we have a ClientNetwork to use
        if (clientNet == null)
        {
            clientNet = GetComponent<ClientNetwork>();
        }
        if (clientNet == null)
        {
            clientNet = (ClientNetwork)gameObject.AddComponent(typeof(ClientNetwork));
        }
    }

    // Start the process to login to a server
    public void ConnectToServer(string aServerAddress, int aPort)
    {
        if (loginInProcess)
        {
            return;
        }
        loginInProcess = true;

        ClientNetwork.port = aPort;
        clientNet.Connect(aServerAddress, ClientNetwork.port, "", "", "", 0);
    }

    void OnNetStatusConnected()
    {
        loginScreen.SetActive(false);
        Debug.Log("OnNetStatusConnected called");

        clientNet.AddToArea(1);
    }

    void OnNetStatusDisconnected()
    {
        Debug.Log("OnNetStatusDisconnected called");
        SceneManager.LoadScene("Client");

        loginInProcess = false;
    }

    public void OnChangeArea()
    {
        Debug.Log("You have joined the game.");
    }

    void OnDestroy()
    {
        if (clientNet.IsConnected())
        {
            clientNet.Disconnect("Peace out");
        }
    }

    // LOGIN PHASE
    // RPC called by the server to tell this client what their player id is
    public void SetPlayerId(int playerID)
    {
        this.myPlayerId = playerID;
        players[myPlayerId] = new Player();
    }

    // RPC called by the server to tell this client which team they are on
    public void SetTeam(int team)
    {
        players[myPlayerId].team = team;
    }

    // RPC called by the server to tell this client a new player has connected to the game
    public void NewPlayerConnected(int playerId, int team)
    {
        players[playerId] = new Player();
        players[playerId].team = team;
    }

    // RPC called by the server to tell this client a player's name has been changed
    public void PlayerNameChanged(int playerId, string name)
    {
        players[playerId].name = name;
    }

    // RPC called by the server to tell this client if it's a player's turn
    public void PlayerIsReady(int playerId, bool isReady)
    {
        players[playerId].isReady = isReady;
    }

    // RPC called by the server to tell this client a player has changed their character class
    public void PlayerClassChanged(int playerId, int newCharacterClass)
    {
        players[playerId].characterClass = newCharacterClass;
    }

    // RPC called by the server to tell this client the game is about to start within time.
    public void GameStart(int time)
    {
        Debug.Log("The game is about to start within " + time + " seconds.");
    }

    // GAME PHASE
    // RPC called by the server to tell this client the size of the map.
    public void SetMapSize(int x, int y) 
    {
        mapXSize = x;
        mapYSize = y;
        boardState = new int[x, y];
    }
    // RPC called by the server to tell this client position [x,y] is blocked.
    public void SetBlockedSpace(int x, int y) 
    {
        boardState[x, y] = -1;
    }
    // RPC called by the server to tell this client to update the position of a player.
    public void SetPlayerPosition(int playerId, int x, int y) 
    {
        players[playerId].xPos = x;
        players[playerId].yPos = y;
    }
    // RPC called by the server to tell this client it is a start of a player's turn.
    public void StartTurn(int playerId) 
    {
        players[playerId].isReady = true;
    }
    // RPC called by the server to tell this client a player has made an attack at position [x,y].
    public void AttackMade(int playerId, int x, int y)
    {
        Debug.Log("Player " + playerId + " has made an attack at position [x,y].");
    }
    // RPC called by the server to tell this client to display a chat message.
    public void DisplayChatMessage(string message) 
    {
        Debug.Log(message);
    }
    // RPC called by the server to tell this client to update a player's health.
    public void UpdateHealth(int playerId, int newHealth) 
    {
        players[playerId].health = newHealth;
    }
}
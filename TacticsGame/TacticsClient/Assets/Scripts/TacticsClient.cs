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

    public GameObject loginPanel;

    public GameObject characterSelectionPanel;

    public GameObject textGameAboutToStart;

    // Represents a player
    public class PlayerInfo
    {
        public string name = "Unknown";
        public int playerId = 0;
        public int characterClass = 0;
        public int health = 0;
        public int team = 0;
        public int xPos;
        public int yPos;
        public bool isReady;
        public GameObject playerObject = null;
    }
    Dictionary<int, PlayerInfo> players = new Dictionary<int, PlayerInfo>();
    public Dictionary<int, PlayerInfo> Players
    {
        get
        {
            return players;
        }
    }

    // My player id
    int myPlayerId;

    // Info about the map
    int[,] boardState = new int[0,0];
    int mapXSize = 0;
    int mapYSize = 0;

    // Game board.
    public GameBoard gameBoard;

    // Blocked space prefab.
    public GameObject blockedSpacePrefab;

    // Player piece prefab.
    public GameObject playerPiecePrefab;

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

        if (gameBoard == null)
        {
            gameBoard = FindObjectOfType<GameBoard>();
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
        // Disable the login panel.
        loginPanel.SetActive(false);
        Debug.Log("OnNetStatusConnected called");

        // Enable the character selection panel.
        characterSelectionPanel.SetActive(true);

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
    public void SetPlayerId(int playerId)
    {
        this.myPlayerId = playerId;
        players[myPlayerId] = new PlayerInfo();
        players[myPlayerId].playerId = playerId;

        // The game hasn't started yet, so we should hide the piece.
        // players[myPlayerId].playerObject.SetActive(false);

        Debug.Log("Your ID is: " + playerId);
    }
    
    // RPC called by the server to tell this client which team they are on
    public void SetTeam(int team)
    {
        players[myPlayerId].team = team;

        Debug.Log("Your team is: " + team);
    }

    // RPC called by the server to tell this client a new player has connected to the game
    public void NewPlayerConnected(int playerId, int team)
    {
        players[playerId] = new PlayerInfo();
        players[playerId].playerId = playerId;
        players[playerId].team = team;

        Debug.Log("Player " + playerId + " has entered the game and joined team " + team);
    }

    // RPC called by the server to tell this client a player's name has been changed
    public void PlayerNameChanged(int playerId, string name)
    {
        players[playerId].name = name;

        Debug.Log("Player " + playerId + " has changed their name to " + name);
    }

    // RPC called by the server to tell this client if it's a client is ready to play.
    public void PlayerIsReady(int playerId, bool isReady)
    {
        players[playerId].isReady = isReady;

        if(isReady)
        {
            Debug.Log("Player " + playerId + " is ready to play.");
        }
        else
        {
            Debug.Log("Player " + playerId + " is not ready to play.");
        }
    }

    // RPC called by the server to tell this client a client has changed their character class
    public void PlayerClassChanged(int playerId, int newCharacterClass)
    {
        players[playerId].characterClass = newCharacterClass;

        string characterClassName = "";
        switch(newCharacterClass)
        {
            case 1:
                characterClassName = "Warrior";
                break;
            case 2:
                characterClassName = "Rogue";
                break;
            case 3:
                characterClassName = "Wizard";
                break;
        }

        Debug.Log("Player " + playerId + " has selected class " + characterClassName);
    }

    // RPC called by the server to tell this client the game is about to start within time.
    public void GameStart(int time)
    {
        // Disable the character customization panel.
        characterSelectionPanel.SetActive(false);

        // Spawn player objects.
        foreach(KeyValuePair<int, PlayerInfo> playerInfo in players)
        {
            players[playerInfo.Key].playerObject = Instantiate(playerPiecePrefab, Vector3.zero, Quaternion.identity);

            // Set the properties of the player object.
            Player player = players[playerInfo.Key].playerObject.GetComponent<Player>();
            player.SetName(players[playerInfo.Key].name);
            player.SetCharacterType(players[playerInfo.Key].characterClass);
            player.SetTeam(players[playerInfo.Key].team);

            // Temporarily hide the object.
            players[playerInfo.Key].playerObject.SetActive(false);
        }

        // Enable the "The game is about to start wihtin ... seconds" text.
        textGameAboutToStart.SetActive(true);
        StartCoroutine(DisableTextGameAboutToStart(time));

        Debug.Log("The game is about to start within " + time + " seconds.");
    }

    // Disable the "The game is about to start wihtin ... seconds" text after a couple of seconds.
    IEnumerator DisableTextGameAboutToStart(int time)
    {
        // Wait for seconds.
        yield return new WaitForSeconds(time);

        // Enable the "The game is about to start wihtin ... seconds" text. 
        textGameAboutToStart.SetActive(false);

        // Show the player objects.
        foreach (KeyValuePair<int, PlayerInfo> playerInfo in players)
        {
            players[playerInfo.Key].playerObject.SetActive(true);
        }
    }

    // GAME PHASE
    // RPC called by the server to tell this client the size of the map.
    public void SetMapSize(int x, int y) 
    {
        // Get info of the map.
        mapXSize = x;
        mapYSize = y;
        boardState = new int[x, y];

        // Generate a game board.
        gameBoard.gameObject.SetActive(true);
        gameBoard.sizeX = x;
        gameBoard.sizeY = y;
        gameBoard.GenerateNewGameBoard();
    }
    // RPC called by the server to tell this client position [x,y] is blocked.
    public void SetBlockedSpace(int x, int y) 
    {
        boardState[x, y] = -1;

        // Create a blocked space object on the game board.
        GameObject blockedSpaceObject = Instantiate<GameObject>(blockedSpacePrefab, gameBoard.transform.position + new Vector3(1 + 2 * x, 1 + 2 * y, -2.0f), gameBoard.transform.rotation, gameBoard.transform);
    }
    // RPC called by the server to tell this client to update the position of a player.
    public void SetPlayerPosition(int playerId, int x, int y) 
    {
        players[playerId].xPos = x;
        players[playerId].yPos = y;

        players[playerId].playerObject.transform.position = gameBoard.transform.position + new Vector3(1 + 2 * x, 1 + 2 * y, -2.0f);
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
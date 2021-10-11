/**
 * TacticsClient.cs
 * Description: This script handles RPC calls from the server.
 * Programmer: Khoi Ho
 * Credit(s): Professor David Carigg for providing the example client script
 */

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

    // GUI
    public GameObject loginPanel;
    public GameObject characterSelectionPanel;
    public GameObject gameAboutToStartText;
    public GameObject gamePhrasePanel;
    public GameObject guiLogic;

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

    // List of players that connect to the server.
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
    public int MyPlayerId
    {
        get
        {
            return myPlayerId;
        }
    }

    // Info about the map
    int[,] boardState = new int[0, 0];
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

        // Make sure that this script references the game board.
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

    // === LOGIN PHASE ===

    // RPC called by the server to tell this client what their player id is
    public void SetPlayerId(int playerId)
    {
        // Create a new player info for this client.
        this.myPlayerId = playerId;
        players[myPlayerId] = new PlayerInfo();
        players[myPlayerId].playerId = playerId;

        // Display the updated player list.
        guiLogic.GetComponent<CharacterCustomizationLogic>().UpdateListOfPlayers();

        Debug.Log("Your ID is: " + playerId + ".");
    }

    // RPC called by the server to tell this client which team they are on
    public void SetTeam(int team)
    {
        // Set the team for this client.
        players[myPlayerId].team = team;

        // Display the updated player list.
        guiLogic.GetComponent<CharacterCustomizationLogic>().UpdateListOfPlayers();

        Debug.Log("Your team is: " + team + ".");
    }

    // RPC called by the server to tell this client a new player has connected to the game
    public void NewPlayerConnected(int playerId, int team)
    {
        // Create a player info for the new player that has just joined the server.
        players[playerId] = new PlayerInfo();
        players[playerId].playerId = playerId;
        players[playerId].team = team;

        // Display the updated player list.
        guiLogic.GetComponent<CharacterCustomizationLogic>().UpdateListOfPlayers();

        Debug.Log("Player " + playerId + " has entered the game and joined team " + team + ".");
    }

    // RPC called by the server to tell this client a player's name has been changed
    public void PlayerNameChanged(int playerId, string name)
    {
        // Set the name of the client.
        string oldName = players[playerId].name;
        players[playerId].name = name;

        // Display the updated player list.
        guiLogic.GetComponent<CharacterCustomizationLogic>().UpdateListOfPlayers();

        Debug.Log(oldName + "(" + playerId + ") has changed their name to " + players[playerId].name + ".");
    }

    // RPC called by the server to tell this client if it's a client is ready to play.
    public void PlayerIsReady(int playerId, bool isReady)
    {
        // Set whether the client is ready or not.
        players[playerId].isReady = isReady;

        // Display the updated player list.
        guiLogic.GetComponent<CharacterCustomizationLogic>().UpdateListOfPlayers();

        if (isReady)
        {
            Debug.Log(players[playerId].name + "(" + playerId + ") is ready to play.");
        }
        else
        {
            Debug.Log(players[playerId].name + "(" + playerId + ") is not ready to play.");
        }
    }

    // RPC called by the server to tell this client a client has changed their character class
    public void PlayerClassChanged(int playerId, int newCharacterClass)
    {
        players[playerId].characterClass = newCharacterClass;

        // Display the updated player list.
        guiLogic.GetComponent<CharacterCustomizationLogic>().UpdateListOfPlayers();

        Debug.Log(players[playerId].name + "(" + playerId + ") has selected class " + newCharacterClass + ".");
    }

    // RPC called by the server to tell this client the game is about to start within time.
    public void GameStart(int time)
    {
        // Disable the character customization panel.
        characterSelectionPanel.SetActive(false);

        // Spawn player game objects.
        foreach (KeyValuePair<int, PlayerInfo> playerInfo in players)
        {
            // Spawn a player game object.
            players[playerInfo.Key].playerObject = SpawnPlayerObject(playerInfo.Key);

            // Temporarily hide the game object until the game starts.
            players[playerInfo.Key].playerObject.SetActive(false);
        }

        // Enable the "The game is about to start wihtin ... seconds" text.
        gameAboutToStartText.SetActive(true);
        StartCoroutine(DisableTextGameAboutToStart(time));

        Debug.Log("The game is about to start within " + time + " seconds.");
    }

    // Spawn a player game object.
    GameObject SpawnPlayerObject(int playerId)
    {
        // Create a player game object.
        GameObject playerGameObject = Instantiate(playerPiecePrefab, Vector3.zero, Quaternion.identity);

        // Set the properties of the player game object.
        Player player = playerGameObject.GetComponent<Player>();
        player.SetName(players[playerId].name);
        player.SetCharacterType(players[playerId].characterClass);
        player.SetTeamMaterial(players[playerId].team);
        player.SetHealth(players[playerId].health);

        // If the game object is controlled by this client, change text color to distinguish it from other clients.
        if (playerId == myPlayerId)
            player.SetTextColor(Color.green);

        return playerGameObject;
    }

    // Disable the "The game is about to start wihtin ... seconds" text after a couple of seconds.
    IEnumerator DisableTextGameAboutToStart(int time)
    {
        // Wait for seconds.
        yield return new WaitForSeconds(time);

        // Disable the "The game is about to start wihtin ... seconds" text. 
        gameAboutToStartText.SetActive(false);

        // Show the player game objects.
        foreach (KeyValuePair<int, PlayerInfo> playerInfo in players)
        {
            players[playerInfo.Key].playerObject.SetActive(true);
        }

        // Show the Game Phrase panel.
        gamePhrasePanel.SetActive(true);
    }

    // === GAME PHASE ===

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

        Debug.Log("Generated a gameboard with size " + x + "x" + "y.");
    }

    // RPC called by the server to tell this client position [x,y] is blocked.
    public void SetBlockedSpace(int x, int y)
    {
        boardState[x, y] = -1;

        // Create a blocked space object on the game board.
        GameObject blockedSpaceObject = Instantiate<GameObject>(blockedSpacePrefab, gameBoard.transform.position + new Vector3(1 + 2 * x, 1 + 2 * y, -2.0f), gameBoard.transform.rotation, gameBoard.transform);

        Debug.Log("Position [" + x + "," + y + "] is blocked.");
    }

    // RPC called by the server to tell this client to update the position of a player.
    public void SetPlayerPosition(int playerId, int x, int y)
    {
        players[playerId].xPos = x;
        players[playerId].yPos = y;

        players[playerId].playerObject.transform.position = gameBoard.transform.position + new Vector3(1 + 2 * x, 1 + 2 * y, -2.0f);

        Debug.Log(players[playerId].name + "(" + playerId + ") has moved to position [" + x + "," + y + "].");
    }

    // RPC called by the server to tell this client it is a start of a player's turn.
    public void StartTurn(int playerId)
    {
        guiLogic.GetComponent<GamePhraseLogic>().UpdatePlayersTurn(playerId);

        Debug.Log(players[playerId].name + "(" + playerId + ")'s turn starts.");
    }

    // RPC called by the server to tell this client a player has made an attack at position [x,y].
    public void AttackMade(int playerId, int x, int y)
    {
        Debug.Log(players[playerId].name + "(" + playerId + ") has made an attack at position [" + x + "," + y + "].");
    }

    // RPC called by the server to tell this client to display a chat message.
    public void DisplayChatMessage(string message)
    {
        guiLogic.GetComponent<GamePhraseLogic>().UpdateChatBox(message);

        Debug.Log(message);
    }

    // RPC called by the server to tell this client to update a player's health.
    public void UpdateHealth(int playerId, int newHealth)
    {
        players[playerId].health = newHealth;

        players[playerId].playerObject.GetComponent<Player>().SetHealth(newHealth);

        Debug.Log(players[playerId].name + "(" + playerId + ")'s new health is " + newHealth + ".");
    }
}
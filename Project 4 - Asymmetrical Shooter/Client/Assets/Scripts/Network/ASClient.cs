/**
 * TagClient.cs
 * Description: This script handles the logic of the game on the client side.
 * Programmer: Khoi Ho
 * Credit(s): Professor Carrigg (for providing the example source code).
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ASClient : MonoBehaviour
{
    public ClientNetwork clientNet;

    // Are we in the process of logging into a server
    private bool loginInProcess = false;

    [Header("GUI")]
    public GameObject loginPanel;
    public GameObject hudPanel;
    public TitleScreenLogic mainMenuLogic;
    public HUDLogic hudLogic;

    [Header("Camera")]
    public Camera mainCamera;
    public Camera minimapCamera;
    public Camera spawnerCamera;

    // Where can the player characters be spawned?
    [Header("Start Positions")]
    public List<Transform> playerStartPositions;

    public class PlayerData
    {
        public int playerId = -1;
        public int teamId = 0;
        public string name;
    }

    // List of players that are in the game.
    public Dictionary<int, PlayerData> players = new Dictionary<int, PlayerData>();

    // The id of this client.
    [HideInInspector] public int myPlayerId = -1;

    // The game object that the player controls.
    // + If the player is a shooter, then myPlayerGameObject is a character.
    // + If the player is a spawner, then myPlayerGameObject is spawnerCamera.
    [HideInInspector] public GameObject myPlayerGameObject;

    // Reference to the InputLock component of myPlayerGameObject.
    // We need this component to lock player input when chat message box is enabled.
    [HideInInspector] public InputLock inputLock;

    // Initialize the client
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

        if(!mainCamera)
        {
            mainCamera = Camera.main;
        }
    }

    private void Start()
    {
        // Enable cursor, in case the client goes back to the main menu.
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // Start the process to login to a server
    public void ConnectToServer(string aServerAddress, int aPort)
    {
        // If we're not already logging in
        if (loginInProcess)
        {
            return;
        }
        loginInProcess = true;

        ClientNetwork.port = aPort;

        // Attempt to connect to the server with the given address and port
        clientNet.Connect(aServerAddress, ClientNetwork.port, "", "", "", 0);
    }

    // When the client has finished connecting to the server
    void OnNetStatusConnected()
    {
        loginPanel.SetActive(false);
        hudPanel.SetActive(true);
        Debug.Log("OnNetStatusConnected called");

        // Add this client to an area
        clientNet.AddToArea(1);
    }
  
    // When the client starts disconnecting from the server
    void OnNetStatusDisconnecting()
    {
        Debug.Log("OnNetStatusDisconnecting called");
    }

    // When the client has finished disconnecting from the server
    void OnNetStatusDisconnected()
    {
        Debug.Log("OnNetStatusDisconnected called");

        // Restart the demo scene
        SceneManager.LoadScene("Client");

        loginInProcess = false;
    }

    // When the client has successfully told the server to be put into an area
    public void OnChangeArea()
    {
        Debug.Log("OnChangeArea called");

        // Tell the server to set the name of the client.
        clientNet.CallRPC("SetName", UCNetwork.MessageReceiver.ServerOnly, -1, mainMenuLogic.playerName.text);

        if (players[myPlayerId].teamId == 1)
        {
            // Request the server to spawn a character for this shooter.
            RequestSpawnShooter();
        }
        else if(players[myPlayerId].teamId == 2)
        {
            myPlayerGameObject = spawnerCamera.gameObject;

            // Switch from main menu camera to spawner camera.
            spawnerCamera.gameObject.SetActive(true);
            spawnerCamera.enabled = true;
            mainCamera.gameObject.SetActive(false);

            // Display HUD for spawner.
            hudLogic.DisplayHUD(false);

            // Reference InputLock component.
            inputLock = myPlayerGameObject.GetComponent<InputLock>();
        }
    }

    void RequestSpawnShooter()
    {
        // Randomly select a start point.
        Transform startPoint = playerStartPositions[Random.Range(0, playerStartPositions.Count)];

        // Request the server to spawn a character for this shooter.
        clientNet.CallRPC("SpawnShooter", UCNetwork.MessageReceiver.ServerOnly, -1, startPoint.position, startPoint.rotation);
    }

    void OnDestroy()
    {
        if (myPlayerGameObject)
        {
            clientNet.Destroy(myPlayerGameObject.GetComponent<NetworkSync>().GetId());
        }
        if (clientNet.IsConnected())
        {
            clientNet.Disconnect("Peace out");
        }
    }

    // ----------------- 
    // RPC CALLS
    // -----------------

    // Set the player ID for this client.
    public void SetPlayerId(int playerId)
    {
        myPlayerId = playerId;
        players[myPlayerId] = new PlayerData();
        players[myPlayerId].playerId = playerId;

        Debug.Log("Your player ID is: " + playerId);
    }

    // Set the team ID for this client.
    public void SetTeamId(int teamId)
    {
        players[myPlayerId].teamId = teamId;
    }

    // A player has joined the game.
    public void NewPlayerConnected(int playerId, int teamId)
    {
        // Create a player info for the new player that has just joined the server.
        players[playerId] = new PlayerData();
        players[playerId].playerId = playerId;
        players[playerId].teamId = teamId;

        Debug.Log("Player " + playerId + " has entered the game.");
    }

    // A client has disconnected from the server.
    public void ClientDisconnected(int playerId)
    {
        // Remove the player that matches the player id.
        players.Remove(playerId);
    }

    // A player has changed their name.
    public void PlayerNameChanged(int playerId, string name)
    {
        // Set the name of the client.
        string oldName = players[playerId].name;
        players[playerId].name = name;

        Debug.Log(oldName + "(" + playerId + ") has changed their name to " + players[playerId].name + ".");
    }

    // The shooter of this client has been successfully spawned.
    public void ShooterSpawned(int objNetId)
    {
        myPlayerGameObject = clientNet.GetGameObject(objNetId);
        if(!myPlayerGameObject)
        {
            clientNet.Disconnect("Character not found.");
        }

        // Hide the character's game tag on this client.
        myPlayerGameObject.GetComponentInChildren<TextMesh>().gameObject.SetActive(false);

        // Switch from main menu camera to character's camera.
        Camera characterCamera = myPlayerGameObject.GetComponentInChildren<Camera>(true);
        if (characterCamera)
        {
            characterCamera.gameObject.SetActive(true);
            characterCamera.enabled = true;

            mainCamera.gameObject.SetActive(false);
        }

        // Make minimap camera focus on the shooter's network object.
        FollowingCameraMovement minimapCameraMovement = minimapCamera.GetComponent<FollowingCameraMovement>();
        if (minimapCameraMovement)
        {
            minimapCameraMovement.focusedGameObject = myPlayerGameObject.GetComponentInChildren<Rigidbody>().gameObject;
        }

        // Lock cursor.
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Display HUD for shooter.
        hudLogic.DisplayHUD(true);

        // Reference InputLock component.
        inputLock = myPlayerGameObject.GetComponent<InputLock>();
    }

    // Health changed.
    public void SetHealth(float newHealth)
    {
        hudLogic.SetHealth(newHealth);
    }

    // Receive a chat message.
    public void ReceiveChatMessage(string message)
    {
        hudLogic.ReceiveChatMessage(message);
    }
}
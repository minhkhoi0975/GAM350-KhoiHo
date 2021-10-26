using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TagClient : MonoBehaviour
{
    public ClientNetwork clientNet;

    // Are we in the process of logging into a server
    private bool loginInProcess = false;

    public GameObject loginScreen;

    public class Player
    {
        public int playerId = -1;
        public int gameObjectNetworkId = -1;
    }

    // List of players that are in the game.
    public Dictionary<int, Player> players = new Dictionary<int, Player>();

    // The id of this client.
    public int myPlayerId;

    // The id of the current hunter.
    public int currentHunterId;

    // Reference to the character game object of this client.
    public GameObject myPlayerGameObject;

    // Where can the player characters be spawned?
    public List<Transform> playerStartPositions;

    float deltaTime = 0;
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
    }

    private void Start()
    {
        //Instantiate(myPlayer);
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
        loginScreen.SetActive(false);
        Debug.Log("OnNetStatusConnected called");

        // Add this client to an area
        clientNet.AddToArea(1);
    }

    // When the client starts disconnecting from the server
    void OnNetStatusDisconnecting()
    {
        Debug.Log("OnNetStatusDisconnecting called");

        if (myPlayerGameObject)
        {
            clientNet.Destroy(myPlayerGameObject.GetComponent<NetworkSync>().GetId());
            myPlayerGameObject = null;
        }
    }

    // When the client has finished disconnecting from the server
    void OnNetStatusDisconnected()
    {
        Debug.Log("OnNetStatusDisconnected called");

        // Restart the demo scene
        SceneManager.LoadScene("Client");

        loginInProcess = false;

        if (myPlayerGameObject)
        {
            clientNet.Destroy(myPlayerGameObject.GetComponent<NetworkSync>().GetId());
            myPlayerGameObject = null;
        }
    }

    // When the client has successfully told the server to be put into an area
    public void OnChangeArea()
    {
        Debug.Log("OnChangeArea called");

        // Randomly select a start point.
        Transform startPoint = playerStartPositions[Random.Range(0, playerStartPositions.Count)];

        // Create a character for this client.
        myPlayerGameObject = clientNet.Instantiate("PlayerCharacter", startPoint.position, startPoint.rotation);

        // Make the camera focus on this client.
        CameraMovement cameraMovement = Camera.main.GetComponent<CameraMovement>();
        if(cameraMovement)
        {
            cameraMovement.focusedGameObject = myPlayerGameObject;
        }

        myPlayerGameObject.GetComponent<NetworkSync>().AddToArea(1);

        clientNet.CallRPC("PlayerGameObjectSpawned", UCNetwork.MessageReceiver.ServerOnly, -1, myPlayerId, myPlayerGameObject.GetComponent<NetworkSync>().GetId());
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

    // Set the player ID for this client.
    public void SetPlayerId(int playerId)
    {
        myPlayerId = playerId;
        players[myPlayerId] = new Player();
        players[myPlayerId].playerId = playerId;

        Debug.Log("Your player ID is: " + playerId);
    }

    // A player has joined the game.
    public void NewPlayerConnected(int playerId)
    {
        // Create a player info for the new player that has just joined the server.
        players[playerId] = new Player();
        players[playerId].playerId = playerId;

        Debug.Log("Player " + playerId + " has entered the game.");
    }

    public void ClientDisconnected(int playerId)
    {
        players.Remove(playerId);
    }

    // The hunter has been changed.
    public void SetHunter(int playerId)
    {
        currentHunterId = playerId;

        // Set the material of the hunter and the preys.
        if(myPlayerId == currentHunterId)
        {
            myPlayerGameObject.GetComponent<CharacterAppearance>().SetMaterial(true);
        }
        else
        {
            myPlayerGameObject.GetComponent<CharacterAppearance>().SetMaterial(false);
        }

        Debug.Log("Player " + playerId + "has become the hunter.");
    }
}
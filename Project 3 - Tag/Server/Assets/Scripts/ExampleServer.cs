using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LitJson;

public class ExampleServer : MonoBehaviour
{
    public ServerNetwork serverNet;

    public int portNumber = 603;

    public GameObject playerObject;

    Dictionary<int, GameObject> playerGameObjects = new Dictionary<int, GameObject>();
    
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

        //serverNet.EnableLogging("rpcLog.txt");
    }

    // CALLBACK FUNCTIONS
    //  The following functions will be called by the ServerNetwork script while the game is running:

    // A client has just requested to connect to the server
    void ConnectionRequest(ServerNetwork.ConnectionRequestInfo data)
    {
        Debug.Log("Connection request from " + data.username);

        // Approve the connection
        serverNet.ConnectionApproved(data.id);
    }

    // A client has finished connecting to the server
    void OnClientConnected(long aClientId)
    {
        
    }

    // A client has disconnected
    void OnClientDisconnected(long aClientId)
    {

    }

    // A network object has been instantiated by a client
    void OnInstantiateNetworkObject(ServerNetwork.IntantiateObjectData aObjectData)
    {
        // Get the network object information, store in a dictionary
        ServerNetwork.NetworkObject obj = serverNet.GetNetObjById(aObjectData.netObjId);
        playerGameObjects[aObjectData.netObjId] = Instantiate(playerObject, obj.position, obj.rotation);
    }

    // A client has been added to a new area
    void OnAddArea(ServerNetwork.AreaChangeInfo aInfo)
    {

    }

    // An object has been added to a new area
    void AddedObjectToArea(int aNetworkId)
    {

    }

    // Initialization data should be sent to a network object
    void InitializeNetworkObject(ServerNetwork.InitializationInfo aInfo)
    {
        
    }

    // A game object has been destroyed
    void OnDestroyNetworkObject(int aObjectId)
    {

    }

    private void Update()
    {
        Dictionary<int, ServerNetwork.NetworkObject> allObjs = serverNet.GetAllObjects();
        
    }

    private void FixedUpdate()
    {
        
    }

    public void NetObjectUpdated(int aNetId)
    {
        Debug.Log("Object has been updated:" + aNetId);

        ServerNetwork.NetworkObject obj = serverNet.GetNetObjById(aNetId);
        playerGameObjects[aNetId].transform.position = obj.position;
        playerGameObjects[aNetId].transform.localRotation = obj.rotation;

        //...
        long ownerClientId = serverNet.GetOwnerClientId(aNetId);
        serverNet.CallRPC("IsHunter", ownerClientId, aNetId, false);
        //...
        //serverNet.CallRPC("IsHunter", otherClientId, otherNetId, true);
    }
}

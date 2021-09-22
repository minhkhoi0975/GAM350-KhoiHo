Remote Procedure Calls (RPCs)
=======================================

Overview
--------
Remote Procedure Calls are a simple way to fit networking interactions into a typical game's code structure. They can be conceptualized to work the same way as normal function calls, but initiated by a remote machine. Once communicated across the network, RPCs use reflection to find and invoke the matching method on the receiving object (indicated by the object's network ID).

RPCs are commonly used for individual descrete actions within a game: when a player casts a spell, presses a button, opens a door, etc. Unlike the object synchronization methods, RPCs act very similarly to local function calls. 

Format of an RPC
----------------
Remote procedure calls start with a message to the server. Included in this message is:
    - The name of the function to execute
    - The object that contains a component with the function to execute
    - Which clients should execute the function
    - Any additional parameters to the function

As an example: If a component on a networked game object, or the object that has the ClientNetwork or ServerNetwork component, had the following function:

.. code-block:: C#

    public void ExampleFunction(int number, string word) {
        Debug.Log(number);
        Debug.Log(word);
    }

you could call this from another machine connected to the game like this:

.. code-block:: C#

    clientNetwork.CallRPC("ExampleFunction", MessageReceiver.AllClients, -1, 101, "Hello");
    
this would send a message to the server, which would then cause the server to send a message to all connected clients, which would then execute the ExampleFunction method on the same object that has the ClientNetwork script passing the parameters 101 and "Hello".

The CallRPC function is defined in the ClientNetwork and ServerNetwork scripts as:

.. code-block:: C#

    override public void CallRPC(string aFunctionName, MessageReceiver aReceiver, int aNetworkId, params object[] aParams)

The parameters to this function are: the name of the function to be executed, which clients should execute this function, the network id of object that should execute the function, and then any number of parameters that should be passed to the function.

Defining new remote procedure calls is easy, as they are written exactly like new C# methods.


RPCs from the client
--------------------
TODO: Discuss calling RPCs from the client to the server. Examples on why you would do this.

Calling RPCs from a client can be done in two ways. The first is using the CallRPC method found in ClientNetwork. The second is using the CallRPC method found in the NetworkSync component on a networked game object. Both methods have the same results and parameters, with the version within the NetworkSync component assuming that the game object is the one that should be executing the funtion on remote machines. 


RPCs from the server
--------------------
TODO: Calling RPCs from the server to the clients (or single client). Examples on why you would do this.


Determining where an RPC goes - MessageReceiver
-----------------------------------------------
TODO: Discuss the different options for the MessageReceiver parameter for the CallRPC function. What does each of them mean, and why would you use each? (ServerOnly = 1, AllClients = 2, OtherClients = 4, AllClientsInArea = 8, OtherClientsInArea = 16, SingleClient = 32)


Supported Data Types
--------------------
TODO: Discuss the data types that the networking library allows you to send. Discuss how to add additional data types (WriteRPCParams and ReadRPCParams).


RPCs from one client to another
-------------------------------
TODO: Discuss how and why clients would call RPCs that arrive to a single other client. Discuss why this isn't natually supported by the networking library.

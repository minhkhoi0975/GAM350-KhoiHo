Networked Objects
=======================================

Networked objects (as well as RPCs), make up the bulk of what the networking library supports. Most full games will be built using a combination of networked objects, as well as remote procedure calls. Knowing when to use one over the other is entirely up to the developers of the game, but some best practices can guide those decisions.

In general, a networked object is a game object which is instantiated on every client connected to the server. As the object moves on one client, information about it's transform is automatically sent through the server to the other connected clients. A simple example of this may be a game object representing a player's main character. When the game object representing the character is instantiated, it will automatically be created on all other clients connected to the server. As the player moves their character in the simulation, the game object representing that character on all of the other clients will move as well, keeping all of their positions and rotations in sync.


Instantiating
-------------
Instantiating a network object is similar to instantiating any other object in Unity, with a few minor differences. First, use the Instantiate method found in ClientNetwork, instead of Unity's built-in Instantiate method. The first parameter of this method is the string name of a prefab you would like to instantiate. This string name will be sent to the server, as well as any other clients which need to instantiate the object. For Unity to properly instantiate the object, a prefab with that name must be in a Resources directory.

Note: Loading objects from within a Resources directory based on their names is no longer recommended by Unity (but is still fully functional). It will be worth exploring updating this logic to instead use one of Unity's newer systems for dynamic asset loading, like Addressables or Asset Bundles.

The server can spawn objects in a similar manner, however, please see the documentation on ownership to understand how server-instantiated objects behave.

Networked objects need to have the NetworkSync component. This component assists the networking system by tracking a unique id for the object, handling remote procedure calls, as well as handling object synchronization messages. If you do not feel that your object needs these features, then it may be better to call an RPC to AllClients and have them all instantiate local objects.

All networked objects have unique ids associated with them, which assists the networking system in sending messages about specific objects to all clients. One feature of UCNetwork is that local object instantiation happens synchronously, meaning it will immediately instantiate an object on your local client when you call ClientNetwork.Instantiate. In order to accomplish this, while still having unique object ids across the entire network, every client is given a pool of unique ids (default is 500). When a client instantiates an object, it picks a free id in the id pool it has been given and tells the server which id it has chosen for the new network object. The server tracks the size of each client's pool of ids and will send additional ids to a client when it starts running low.

Destroying
-------------
When a networked game object is destroyed, it need to inform the server (which will inform the other clients) that the object needs to be removed from the simulation. As objects are destroyed, the NetworkSync calls ClientNetwork.Destroy, which will send a message to the server that the object should be removed from all clients.

Note: What happens when a client who does not own an object tells the server that it has been destroyed?


Network Sync Component Details
------------------------------
TODO: What does a network sync component provide? Why is it required? What callbacks does the network sync component provide to the object it is attached to, if any, and what additional functionality does it provide? How can you add additional data to the Network Synchronization Messages? What is a LiteSync message?


Remote Procedure Calls
----------------------
TODO: Discuss sending remote procedure calls to a single game object. Discuss how sending a RPC to a single game object works, and why you would use it. More detail regarding RPCs will be provided in the RPC documentation.


Smooth Movement
---------------
TODO: Discuss network interpolation and extrapolation. Why is this needed? How does it account for lag? What considerations would need to be make regarding objects with physics, or fast paced action games?

Quickstart
=======================================

The goal of this section is to be short, easy to follow, and closer to a tutorial than reference documentation. If someone follows the instructions on this page, they should be up and running with an example client and server, with a bit of guidance on where to go next to start adding their own game logic.

To start, download the latest version of the Client and Server projects.

Running the Server
------------------
1. Open the Server project and load the scene called "Server"
2. Hit play.

By default, the server will run on port 603 (the area code for New Hampshire). The example server logic will automatically approve all connection requests.

Running the Client
------------------
1. Open the Client project and load the scene called "Client"
2. Hit play.

Connecting to the server
~~~~~~~~~~~~~~~~~~~~~~~~
The client will allow you to enter the IP address and port of the server. If you are running the server locally with default settings, you can enter "127.0.0.1" for the IP address and "603" for the port.

The connection process starts by sending a request to the server to connect. The default server logic will automaitcally approve all connection requests, and your client will be approved. Once your client is notified that it has properly connected to the server, it will add itself to area 1 (see the section on area filtering for more information), spawn its own "Player" object and add the player object to area 1.

To test with more client connections, you should build the client project and run it outside of the Unity editor.


TODO: This should be a quickstart guide to use the networking library. A short tutorial on how to get an example client and server project, as well as set it up so they can connect to each other, as well as simple examples of RPCs and Networked Objects should be included.

TODO: What is the process of creating a custom server? What considerations are needed when developing a server?

TODO: What is the process of creating a custom client using this library? What considerations are needed when developing a client?

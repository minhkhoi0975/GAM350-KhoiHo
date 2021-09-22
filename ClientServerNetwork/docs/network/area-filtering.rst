Area Filtering
=======================================

The concept of "area filtering" allows the networking library to send data to specific groups of clients. For example, if client A, B, and C are in a dungeon while client D is back in town, you probably don't need to send the data relating to what is going on in the dungeon to client D.

Examples
--------
Rogue
~~~~~
TODO: Describe why this system would be used for a character who could hide themselves from other players

Team Chat
~~~~~~~~~
TODO: Describe how this system could be used for handling something like team chat, where a specific group of players should be able to communicate with each other

Virtual Areas
~~~~~~~~~~~~~
TODO: Describe how and why this system could be used for supporting different locations in a larger game, such are multi-floor dungeons, and clients who have completely different scenes loaded


Using Area Filters
------------------
TODO: Describe how to use area filtering, including adding clients and networked game objects to areas, removing them from areas

Synchronizing Game State
~~~~~~~~~~~~~~~~~~~~~~~~
TODO: With clients in different areas, describe how clients are told the current state of an area when adding themselves to an area which already has networked game objects, etc.


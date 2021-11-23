/**
 * PlayerData.cs
 * Description: This class stores data about a player.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData
{
    public long clientId;
    public bool isConnected;

    public int playerId = 0;
    public int teamId = 0;            // 1 = shooter, 2 = spawner
    public string name = "";
    public int shooterObjNetId = -1;  // If the client is a shooter, this is the network id of their character game object. If the client is a spawner, the value is always -1.
}

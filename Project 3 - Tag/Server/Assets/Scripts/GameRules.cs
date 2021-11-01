/**
 * GameRules.cs
 * Description: This class contains the rules of a game.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameRules
{
    // How long the new hunter must wait before they can start hunting.
    public float hunterCooldown = 10.0f;

    // The movement speeds of the hunter and the preys.
    public float hunterMovementSpeed = 180.0f;
    public float preyMovementSpeed = 150.0f;

    // The field of view of the hunter and the preys.
    public float hunterFieldOfView = 3.0f;
    public float preyFieldOfView = 10.0f;
}

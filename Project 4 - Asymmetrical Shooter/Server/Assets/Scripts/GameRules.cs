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
    // The movement speeds of shooters
    public float preyMovementSpeed = 100.0f;

    // The maximum number of NPCs that can appear at the same time = Number of shooters * npcCountMultiplier.
    public int npcCountMultiplier = 5;
}

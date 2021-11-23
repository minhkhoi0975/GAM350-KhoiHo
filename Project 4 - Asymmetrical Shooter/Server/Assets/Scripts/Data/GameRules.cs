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
    [Header("Movement")]
    public float shooterMovementSpeed = 80.0f;
    public float npcMovementSpeed = 8.0f;

    [Header("Health")]
    public float shooterHealth = 100.0f;
    public float npcHealth = 50.0f;

    [Header("Combat")]
    public float projectileDamage = 20.0f;

    [Header("NPC Spawning")]
    // The maximum number of NPCs that can appear at the same time = Number of shooters * maxNpcShooterRatio.
    public int maxNpcShooterRatio = 5;
}
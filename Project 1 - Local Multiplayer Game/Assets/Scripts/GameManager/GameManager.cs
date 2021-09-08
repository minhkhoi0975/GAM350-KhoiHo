/**
 * GameManager.cs
 * Description: This script stores variables which are not deleted when a new level is loaded.
 * Programer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    public static GameManager Instance 
    { 
        get
        {
            return instance;
        }
    }

    public const int MAX_NUMBER_OF_PLAYERS = 2;

    public int[] PlayerScores = new int[MAX_NUMBER_OF_PLAYERS]; // The scores of the players.

    private void Awake()
    {
        // Game manager does not exist. Create a new one, and make sure that it is not deleted when a new level is loaded.
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        // Game manager already exists. Delete this.
        else
        {
            Destroy(gameObject);
        }
    }
}

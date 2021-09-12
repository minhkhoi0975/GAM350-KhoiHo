/**
 * GameManager.cs
 * Description: This script stores variables which are not deleted when a new level is loaded.
 * Programer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public bool[] isPlayerInputActive = new bool[MAX_NUMBER_OF_PLAYERS];   // Used to check whether there is a player controller at a particular input index.
    public int[] playerScores = new int[MAX_NUMBER_OF_PLAYERS];            // The scores of the players.

    public string previouslySelectedLevel;

    private void Awake()
    {
        // The player input with index 0 is always active.
        isPlayerInputActive[0] = true;

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

    public int GetNumberOfActiveInputs()
    {
        int numberOfActiveInputs = 0;

        for(int i = 0; i < MAX_NUMBER_OF_PLAYERS; i++)
        {
            numberOfActiveInputs += isPlayerInputActive[i] ? 1 : 0;
        }

        return numberOfActiveInputs;
    }

    public void ResetPlayerStats()
    {
        for(int i = 0; i < MAX_NUMBER_OF_PLAYERS; i++)
        {
            isPlayerInputActive[i] = false;
            playerScores[i] = 0;
        }

        isPlayerInputActive[0] = true;
    }
}

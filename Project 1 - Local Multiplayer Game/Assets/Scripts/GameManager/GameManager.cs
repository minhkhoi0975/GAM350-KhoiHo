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

    // The scores of player 1 and player 2.
    int player1Score, player2Score;

    public int Player1Score { get { return player1Score; } set { player1Score = value < 0 ? 0 : value; } }
    public int Player2Score { get { return player2Score; } set { player2Score = value < 0 ? 0 : value; } }

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

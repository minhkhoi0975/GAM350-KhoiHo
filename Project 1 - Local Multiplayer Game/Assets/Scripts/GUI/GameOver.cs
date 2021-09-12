using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    [SerializeField] Text textPlayerScores;            // The text that shows players' scores.

    private void Awake()
    {
        if(textPlayerScores)
        {
            string text = "";

            // Display the score of each player.
            for(int i = 0; i < GameManager.MAX_NUMBER_OF_PLAYERS; i++)
            {
                if(GameManager.Instance.isPlayerInputActive[i])
                {
                    text += string.Concat("Player ", i + 1, ": ", GameManager.Instance.playerScores[i], "\n");
                }
            }

            textPlayerScores.text = text;
        }
    }

    public void LoadRecentlyPlayedLevel()
    {
        GameManager.Instance.ResetPlayerStats();
        SceneManager.LoadScene(GameManager.Instance.previouslySelectedLevel);
        Debug.Log(GameManager.Instance.previouslySelectedLevel);
    }

    public void GoToMainMenu()
    {
        GameManager.Instance.ResetPlayerStats();
        SceneManager.LoadScene("MainMenu");
    }
}

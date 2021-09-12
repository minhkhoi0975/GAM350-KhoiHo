/**
 * MainMenu.cs
 * Description: This script contains methods for the buttons in the main menu.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    // List of levels player(s) can play.
    public static readonly List<string> playableLevels = new List<string>(new string[] {"Level1", "Level2"});

    [SerializeField] Dropdown dropdownSelectedLevel;

    public void Awake()
    {
        if(dropdownSelectedLevel == null)
        {
            dropdownSelectedLevel = GetComponentInChildren<Dropdown>();
        }

        // Set the list of levels.
        if(dropdownSelectedLevel)
        {
            for(int i = 0; i < playableLevels.Count; i++)
            {
                dropdownSelectedLevel.options.Add(new Dropdown.OptionData(playableLevels[i]));
            }
        }
    }

    public void UpdatePreviouslyPlayedLevel()
    {
        GameManager.Instance.previouslySelectedLevel = dropdownSelectedLevel.options[dropdownSelectedLevel.value].text;
    }

    // Load the selected level.
    public void LoadSelectedLevel()
    {
        if (dropdownSelectedLevel)
        {
            // Get the selected level from dropdown.
            string selectedLevel = dropdownSelectedLevel.options[dropdownSelectedLevel.value].text;
            GameManager.Instance.previouslySelectedLevel = selectedLevel;

            // Reset the inputs and the stats (in case the players have just played a game and got back to the main menu.
            GameManager.Instance.ResetPlayerInputs();
            GameManager.Instance.ResetPlayerStats();    
            
            // Load level.
            SceneManager.LoadScene(selectedLevel);
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}

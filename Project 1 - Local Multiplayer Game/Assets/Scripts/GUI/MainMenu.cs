/**
 * MainMenu.cs
 * Description: This script contains methods for the buttons in the main menu.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void LoadLevel(string LevelName)
    {
        SceneManager.LoadScene(LevelName);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}

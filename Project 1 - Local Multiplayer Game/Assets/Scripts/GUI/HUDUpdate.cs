/**
 * HUDUpdate.cs
 * Description: This script updates the HUD of the game.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDUpdate : MonoBehaviour
{
    [SerializeField] Text[] textPlayerIndexNumber;
    [SerializeField] Text[] textPlayerScores;

    [SerializeField] Timer timer;
    [SerializeField] Text textTimer;

    // Update is called once per frame
    void Update()
    {
        // Update the scores on screen.
        for(int i = 0; i < textPlayerScores.Length; i++)
        {
            if (GameManager.Instance.isPlayerInputActive[i])
            {
                textPlayerIndexNumber[i].text = "Player " + (i + 1);
                textPlayerScores[i].text = GameManager.Instance.playerScores[i].ToString();
            }
            else
            {
                textPlayerIndexNumber[i].text = "";
                textPlayerScores[i].text = "";
            }
        }

        // Update the timer on scrfeen.
        int minutes = (int)timer.TimeRemainingInSeconds / 60;
        int seconds = (int)timer.TimeRemainingInSeconds % 60;
        textTimer.text = string.Format("{0}:{1}", minutes.ToString("D2"), seconds.ToString("D2"));
    }
}

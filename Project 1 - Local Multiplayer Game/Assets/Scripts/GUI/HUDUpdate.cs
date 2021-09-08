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
    [SerializeField] Text[] textPlayerScores;

    // Update is called once per frame
    void Update()
    {
        // Update the scores on screen.
        for(int i = 0; i < textPlayerScores.Length; i++)
        {
            textPlayerScores[i].text = GameManager.Instance.PlayerScores[i].ToString();
        }
    }
}

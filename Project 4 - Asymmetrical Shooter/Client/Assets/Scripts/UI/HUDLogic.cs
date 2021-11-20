/**
 * HUDLogic.cs
 * Description: This script handles the logic of the HUD panel.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDLogic : MonoBehaviour
{
    // Reference to sub panels.
    [Header("Panels")]
    public GameObject commonPanel;
    public GameObject shooterPanel;
    public GameObject spawnerPanel;

    [Header("Shooter Panel")]
    public Text healthText;

    public void DisplayHUD(bool shooter)
    {
        commonPanel.SetActive(true);

        if(shooter)
        {
            shooterPanel.SetActive(true);
            spawnerPanel.SetActive(false);
        }
        else
        {
            shooterPanel.SetActive(false);
            spawnerPanel.SetActive(true);
        }
    }

    public void SetHealth(float newHealth)
    {
        healthText.text = "HP: " + (int)newHealth;
    }
}

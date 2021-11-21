/**
 * HUDInput.cs
 * Description: This script handles HUD input, such as toggling or untoggling chat box.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HUDLogic))]
public class HUDInput : MonoBehaviour
{
    // Reference to HUDLogic component.
    public HUDLogic hudLogic;

    private void Awake()
    {
        if(hudLogic)
        {
            hudLogic = GetComponent<HUDLogic>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Input can only be received after the player connects to the server.
        if (hudLogic.client.myPlayerId == -1)
            return;

        // Send message to all players.
        if(Input.GetButtonDown("MessageAll"))
        {
            if (!hudLogic.sendMessagePanel.activeInHierarchy)
            {
                hudLogic.messageTypeDropDown.value = 0;
                hudLogic.ToggleSendMessagePanel();
            }
        }

        // Send message to teammates only.
        if (Input.GetButtonDown("MessageTeam"))
        {
            if (!hudLogic.sendMessagePanel.activeInHierarchy)
            {
                hudLogic.messageTypeDropDown.value = 1;
                hudLogic.ToggleSendMessagePanel();
            }
        }

        if (Input.GetButtonDown("Submit"))
        {
            if (hudLogic.sendMessagePanel.activeInHierarchy)
            {
                hudLogic.SendChatMessage();
            }

            // Send message using the previous type of message.
            else
            {
                hudLogic.ToggleSendMessagePanel();
            }
        }
    }
}

/**
 * HUDLogic.cs
 * Description: This script handles the logic of the HUD panel.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HUDLogic : MonoBehaviour
{
    // References to sub panels.
    [Header("Panels")]
    public GameObject commonPanel;
    public GameObject shooterPanel;
    public GameObject spawnerPanel;

    [Header("Common Panel")]
    public Text chatHistoryText;
    public InputField messageInputField;
    public Dropdown messageTypeDropDown;

    [Header("Shooter Panel")]
    public Text healthText;

    [Header("Chat Box")]
    public GameObject sendMessagePanel;
    public int maxMessageCount = 10;               // How many messages can be display in chatHistoryText?
    List<string> messages = new List<string>();    // Message history.

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

    // **************
    // Common Panel
    // **************

    public void ToggleSendMessagePanel()
    {
        // Enable cursor.
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Make the chat box object active.
        sendMessagePanel.SetActive(true);

        // Make the event system focus on the message input field.
        messageInputField.Select();
        messageInputField.OnSelect(null);
    }

    public void UntoggleSendMessagePanel()
    {
        // Make the chat box inactive.
        sendMessagePanel.SetActive(false);
    }

    public void SendChatMessage()
    {
        // Trim the message.
        messageInputField.text = messageInputField.text.Trim();

        // Empty the input field.
        messageInputField.text = "";

        // Untoggle the chat box.
        UntoggleSendMessagePanel();
    }

    public void ReceiveChatMessage(string newMessage)
    {
        // Add a new message to the message history.
        messages.Add(newMessage);
        if(messages.Count > maxMessageCount)
        {
            messages.RemoveAt(0);
        }

        DisplayMessages();

        StartCoroutine(ClearMessages());
    }

    public void DisplayMessages()
    {
        chatHistoryText.text = "";
        foreach (string message in messages)
        {
            chatHistoryText.text += message + "\n";
        }     
    }

    // Slowly remove messages one by one.
    IEnumerator ClearMessages()
    {
        yield return new WaitForSeconds(15.0f);

        messages.RemoveAt(0);
        DisplayMessages();

        if(messages.Count > 0)
        {
            ClearMessages();
        }
    }

    // **************
    // Shooter Panel
    // **************

    public void SetHealth(float newHealth)
    {
        healthText.text = "HP: " + (int)newHealth;
    }
}

/**
 * GamePhraseLogic.cs
 * Description: This script handles the logic for the Game Phrase panel.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePhraseLogic : MonoBehaviour
{
    public Text playersTurnText;

    public Text chatHistoryText;
    public int maxNumMessages = 10;   // The maximum number of messages that can be displayed in chatHistoryText.

    public InputField messageInputField;
    public Dropdown messageTypeDropDown;

    public TacticsClient client;

    List<string> messages = new List<string>();

    // Display which player's turn starts.
    public void UpdatePlayersTurn(int playerId)
    {
        playersTurnText.text = client.Players[playerId].name + "'s Turn";
        if (playerId == client.MyPlayerId)
        {
            playersTurnText.color = Color.green;
        }
        else
        {
            playersTurnText.color = Color.red;
        }  
    }

    // Tell the server to pass this client.
    public void Pass()
    {
        client.clientNet.CallRPC("PassTurn", UCNetwork.MessageReceiver.ServerOnly, -1);
    }

    // Update the chat box.
    public void UpdateChatBox(string newMessage)
    {
        // Remove the oldest message if there are too many messages.
        if(messages.Count > maxNumMessages)
        {
            messages.RemoveAt(0);
        }

        // Add the latest message.
        messages.Add(newMessage);

        // Display the messages.
        chatHistoryText.text = "";
        foreach(string message in messages)
        {
            chatHistoryText.text += message + "\n";
        }
    }

    // Send a message.
    public void SendMessage()
    {
        // Don't send a message if its content is empty.
        if (messageInputField.text.Trim() == "")
            return;

        // Send a message to team.
        if(messageTypeDropDown.value == 0)
        {
            client.clientNet.CallRPC("SendTeamChat", UCNetwork.MessageReceiver.ServerOnly, -1, messageInputField.text);
        }
        // Send a message to everyone.
        else
        {
            client.clientNet.CallRPC("SendChat", UCNetwork.MessageReceiver.ServerOnly, -1, messageInputField.text);
        }

        // Empty the message input.
        messageInputField.text = "";
    }
}
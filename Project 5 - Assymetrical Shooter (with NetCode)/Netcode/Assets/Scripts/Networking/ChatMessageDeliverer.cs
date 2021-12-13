/**
 * ChatManager.cs
 * Description: This script deliver a chat message from one player to other players.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ChatMessageDeliverer : MonoBehaviour
{
    // Called when a message is received.
    public delegate void MessageReceived(string message);
    public MessageReceived messageReceivedCallback;

    public const int MAX_MESSAGE_LENGTH = 250;

    private void Start()
    {
        messageReceivedCallback += HUDLogic.Singleton.ReceiveChatMessage;

        NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("SendMessage", SendMessage);
        NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("ReceiveMessage", ReceiveMessage);
    }

    // Request the server to send a message to clients.
    // messageType = 1: send to all players, messageType = 2: send to team members only
    public void RequestSendMessage(string message, int messageType)
    {
        if (message.Trim().Length < 1)
            return;

        using FastBufferWriter writer = new FastBufferWriter(1 + MAX_MESSAGE_LENGTH, Unity.Collections.Allocator.Temp);
        writer.WriteByte((byte)messageType);
        writer.WriteValueSafe(message);
        NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("SendMessage", 0, writer);     
    }

    // From the server, send a message to clients.
    public void SendMessage(ulong senderClientId, FastBufferReader messagePayload)
    {
        if (!ASServer.Singleton.players.ContainsKey(senderClientId))
            return;

        // Read the message sent by the client.
        byte messageType;
        string message;
        messagePayload.ReadByteSafe(out messageType);
        messagePayload.ReadValueSafe(out message);

        if (messageType == 1)
        {
            message = ASServer.Singleton.players[senderClientId].name + ": " + message;

            using FastBufferWriter writer = new FastBufferWriter(1 + MAX_MESSAGE_LENGTH, Unity.Collections.Allocator.Temp);
            writer.WriteValueSafe(message);
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("ReceiveMessage", writer);
        }
        else
        {
            message = ASServer.Singleton.players[senderClientId].name + "[TEAM]: " + message;

            // Get all players of the same team.
            List<ulong> targetClientIds = new List<ulong>();

            foreach (KeyValuePair<ulong, ASServer.PlayerData> player in ASServer.Singleton.players)
            {
                if (player.Value.team == ASServer.Singleton.players[senderClientId].team)
                {
                    targetClientIds.Add(player.Key);
                }
            }

            using FastBufferWriter writer = new FastBufferWriter(1 + MAX_MESSAGE_LENGTH, Unity.Collections.Allocator.Temp);
            writer.WriteValueSafe(message);
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("ReceiveMessage", targetClientIds, writer);
        }

        // Debug.Log("Message sent: " + message);
    }

    // Receive a message from the server.
    public void ReceiveMessage(ulong senderClientId, FastBufferReader messagePayload)
    {
        string message;
        messagePayload.ReadValueSafe(out message);
        messageReceivedCallback?.Invoke(message);

        // Debug.Log("Message received: " + message);
    }
}

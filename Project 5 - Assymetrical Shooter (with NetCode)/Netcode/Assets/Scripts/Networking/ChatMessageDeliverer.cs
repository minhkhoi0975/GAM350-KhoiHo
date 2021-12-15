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

        NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("SendMessage", OnSendMessage);
        NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("ReceiveMessage", OnReceiveMessage);
    }

    // Request the server to send a message to clients.
    // messageType = 1: send to all players, messageType = 2: send to team members only
    public void RequestSendMessage(string message, int messageType)
    {
        if (message.Trim().Length < 1)
            return;

        // If the client is a host, send message immediately.
        if (NetworkManager.Singleton.IsHost)
        {
            SendMessage(0, (byte)messageType, message);
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            using FastBufferWriter writer = new FastBufferWriter(1 + MAX_MESSAGE_LENGTH, Unity.Collections.Allocator.Temp);
            writer.WriteByteSafe((byte)messageType);
            writer.WriteValueSafe(message);
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("SendMessage", 0, writer);
        }

        Debug.Log("Message requested: " + message);
    }

    // From the server, send a message to clients.
    public void OnSendMessage(ulong senderClientId, FastBufferReader messagePayload)
    {
        if (!ASServer.Singleton.players.ContainsKey(senderClientId))
            return;

        // Read the message sent by the client.
        byte messageType;
        string message;
        messagePayload.ReadByteSafe(out messageType);
        messagePayload.ReadValueSafe(out message);

        SendMessage(senderClientId, messageType, message);
    }

    // Receive a message from the server.
    public void OnReceiveMessage(ulong senderClientId, FastBufferReader messagePayload)
    {
        string message;
        messagePayload.ReadValueSafe(out message);
        messageReceivedCallback?.Invoke(message);

        Debug.Log("Message received: " + message);
    }

    void SendMessage(ulong senderClientId, byte messageType, string message)
    {
        if (!ASServer.Singleton.players.ContainsKey(senderClientId))
            return;

        if (messageType == 1)
        {
            message = ASServer.Singleton.players[senderClientId].name + ": " + message;
            SendMessageToAllClients(message);
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

            SendMessageToSpecificClients(targetClientIds, message);
        }

        Debug.Log("Message sent: " + message);
    }

    void SendMessageToSpecificClients(List<ulong> targetClientIds, string message)
    {
        // The host (cliendId = 0) also receives the message.
        if (targetClientIds.Contains(0))
        {
            messageReceivedCallback?.Invoke(message);
        }

        using FastBufferWriter writer = new FastBufferWriter(1 + MAX_MESSAGE_LENGTH, Unity.Collections.Allocator.Temp);
        writer.WriteValueSafe(message);
        NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("ReceiveMessage", targetClientIds, writer);
    }

    void SendMessageToAllClients(string message)
    {
        // The host also receive the message.
        if (NetworkManager.Singleton.IsHost)
        {
            messageReceivedCallback?.Invoke(message);
        }

        using FastBufferWriter writer = new FastBufferWriter(1 + MAX_MESSAGE_LENGTH, Unity.Collections.Allocator.Temp);
        writer.WriteValueSafe(message);
        NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("ReceiveMessage", writer);
    }
}

/**
 * CharacterCustomizationLogic.cs
 * Description: This script handles the logic of the character selection panel.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCustomizationLogic : MonoBehaviour
{
    public Text playerName;
    public Dropdown characterType;
    public TacticsClient client;

    public void Ready()
    {
        client.clientNet.CallRPC("SetName", UCNetwork.MessageReceiver.ServerOnly, -1, playerName.text);
        client.clientNet.CallRPC("SetCharacterType", UCNetwork.MessageReceiver.ServerOnly, -1, characterType.value + 1);
        client.clientNet.CallRPC("IsReady", UCNetwork.MessageReceiver.ServerOnly, -1, true);
    }
}

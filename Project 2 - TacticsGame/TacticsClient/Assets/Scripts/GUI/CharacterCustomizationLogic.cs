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
    public Text listOfPlayers;
    public Button buttonReady;

    public TacticsClient client;

    bool isPlayerReady = false;

    // Tell the server to change the character's name.
    public void ChangePlayerName()
    {
        client.clientNet.CallRPC("SetName", UCNetwork.MessageReceiver.ServerOnly, -1, playerName.text);
    }

    // Tell the server to change the character's type (warrior/rogue/wizard).
    public void ChangeCharacterType()
    {
        client.clientNet.CallRPC("SetCharacterType", UCNetwork.MessageReceiver.ServerOnly, -1, characterType.value);
    }

    // Tell the server whether the client is ready or not.
    public void ChangeIsReady()
    {
        // Don't make the client ready if the character's name is empty or the client has not chosen a character type.
        if (playerName.text.Trim() == "" || characterType.value == 0)
            isPlayerReady = false;
        else
            isPlayerReady = !isPlayerReady;
        client.clientNet.CallRPC("Ready", UCNetwork.MessageReceiver.ServerOnly, -1, isPlayerReady);

        buttonReady.GetComponentInChildren<Text>().text = isPlayerReady ? "Unready" : "Ready";
    }

    // Update the list of players in the game session.
    public void UpdateListOfPlayers()
    {
        listOfPlayers.text = "";

        if(listOfPlayers)
        {
            foreach(KeyValuePair<int, TacticsClient.PlayerInfo> player in client.Players)
            {
                // Print player ID.
                listOfPlayers.text += player.Key + "\t\t";

                // Print character name.
                listOfPlayers.text += player.Value.name + "\t\t";

                // Print character class.
                switch(player.Value.characterClass)
                {
                    case 0:
                        listOfPlayers.text += "Not Decided\t\t";
                        break;
                    case 1:
                        listOfPlayers.text += "Warrior\t\t";
                        break;
                    case 2:
                        listOfPlayers.text += "Rouge\t\t";
                        break;
                    case 3:
                        listOfPlayers.text += "Wizard\t\t";
                        break;
                }

                // Print player team.
                listOfPlayers.text += "Team " + player.Value.team + "\t\t";

                // Print whether the player is ready.
                listOfPlayers.text += (player.Value.isReady ? "Ready" : "Not Ready") + "\n"; 
            }
        }
    }
}
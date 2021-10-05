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

    bool isReady = false;

    public void ChangePlayerName()
    {
        client.clientNet.CallRPC("SetName", UCNetwork.MessageReceiver.ServerOnly, -1, playerName.text);
    }

    public void ChangeCharacterType()
    {
        client.clientNet.CallRPC("SetCharacterType", UCNetwork.MessageReceiver.ServerOnly, -1, characterType.value);
    }

    public void ChangeIsReady()
    {
        if (playerName.text == "" || characterType.value == 0)
            isReady = false;
        else
            isReady = !isReady;

        client.clientNet.CallRPC("Ready", UCNetwork.MessageReceiver.ServerOnly, -1, isReady);
        buttonReady.GetComponentInChildren<Text>().text = isReady ? "Unready" : "Ready";
    }

    private void Update()
    {
        UpdateListOfPlayers();
    }

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

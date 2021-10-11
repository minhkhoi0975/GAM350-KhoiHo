/**
 * Player.cs
 * Description: This script handles a character's game object.
 * Programmer: Khoi Ho
 */

using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    // Materials for team 1 and 2.
    public Material team1Material, team2Material;

    public TextMesh textName;
    public TextMesh textCharacterType;
    public TextMesh textHealth;

    // Set the name of the character.
    public void SetName(string name)
    {
        textName.text = name;
    }

    // Set the character type (Warrior/Rogue/Wizard) of the character.
    public void SetCharacterType(int characterType)
    {
        switch (characterType)
        {
            case 1:
                textCharacterType.text = "Warrior";
                break;
            case 2:
                textCharacterType.text = "Rogue";
                break;
            case 3:
                textCharacterType.text = "Wizard";
                break;
        }
    }

    // Set the material to match the team.
    public void SetTeamMaterial(int team)
    {
        GetComponent<MeshRenderer>().material = team == 1 ? team1Material : team2Material;
    }

    // Set the health of the character.
    public void SetHealth(int health)
    {
        textHealth.text = health.ToString();
    }

    // Set the color of the texts.
    public void SetTextColor(Color color)
    {
        textName.color = color;
        textCharacterType.color = color;
        textHealth.color = color;
    }
}
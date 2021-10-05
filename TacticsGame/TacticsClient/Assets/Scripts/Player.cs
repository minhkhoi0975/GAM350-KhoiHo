using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    public Material team1Material;
    public Material team2Material;

    public TextMesh textName;
    public TextMesh textCharacterType;
    public TextMesh textTeam;

    float speed = 5;

    // Update is called once per frame
    void Update()
    {
        if (GetComponent<NetworkSync>().owned)
        {
            Vector3 movement = new Vector3(Input.GetAxis("Horizontal") * speed * Time.deltaTime, Input.GetAxis("Vertical") * speed * Time.deltaTime, 0);
            transform.position += movement;
        }
    }

    public void ChangeName(string name)
    {
        textTeam.text = name;
    }

    public void ChangeCharacterType(int characterType)
    {
        switch(characterType)
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

    public void ChangeTeam(int team)
    {
        textTeam.text = team.ToString();
        GetComponent<MeshRenderer>().material = team == 1 ? team1Material : team2Material;
    }
}
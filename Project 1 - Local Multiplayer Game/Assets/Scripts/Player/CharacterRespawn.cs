using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterRespawn : MonoBehaviour
{
    [SerializeField] GameObject CharacterPrefab;
    [SerializeField] Material CharacterBodyMaterial; // The material for the character's body.

    [SerializeField] private byte controlID = 1;  // The ID number that determines how this character is controlled (1 = WASD, 2 = Arrows).
    public byte ControlID
    {
        get
        {
            return controlID;
        }
        set
        {
            controlID = (byte)(value < 1 ? 1 : (value > 2 ? 2 : value));
        }
    }

    GameObject CurrentCharacterInstance;

    private void Awake()
    {
        // Hide the dummy.
        transform.GetChild(0).gameObject.SetActive(false);
    }

    private void Start()
    {
        Respawn();
    }

    private void Update()
    {
        // Respawn key is the same as firing key.
        if(Input.GetButtonDown("Fire1") && ControlID == 1 || Input.GetButtonDown("Fire2") && ControlID == 2)
        {
            Respawn();
        }
    }

    void Respawn()
    {
        // Don't spawn if CurrentCharacterInstance is not null.
        if(!CurrentCharacterInstance)
        {
            // Create a character.
            CurrentCharacterInstance = Instantiate(CharacterPrefab, transform.position, transform.rotation);

            // Set the control ID of the character.
            CharacterController CharacterController = CurrentCharacterInstance.GetComponentInChildren<CharacterController>();
            if(CharacterController)
            {
                CharacterController.ControlID = this.ControlID;
            }

            // Set the material of the character's body.
            Renderer CharacterBodyRenderer = CurrentCharacterInstance.GetComponentInChildren<Renderer>();
            if(CharacterBodyRenderer)
            {
                CharacterBodyRenderer.material = CharacterBodyMaterial;
            }
        }
    }
}

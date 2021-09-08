/**
 * PlayerCharacterInput.cs
 * Description: This script handles the inputs for player character.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Character))]
public class PlayerCharacterInput : MonoBehaviour
{
    [SerializeField] Character characterComponent;
    public int inputIndex = 0;                    // Each player has a unique input index. The index of the first player is 0.

    // Start is called before the first frame update
    void Awake()
    {
        if(characterComponent == null)
        {
            characterComponent = GetComponent<Character>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Firing
        if(Input.GetButtonDown("Fire" + inputIndex))
        {
            characterComponent.FireProjectile();
        }
    }

    private void FixedUpdate()
    {
        // Movement
        float vertical = Input.GetAxis("Vertical" + inputIndex);
        float horizontal = Input.GetAxis("Horizontal" + inputIndex);

        characterComponent.MoveCharacter(vertical, horizontal);
    }
}

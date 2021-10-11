using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInput : MonoBehaviour
{
    TacticsClient client;

    [SerializeField] Text cursorPositionText; // The position of the cursor on the game board.

    // Update is called once per frame
    void Update()
    {
        int gameBoardPosX, gameBoardPosY;

        // Update cursor's position on game board.


        // Left click - Move
        if (Input.GetButtonDown("Fire1"))
        {

        }

        // Right click - Attack
        if(Input.GetButtonDown("Fire2"))
        {

        }
    }
}

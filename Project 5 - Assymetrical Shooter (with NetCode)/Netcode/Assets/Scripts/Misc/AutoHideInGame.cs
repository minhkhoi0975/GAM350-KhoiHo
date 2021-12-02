/**
 * AutoHideInGame.cs
 * Description: This script automatically hides the game object when the game starts.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoHideInGame : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(false);
    }
}

/**
 * CharacterAppearance.cs
 * Description: This script manages the appearance of the character.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAppearance : MonoBehaviour
{
    public MeshRenderer meshRenderer;   // Reference to the mesh renderer of the character.

    public Material preyMaterial;       // The material when the character is a prey.
    public Material hunterMaterial;     // The material when the character is a hunter.

    public void Awake()
    {
        if(!meshRenderer)
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }
        if(!meshRenderer)
        {
            meshRenderer = GetComponentInChildren<MeshRenderer>(true);
        }
    }

    // Set the material of the character.
    public void SetMaterial(bool isHunter)
    {
        if(isHunter)
        {
            meshRenderer.material = hunterMaterial;
        }
        else
        {
            meshRenderer.material = preyMaterial;
        }
    }
}

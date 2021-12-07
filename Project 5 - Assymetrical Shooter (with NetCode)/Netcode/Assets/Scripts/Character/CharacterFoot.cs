/**
 * CharacterFoot.cs
 * Description: This script checks if the character is grounded.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CharacterFoot : NetworkBehaviour
{
    // The max angle of the slope on which the character can stand.
    public float maxSlopeAngle = 60.0f;

    // Is the character on the ground?
    NetworkVariable<bool> isGrounded = new NetworkVariable<bool>(false);
    public bool IsGrounded
    {
        get
        {
            return isGrounded.Value;
        }
    }

    // Is this character on a slope?
    NetworkVariable<bool> isOnSlope = new NetworkVariable<bool>(false);
    public bool IsOnSlope
    {
        get
        {
            return isOnSlope.Value;
        }
    }

    // Information about the ground the character stands on.
    RaycastHit groundInfo;
    public RaycastHit GroundInfo
    {
        get
        {
            return groundInfo;
        }
    }

    // The slope angle of the ground the character stands on.
    float slopeAngle;
    public float SlopeAngle
    {
        get
        {
            return slopeAngle;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (IsServer)
        {
            UpdateIsGrounded();
        }
    }

    private void UpdateIsGrounded()
    {
        isGrounded.Value = Physics.Raycast(transform.position, Vector3.down, out groundInfo, 0.2f);
        if (isGrounded.Value)
        {
            // Calculate the slope angle and determine whether the character is on a slope or not.
            slopeAngle = Vector3.Angle(groundInfo.normal, Vector3.up);
            if (slopeAngle >= 1.0f && slopeAngle <= maxSlopeAngle)
            {
                isOnSlope.Value = true;
            }
            else
            {
                isOnSlope.Value = false;
            }
        }
        else
        {
            slopeAngle = 0.0f;
            isOnSlope.Value = false;
        }
    }
}
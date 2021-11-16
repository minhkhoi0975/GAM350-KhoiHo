/**
 * CharacterFoot.cs
 * Description: This script checks if the character is grounded.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterFoot : MonoBehaviour
{
    // Reference to the network sync component.
    // Only update whether the character is grounded if the character is owned by this character object.
    [SerializeField] NetworkSync networkSync;

    // The max angle of the slope on which the character can stand.
    public float maxSlopeAngle = 60.0f;

    // Is the character on the ground?
    bool isGrounded = false;
    public bool IsGrounded
    {
        get
        {
            return isGrounded;
        }
    }

    // Is this character on a slope?
    bool isOnSlope = false;
    public bool IsOnSlope
    {
        get
        {
            return isOnSlope;
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

    // Store the previous position of the foot.
    Vector3 previousFootPosition;

    // Update is called once per frame
    void Update()
    {
        if (networkSync && networkSync.enabled && !networkSync.owned)
            return;

        UpdateIsGrounded();
    }

    private void UpdateIsGrounded()
    {
        // Don't update if the character does not move.
        if (transform.position == previousFootPosition)
            return;

        isGrounded = Physics.Raycast(transform.position, Vector3.down, out groundInfo, 0.2f);
        if (isGrounded)
        {
            // Calculate the slope angle and determine whether the character is on a slope or not.
            slopeAngle = Vector3.Angle(groundInfo.normal, Vector3.up);
            if (slopeAngle >= 1.0f && slopeAngle <= maxSlopeAngle)
            {
                isOnSlope = true;
            }
            else
            {
                isOnSlope = false;
            }
        }
        else
        {
            slopeAngle = 0.0f;
            isOnSlope = false;
        }

        // Update the current position of the foot.
        previousFootPosition = transform.position;
    }
}
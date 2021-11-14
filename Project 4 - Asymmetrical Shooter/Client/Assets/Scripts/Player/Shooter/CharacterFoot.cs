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

    bool isGrounded = false;
    public bool IsGrounded
    {
        get
        {
            return isGrounded;
        }
    }

    RaycastHit groundInfo;
    public RaycastHit GroundInfo
    {
        get
        {
            return groundInfo;
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
        //if (transform.position == previousFootPosition)
        //    return;

        isGrounded = Physics.Raycast(transform.position, Vector3.down, out groundInfo, 0.1f);
        previousFootPosition = transform.position;

        // Debug.Log(isGrounded ? "Grounded" : "Not Grounded");
    }
}
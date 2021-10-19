using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Vector3 moveDirection;
    public float moveSpeed = 10.0f;

    // Update is called once per frame
    void Update()
    {
        if (GetComponent<NetworkSync>().owned && moveDirection.magnitude != 0.0f)
        {
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnProjectileClient : MonoBehaviour
{
    [SerializeField] Camera camera;
    [SerializeField] ExampleClient client;

    List<GameObject> projectiles = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        if(!camera)
        {
            camera = FindObjectOfType<Camera>();        
        }

        if(!client)
        {
            client = GetComponent<ExampleClient>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("Fire1"))
        {
            Vector3 cursorPosition = camera.ScreenToWorldPoint(Input.mousePosition);
            cursorPosition.z = 10.0f;

            SpawnProjectile(cursorPosition);
        }

        if(Input.GetButtonDown("Fire2"))
        {
            DestroyProjectiles();
        }
    }

    private void SpawnProjectile(Vector3 cursorPosition)
    {
        // Create a projectile.
        GameObject projectile = client.clientNet.Instantiate("Projectile", client.playerGameObject.transform.position, Quaternion.identity);

        // Add the projectile to area 1.
        projectile.GetComponent<NetworkSync>().AddToArea(1);

        // Set the move direction of the projectile.
        projectile.GetComponent<Projectile>().moveDirection = (cursorPosition - projectile.transform.position).normalized;

        // Add to list.
        projectiles.Add(projectile);
    }

    void DestroyProjectiles()
    {
        foreach(GameObject projectile in projectiles)
        {
            client.clientNet.Destroy(projectile.GetComponent<NetworkSync>().GetId());
        }

        projectiles.Clear();
    }
}

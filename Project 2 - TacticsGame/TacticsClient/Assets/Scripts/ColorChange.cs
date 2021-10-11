using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorChange : MonoBehaviour
{
    public ClientNetwork client;
    public Material material;

    private void Awake()
    {
        if (!client)
        {
            client = FindObjectOfType<ClientNetwork>();
        }

        // Create a copy of the material so that when a cube changes color, other cubes don't change color.
        if (!material)
        {
            GetComponent<MeshRenderer>().material = material;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            // Raycast to check if the player clicks an object.
            RaycastHit hitInfo;
            Ray cameraToMouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(cameraToMouseRay, out hitInfo))
            {
                GameObject hitObject = hitInfo.collider.gameObject;

                // If the player clicks an object, check the colors of all the object of the same type.
                if(hitObject.GetComponent<Player>())
                {
                    // Get a random color.
                    float r = Random.Range(0.0f, 1.0f);
                    float g = Random.Range(0.0f, 1.0f);
                    float b = Random.Range(0.0f, 1.0f);

                    // Call RPC to all clients to change color.
                    client.CallRPC("ChangeColor", UCNetwork.MessageReceiver.AllClients, hitObject.GetComponent<NetworkSync>().GetId(), r, g, b, 1.0f);
                    Debug.Log("Changed color.");
                }    
            }
        }
    }

    public void ChangeColor(float r, float g, float b, float a)
    {
        Renderer renderer = GetComponent<MeshRenderer>();
        renderer.material.color = new Color(r, g, b, a);
    }
}

using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

    float speed = 5;

    bool hunter = false;

    // Update is called once per frame
    void Update()
    {
        if (GetComponent<NetworkSync>().owned)
        {
            Vector3 movement = new Vector3(Input.GetAxis("Horizontal") * speed * Time.deltaTime, Input.GetAxis("Vertical") * speed * Time.deltaTime, 0);
            transform.position += movement;
        }
    }

    // RPC to be called by the server
    public void IsHunter(bool aIsHunter)
    {
        hunter = aIsHunter;

        if (hunter)
        {
            speed = 8;
        }
        else
        {
            speed = 5;
        }
    }
    
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GameBoardTilePosition))]
public class GameBoardTileInput : MonoBehaviour
{
    GameBoardTilePosition tile;

    // The material of the tile when the cursor is still over the tile.
    [SerializeField] Material mouseOverMaterial;

    // The material of the tile before the cursor is over the tile.
    Material previousMaterial;

    // Return true if the cursor is over the tile
    bool isCursorOver = false;

    // Reference to client.
    public TacticsClient client;

    // Start is called before the first frame update
    void Start()
    {
        if(!tile)
        {
            tile = GetComponent<GameBoardTilePosition>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isCursorOver)
            return;

        if(Input.GetButtonDown("Fire1"))
        {
            Move();
        }
        else if(Input.GetButtonDown("Fire2"))
        {
            Attack();
        }
    }

    // When the cursor is over the tile, change the material of the tile.
    private void OnMouseEnter()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

        previousMaterial = meshRenderer.material;
        meshRenderer.material = mouseOverMaterial;

        isCursorOver = true;
    }

    // When the cursor gets out of the tile, restore the color of the tile. 
    private void OnMouseExit()
    {
        GetComponent<MeshRenderer>().material = previousMaterial;

        isCursorOver = false;
    }

    void Move()
    {
        client.clientNet.CallRPC("RequestMove", UCNetwork.MessageReceiver.ServerOnly, -1, tile.xPos, tile.yPos);
    }

    void Attack()
    {
        client.clientNet.CallRPC("RequestAttack", UCNetwork.MessageReceiver.ServerOnly, -1, tile.xPos, tile.yPos);
    }
}

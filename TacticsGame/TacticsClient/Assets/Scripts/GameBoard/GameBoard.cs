using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBoard : MonoBehaviour
{
    // The size of the board.
    public int sizeX, sizeY;

    // The prefab of a tile.
    public GameObject tilePrefab;

    // Two adjacent tiles have different materials.
    public Material tileMaterial1, tileMaterial2;

    GameObject[,] tiles = null;


    // Start is called before the first frame update
    void Start()
    {
        GenerateNewGameBoard();
    }

    public void GenerateNewGameBoard()
    {
        if (!tilePrefab || !tileMaterial1 || !tileMaterial2)
            return;

        // Remove the old game board.
        if (tiles != null)
        {
            RemoveOldGameBoard();
        }

        // Create a new game board.
        tiles = new GameObject[sizeX, sizeY];
        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                // Create a tile.
                tiles[i, j] = Instantiate(tilePrefab, transform.position + new Vector3(1 + 2 * i, 1 + 2 * j, 0), transform.rotation, transform);

                // Set the material of the tile.
                if ((i + j) % 2 == 0)
                {
                    tiles[i, j].GetComponent<MeshRenderer>().material = tileMaterial1;
                }
                else
                {
                    tiles[i, j].GetComponent<MeshRenderer>().material = tileMaterial2;
                }
            }
        }
    }

    public void RemoveOldGameBoard()
    {
        int oldBoardSizeX = tiles.GetLength(0);
        int oldBoardSizeY = tiles.GetLength(1);

        for (int i = 0; i < oldBoardSizeX; i++)
        {
            for (int j = 0; j < oldBoardSizeY; j++)
            {
                if (tiles[i, j])
                {
                    Destroy(tiles[i, j]);
                }
            }
        }
    }
}

/**
 * GameBoard.cs
 * Description: This script generates a game board.
 * Programmer: Khoi Ho
 */

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

    // Reference to the client.
    public TacticsClient client;

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
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                // Create a tile.
                tiles[x, y] = Instantiate(tilePrefab, transform.position + new Vector3(1 + 2 * x, 1 + 2 * y, 0), transform.rotation, transform);

                // Set the material of the tile.
                if ((x + y) % 2 == 0)
                {
                    tiles[x, y].GetComponent<MeshRenderer>().material = tileMaterial1;
                }
                else
                {
                    tiles[x, y].GetComponent<MeshRenderer>().material = tileMaterial2;
                }

                // Set the position of the tile on the game board.
                GameBoardTilePosition tilePosition = tiles[x, y].GetComponent<GameBoardTilePosition>();
                tilePosition.xPos = x;
                tilePosition.yPos = y;

                // Make the tile reference to the client.
                GameBoardTileInput tileInput = tiles[x, y].GetComponent<GameBoardTileInput>();
                tileInput.client = client;
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
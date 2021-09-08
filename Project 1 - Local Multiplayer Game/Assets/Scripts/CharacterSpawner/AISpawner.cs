/**
 * AISpawner.cs
 * Description: This script is used for spawning AI characters.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISpawner : MonoBehaviour
{
    [SerializeField] GameObject characterPrefab;
    [SerializeField] float spawnDelayInSeconds = 10.0f;

    bool canSpawn = true; // Used to delay spawning.

    private void Awake()
    {
        // Hide the dummy.
        transform.GetChild(0).gameObject.SetActive(false);
    }

    // Start is called before the first frame update
    private void Update()
    {
        if(canSpawn)
        {
            StartCoroutine("SpawnAICharacter");
        }
    }

    IEnumerator SpawnAICharacter()
    {
        // Create an AI character.
        Instantiate(characterPrefab, transform.position, transform.rotation);

        // Delay.
        canSpawn = false;
        yield return new WaitForSeconds(spawnDelayInSeconds);
        canSpawn = true;
    }
}

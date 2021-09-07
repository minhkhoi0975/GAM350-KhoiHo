using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISpawner : MonoBehaviour
{
    [SerializeField] GameObject CharacterPrefab;
    [SerializeField] float SpawnDelay = 10.0f;

    bool CanSpawn = true;

    private void Awake()
    {
        // Hide the dummy.
        transform.GetChild(0).gameObject.SetActive(false);
    }

    // Start is called before the first frame update
    private void Update()
    {
        if(CanSpawn)
        {
            StartCoroutine("SpawnAICharacter");
        }
    }

    IEnumerator SpawnAICharacter()
    {
        // Create an AI character.
        Instantiate(CharacterPrefab, transform.position, transform.rotation);

        // Delay.
        CanSpawn = false;
        yield return new WaitForSeconds(SpawnDelay);
        CanSpawn = true;
    }
}

/**
 * Timer.cs
 * Description: This script handles timers.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Timer : MonoBehaviour
{
    [SerializeField] float timeLimitInSeconds = 20.0f;
    [SerializeField] string gameOverLevel;                        // Load this level when the timer reaches 0.

    private float  timeRemainingInSeconds;
    public float TimeRemainingInSeconds
    {
        get
        {
            return timeRemainingInSeconds;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        timeRemainingInSeconds = timeLimitInSeconds;
    }

    // Update is called once per frame
    void Update()
    {
        if(timeRemainingInSeconds > 0.0f)
        {
            // Reduce time.
            timeRemainingInSeconds -= Time.deltaTime;
        }
        else
        {
            // Update the previously played level.
            GameManager.Instance.previouslySelectedLevel = SceneManager.GetActiveScene().name;

            // Load the "Game Over" scene.
            SceneManager.LoadScene(gameOverLevel);
        }
    }
}

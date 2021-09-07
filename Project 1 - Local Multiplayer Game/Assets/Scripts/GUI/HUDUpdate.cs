using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDUpdate : MonoBehaviour
{
    [SerializeField] Text TxtPlayer1Score;
    [SerializeField] Text TxtPlayer2Score;

    // Update is called once per frame
    void Update()
    {
        if(GameManager.Instance)
        {
            TxtPlayer1Score.text = GameManager.Instance.Player1Score.ToString();
            TxtPlayer2Score.text = GameManager.Instance.Player2Score.ToString();
        }
    }
}

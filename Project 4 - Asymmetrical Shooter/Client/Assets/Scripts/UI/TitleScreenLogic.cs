using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TitleScreenLogic : MonoBehaviour 
{
    public Text server;
    public Text port;
    public Text playerName;
    public ASClient client;

    public void Connect()
    {
        if (playerName.text.Trim().Length != 0)
        {
            client.ConnectToServer(server.text, int.Parse(port.text));
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

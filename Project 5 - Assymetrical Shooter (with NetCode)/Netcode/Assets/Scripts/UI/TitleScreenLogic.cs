using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;
using System.Collections;

public class TitleScreenLogic : MonoBehaviour
{
    static TitleScreenLogic singleton;
    public TitleScreenLogic Singleton
    {
        get
        {
            return singleton;
        }
    }

    public GameObject titleScreenPanel;

    public Text server;
    public Text port;
    public Text playerName;

    private void Awake()
    {
        if (!singleton)
        {
            singleton = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void StartHost()
    {
        ASServer.Singleton.StartServer(server.text, int.Parse(port.text), true);
        HideTitleScreenPanel();
    }

    public void StartServer()
    {
        ASServer.Singleton.StartServer(server.text, int.Parse(port.text), false);
        HideTitleScreenPanel();
    }

    public void StartClient()
    {   
        ASClient.Singleton.StartClient(server.text, int.Parse(port.text));
        HideTitleScreenPanel();
    }

    // Wait until the client has connected to the server.
    public void QuitGame()
    {
        Application.Quit();
    }

    void HideTitleScreenPanel()
    {
        titleScreenPanel.SetActive(false);
    }
}

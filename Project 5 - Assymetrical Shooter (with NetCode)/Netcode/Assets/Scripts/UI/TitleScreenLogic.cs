using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;
using System.Collections;

public class TitleScreenLogic : MonoBehaviour 
{
    public ASServer serverNet;
    public ASClient clientNet;

    public GameObject titleScreenPanel;

    public Text server;
    public Text port;
    public Text playerName;

    public void StartHost()
    {
        serverNet.StartServer(server.text, int.Parse(port.text), true);
        HideTitleScreenPanel();
    }

    public void StartServer()
    {
        serverNet.StartServer(server.text, int.Parse(port.text), false);
        HideTitleScreenPanel();
    }

    public void StartClient()
    {
        clientNet.StartClient(server.text, int.Parse(port.text));
        HideTitleScreenPanel();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    void HideTitleScreenPanel()
    {
        titleScreenPanel.SetActive(false);
    }
}

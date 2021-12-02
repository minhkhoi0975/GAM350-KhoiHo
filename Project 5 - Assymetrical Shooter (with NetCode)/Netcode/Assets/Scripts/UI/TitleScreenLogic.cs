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
        StartCoroutine(WaitingForConnection());
    }

    // Wait until the client has connected to the server.
    IEnumerator WaitingForConnection()
    {
        float requestConnectionTime = Time.realtimeSinceStartup;

        while (!NetworkManager.Singleton.IsClient)
        {
            float currentTime = Time.realtimeSinceStartup;

            // If the waiting time is too long, automatically disconnect the client.
            if(currentTime - requestConnectionTime >= NetworkManager.Singleton.NetworkConfig.ClientConnectionBufferTimeout)
            {
                clientNet.DisconnectFromServer();
                yield return null;
            }
        }

        HideTitleScreenPanel();
        yield return null;
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

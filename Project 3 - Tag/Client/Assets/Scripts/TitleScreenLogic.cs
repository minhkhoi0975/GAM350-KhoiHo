using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TitleScreenLogic : MonoBehaviour 
{
    public Text server;
    public Text port;
    public Text name;
    public TagClient client;

    public void Connect()
    {
        if (name.text.Trim().Length != 0)
        {
            client.ConnectToServer(server.text, int.Parse(port.text));
        }
    }
}

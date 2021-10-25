using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RPCs : MonoBehaviour
{

    public void PrintToLog(string aMessage, int aId, Vector3 test)
    {
        Debug.Log(aMessage);
    }
}

using System.Net;
using UnityEngine;
using BT_NetworkSystem;

public class ClientMonoBehaviour : MonoBehaviour
{
    private ClientNetManager clientNetManager = new();

    public void OnStartClient(string playerName)
    {
        Debug.Log("Start Connection player");
        clientNetManager.Initialize(new Player(playerName, -1));
        
        clientNetManager.StartClient();
        clientNetManager.Connect();
    }

    public void OnCloseClient()
    {
    }

    private void Update()
    {
        clientNetManager.Update();
    }
}
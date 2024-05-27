using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ClientNetManager : NetworkManager
{
    public Player playerData;

    public void OnStart()
    {
        this.port = port;
        this.ipAddress = ip;

        connection = new UdpConnection(ip, port, this);

        playerData = new Player(name, -1);

        MessageManager.Instance.OnSendHandshake(playerData.clientId, playerData.id);
    }

    public override void OnReceiveDataEvent(byte[] data, IPEndPoint ip)
    {
        Debug.Log("a");
    }

    void Update()
    {

        if (!isServer && initialized)
        {
            if ((DateTime.UtcNow - lastMessageRecieved).Seconds > timeOut)
            {
                Debug.Log((DateTime.UtcNow - lastMessageRecieved).Seconds);
                Disconect();
                Debug.Log("disconected from server = ");

                CanvasSwitcher.Instance.SwitchCanvas(modifyCanvas.networkScreen);

            }
        }
    }

    public void Disconnect()
    {
        clients.Clear();
        connection.Close();
        initialized = false;
    }

    public void OnReceiveData(byte[] data, IPEndPoint ip)
    {
        MessageManager.Instance.OnRecieveMessage(data, ip);

        if (OnReceiveEvent != null)
            OnReceiveEvent.Invoke(data, ip);
    }

    public void SendToServer(byte[] data)
    {
        connection.Send(data);
    }

    public override void CheckTimeOut()
    {
        throw new NotImplementedException();
    }

    public override void OnUpdate()
    {
        throw new NotImplementedException();
    }

    public override void CheckPingPong(byte[] data, IPEndPoint ip)
    {
        throw new NotImplementedException();
    }
}

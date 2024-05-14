using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class MessageManager : MonoBehaviourSingleton<MessageManager>
{
    private NetCode netCode = new NetCode();
    private NetMessageToServer netMessageToServer = new NetMessageToServer();
    private NetMessageToClient netMessageToClient = new NetMessageToClient();

    public void OnRecieveMessage(byte[] data, IPEndPoint Ip)
    {
        MessageType typeMessage = (MessageType)BitConverter.ToInt32(data, 0);

        switch (typeMessage)
        {
            case MessageType.MessageToServer:

                Players newPlayer = new Players(netMessageToServer.Deserialize(data).Item2, netMessageToServer.Deserialize(data).Item1);

                newPlayer.id = NetworkManager.Instance.clientId;
                newPlayer.clientId = netMessageToServer.Deserialize(data).Item2;

                NetworkManager.Instance.addPlayer(newPlayer);

                netMessageToClient.data = NetworkManager.Instance.players;

                data = netMessageToClient.Serialize();

                NetworkManager.Instance.clientId++;
                Debug.Log("add new client = Client Id: " + netMessageToClient.data[netMessageToClient.data.Count - 1].clientId + " - Id: " + netMessageToClient.data[netMessageToClient.data.Count - 1].id);

                break;

            case MessageType.MessageToClient:
                NetworkManager.Instance.players = netMessageToClient.Deserialize(data);
                for (int i = 0; i < NetworkManager.Instance.players.Count; i++)
                {
                    if (NetworkManager.Instance.players[i].clientId == NetworkManager.Instance.playerData.clientId)
                    {
                        NetworkManager.Instance.playerData.id = NetworkManager.Instance.players[i].id;
                        break;
                    }
                }
                break;

            case MessageType.Console:
                string playerName = "";
                for (int i = 0; i < NetworkManager.Instance.players.Count; i++)
                {
                    if (NetworkManager.Instance.players[i].id == netCode.Deserialize(data).Item1)
                    {
                        playerName = NetworkManager.Instance.players[i].clientId;
                        break;
                    }
                }

                ChatScreen.Instance.OnReceiveDataEvent(playerName + " : " + netCode.Deserialize(data).Item2);
                break;

            case MessageType.Position:

                break;

            case MessageType.PingPong:
                if (!NetworkManager.Instance.isServer)
                {
                    NetworkManager.Instance.pingText.text = "Ping = " + (DateTime.UtcNow - NetworkManager.Instance.lastMessageSended).Milliseconds;
                }
                SendPingPong(data, Ip);
                break;

            default:
                Debug.LogError("Message type not found");
                break;
        }

        if (NetworkManager.Instance.isServer && typeMessage != MessageType.PingPong)
        {
            NetworkManager.Instance.Broadcast(data);
        }

    }

    private void SendPingPong(byte[] data, IPEndPoint Ip)
    {
        List<byte> outData = new List<byte>();
        outData.AddRange(BitConverter.GetBytes((int)MessageType.PingPong));

        if (NetworkManager.Instance.isServer)
        {
            NetworkManager.Instance.ResetClientTimer(Ip);
            NetworkManager.Instance.SendToClient(outData.ToArray(), Ip);
        }
        else
        {
            NetworkManager.Instance.lastMessageSended = DateTime.UtcNow;

            NetworkManager.Instance.lastMessageRecieved = DateTime.UtcNow;
            NetworkManager.Instance.SendToServer(outData.ToArray());
        }
    }

    private void OnSendMessage(byte[] message)
    {
        if (NetworkManager.Instance.isServer)
        {
            NetworkManager.Instance.Broadcast(message);
        }
        else
        {
            NetworkManager.Instance.SendToServer(message);
        }
    }

    public void OnSendConsoleMessage(string message)
    {
        netCode.data.Item1 = NetworkManager.Instance.playerData.id;
        netCode.data.Item2 = message;

        if (NetworkManager.Instance.isServer)
        {
            ChatScreen.Instance.OnReceiveDataEvent(message);
        }
        OnSendMessage(netCode.Serialize());
    }

    public void OnSendHandshake(string name, int id)
    {
        netMessageToServer.data.Item1 = id; //not assigned id
        netMessageToServer.data.Item2 = name; //client id
        NetworkManager.Instance.SendToServer(netMessageToServer.Serialize());
    }

    public void StartPingPong()
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)MessageType.PingPong));

        NetworkManager.Instance.SendToServer(outData.ToArray());
    }
}

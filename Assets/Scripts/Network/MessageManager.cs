using System;
using System.Net;
using UnityEditor.PackageManager;
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

                (int, string) PlayerData = netMessageToClient.Deserialize(data);

                PlayerData.Item1 = NetworkManager.Instance.clientId;
                PlayerData.Item2 = netMessageToClient.Deserialize(data).Item2;

                NetworkManager.Instance.addPlayer(PlayerData.Item2, PlayerData.Item1);

                netMessageToClient.data = PlayerData;

                data = netMessageToClient.Serialize();

                NetworkManager.Instance.clientId++;
                Debug.Log("add new client = Client Id: " + netMessageToClient.Deserialize(data).Item2 + " - Id: " + netMessageToClient.Deserialize(data).Item1);

                break;

            case MessageType.MessageToClient:
                NetworkManager.Instance.addPlayer(netMessageToServer.Deserialize(data).Item2, netMessageToClient.Deserialize(data).Item1);
                Debug.Log("add new client = Client Id: " + netMessageToClient.Deserialize(data).Item2 + " - Id: " + netMessageToClient.Deserialize(data).Item1);
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

            default:
                Debug.LogError("Message type not found");
                break;
        }

        if (NetworkManager.Instance.isServer)
        {
            NetworkManager.Instance.Broadcast(data);
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
        netCode.data.Item2 = message;

        if (NetworkManager.Instance.isServer)
        {
            ChatScreen.Instance.OnReceiveDataEvent(message);
        }
        OnSendMessage(netCode.Serialize());
    }

    public void OnSendHandshake(string name)
    {
        netMessageToServer.data.Item1 = -1; //not assigned id
        netMessageToServer.data.Item2 = name; //client id
        NetworkManager.Instance.SendToServer(netMessageToServer.Serialize());
    }
}

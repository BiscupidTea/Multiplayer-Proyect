using System;
using UnityEditor.PackageManager;
using UnityEngine;

public class MessageManager : MonoBehaviourSingleton<MessageManager>
{
    private NetCode netCode = new NetCode("");
    private NetMessageToServer netMessageToServer = new NetMessageToServer();
    private NetMessageToClient netMessageToClient = new NetMessageToClient();

    public void OnRecieveMessage(byte[] data)
    {
        MessageType typeMessage = (MessageType)BitConverter.ToInt32(data, 0);

        switch (typeMessage)
        {
            case MessageType.MessageToServer:
                NetworkManager.Instance.addPlayer(netMessageToClient.Deserialize(data).Item3);

                (int, int, string) PlayerData = netMessageToClient.Deserialize(data);
                PlayerData.Item1 = (int)MessageType.MessageToClient;
                PlayerData.Item2 = NetworkManager.Instance.clientId;
                PlayerData.Item3 = netMessageToClient.Deserialize(data).Item3;

                netMessageToClient.data = PlayerData;

                data = netMessageToClient.Serialize();

                NetworkManager.Instance.clientId++;

                break;

            case MessageType.MessageToClient:
                NetworkManager.Instance.addPlayer(netMessageToServer.Deserialize(data));

                break;

            case MessageType.Console:
                ChatScreen.Instance.OnReceiveDataEvent(netCode.Deserialize(data));

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
        netCode.data = message;

            
        if (NetworkManager.Instance.isServer)
        {
            ChatScreen.Instance.OnReceiveDataEvent(message);
        }
        OnSendMessage();
    }
}

using System;
using UnityEngine;

public class MessageManager : MonoBehaviourSingleton<MessageManager>
{
    private NetCode netCode = new NetCode("");
    public void OnRecieveMessage(byte[] data)
    {
        MessageType typeMessage = (MessageType)BitConverter.ToInt32(data, 0);

        switch (typeMessage)
        {
            case MessageType.HandShake:

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

    public void OnSendConsoleMessage(string message)
    {
        netCode.data = message;
        byte[] convertedMessage = netCode.Serialize();

        if (NetworkManager.Instance.isServer)
        {
            NetworkManager.Instance.Broadcast(convertedMessage);
            ChatScreen.Instance.OnReceiveDataEvent(message);
        }
        else 
        { 
            NetworkManager.Instance.SendToServer(convertedMessage);
        }
    }

    public void OnSendHandshake()
    {
       
    }
}

using System;
using UnityEngine;

public class MessageManager : MonoBehaviourSingleton<MessageManager>
{    
    public void OnRecieveMessage(byte[] data)
    {
        if (NetworkManager.Instance.isServer)
        {
           
        }
        else 
        { 
        
        }

        MessageType newMessage = (MessageType)BitConverter.ToInt32(data, 0);

        switch (newMessage)
        {
            case MessageType.HandShake:

                break;

            case MessageType.Console:

                break;

            case MessageType.Position:

                break;

            default:
                Debug.LogError("Message type not found");
                break;
        }
    }
}

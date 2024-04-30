using System.Collections.Generic;
using UnityEngine;
using System;

public enum MessageType
{
    MessageToServer = 0,
    MessageToClient = 1,
    Console = 2,
    Position = 3
}

public abstract class BaseMessage<PayLoadType>
{
    public PayLoadType data;
    public Action<PayLoadType> OnDeserialize;
    public abstract MessageType GetMessageType();
    public abstract byte[] Serialize();
    public abstract PayLoadType Deserialize(byte[] message);
    public abstract PayLoadType GetData();
}

public abstract class OrderableMessage<PayloadType> : BaseMessage<PayloadType>
{
    protected OrderableMessage(byte[] message)
    {
        MsgID = BitConverter.ToUInt64(message, 4);
    }

    protected static ulong lastMsgID = 0;

    protected ulong MsgID = 0;
    protected static Dictionary<PayloadType, ulong> lastExecutedMsgID = new Dictionary<PayloadType, ulong>();
}

public class NetMessageToClient : BaseMessage<List<Players>>
{
    public override List<Players> Deserialize(byte[] message)
    {
        int currentPosition = 0;
        currentPosition += 4;

        int totalPlayers = BitConverter.ToInt32(message, currentPosition);
        Debug.Log("total player: " + totalPlayers);

        currentPosition += 4;

        List<Players> newPlayerList = new List<Players>();


        for (int i = 0; i < totalPlayers; i++)
        {
            //id
            int Id = BitConverter.ToInt32(message, currentPosition);
            currentPosition += 4;

            //client id lenght
            int clientIdLenght = BitConverter.ToInt32(message, currentPosition);
            Debug.Log(clientIdLenght);

            string clientId = "";
            currentPosition += 4;

            //client id
            for (int j = 0; j < clientIdLenght; j++)
            {
                clientId += (char)message[currentPosition];
                currentPosition += 1; 
            }

            Debug.Log(clientId + " : " + Id);
            newPlayerList.Add(new Players(clientId, Id));
        }

        return newPlayerList;
    }

    public override List<Players> GetData()
    {
        return data;
    }

    public override MessageType GetMessageType()
    {
        return MessageType.MessageToClient;
    }

    public override byte[] Serialize()
    {
        List<byte> outData = new List<byte>();
        //message type
        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));

        //total Players
        outData.AddRange(BitConverter.GetBytes(data.Count));

        //insert Players
        for (int i = 0; i < data.Count; i++)
        {
            outData.AddRange(BitConverter.GetBytes(data[i].id));
            outData.AddRange(BitConverter.GetBytes(data[i].clientId.Length));

            for (int j = 0; j < data[i].clientId.Length; j++)
            {
                outData.Add((byte)data[i].clientId[j]);
            }
        }

        return outData.ToArray();
    }
}

public class NetMessageToServer : BaseMessage<(int, string)>
{
    //ID
    //ClientID

    public override (int, string) Deserialize(byte[] message)
    {
        (int, string) outData;

        outData.Item1 = BitConverter.ToInt32(message, 4); //ID

        outData.Item2 = "";//ClientID
        int messageLenght = BitConverter.ToInt32(message, 8);

        for (int i = 0; i < messageLenght; i++)
        {
            outData.Item2 += (char)message[12 + i];
        }
        return outData;
    }

    public override (int, string) GetData()
    {
        return data;
    }

    public override MessageType GetMessageType()
    {
        return MessageType.MessageToServer;
    }

    public override byte[] Serialize()
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));

        outData.AddRange(BitConverter.GetBytes(data.Item1)); //ID

        outData.AddRange(BitConverter.GetBytes(data.Item2.Length)); //ClientID

        for (int i = 0; i < data.Item2.Length; i++)
        {
            outData.Add((byte)data.Item2[i]);
        }

        return outData.ToArray();
    }
}

public class NetVector3 : BaseMessage<Vector3>
{
    private static ulong lastMsgID = 0;
    public NetVector3(Vector3 data)
    {
        this.data = data;
    }

    public override Vector3 Deserialize(byte[] message)
    {
        Vector3 outData;

        outData.x = BitConverter.ToSingle(message, 8);
        outData.y = BitConverter.ToSingle(message, 12);
        outData.z = BitConverter.ToSingle(message, 16);

        return outData;
    }

    public override Vector3 GetData()
    {
        return data;
    }

    public override MessageType GetMessageType()
    {
        return MessageType.Position;
    }

    public override byte[] Serialize()
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
        outData.AddRange(BitConverter.GetBytes(lastMsgID++));
        outData.AddRange(BitConverter.GetBytes(data.x));
        outData.AddRange(BitConverter.GetBytes(data.y));
        outData.AddRange(BitConverter.GetBytes(data.z));

        return outData.ToArray();
    }
}

public class NetCode : BaseMessage<(int, string)>
{
    public override (int, string) Deserialize(byte[] message)
    {
        (int, string) outData;

        //PlayerID
        outData.Item1 = BitConverter.ToInt32(message, 4);

        //message
        outData.Item2 = "";
        int messageLenght = BitConverter.ToInt32(message, 8);

        for (int i = 0; i < messageLenght; i++)
        {
            outData.Item2 += (char)message[12 + i];
        }

        return outData;
    }

    public override (int, string) GetData()
    {
        return data;
    }

    public override MessageType GetMessageType()
    {
        return MessageType.Console;
    }

    public override byte[] Serialize()
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));

        outData.AddRange(BitConverter.GetBytes(data.Item1));
        outData.AddRange(BitConverter.GetBytes(data.Item2.Length));

        for (int i = 0; i < data.Item2.Length; i++)
        {
            outData.Add((byte)data.Item2[i]);
        }

        return outData.ToArray();
    }
}
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Data;

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

public class NetMessageToClient : BaseMessage<(int, string)>
{
    public override (int, string) Deserialize(byte[] message)
    {
        (int, string) outData;

        outData.Item1 = BitConverter.ToInt32(message, 4); //PlayerID
        int messageLenght = BitConverter.ToInt32(message, 8);

        outData.Item2 = ""; //Message

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
        return MessageType.MessageToClient;
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

        Debug.Log("Message to Server from: " + outData.Item2  + " - And the Id is: " + outData.Item1);
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
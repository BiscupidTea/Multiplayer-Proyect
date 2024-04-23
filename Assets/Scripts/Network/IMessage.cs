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

public class NetMessageToClient : BaseMessage<(int, int, string)>
{
    public override (int, int, string) Deserialize(byte[] message)
    {
        (int, int, string) outData;

        outData.Item1 = BitConverter.ToInt32(message, 0);
        outData.Item2 = BitConverter.ToInt32(message, 4);

        outData.Item3 = "";
        int messageLenght = BitConverter.ToInt32(message, 8);

        for (int i = 0; i < messageLenght; i++)
        {
            outData.Item3 += (char)message[8 + i];
        }

        return outData;
    }

    public override (int, int, string) GetData()
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

        outData.AddRange(BitConverter.GetBytes(data.Item2));
        outData.AddRange(BitConverter.GetBytes(data.Item3.Length));

        for (int i = 0; i < data.Item3.Length; i++)
        {
            outData.Add((byte)data.Item3[i]);
        }


        return outData.ToArray();
    }
}

public class NetMessageToServer : BaseMessage<string>
{
    public override string Deserialize(byte[] message)
    {
        string outData;

        outData = "";
        int messageLenght = BitConverter.ToInt32(message, 4);

        for (int i = 0; i < messageLenght; i++)
        {
            outData += (char)message[4 + i];
        }

        return outData;
    }

    public override string GetData()
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

        outData.AddRange(BitConverter.GetBytes(data.Length));

        for (int i = 0; i < data.Length; i++)
        {
            outData.Add((byte)data[i]);
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

public class NetCode : BaseMessage<string>
{
    public NetCode(string consoleMessage)
    {
        data = consoleMessage;
    }

    public override string Deserialize(byte[] message)
    {
        string outData = "";
        int messageLenght = BitConverter.ToInt32(message, 4);

        for (int i = 0; i < messageLenght; i++)
        {
            outData += (char)message[8 + i];
        }

        return outData;
    }

    public override string GetData()
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
        outData.AddRange(BitConverter.GetBytes(data.Length));

        for (int i = 0; i < data.Length; i++)
        {
            outData.Add((byte)data[i]);
        }

        return outData.ToArray();
    }
}
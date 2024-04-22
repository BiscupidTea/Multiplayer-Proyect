using System.Collections.Generic;
using UnityEngine;
using System;

public enum MessageType
{
    HandShake = -1,
    Console = 0,
    Position = 1
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

public class NetHandShake : BaseMessage<int>
{
    public override int Deserialize(byte[] message)
    {
        return BitConverter.ToInt32(message, 4);
    }

    public override int GetData()
    {
        return data;
    }

    public override MessageType GetMessageType()
    {
        return MessageType.HandShake;
    }

    public override byte[] Serialize()
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));

        outData.AddRange(BitConverter.GetBytes(data));


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
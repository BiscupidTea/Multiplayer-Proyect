using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

// 0 - 3 = Message Type
// 4 = Message
// FinalMessage = CheckSum

public enum MessageType
{
    MessageToServer = 0,
    MessageToClient,
    MessageToClientDenied,
    Console,
    Position,
    PingPong,
}

public enum Operation
{
    Add,
    Substract,
    ShiftLeft,
    ShiftRight
}

public class CheckSumReeder
{
    public virtual (uint, uint) ReadCheckSum(List<byte> message)
    {
        (uint, uint) checkSum;
        checkSum.Item1 = 0;
        checkSum.Item2 = 0;

        int messageLenght = message.Count - sizeof(uint) * 2;

        for (int i = 0; i < messageLenght; i++)
        {
            int operationType = message[i] % 4;

            switch (operationType)
            {
                case (int)Operation.Add:

                    checkSum.Item1 += message[i];
                    checkSum.Item2 += message[i];

                    break;

                case (int)Operation.Substract:

                    checkSum.Item1 -= message[i];
                    checkSum.Item2 -= message[i];

                    break;

                case (int)Operation.ShiftRight:

                    checkSum.Item1 >>= message[i];
                    checkSum.Item2 >>= message[i];

                    break;

                case (int)Operation.ShiftLeft:

                    checkSum.Item1 <<= message[i];
                    checkSum.Item2 <<= message[i];

                    break;
            }
        }

        return (checkSum.Item1, checkSum.Item2);
    }

    public virtual bool CheckSumStatus(byte[] message)
    {
        (uint, uint) operation = ReadCheckSum(message.ToList<byte>());

        int checksumStartIndex1 = message.Length - sizeof(uint) * 2;
        int checksumStartIndex2 = message.Length - sizeof(uint);

        if (operation.Item1 == BitConverter.ToUInt32(message, checksumStartIndex1) &&
            operation.Item2 == BitConverter.ToUInt32(message, checksumStartIndex2))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}

public abstract class BaseMessage<PayLoadType>
{
    public static int startPosition = 4;

    public virtual void InsertCheckSum(List<byte> message)
    {
        (uint, uint) checkSum;
        checkSum.Item1 = 0;
        checkSum.Item2 = 0;

        int messageLenght = message.Count;

        for (int i = 0; i < messageLenght; i++)
        {
            int operationType = message[i] % 4;

            switch (operationType)
            {
                case (int)Operation.Add:

                    checkSum.Item1 += message[i];
                    checkSum.Item2 += message[i];

                    break;

                case (int)Operation.Substract:

                    checkSum.Item1 -= message[i];
                    checkSum.Item2 -= message[i];

                    break;

                case (int)Operation.ShiftRight:

                    checkSum.Item1 >>= message[i];
                    checkSum.Item2 >>= message[i];

                    break;

                case (int)Operation.ShiftLeft:

                    checkSum.Item1 <<= message[i];
                    checkSum.Item2 <<= message[i];

                    break;
            }
        }
        message.AddRange(BitConverter.GetBytes(checkSum.Item1));
        message.AddRange(BitConverter.GetBytes(checkSum.Item2));
    }

    public PayLoadType data;
    public Action<PayLoadType> OnDeserialize;
    public abstract MessageType GetMessageType();
    public abstract byte[] Serialize();
    public abstract PayLoadType Deserialize(byte[] message);
    public abstract PayLoadType GetData();
}

public abstract class OrderableMessage<PayloadType> : BaseMessage<PayloadType>
{
    public uint GetId(byte[] message)
    {
        MsgID = BitConverter.ToUInt32(message, 4);

        return MsgID;
    }

    public void SetId(List<byte> message)
    {
        message.AddRange(BitConverter.GetBytes(MsgID));
        MsgID++;
    }

    protected static uint lastMsgID = 0;

    protected uint MsgID = 0;
    protected static Dictionary<PayloadType, ulong> lastExecutedMsgID = new Dictionary<PayloadType, ulong>();
}

public class NetMessageToClient : BaseMessage<List<Players>>
{
    public override List<Players> Deserialize(byte[] message)
    {
        int currentPosition = startPosition;

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

        InsertCheckSum(outData);

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

        outData.Item1 = BitConverter.ToInt32(message, startPosition); //ID

        outData.Item2 = "";//ClientID
        int messageLenght = BitConverter.ToInt32(message, startPosition + 4);

        for (int i = 0; i < messageLenght; i++)
        {
            outData.Item2 += (char)message[startPosition + 8 + i];
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

        InsertCheckSum(outData);

        return outData.ToArray();
    }
}

public class NetVector3 : BaseMessage<Vector3>
{
    public NetVector3(Vector3 data)
    {
        this.data = data;
    }

    public override Vector3 Deserialize(byte[] message)
    {
        Vector3 outData;

        outData.x = BitConverter.ToSingle(message, startPosition);
        outData.y = BitConverter.ToSingle(message, startPosition + 4);
        outData.z = BitConverter.ToSingle(message, startPosition + 8);

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
        //outData.AddRange();
        outData.AddRange(BitConverter.GetBytes(data.x));
        outData.AddRange(BitConverter.GetBytes(data.y));
        outData.AddRange(BitConverter.GetBytes(data.z));

        InsertCheckSum(outData);

        return outData.ToArray();
    }
}

public class NetCode : BaseMessage<(int, string)>
{
    public override (int, string) Deserialize(byte[] message)
    {
        (int, string) outData;

        //PlayerID
        outData.Item1 = BitConverter.ToInt32(message, startPosition);

        //message
        outData.Item2 = "";
        int messageLenght = BitConverter.ToInt32(message, startPosition + 4);

        for (int i = 0; i < messageLenght; i++)
        {
            outData.Item2 += (char)message[startPosition + 8 + i];
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

        InsertCheckSum(outData);
        return outData.ToArray();
    }
}

public class PingPong : BaseMessage<int>
{
    public override int Deserialize(byte[] message)
    {
        int outData;

        outData = BitConverter.ToInt32(message, startPosition);

        return outData;
    }

    public override int GetData()
    {
        return data;
    }

    public override MessageType GetMessageType()
    {
        return MessageType.PingPong;
    }

    public override byte[] Serialize()
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
        outData.AddRange(BitConverter.GetBytes(data));
        InsertCheckSum((outData));

        return outData.ToArray();
    }
}
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

// 0 = Message Type
// 4 = flag
// 8 = Ordenable Message
// 12 = Message
// FinalMessage = CheckSum

[Flags]
public enum MessageFlags
{
    none = 0,
    checksum = 1,
    ordenable = 2,
    important = 4,
}

public enum MessageType
{
    StartHandShake = 0,
    ContinueHandShake,
    Disconnect,
    MessageError,
    PingPong,
    String,
    Vector3,
    Quaternion,
    Time,
}

public enum Operation
{
    Add,
    Substract,
    ShiftLeft,
    ShiftRight
}

public enum ErrorMessageType
{
    invalidUserName,
    ServerFull,
    GameStarted,
}

public enum ServerActionMade
{
    StartGame,
    EndGame,
    close,
}

public enum PlayerActionMade
{
    Shoot,
    hit,
    Death,
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
    public static int flagTypeMessage = 4;
    public static int ordenablePosition = 8;
    public static int messagePosition = 12;

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
    public MessageType type;
    public MessageFlags flags;

    public Action<PayLoadType> OnDeserialize;
    public abstract byte[] Serialize();
    public abstract PayLoadType Deserialize(byte[] message);
    public abstract PayLoadType GetData();
    public abstract MessageFlags GetMessageFlag();
    public abstract MessageType GetMessageType();
}

public abstract class OrderableMessage<PayloadType> : BaseMessage<PayloadType>
{
    public uint GetId(byte[] message)
    {
        MsgID = BitConverter.ToUInt32(message, ordenablePosition);

        return MsgID;
    }

    public void SetId(List<byte> message)
    {
        message.AddRange(BitConverter.GetBytes(MsgID));
        MsgID++;
    }

    protected static uint lastMsgID = 0;

    protected uint MsgID = 0;
    protected uint LastExecutedID = 0;
}

public class NetContinueHandShake : OrderableMessage<List<Player>>
{
    public NetContinueHandShake()
    {
        type = MessageType.ContinueHandShake;
        flags = MessageFlags.important;
    }

    public override List<Player> Deserialize(byte[] message)
    {
        int currentPosition = messagePosition;

        int totalPlayers = BitConverter.ToInt32(message, currentPosition);
        Debug.Log("total player: " + totalPlayers);

        currentPosition += 4;

        List<Player> newPlayerList = new List<Player>();


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
            newPlayerList.Add(new Player(clientId, Id));
        }

        return newPlayerList;
    }

    public override List<Player> GetData()
    {
        return data;
    }

    public override MessageFlags GetMessageFlag()
    {
        return flags;
    }

    public override MessageType GetMessageType()
    {
        return type;
    }

    public override byte[] Serialize()
    {
        List<byte> outData = new List<byte>();
        //message type
        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));

        //set flag
        outData.AddRange(BitConverter.GetBytes((int)GetMessageFlag()));

        //set id
        SetId(outData);

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

public class NetHandShake : OrderableMessage<string>
{
    public NetHandShake(MessageType messageType)
    {
        type = messageType;
        flags = MessageFlags.important;
    }

    public override string Deserialize(byte[] message)
    {
        string outData;

        outData = "";
        int messageLenght = BitConverter.ToInt32(message, messagePosition + 4);

        for (int i = 0; i < messageLenght; i++)
        {
            outData += (char)message[messagePosition + 8 + i];
        }
        return outData;
    }

    public override string GetData()
    {
        return data;
    }

    public override MessageFlags GetMessageFlag()
    {
        return flags;
    }

    public override MessageType GetMessageType()
    {
        return type;
    }

    public override byte[] Serialize()
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));

        //set flag
        outData.AddRange(BitConverter.GetBytes((int)GetMessageFlag()));

        //set id
        SetId(outData);

        outData.AddRange(BitConverter.GetBytes(data.Length)); //ClientID

        for (int i = 0; i < data.Length; i++)
        {
            outData.Add((byte)data[i]);
        }

        InsertCheckSum(outData);

        return outData.ToArray();
    }
}

public class NetVector3 : OrderableMessage<(Vector3, int)>
{
    public NetVector3()
    {
        type = MessageType.Vector3;
        flags = MessageFlags.important;
    }
    public override (Vector3, int) Deserialize(byte[] message)
    {
        (Vector3, int) outData;

        outData.Item2 = BitConverter.ToInt32(message, messagePosition);
        outData.Item1.x = BitConverter.ToSingle(message, messagePosition + 4);
        outData.Item1.y = BitConverter.ToSingle(message, messagePosition + 8);
        outData.Item1.z = BitConverter.ToSingle(message, messagePosition + 12);

        return outData;
    }

    public override (Vector3, int) GetData()
    {
        return data;
    }

    public override MessageFlags GetMessageFlag()
    {
        return flags;
    }

    public override MessageType GetMessageType()
    {
        return type;
    }

    public override byte[] Serialize()
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));

        //set flag
        outData.AddRange(BitConverter.GetBytes((int)GetMessageFlag()));

        SetId(outData);

        outData.AddRange(BitConverter.GetBytes(data.Item2));
        outData.AddRange(BitConverter.GetBytes(data.Item1.x));
        outData.AddRange(BitConverter.GetBytes(data.Item1.y));
        outData.AddRange(BitConverter.GetBytes(data.Item1.z));

        InsertCheckSum(outData);

        return outData.ToArray();
    }
}

public class NetQuaternion : OrderableMessage<(Quaternion, int)>
{
    public NetQuaternion()
    {
        type = MessageType.Quaternion;
        flags = MessageFlags.important;
    }
    public override (Quaternion, int) Deserialize(byte[] message)
    {
        (Quaternion, int) outData;

        outData.Item2 = BitConverter.ToInt32(message, messagePosition);
        outData.Item1.x = BitConverter.ToSingle(message, messagePosition + 4);
        outData.Item1.y = BitConverter.ToSingle(message, messagePosition + 8);
        outData.Item1.z = BitConverter.ToSingle(message, messagePosition + 12);
        outData.Item1.w = BitConverter.ToSingle(message, messagePosition + 16);

        return outData;
    }

    public override (Quaternion, int) GetData()
    {
        return data;
    }

    public override MessageFlags GetMessageFlag()
    {
        return flags;
    }

    public override MessageType GetMessageType()
    {
        return type;
    }

    public override byte[] Serialize()
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));

        //set flag
        outData.AddRange(BitConverter.GetBytes((int)GetMessageFlag()));

        SetId(outData);

        outData.AddRange(BitConverter.GetBytes(data.Item2));
        outData.AddRange(BitConverter.GetBytes(data.Item1.x));
        outData.AddRange(BitConverter.GetBytes(data.Item1.y));
        outData.AddRange(BitConverter.GetBytes(data.Item1.z));
        outData.AddRange(BitConverter.GetBytes(data.Item1.w));

        InsertCheckSum(outData);

        return outData.ToArray();
    }
}

public class NetString : OrderableMessage<string>
{
    public NetString()
    {
        type = MessageType.String;
        flags = MessageFlags.important;
    }
    public override string Deserialize(byte[] message)
    {
        string outData;

        //message
        outData = "";
        int messageLenght = BitConverter.ToInt32(message, messagePosition);

        for (int i = 0; i < messageLenght; i++)
        {
            outData += (char)message[messagePosition + 4 + i];
        }

        return outData;
    }

    public override string GetData()
    {
        return data;
    }

    public override MessageFlags GetMessageFlag()
    {
        return flags;
    }

    public override MessageType GetMessageType()
    {
        return type;
    }

    public override byte[] Serialize()
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));

        //set flag
        outData.AddRange(BitConverter.GetBytes((int)GetMessageFlag()));

        //set id
        SetId(outData);

        outData.AddRange(BitConverter.GetBytes(data.Length));

        for (int i = 0; i < data.Length; i++)
        {
            outData.Add((byte)data[i]);
        }

        InsertCheckSum(outData);
        return outData.ToArray();
    }
}

public class PingPong : OrderableMessage<int>
{
    public PingPong() 
    {
        type = MessageType.PingPong;
        flags = MessageFlags.none;
    }

    public override int Deserialize(byte[] message)
    {
        int outData;

        outData = BitConverter.ToInt32(message, messagePosition);

        return outData;
    }

    public override int GetData()
    {
        return data;
    }

    public override MessageFlags GetMessageFlag()
    {
        return flags;
    }

    public override MessageType GetMessageType()
    {
        return type;
    }

    public override byte[] Serialize()
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));

        //set flag
        outData.AddRange(BitConverter.GetBytes((int)GetMessageFlag()));

        SetId(outData); 
        
        outData.AddRange(BitConverter.GetBytes(data));
        InsertCheckSum((outData));

        return outData.ToArray();
    }
}

public class NetTimer : OrderableMessage<float>
{
    NetTimer()
    {
        type = MessageType.Time;
        flags = MessageFlags.ordenable;
    }

    public override float Deserialize(byte[] message)
    {
        float outData;

        outData = BitConverter.ToInt32(message, messagePosition);

        return outData;
    }

    public override float GetData()
    {
        return data;
    }

    public override MessageFlags GetMessageFlag()
    {
        return flags;
    }

    public override MessageType GetMessageType()
    {
        return type;
    }

    public override byte[] Serialize()
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));

        //set flag
        outData.AddRange(BitConverter.GetBytes((int)GetMessageFlag()));

        SetId(outData);

        outData.AddRange(BitConverter.GetBytes((int)data));

        InsertCheckSum(outData);

        return outData.ToArray();
    }
}

//public class NetServerActionMade : OrderableMessage<(ServerActionMade, int)>
//{
//    public override (ServerActionMade, int) Deserialize(byte[] message)
//    {
//        (ServerActionMade, int) outData;

//        outData.Item1 = (ServerActionMade)BitConverter.ToInt32(message, messagePosition);
//        outData.Item2 = BitConverter.ToInt32(message, messagePosition + 4);

//        return outData;
//    }

//    public override (ServerActionMade, int) GetData()
//    {
//        return data;
//    }

//    public override MessageType GetMessageType()
//    {
//        return type;
//    }

//    public override byte[] Serialize()
//    {
//        List<byte> outData = new List<byte>();

//        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));

//        SetId(outData); //set id

//        outData.AddRange(BitConverter.GetBytes((int)data.Item1));
//        outData.AddRange(BitConverter.GetBytes(data.Item2));
//        InsertCheckSum(outData);

//        return outData.ToArray();
//    }
//}

//public class NetPlayerActionMade : OrderableMessage<(PlayerActionMade, int, Vector3)>
//{
//    public override (PlayerActionMade, int, Vector3) Deserialize(byte[] message)
//    {
//        (PlayerActionMade, int, Vector3) outData;

//        outData.Item1 = (PlayerActionMade)BitConverter.ToInt32(message, messagePosition);
//        outData.Item2 = BitConverter.ToInt32(message, messagePosition + 4);
//        outData.Item3.x = BitConverter.ToSingle(message, messagePosition + 8);
//        outData.Item3.y = BitConverter.ToSingle(message, messagePosition + 12);
//        outData.Item3.z = BitConverter.ToSingle(message, messagePosition + 16);

//        return outData;
//    }

//    public override (PlayerActionMade, int, Vector3) GetData()
//    {
//        return data;
//    }

//    public override MessageType GetMessageType()
//    {
//        return type;
//    }

//    public override byte[] Serialize()
//    {
//        List<byte> outData = new List<byte>();

//        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));

//        SetId(outData); //set id

//        outData.AddRange(BitConverter.GetBytes((int)data.Item1));
//        outData.AddRange(BitConverter.GetBytes(data.Item2));
//        outData.AddRange(BitConverter.GetBytes(data.Item3.x));
//        outData.AddRange(BitConverter.GetBytes(data.Item3.y));
//        outData.AddRange(BitConverter.GetBytes(data.Item3.z));

//        InsertCheckSum(outData);

//        return outData.ToArray();
//    }
//}

public class ErrorMessage : OrderableMessage<ErrorMessageType>
{
    public ErrorMessage()
    {
        type = MessageType.MessageError;
    }

    public override ErrorMessageType Deserialize(byte[] message)
    {
        int outData;

        outData = BitConverter.ToInt32(message, messagePosition);

        return (ErrorMessageType)outData;
    }

    public override ErrorMessageType GetData()
    {
        return data;
    }

    public override MessageFlags GetMessageFlag()
    {
        return flags;
    }

    public override MessageType GetMessageType()
    {
        return type;
    }

    public override byte[] Serialize()
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));

        //set flag
        outData.AddRange(BitConverter.GetBytes((int)GetMessageFlag()));

        //set id
        SetId(outData);

        outData.AddRange(BitConverter.GetBytes((int)data));
        InsertCheckSum((outData));

        return outData.ToArray();
    }
}
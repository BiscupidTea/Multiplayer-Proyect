using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using BT_NetworkSystem;
using UnityEditor;
using UnityEngine.Events;
using MessageType = BT_NetworkSystem.MessageType;

public class ClientNetManager : NetworkManager
{
    public bool isConnected;

    private List<CacheMessage> messagesToSend = new();

    private DateTime currentTimePing = DateTime.UtcNow;

    public Dictionary<MessageType, uint> LastMessage = new Dictionary<MessageType, uint>();

    public Dictionary<MessageType, List<CacheMessage>> pendingMessages =
        new Dictionary<MessageType, List<CacheMessage>>();

    public UnityEvent<FactoryData> OnInstanceObject;
    
    public ClientNetManager() : base()
    {
    }

    public void StartClient()
    {
        OnStart();
    }

    protected override void OnStart()
    {
        base.OnStart();

        isConnected = false;
    }

    public void Connect()
    {
        Debug.Log(ipAddress + " , " + port + " , " + myPlayer.clientId);
        connection = new UdpConnection(ipAddress, port, myPlayer.clientId, this);

        isConnected = true;
    }

    protected override void Disconnect()
    {
        if (isConnected)
        {
            base.Disconnect();

            isConnected = false;

            connection.Close();
            Debug.Log("disconnect");
            //EditorApplication.isPlaying = false;
        }
    }

    public override void OnReceiveDataEvent(byte[] data, IPEndPoint ip)
    {
        Debug.Log("Something Recieved");

        if (ip == null)
        {
            return;
        }

        CheckSumReeder checkSumReeder = new CheckSumReeder();
        int currentFlags = BitConverter.ToInt32(data, 4);

        MessageType messageType = (MessageType)BitConverter.ToInt32(data, 0);
        MessageFlags flags = (MessageFlags)currentFlags;

        bool haveCheckSum = flags.HasFlag(MessageFlags.checksum);
        bool isOrdenable = flags.HasFlag(MessageFlags.ordenable);
        bool isImportant = flags.HasFlag(MessageFlags.important);

        uint ordenableNumber = BitConverter.ToUInt32(data, 8);

        Debug.Log("Message recieved - " + messageType);

        if (haveCheckSum && checkSumReeder.CheckSumStatus(data))
        {
            if (isOrdenable && isImportant)
            {
                if (!LastMessage.ContainsKey(messageType))
                {
                    LastMessage.Add(messageType, ordenableNumber);
                    //Debug.Log(messageType +  " New " + ordenableNumber);
                }
                else
                {
                    
                    if (ordenableNumber == LastMessage[messageType] + 1)
                    {
                        LastMessage[messageType] = ordenableNumber;
                        //Debug.Log(ordenableNumber + " == " + LastMessage[messageType] + 1);

                    }
                    else
                    {
                        //Debug.Log(ordenableNumber + " != " + LastMessage[messageType] + 1);

                        if (!pendingMessages.ContainsKey(messageType))
                        {
                            pendingMessages.Add(messageType, new List<CacheMessage>());
                        }
                        pendingMessages[messageType].Add(new CacheMessage(data, ordenableNumber, messageType));

                        return;
                    }
                }
            }
            else if (isOrdenable)
            {
                if (!LastMessage.ContainsKey(messageType))
                {
                    LastMessage.Add(messageType, ordenableNumber);
                }
                else
                {
                    if (ordenableNumber > LastMessage[messageType])
                    {
                        LastMessage[messageType] = ordenableNumber;
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }
        else
        {
            Debug.Log("Message Corrupted");
            return;
        }

        ExecuteMessage(data, ip, messageType);

        CheckPendingMessage(data, ip, messageType, ordenableNumber);
    }

    private void CheckPendingMessage(byte[] data, IPEndPoint ip, MessageType messageType, uint ordenableNumber)
    {
        if (pendingMessages.Count > 0)
        {
            foreach (CacheMessage message in pendingMessages[messageType])
            {
                if (message.id == ordenableNumber + 1)
                {
                    ExecuteMessage(data, ip, messageType);
                    LastMessage[messageType] = message.id;

                    CheckPendingMessage(data, ip, messageType, LastMessage[messageType]);
                    break;
                }
            }
        }
    }

    private void ExecuteMessage(byte[] data, IPEndPoint ip, MessageType messageType)
    {
        switch (messageType)
        {
            case MessageType.ConfirmImportantMessage:
                MessageConfirmation(data);
                break;

            case MessageType.ContinueHandShake:
                RefreshPlayerList(data);
                break;

            case MessageType.MessageError:
                CheckErrorType(data);
                break;

            case MessageType.String:
                MessageRecieved(data);
                break;

            case MessageType.Vector3:
                MovePlayer(data);
                break;

            case MessageType.Quaternion:
                RotatePlayer(data);
                break;
            
            case MessageType.FactoryRequest:
                InstanceObject(data);
                break;

            case MessageType.PingPong:
                Debug.Log("send pingpong");
                CheckPingPong(data, ip);
                break;

            case MessageType.Time:
                break;

            default:
                Debug.Log("Message type Not Found");
                break;
        }
    }

    public void InstanceObject(byte[] data)
    {
        FactoryMessage factoryMessage = new FactoryMessage();
        FactoryData newData = factoryMessage.Deserialize(data);
        
        OnInstanceObject.Invoke(newData);
    }
    
    public override void MessageConfirmation(byte[] data)
    {
        ConfirmationMessage confirmationMessage = new ConfirmationMessage();
        confirmationMessage.data = confirmationMessage.Deserialize(data);

        foreach (var m in messagesToSend)
        {
            if (m.type == confirmationMessage.data && m.id == confirmationMessage.GetId(data) && !m.Received)
            {
                Debug.Log("Message confirmed");
                m.Received = true;
            }
        }
    }

    private void RotatePlayer(byte[] data)
    {
        throw new NotImplementedException();
    }

    private void MovePlayer(byte[] data)
    {
        throw new NotImplementedException();
    }

    private void MessageRecieved(byte[] data)
    {
        throw new NotImplementedException();
    }

    private ErrorMessageType CheckErrorType(byte[] data)
    {
        ErrorMessage errorMessage = new ErrorMessage();
        return errorMessage.Deserialize(data);
    }

    private void RefreshPlayerList(byte[] data)
    {
        NetContinueHandShake newPlayers = new NetContinueHandShake();
        players = newPlayers.Deserialize(data);
        foreach (var p in newPlayers.Deserialize(data))
        {
            if (p.clientId == myPlayer.clientId)
            {
                myPlayer.clientId = p.clientId;
                myPlayer.id = p.id;
            }
        }
    }

    public override void CheckTimeOut()
    {
        if ((DateTime.UtcNow - currentTimePing).Seconds > TimeOut)
        {
            Disconnect();
            //Debug.Log("Disconnected from server = Time out: " + (DateTime.UtcNow - currentTimePing).Seconds);
            // Debug.Log("UtcNow =" + DateTime.UtcNow);
            // Debug.Log("currentTimePing =" + currentTimePing);
        }
    }

    public override void OnUpdateMessages()
    {
        if (messagesToSend.Count > 0)
        {
            foreach (CacheMessage message in messagesToSend)
            {
                if ((DateTime.UtcNow - message.lastEmission).Seconds > ImportantMessageTimeOut && !message.Received)
                {
                    Debug.Log("Send important message, type =" + message.type);
                    SendToServer(message.message);
                    message.lastEmission = DateTime.UtcNow;
                }
            }
        }
    }

    public override void CheckPingPong(byte[] data, IPEndPoint ip)
    {
        currentTimePing = DateTime.UtcNow;
        PingPong pingPong = new PingPong();
        SendMessageToServer(pingPong.Serialize(), MessageType.PingPong);
    }

    public void SendMessageToServer(byte[] data, MessageType messageType)
    {
        messagesToSend.Add(new CacheMessage(data, BitConverter.ToUInt32(data, 8), messageType));
        SendToServer(data);
    }

    private void SendToServer(byte[] data)
    {
        connection.Send(data);
    }
}
using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ClientNetManager : NetworkManager
{
    [SerializeField] private bool isConnected;

    private List<CacheMessage> messagesToSend = new();

    private DateTime currentTimePing;
    protected override void OnStart()
    {
        base.OnStart();

        isConnected = false;
        connection = new UdpConnection(ipAddress, port, myPlayer.clientId, this);
    }


    protected override void Disconnect()
    {
        base.Disconnect();

        if (isConnected)
        {
            NetHandShake netHandShakeExit = new NetHandShake(MessageType.Disconnect);
            netHandShakeExit.data = myPlayer.clientId;
            SendToServer(netHandShakeExit.Serialize());

            isConnected = false;

            connection.Close();

            //switch to network screen
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

        if (haveCheckSum && checkSumReeder.CheckSumStatus(data))
        {

            if (isOrdenable && isImportant)
            {

                if (!LastMessage.ContainsKey(messageType))
                {
                    LastMessage.Add(messageType, ordenableNumber);
                }
                else
                {
                    if (ordenableNumber == LastMessage[messageType] + 1)
                    {
                        LastMessage[messageType] = ordenableNumber;
                    }
                    else
                    {
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
            return;
        }

        Debug.Log("Message recieved - " + messageType);

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

            case MessageType.PingPong:
                CheckPingPong(data, ip);
                break;

            case MessageType.Time:
                break;

            default:
                Debug.Log("Message type Not Found");
                break;
        }
    }

    public override void MessageConfirmation(byte[] data)
    {
        ConfirmationMessage confirmationMessage = new ConfirmationMessage();
        confirmationMessage.data = confirmationMessage.Deserialize(data);

        foreach (var m in messagesToSend)
        {
            if (m.type == confirmationMessage.data && m.id == confirmationMessage.GetId(data) && !m.Received)
            {
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
            Debug.Log("disconnected from server = Time out: " + (DateTime.UtcNow - currentTimePing).Seconds);
        }
    }

    public override void CheckPingPong(byte[] data, IPEndPoint ip)
    {
        currentTimePing = DateTime.UtcNow;
        PingPong pingPong = new PingPong();
        SendToServer(pingPong.Serialize());
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

    public override void OnUpdate()
    {
        if (messagesToSend.Count > 0)
        {
            foreach (CacheMessage message in messagesToSend)
            {
                if ((DateTime.UtcNow - message.lastEmission).Seconds > ImportantMessageTimeOut)
                {
                    SendToServer(message.message);
                    message.lastEmission = DateTime.UtcNow;
                }
            }
        }
    }

}

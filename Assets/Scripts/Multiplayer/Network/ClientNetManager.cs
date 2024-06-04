using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ClientNetManager : NetworkManager
{
    public Player playerData;
    [SerializeField] private bool isConnected;

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
            netHandShakeExit.data = playerData.clientId;
            SendToServer(netHandShakeExit.Serialize());

            isConnected = false;

            connection.Close();

            //switch to network screen
        }
    }

    public override void OnReceiveDataEvent(byte[] data, IPEndPoint ip)
    {
        CheckSumReeder checkSumReeder = new CheckSumReeder();
        int currentFlags = BitConverter.ToInt32(data, 4);

        MessageType messageType = (MessageType)BitConverter.ToInt32(data, 0);
        MessageFlags flags = (MessageFlags)currentFlags;

        bool haveCheckSum = flags.HasFlag(MessageFlags.checksum);
        bool isOrdenable = flags.HasFlag(MessageFlags.ordenable);
        bool isImportant = flags.HasFlag(MessageFlags.important);

        if (haveCheckSum && checkSumReeder.CheckSumStatus(data))
        {
            if (isOrdenable)
            {
                if (ordenableMessages.ContainsKey(messageType))
                {

                }
                else
                {
                    ordenableMessages.Add(messageType, new OrderableMessage<>);
                }

                if (isImportant)
                {

                }
            }
        }
        else
        {
            return;
        }

        switch (messageType)
        {
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
    }

    public void SendToServer(byte[] data)
    {
        connection.Send(data);
    }

    public override void OnUpdate()
    {

    }

}

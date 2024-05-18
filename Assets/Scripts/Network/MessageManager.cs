using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;

public class MessageManager : MonoBehaviourSingleton<MessageManager>
{
    private CheckSumReeder checkSumReeder = new CheckSumReeder();
    private NetCode netCode = new NetCode();
    private PingPong pingPong = new PingPong();
    private NetMessageToServer netMessageToServer = new NetMessageToServer();
    private NetMessageToClient netMessageToClient = new NetMessageToClient();
    bool PrivateMessage = false;

    public void OnRecieveMessage(byte[] data, IPEndPoint Ip)
    {
        MessageType typeMessage = (MessageType)BitConverter.ToInt32(data, 0);

        PrivateMessage = false;
        if (checkSumReeder.CheckSumStatus(data))
        {
            switch (typeMessage)
            {
                case MessageType.MessageToServer:

                    Players newPlayer = new Players(netMessageToServer.Deserialize(data).Item2, netMessageToServer.Deserialize(data).Item1);

                    newPlayer.id = NetworkManager.Instance.clientId;
                    newPlayer.clientId = netMessageToServer.Deserialize(data).Item2;

                    if (CheckAlreadyUseName(newPlayer.clientId))
                    {
                        List<byte> outData = new List<byte>();

                        outData.AddRange(BitConverter.GetBytes((int)MessageType.MessageToClientDenied));

                        NetworkManager.Instance.SendToClient(outData.ToArray(), Ip);

                        Debug.Log("the Name " + newPlayer.clientId + " is aleredy in use");
                        PrivateMessage = true;
                    }
                    else
                    {
                        NetworkManager.Instance.AddClient(Ip);
                        NetworkManager.Instance.addPlayer(newPlayer);

                        netMessageToClient.data = NetworkManager.Instance.players;

                        data = netMessageToClient.Serialize();

                        NetworkManager.Instance.clientId++;
                        Debug.Log("add new client = Client Id: " + netMessageToClient.data[netMessageToClient.data.Count - 1].clientId + " - Id: " + netMessageToClient.data[netMessageToClient.data.Count - 1].id);
                        PrivateMessage = false;
                    }
                    break;

                case MessageType.MessageToClient:
                    Debug.Log("message to client");

                    NetworkManager.Instance.players = netMessageToClient.Deserialize(data);
                    for (int i = 0; i < NetworkManager.Instance.players.Count; i++)
                    {
                        if (NetworkManager.Instance.players[i].clientId == NetworkManager.Instance.playerData.clientId)
                        {
                            NetworkManager.Instance.playerData.id = NetworkManager.Instance.players[i].id;
                            LoadingScreen.Instance.connectToChat();
                            break;
                        }
                    }

                    StartPingPong();

                    PrivateMessage = false;
                    break;

                case MessageType.MessageToClientDenied:
                    LoadingScreen.Instance.ShowBackToMenu();
                    Debug.Log("the Name is aleredy in use");
                    break;

                case MessageType.Console:
                    string playerName = "";
                    for (int i = 0; i < NetworkManager.Instance.players.Count; i++)
                    {
                        if (NetworkManager.Instance.players[i].id == netCode.Deserialize(data).Item1)
                        {
                            playerName = NetworkManager.Instance.players[i].clientId;
                            break;
                        }
                    }

                    ChatScreen.Instance.OnReceiveDataEvent(playerName + " : " + netCode.Deserialize(data).Item2);
                    PrivateMessage = false;
                    break;

                case MessageType.Position:

                    break;

                case MessageType.PingPong:
                    if (!NetworkManager.Instance.isServer)
                    {
                        NetworkManager.Instance.pingText.text = "Ping = " + (DateTime.UtcNow - NetworkManager.Instance.lastMessageSended).Milliseconds;
                    }
                    SendPingPong(Ip, data);
                    PrivateMessage = true;
                    break;

                default:
                    Debug.LogError("Message type not found");
                    break;
            }
        }
        else
        {
            Debug.Log("Message Corrupt");
        }

        if (NetworkManager.Instance.isServer && !PrivateMessage)
        {
            NetworkManager.Instance.Broadcast(data);
        }
    }

    private void SendPingPong(IPEndPoint Ip, byte[] data)
    {
        if (NetworkManager.Instance.isServer)
        {
            NetworkManager.Instance.ResetClientTimer(pingPong.Deserialize(data));
            NetworkManager.Instance.SendToClient(pingPong.Serialize(), Ip);
        }
        else
        {
            NetworkManager.Instance.lastMessageSended = DateTime.UtcNow;
            NetworkManager.Instance.lastMessageRecieved = DateTime.UtcNow;

            pingPong.data = NetworkManager.Instance.playerData.id;

            NetworkManager.Instance.SendToServer(pingPong.Serialize());
        }
    }

    private void OnSendMessage(byte[] message)
    {
        if (NetworkManager.Instance.isServer)
        {
            NetworkManager.Instance.Broadcast(message);
        }
        else
        {
            NetworkManager.Instance.SendToServer(message);
        }
    }

    public void OnSendConsoleMessage(string message)
    {
        netCode.data.Item1 = NetworkManager.Instance.playerData.id;
        netCode.data.Item2 = message;

        if (NetworkManager.Instance.isServer)
        {
            ChatScreen.Instance.OnReceiveDataEvent(message);
        }
        OnSendMessage(netCode.Serialize());
    }

    public void OnSendHandshake(string name, int id)
    {
        netMessageToServer.data.Item1 = id; //not assigned id
        netMessageToServer.data.Item2 = name; //client id
        NetworkManager.Instance.SendToServer(netMessageToServer.Serialize());
    }

    public void StartPingPong()
    {
        Debug.Log("StartPingPong");

        pingPong.data = NetworkManager.Instance.playerData.id;
        NetworkManager.Instance.lastMessageRecieved = DateTime.UtcNow;
        NetworkManager.Instance.initialized = true;
        NetworkManager.Instance.SendToServer(pingPong.Serialize());
    }

    private bool CheckAlreadyUseName(string newPlayerName)
    {
        for (int i = 0; i < NetworkManager.Instance.players.Count; i++)
        {
            if (NetworkManager.Instance.players.ToArray()[i].clientId == newPlayerName)
            {
                return true;
            }
        }

        return false;
    }
}

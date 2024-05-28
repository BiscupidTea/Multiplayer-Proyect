using System;
using System.Net;
using UnityEditor;
using UnityEngine;

public class MessageManager : MonoBehaviourSingleton<MessageManager>
{
    private CheckSumReeder checkSumReeder = new CheckSumReeder();
    private NetString netCode = new NetString();
    private PingPong pingPong = new PingPong();
    private NetTimer netTimer = new NetTimer();
    private NetVector3 netVector3 = new NetVector3();
    private NetQuaternion netQuaternion = new NetQuaternion();
    private ErrorMessage errorMessage = new ErrorMessage();
    private NetServerActionMade neServertActionMade = new NetServerActionMade();
    private NetPlayerActionMade netPlayerActionMade = new NetPlayerActionMade();
    private NetHandShake netMessageToServer = new NetHandShake();
    private NetContinueHandShake netMessageToClient = new NetContinueHandShake();
    bool PrivateMessage = false;

    public void OnRecieveMessage(byte[] data, IPEndPoint Ip)
    {
        MessageType typeMessage = (MessageType)BitConverter.ToInt32(data, 0);

        PrivateMessage = false;
        if (checkSumReeder.CheckSumStatus(data))
        {
            switch (typeMessage)
            {
                case MessageType.MessageToClient:
                    Debug.Log("message to client");

                    NetworkManager.Instance.players = netMessageToClient.Deserialize(data);
                    for (int i = 0; i < NetworkManager.Instance.players.Count; i++)
                    {
                        if (NetworkManager.Instance.players[i].clientId == NetworkManager.Instance.playerData.clientId)
                        {
                            NetworkManager.Instance.playerData.id = NetworkManager.Instance.players[i].id;
                            break;
                        }
                    }

                    if (!NetworkManager.Instance.initialized)
                    {
                        StartPingPong();
                        CanvasSwitcher.Instance.SwitchCanvas(modifyCanvas.lobby);
                        NetworkManager.Instance.initialized = true;
                    }

                    Lobby.Instance.UpdateLobby();

                    PrivateMessage = false;
                    break;

                case MessageType.MessageError:
                    Debug.Log("Error message recived");

                    switch (errorMessage.Deserialize(data))
                    {
                        case ErrorMessageType.invalidUserName:
                            LoadingScreen.Instance.ShowErrorMessage("Username Already use");
                            break;

                        case ErrorMessageType.ServerFull:
                            LoadingScreen.Instance.ShowErrorMessage("Server full");
                            break;

                        case ErrorMessageType.GameStarted:
                            LoadingScreen.Instance.ShowErrorMessage("Game alredy Started");
                            break;

                        default:
                            break;
                    }
                    break;

                case MessageType.String:
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

                case MessageType.Time:
                    if (GameManager.Instance.playing)
                    {
                        GameManager.Instance.SetTime(netTimer.Deserialize(data));
                    }
                    else
                    {
                        Lobby.Instance.SetTime(netTimer.Deserialize(data));
                    }
                    PrivateMessage = false;
                    break;

                case MessageType.Position:
                    netVector3.data = netVector3.Deserialize(data);
                    GameManager.Instance.MovePlayer(netVector3.data.Item1, netVector3.data.Item2);

                    PrivateMessage = false;
                    break;

                case MessageType.Rotation:
                    netQuaternion.data = netQuaternion.Deserialize(data);
                    GameManager.Instance.RotatePlayer(netQuaternion.data.Item1, netVector3.data.Item2);

                    PrivateMessage = false;
                    break;

                case MessageType.PingPong:
                    if (!NetworkManager.Instance.isServer)
                    {
                        NetworkManager.Instance.pingText.text = "Ping = " + (DateTime.UtcNow - NetworkManager.Instance.lastMessageSended).Milliseconds;
                    }
                    SendPingPong(Ip, data);
                    PrivateMessage = true;
                    break;

                case MessageType.ServerAction:
                    switch (neServertActionMade.Deserialize(data).Item1)
                    {
                        case ServerActionMade.StartGame:
                            GameManager.Instance.StartGame();
                            break;
                        case ServerActionMade.EndGame:
                            GameManager.Instance.ShowWin(neServertActionMade.Deserialize(data).Item2);
                            break;
                        case ServerActionMade.close:

                            break;
                    }
                    break;

                case MessageType.PlayerAction:
                    switch (netPlayerActionMade.Deserialize(data).Item1)
                    {
                        case PlayerActionMade.Shoot:
                            Debug.Log("player shoot");
                            GameManager.Instance.ShootPlayer(netPlayerActionMade.Deserialize(data).Item3, netPlayerActionMade.Deserialize(data).Item2);
                            break;

                        case PlayerActionMade.hit:
                            Debug.Log("player hit");
                            GameManager.Instance.HitPlayer(netPlayerActionMade.Deserialize(data).Item3, netPlayerActionMade.Deserialize(data).Item2);
                            break;

                        case PlayerActionMade.Death:
                            Debug.Log("player death");
                            GameManager.Instance.KillPlayer(netPlayerActionMade.Deserialize(data).Item2);
                            break;
                    }
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
        else if (NetworkManager.Instance.isServer && PrivateMessage)
        {
            NetworkManager.Instance.SendToClient(data, Ip);
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

    public void SendNewListOfPlayers()
    {
        netMessageToClient.data = NetworkManager.Instance.players;

        byte[] data = netMessageToClient.Serialize();
        Lobby.Instance.UpdateLobby();
        NetworkManager.Instance.Broadcast(data);
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

    private byte[] ThrowErrorMessage(ErrorMessageType errorMessageSend)
    {
        errorMessage.data = errorMessageSend;
        Debug.Log("SendErrorMessage = " + errorMessageSend.ToString());
        return errorMessage.Serialize();
    }

}

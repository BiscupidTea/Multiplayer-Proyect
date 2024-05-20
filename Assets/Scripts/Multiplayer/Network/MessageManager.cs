using System;
using System.Net;
using UnityEngine;

public class MessageManager : MonoBehaviourSingleton<MessageManager>
{
    private CheckSumReeder checkSumReeder = new CheckSumReeder();
    private NetCode netCode = new NetCode();
    private PingPong pingPong = new PingPong();
    private NetTimer netTimer = new NetTimer();
    private NetVector3 netVector3 = new NetVector3();
    private NetQuaternion netQuaternion = new NetQuaternion();
    private ErrorMessage errorMessage = new ErrorMessage();
    private NetServerActionMade netActionMade = new NetServerActionMade();
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

                    if (!NetworkManager.Instance.gameStarted)
                    {
                        Players newPlayer = new Players(netMessageToServer.Deserialize(data).Item2, netMessageToServer.Deserialize(data).Item1);

                        newPlayer.id = NetworkManager.Instance.clientId;
                        newPlayer.clientId = netMessageToServer.Deserialize(data).Item2;

                        if (CheckAlreadyUseName(newPlayer.clientId))
                        {
                            data = ThrowErrorMessage(ErrorMessageType.UsernameAlredyUse);

                            PrivateMessage = true;
                        }
                        else
                        {
                            Debug.Log(NetworkManager.Instance.players.Count + " < " + 4);
                            if (NetworkManager.Instance.players.Count <= 4)
                            {
                                NetworkManager.Instance.AddClient(Ip);
                                NetworkManager.Instance.addPlayer(newPlayer);

                                netMessageToClient.data = NetworkManager.Instance.players;

                                data = netMessageToClient.Serialize();

                                NetworkManager.Instance.clientId++;
                                Lobby.Instance.UpdateLobby();
                                Debug.Log("add new client = Client Id: " + netMessageToClient.data[netMessageToClient.data.Count - 1].clientId + " - Id: " + netMessageToClient.data[netMessageToClient.data.Count - 1].id);
                                PrivateMessage = false;
                            }
                            else
                            {
                                data = ThrowErrorMessage(ErrorMessageType.ServerFull);

                                PrivateMessage = true;
                            }
                        }
                    }
                    else
                    {
                        data = ThrowErrorMessage(ErrorMessageType.GameStarted);
                        PrivateMessage = true;
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
                        case ErrorMessageType.UsernameAlredyUse:
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

                case MessageType.Time:
                    if (GameManager.Instance.playing)
                    {

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

                case MessageType.ActionMadeBy:
                    switch (netActionMade.Deserialize(data))
                    {
                        case ServerActionMade.StartGame:
                            GameManager.Instance.StartGame();
                            break;
                        case ServerActionMade.EndGame:
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

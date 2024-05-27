using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ServerNetManager : NetworkManager
{
    private Dictionary<int, Client> clients = new Dictionary<int, Client>();
    private readonly Dictionary<IPEndPoint, int> ipToId = new Dictionary<IPEndPoint, int>();
    public int clientId = 0; // This id should be generated during first handshake

    public GameManager gameManager;

    [SerializeField] private StringChannelSO MessageChannelSo;

    private int maxPlayers = 4;
    private float lobbyTime;
    private float preGameTime;
    private float gameTime;

    protected override void OnStart()
    {
        clients.Clear();
        ipToId.Clear();
        connection = new UdpConnection(port, this);
    }

    protected override void Disconnect()
    {
        clients.Clear();
        connection.Close();
    }

    public bool TryAddClient(IPEndPoint ip, string userName)
    {
        if (!ipToId.ContainsKey(ip))
        {
            if (true)//game started
            {
                if (clients.Count < maxPlayers)
                {
                    if (CheckUserName(userName))
                    {

                        int id = clientId;
                        ipToId[ip] = clientId;

                        clients.Add(clientId, new Client(ip, id, Time.realtimeSinceStartup));
                        AddPlayer(new Player(userName, clientId));
                        clientId++;

                        //call created player


                        //check minimum player

                        Debug.Log("ADD CLIENT::Client Ip = " + ip.Address + " - Client Id = " + userName);
                        return true;
                    }
                    else
                    {
                        ErrorMessage errorMessage = new ErrorMessage(ErrorMessageType.invalidUserName);
                        SendToClient(errorMessage.Serialize(), ip);
                    }
                }
                else
                {
                    ErrorMessage errorMessage = new ErrorMessage(ErrorMessageType.ServerFull);
                    SendToClient(errorMessage.Serialize(), ip);
                }
            }
            else
            {
                ErrorMessage errorMessage = new ErrorMessage(ErrorMessageType.GameStarted);
                SendToClient(errorMessage.Serialize(), ip);
            }

        }

        return false;
    }

    public void DisconnectPlayer(Client client)
    {
        Player playerDelete = new("",-100);
        foreach (Player player in players)
        {
            if (client.id == player.id)
            {
                playerDelete = player;
                break;
            }
        }

        Debug.Log("Removing client: " + client.clientId);

        client.IsConected = false;
        players.Remove(playerDelete);

        NetContinueHandShake netContinueHand = new NetContinueHandShake(players);
        Broadcast(netContinueHand.Serialize());
    }

    private bool CheckUserName(string userNameCheck)
    {
        foreach (var currentPlayer in players)
        {
            if (currentPlayer.clientId == userNameCheck)
            {
                return false;
            }
        }

        return true;
    }

    public override void OnReceiveDataEvent(byte[] data, IPEndPoint ip)
    {
        MessageType messageType = (MessageType)BitConverter.ToInt32(data, 0);

        switch (messageType) 
        {
            case MessageType.StartHandShake:
                StartHandShake(data, ip);
                break;

            case MessageType.String:
                WriteConsoleMessage(data);
                break;

            case MessageType.PingPong:
                CheckPingPong(data, ip);
                break;

                default:
                Debug.LogError("Received Unknown Message: " + messageType);
                break;
        }
    }

    public void SendToClient(byte[] data, IPEndPoint ip)
    {
        connection.Send(data, ip);
    }

    public void SendToClient(byte[] data, string userName, IPEndPoint ip)
    {
        IPEndPoint clientIp = ip;

        foreach (var client in clients)
        {
            if (client.Value.clientId == userName)
            {
                clientIp = client.Value.ipEndPoint;
                break;
            }
        }

        connection.Send(data, clientIp);
    }

    public void Broadcast(byte[] data)
    {
        using (var iterator = clients.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                if (iterator.Current.Value.IsConected)
                {
                    connection.Send(data, iterator.Current.Value.ipEndPoint);
                }
            }
        }
    }

    private void StartHandShake(byte[] data, IPEndPoint ip)
    {
        NetHandShake netHandShake = new NetHandShake();
        string newPlayerName = netHandShake.Deserialize(data);

        if (TryAddClient(ip, newPlayerName))
        {
            //re send handshake
            NetContinueHandShake netContinueHand = new NetContinueHandShake(players);
            Broadcast(netContinueHand.Serialize());

            //start player ping
            PingPong pingPong = new PingPong();
            GetClient(ip).LastMessageRecived = DateTime.UtcNow;
            SendToClient(pingPong.Serialize(), ip);

            //inform player entered
            NetString welcomeMessage = new NetString();
            welcomeMessage.data = ("The player " + newPlayerName + " Has joined");
            Broadcast(welcomeMessage.Serialize());
        }
    }

    public override void CheckPingPong(byte[] data, IPEndPoint ip)
    {
        PingPong pingPong = new PingPong();
        ResetClientTimer(ipToId[ip]);
        SendToClient(pingPong.Serialize(), ip);
    }

    public void ResetClientTimer(int PlayerId)
    {
        foreach (var client in clients) 
        {
            if (client.Value.id == PlayerId)
            {
                client.Value.resetTimer();
            }
        }
    }

    public override void OnUpdate()
    {
        
    }

    public override void CheckTimeOut()
    {
        if (clients.Count > 0)
        {
            foreach (var client in clients)
            {
                if (TimeOut < (DateTime.UtcNow - client.Value.LastMessageRecived).TotalSeconds)
                {
                    DisconnectPlayer(client.Value);
                }
            }
        }
    }

    private void WriteConsoleMessage(byte[] data)
    {
        NetString message = new NetString();
        string newMessage = message.Deserialize(data);

        MessageChannelSo.RaiseEvent(newMessage);
    }

    private Client GetClient(IPEndPoint ip)
    {
        foreach (Client client in clients.Values)
        {
            if (client.id == ipToId[ip])
            {
                return client;
            }
        }

        return null;
    }
    private Client GetClient(int playerId)
    {
        foreach (var client in clients)
        {
            if (client.Value.id == playerId)
            {
                return client.Value;
            }
        }

        return null;
    }
}

using BT_NetworkSystem;
using System.Net;

public class ServerNetManager : NetworkManager
{
    private Dictionary<int, Client> clients = new Dictionary<int, Client>();
    private readonly Dictionary<IPEndPoint, int> ipToId = new Dictionary<IPEndPoint, int>();
    public int clientId = 0; // This id should be generated during first handshake

    private Dictionary<Client, List<CacheMessage>> messagesToSend = new();

    private int maxPlayers = 4;
    // private float lobbyTime;
    // private float preGameTime;
    // private float gameTime;

    public bool gameStarted;
    private int cacheMessages = 0;

    public ServerNetManager() : base()
    {
    }

    public void startServer()
    {
        OnStart();
    }

    protected override void OnStart()
    {
        clients.Clear();
        ipToId.Clear();
        connection = new UdpConnection(port, this);
        gameStarted = true;
    }

    protected override void Disconnect()
    {
        clients.Clear();
        connection.Close();
        gameStarted = false;
    }

    public bool TryAddClient(IPEndPoint ip, string userName)
    {
        if (!ipToId.ContainsKey(ip))
        {
            if (gameStarted)
            {
                if (CheckCurrentPlayers())
                {
                    if (CheckUserName(userName))
                    {
                        int id = clientId;
                        ipToId[ip] = clientId;

                        clients.Add(clientId, new Client(ip, id));
                        AddPlayer(new Player(userName, clientId));
                        clientId++;
                        Console.WriteLine("ADD CLIENT::Client Ip = " + ip.Address + " - Client Id = " + userName +
                                          "\n");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("Can't add player: InvalidUserName\n");

                        ErrorMessage errorMessage = new ErrorMessage();
                        errorMessage.data = ErrorMessageType.InvalidUserName;
                        SendToClient(errorMessage.Serialize(), ip);
                    }
                }
                else
                {
                    Console.WriteLine("Can't add player: ServerFull\n");

                    ErrorMessage errorMessage = new ErrorMessage();
                    errorMessage.data = ErrorMessageType.ServerFull;
                    SendToClient(errorMessage.Serialize(), ip);
                }
            }
            else
            {
                Console.WriteLine("Can't add player: GameStarted\n");

                ErrorMessage errorMessage = new ErrorMessage();
                errorMessage.data = ErrorMessageType.GameStarted;
                SendToClient(errorMessage.Serialize(), ip);
            }
        }

        Console.WriteLine("Can't add player: ip alredy in use\n");
        return false;
    }

    private bool CheckCurrentPlayers()
    {
        int currentsPlayers = 0;
        foreach (var player in clients)
        {
            if (player.Value.IsConected)
            {
                currentsPlayers++;
            }
        }

        if (currentsPlayers > maxPlayers)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public void DisconnectPlayer(Client client)
    {
        Player playerDelete = new("", -100);
        foreach (Player player in players)
        {
            if (client.id == player.id)
            {
                playerDelete = player;
                break;
            }
        }

        Console.WriteLine("Remove CLIENT::Id = " + playerDelete.id + " - Client Id = " + playerDelete.clientId);

        client.IsConected = false;
        players.Remove(playerDelete);

        NetContinueHandShake netContinueHand = new NetContinueHandShake();
        netContinueHand.data = players;
        Broadcast(netContinueHand.Serialize());
    }

    private bool CheckUserName(string userNameCheck)
    {
        foreach (var currentPlayer in players)
        {
            if (currentPlayer.clientId == userNameCheck && GetClient(currentPlayer.clientId).IsConected)
            {
                return false;
            }
        }

        return true;
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

        uint ordenableNumber = BitConverter.ToUInt32(data, 8);

        //Console.Write("message recived - " + messageType + "\n");

        if (haveCheckSum && checkSumReeder.CheckSumStatus(data))
        {
            if (messageType != MessageType.StartHandShake)
            {
                if (isOrdenable && isImportant)
                {
                    if (!GetClient(ip).LastMessage.ContainsKey(messageType))
                    {
                        GetClient(ip).LastMessage.Add(messageType, ordenableNumber);
                        //Console.Write("Create new key\n");
                    }
                    else
                    {
                        if (ordenableNumber == GetClient(ip).LastMessage[messageType] + 1)
                        {
                            GetClient(ip).LastMessage[messageType] = ordenableNumber;
                            // Console.Write("Set last Message - ordenable important\n");
                        }
                        else
                        {
                            GetClient(ip).pendingMessages[messageType]
                                .Add(new CacheMessage(data, ordenableNumber, messageType));
                            //Console.Write("Save Message - Wait Previous Message\n");
                            return;
                        }
                    }
                }
                else if (isOrdenable)
                {
                    if (!GetClient(ip).LastMessage.ContainsKey(messageType))
                    {
                        GetClient(ip).LastMessage.Add(messageType, ordenableNumber);
                        //Console.Write("Create new key\n");
                    }
                    else
                    {
                        if (ordenableNumber > GetClient(ip).LastMessage[messageType])
                        {
                            GetClient(ip).LastMessage[messageType] = ordenableNumber;
                            //Console.Write("Set last Message - ordenable\n");
                        }
                        else
                        {
                            //Console.Write("Discard Message - ordenable\n");
                            return;
                        }
                    }
                }
            }
        }
        else
        {
            Console.Write("Discard Message - checksum\n");
            return;
        }

        //Console.Write("Message received - " + messageType);

        ExecuteMessage(data, ip, messageType);

        CheckPendingMessage(data, ip, messageType, ordenableNumber);
    }

    private void CheckPendingMessage(byte[] data, IPEndPoint ip, MessageType messageType, uint ordenableNumber)
    {
        foreach (Client c in clients.Values)
        {
            if (c.pendingMessages.Count > 0)
            {
                foreach (CacheMessage message in c.pendingMessages[messageType])
                {
                    if (message.id == ordenableNumber + 1)
                    {
                        ExecuteMessage(data, ip, messageType);
                        GetClient(ip).LastMessage[messageType] = message.id;

                        CheckPendingMessage(data, ip, messageType, GetClient(ip).LastMessage[messageType]);
                        break;
                    }
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

            case MessageType.StartHandShake:
                StartHandShake(data, ip);
                break;

            case MessageType.PingPong:
                CheckPingPong(data, ip);
                break;

            case MessageType.Disconnect:
                NetHandShake endHandShake = new NetHandShake(MessageType.Disconnect);
                DisconnectPlayer(GetClient(endHandShake.Deserialize(data)));
                break;

            default:
                Console.Write("Message type Not Found\n");
                break;
        }
    }

    public override void MessageConfirmation(byte[] data)
    {
        ConfirmationMessage confirmationMessage = new ConfirmationMessage();
        confirmationMessage.data = confirmationMessage.Deserialize(data);

        foreach (var client in messagesToSend)
        {
            foreach (var m in client.Value)
            {
                if (m.type == confirmationMessage.data && m.id == confirmationMessage.GetId(data) && !m.Received)
                {
                    m.Received = true;
                    Console.Write("Message confirmation recived\n");
                }
            }
        }
    }

    public void SendToClient(byte[] data, IPEndPoint ip)
    {
        connection.Send(data, ip);
    }

    public void SendToClient(byte[] data, string userName)
    {
        foreach (var client in clients)
        {
            if (client.Value.clientId == userName)
            {
                connection.Send(data, client.Value.ipEndPoint);
                break;
            }
        }
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
        NetHandShake netHandShake = new NetHandShake(MessageType.StartHandShake);
        string newPlayerName = netHandShake.Deserialize(data);

        if (TryAddClient(ip, newPlayerName))
        {
            //re send handshake
            NetContinueHandShake netContinueHand = new NetContinueHandShake();
            netContinueHand.data = players;
            Broadcast(netContinueHand.Serialize());

            //start player ping
            PingPong pingPong = new PingPong();
            GetClient(ip).LastMessageRecived = DateTime.UtcNow;
            SendToClient(pingPong.Serialize(), ip);
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

    public override void OnUpdateMessages()
    {
        cacheMessages = messagesToSend.Count;
        if (messagesToSend.Count > 0)
        {
            foreach (var client in messagesToSend)
            {
                foreach (var m in client.Value)
                {
                    if ((DateTime.UtcNow - m.lastEmission).Seconds > ImportantMessageTimeOut)
                    {
                        SendToClient(m.message, client.Key.clientId);
                        m.lastEmission = DateTime.UtcNow;
                    }
                }
            }
        }
    }

    public override void CheckTimeOut()
    {
        if (clients.Count > 0)
        {
            foreach (var client in clients)
            {
                if (TimeOut < (DateTime.UtcNow - client.Value.LastMessageRecived).TotalSeconds &&
                    client.Value.IsConected)
                {
                    DisconnectPlayer(client.Value);
                }
            }
        }
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

    private Client GetClient(int Id)
    {
        foreach (var client in clients)
        {
            if (client.Value.id == Id)
            {
                return client.Value;
            }
        }

        return null;
    }

    private Client GetClient(string ClientId)
    {
        foreach (var client in clients)
        {
            if (client.Value.clientId == ClientId)
            {
                return client.Value;
            }
        }

        return null;
    }
}
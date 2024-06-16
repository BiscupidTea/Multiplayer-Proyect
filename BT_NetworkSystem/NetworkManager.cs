namespace BT_NetworkSystem;
using System.Net;


public abstract class NetworkManager : IReceiveData
{
    public IPAddress ipAddress
    {
        get; private set;
    }

    public int port
    {
        get; private set;
    }

    public int TimeOut = 5;
    public int ImportantMessageTimeOut = 3;

    public Action<byte[], IPEndPoint> OnReceiveEvent;

    public UdpConnection connection;

    public List<Player> players = new List<Player>();

    public Player myPlayer;

    public Dictionary<MessageType, List<CacheMessage>> pendingMessages = new();
    public Dictionary<MessageType, uint> LastMessage = new();

    public void Initialize(int port, IPAddress iPAddress)
    {
        this.port = port;
        this.ipAddress = iPAddress;
    }

    public void Initialize(int port, IPAddress iPAddress, Player player)
    {
        this.port = port;
        this.ipAddress = iPAddress;
        this.myPlayer = player;
    }

    public NetworkManager()
    {
        OnStart();
    }
    
    ~NetworkManager()
    {
        Disconnect();
    }
    

    protected virtual void Disconnect()
    {

    }

    protected virtual void OnStart()
    {
        players.Clear();
    }

    public void OnReceiveData(byte[] data, IPEndPoint ip)
    {
        OnReceiveDataEvent(data, ip);

        if (OnReceiveEvent != null)
            OnReceiveEvent.Invoke(data, ip);
    }

    public abstract void OnReceiveDataEvent(byte[] data, IPEndPoint ip);

    public void AddPlayer(Player newPlayer)
    {
        players.Add(newPlayer);
    }

    public Player GetPlayer(int id)
    {
        foreach (Player player in players)
        {
            if (player.id == id)
            {
                return player;
            }
        }

        return new Player("Player not Found", -10);
    }

    public abstract void CheckTimeOut();
    public abstract void OnUpdateMessages();
    public abstract void CheckPingPong(byte[] data, IPEndPoint ip);
    public abstract void MessageConfirmation(byte[] data);


    public void Update()
    {
        CheckTimeOut();
        OnUpdateMessages();

        // Flush the data in main thread
        if (connection != null)
            connection.FlushReceiveData();
    }

}
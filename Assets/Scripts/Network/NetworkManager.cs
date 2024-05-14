using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct Client
{
    public float timeStamp;
    public int id;
    public IPEndPoint ipEndPoint;
    public DateTime LastMessageRecived;

    public Client(IPEndPoint ipEndPoint, int id, float timeStamp)
    {
        this.timeStamp = timeStamp;
        this.id = id;
        this.ipEndPoint = ipEndPoint;
        this.LastMessageRecived = DateTime.UtcNow;
    }

    public void resetTimer()
    {
        LastMessageRecived = DateTime.UtcNow;
    }
}

[Serializable]
public struct Players
{
    public string clientId;
    public int id;

    public Players(string clientName, int id)
    {
        this.clientId = clientName;
        this.id = id;
    }
}

public class NetworkManager : MonoBehaviourSingleton<NetworkManager>, IReceiveData
{
    public IPAddress ipAddress
    {
        get; private set;
    }

    public int port
    {
        get; private set;
    }

    public bool isServer
    {
        get; private set;
    }

    public int TimeOut = 30;

    public Action<byte[], IPEndPoint> OnReceiveEvent;

    private UdpConnection connection;

    private Dictionary<int, Client> clients = new Dictionary<int, Client>();
    public List<Players> players = new List<Players>();
    public Players playerData;
    private readonly Dictionary<IPEndPoint, int> ipToId = new Dictionary<IPEndPoint, int>();
    public Text pingText;

    public int clientId = 0; // This id should be generated during first handshake

    public DateTime lastMessageRecieved = DateTime.UtcNow;
    public DateTime lastMessageSended = DateTime.UtcNow;
    private int timeOut = 5;

    public void StartServer(int port)
    {
        isServer = true;
        this.port = port;
        connection = new UdpConnection(port, this);
    }

    public void StartClient(IPAddress ip, int port, string name)
    {
        isServer = false;

        this.port = port;
        this.ipAddress = ip;

        connection = new UdpConnection(ip, port, this);

        playerData = new Players(name, -1);

        MessageManager.Instance.OnSendHandshake(playerData.clientId, playerData.id);
        MessageManager.Instance.StartPingPong();
    }

    void AddClient(IPEndPoint ip)
    {
        if (!ipToId.ContainsKey(ip))
        {
            Debug.Log("Adding client: " + ip.Address);

            int id = clientId;
            ipToId[ip] = clientId;

            clients.Add(clientId, new Client(ip, id, Time.realtimeSinceStartup));
        }
    }

    void Disconect()
    {
        clients.Clear();
    }

    public void addPlayer(Players newPlayer)
    {
        players.Add(newPlayer);
    }

    void RemoveClient(IPEndPoint ip)
    {
        if (ipToId.ContainsKey(ip))
        {
            Debug.Log("Removing client: " + ip.Address);
            clients.Remove(ipToId[ip]);
        }
    }

    public void OnReceiveData(byte[] data, IPEndPoint ip)
    {
        AddClient(ip);

        MessageManager.Instance.OnRecieveMessage(data, ip);

        if (OnReceiveEvent != null)
            OnReceiveEvent.Invoke(data, ip);
    }

    public void SendToServer(byte[] data)
    {
        connection.Send(data);
    }

    public void SendToClient(byte[] data, IPEndPoint ip)
    {
        connection.Send(data, ip);
    }

    public void Broadcast(byte[] data)
    {
        using (var iterator = clients.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                connection.Send(data, iterator.Current.Value.ipEndPoint);
            }
        }
    }

    void Update()
    {
        // Flush the data in main thread
        if (connection != null)
            connection.FlushReceiveData();

        if (!isServer)
        {
            if ((DateTime.UtcNow - lastMessageRecieved).Seconds > timeOut)
            {
                Disconect();
                Debug.Log("disconected from server = " + (DateTime.UtcNow - lastMessageRecieved).TotalSeconds);
            }
        }
        else
        {
            for (int i = 0; i < clients.Count; i++)
            {
                if ((DateTime.UtcNow - clients[i].LastMessageRecived).Seconds > timeOut)
                {
                    RemoveClient(clients[i].ipEndPoint);
                    Debug.Log("disconect player: " + players[i].clientId +" = " + (DateTime.UtcNow - lastMessageRecieved).TotalSeconds);
                }
            }
        }
    }

    public void ResetClientTimer(IPEndPoint ip)
    {
        for (int i = 0; i < clients.Count; i++)
        {
            if (clients[i].ipEndPoint == ip)
            {
                clients[i].resetTimer();
                Debug.Log("reset timer player");
            }
        }
    }

}

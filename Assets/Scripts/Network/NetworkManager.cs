using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class Client
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
        this.LastMessageRecived = DateTime.UtcNow;
    }
}

[Serializable]
public class Players
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
    public bool initialized;

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
    }

    public void AddClient(IPEndPoint ip)
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
        initialized = false;
    }

    public void addPlayer(Players newPlayer)
    {
        players.Add(newPlayer);
    }

    public void RemoveClient(IPEndPoint ip)
    {
        if (ipToId.ContainsKey(ip))
        {
            Debug.Log("Removing client: " + ip.Address);
            clients.Remove(ipToId[ip]);
        }
    }

    public void OnReceiveData(byte[] data, IPEndPoint ip)
    {
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

        if (!isServer && initialized)
        {
            if ((DateTime.UtcNow - lastMessageRecieved).Seconds > timeOut)
            {
                Debug.Log((DateTime.UtcNow - lastMessageRecieved).Seconds);
                NetworkScreen.Instance.SwitchToNetworkScreen();
                Disconect();
                Debug.Log("disconected from server = ");
            }
        }

        if (isServer)
        {
            for (int i = 0; i < clients.Count; i++)
            {
                if ((DateTime.UtcNow - clients[i].LastMessageRecived).Seconds > timeOut)
                {
                    RemoveClient(clients[i].ipEndPoint);
                }
            }
        }
    }

    public void ResetClientTimer(int PlayerId)
    {
        for (int i = 0; i < clients.Count; i++)
        {
            if (clients[i].id == PlayerId)
            {
                clients[i].resetTimer();
            }
        }
    }

}

using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System;

namespace MultiplayerLib
{
    public class Network
    {
        public class Player
        {
            public string clientId;
            public int id;

            public Player(string clientName, int id)
            {
                this.clientId = clientName;
                this.id = id;
            }
        }

        public IPAddress ipAddress
        {
            get; private set;
        }

        public int port
        {
            get; private set;
        }

        public int TimeOut = 5;

        public Action<byte[], IPEndPoint> OnReceiveEvent;

        public UdpConnection connection;

        public List<Player> players = new List<Player>();

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

        protected virtual void OnUpdate()
        {
            CheckTimeOut();
            OnUpdate();

            // Flush the data in main thread
            if (connection != null)
                connection.FlushReceiveData();
        }

        public abstract void CheckTimeOut();
        public abstract void CheckPingPong(byte[] data, IPEndPoint ip);
    }

    public class UdpConnection
    {
        private struct DataReceived
        {
            public byte[] data;
            public IPEndPoint ipEndPoint;
        }

        private readonly UdpClient connection;
        private IReceiveData receiver = null;
        private Queue<DataReceived> dataReceivedQueue = new Queue<DataReceived>();

        object handler = new object();

        public UdpConnection(int port, IReceiveData receiver = null)
        {
            connection = new UdpClient(port);

            this.receiver = receiver;

            connection.BeginReceive(OnReceive, null);
        }

        public UdpConnection(IPAddress ip, int port, IReceiveData receiver = null)
        {
            connection = new UdpClient();
            connection.Connect(ip, port);

            this.receiver = receiver;

            connection.BeginReceive(OnReceive, null);
        }

        public void Close()
        {
            connection.Close();
        }

        public void FlushReceiveData()
        {
            lock (handler)
            {
                while (dataReceivedQueue.Count > 0)
                {
                    DataReceived dataReceived = dataReceivedQueue.Dequeue();
                    if (receiver != null)
                        receiver.OnReceiveData(dataReceived.data, dataReceived.ipEndPoint);
                }
            }
        }

        void OnReceive(IAsyncResult ar)
        {
            DataReceived dataReceived = new DataReceived();
            try
            {
                dataReceived.data = connection.EndReceive(ar, ref dataReceived.ipEndPoint);
            }
            catch (SocketException e)
            {
                // This happens when a client disconnects, as we fail to send to that port.
                UnityEngine.Debug.LogWarning("[UdpConnection] " + e.Message);
            }

            lock (handler)
            {
                dataReceivedQueue?.Enqueue(dataReceived);
            }

            connection.BeginReceive(OnReceive, null);
        }

        public void Send(byte[] data)
        {
            connection.Send(data, data.Length);
        }

        public void Send(byte[] data, IPEndPoint ipEndpoint)
        {
            connection.Send(data, data.Length, ipEndpoint);
        }
    }
}


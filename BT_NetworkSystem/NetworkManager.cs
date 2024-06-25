using System;
using System.Collections.Generic;
using System.Net;

namespace BT_NetworkSystem
{
    public abstract class NetworkManager : IReceiveData
    {
        public IPAddress ipAddress { get; private set; }

        public int port { get; private set; }

        public int TimeOut = 5;
        public int ImportantMessageTimeOut = 3;

        public Action<byte[], IPEndPoint> OnReceiveEvent;

        public UdpConnection connection { get; set; }

        public List<Player> players = new List<Player>();

        public Player myPlayer = new Player("PlayerNotInitialized", -1);

        public void Initialize(Player player)
        {
            this.port = GetServerData().Item1;
            this.ipAddress = GetServerData().Item2;
            this.myPlayer = player;
        }

        public void Initialize(int port, Player player)
        {
            this.port = port;
            this.ipAddress = GetServerData().Item2;
            this.myPlayer.id = player.id;
            this.myPlayer.clientId = player.clientId;
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

        public (int, IPAddress) GetServerData()
        {
            string addressField = "192.168.200.157";
            string portField = "12345";

            (int, IPAddress) data;
            data.Item1 = System.Convert.ToInt32(portField);
            data.Item2 = IPAddress.Parse(addressField);

            return data;
        }
    }
}
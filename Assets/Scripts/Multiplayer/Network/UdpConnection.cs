using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

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
        try
        {
            connection = new UdpClient(port);
            connection.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            this.receiver = receiver;

            connection.BeginReceive(OnReceive, null);
        }
        catch (Exception e)
        {

        }

    }

    public UdpConnection(IPAddress ip, int port, string tag, IReceiveData receiver = null)
    {
        try
        {
            connection = new UdpClient();
            connection.Connect(ip, port);
            this.receiver = receiver;

            connection.BeginReceive(OnReceive, null);

            NetHandShake handShake = new NetHandShake(MessageType.StartHandShake);
            handShake.data = tag;
            Send(handShake.Serialize());

        }
        catch (Exception e)
        {

            
        }
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
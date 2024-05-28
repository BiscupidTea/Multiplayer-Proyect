using System;
using System.Net;

[Serializable]
public class Client
{
    public int id;
    public string clientId;
    public bool IsConected;
    public IPEndPoint ipEndPoint;
    public DateTime LastMessageRecived;

    public Client(IPEndPoint ipEndPoint, int id)
    {
        this.id = id;
        this.ipEndPoint = ipEndPoint;
        IsConected = true;
        this.LastMessageRecived = DateTime.UtcNow;
    }

    public void resetTimer()
    {
        this.LastMessageRecived = DateTime.UtcNow;
    }
}

﻿using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

[Serializable]
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

public abstract class NetworkManager : MonoBehaviour, IReceiveData
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

    public abstract void CheckTimeOut();
    public abstract void OnUpdate();
    public abstract void CheckPingPong(byte[] data, IPEndPoint ip);


    void Update()
    {
        CheckTimeOut();
        OnUpdate();

        // Flush the data in main thread
        if (connection != null)
            connection.FlushReceiveData();
    }

}

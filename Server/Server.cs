using System;
using System.Net;
using BT_NetworkSystem;

public class Server
{
    static void Main(string[] args)
    {
        ServerNetManager server = new ServerNetManager();

        string addressInputField = "127.0.0.1";
        string portInputField = "55555";

        server.Initialize(
            System.Convert.ToInt32(portInputField),
            IPAddress.Parse(addressInputField),
            new Player("server", -1));
        
        server.startServer();

        Console.Write("Server Started");
        
        while (server.gameStarted)
        {
            server.Update();
        }
    }
}
using System;
using System.Net;
using BT_NetworkSystem;

public class Server
{
    static void Main(string[] args)
    {
        ServerNetManager server = new ServerNetManager();
        server.Initialize(new Player("server", -1));
        
        server.startServer();

        Console.Write("Server Started\n");
        Console.Write("Adress : " + server.ipAddress + "\n");
        Console.Write("Port : " + server.port + "\n");
        
        while (server.gameStarted)
        {
            Thread.Sleep(50);
            server.Update();
        }
        Console.WriteLine();
    
    }
}
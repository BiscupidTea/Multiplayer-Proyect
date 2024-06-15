using System;

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
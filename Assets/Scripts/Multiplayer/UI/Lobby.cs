using System;
using UnityEngine;
using UnityEngine.UI;

public class Lobby : MonoBehaviour
{
    [SerializeField] private Text[] playerSpaces;
    [SerializeField] private bool Start = false;
    [SerializeField] private float maxTime;
    private float timer;
    private NetTimer netTimer = new NetTimer();
    private NetServerActionMade actionMade = new NetServerActionMade();
    [SerializeField] private Text Timer;

    protected override void Initialize()
    {
        for (int i = 0; i < playerSpaces.Length; i++)
        {
            playerSpaces[i].text = "Empty";
        }

        this.gameObject.SetActive(false);
    }

    public void UpdateLobby()
    {
        for (int i = 0; i < playerSpaces.Length; i++)
        {
            if (i < NetworkManager.Instance.players.Count)
            {
                playerSpaces[i].text = NetworkManager.Instance.players.ToArray()[i].clientId;
            }
            else
            {
                playerSpaces[i].text = "Empty";
            }
        }

        if (NetworkManager.Instance.players.Count >= 2 && NetworkManager.Instance.isServer && !Start)
        {
            StartTimer();
        }
    }

    private void StartTimer()
    {
        Start = true;
        timer = maxTime;

        netTimer.data = timer;
        NetworkManager.Instance.Broadcast(netTimer.Serialize());
    }

    private void RestartTimer()
    {
        Start = false;
        timer = maxTime;
        Timer.text = "waiting other players";

        netTimer.data = timer;
        NetworkManager.Instance.Broadcast(netTimer.Serialize());
    }

    private void UpdateTimer()
    {
        timer -= Time.deltaTime;
        Timer.text = ((int)timer).ToString();

        netTimer.data = timer;
        NetworkManager.Instance.Broadcast(netTimer.Serialize());
    }

    public void SetTime(float newTime)
    {
        Timer.text = newTime.ToString();
    }

    private void Update()
    {
        if (Start && NetworkManager.Instance.isServer)
        {
            if (NetworkManager.Instance.players.Count < 2)
            {
                RestartTimer();
            }

            if (timer >= 0)
            {
                UpdateTimer();
            }
            else
            {
                actionMade.data.Item1 = ServerActionMade.StartGame;
                actionMade.data.Item2 = -1;
                NetworkManager.Instance.Broadcast(actionMade.Serialize());
                GameManager.Instance.StartGame();
                Timer.text = "waiting other players";
            }
        }
    }
}
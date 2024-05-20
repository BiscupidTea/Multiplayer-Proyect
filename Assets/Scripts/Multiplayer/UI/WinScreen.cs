using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class WinScreen : MonoBehaviourSingleton<WinScreen>
{
    [SerializeField] private bool Start = false;
    [SerializeField] private float maxTime;
    private float timer;
    private NetTimer netTimer = new NetTimer();
    [SerializeField] private Text Timer;
    [SerializeField] private Text Winner;
    private NetServerActionMade actionMade = new NetServerActionMade();

    protected override void Initialize()
    {
        this.gameObject.SetActive(false);
    }

    public void StartTimer(string winnerPlayer)
    {
        this.gameObject.SetActive(true);

        Start = true;
        timer = maxTime;

        netTimer.data = timer;
        NetworkManager.Instance.Broadcast(netTimer.Serialize());

        Winner.text = winnerPlayer;
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
                actionMade.data.Item1 = ServerActionMade.EndGame;
                actionMade.data.Item2 = -1;
                NetworkManager.Instance.Broadcast(actionMade.Serialize());
            }
        }
    }
}

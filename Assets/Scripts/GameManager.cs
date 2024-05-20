using System;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviourSingleton<GameManager>
{
    [SerializeField] private PlayerSpawn[] playerSpawn;
    [SerializeField] private GameObject[] playerList;
    private NetServerActionMade ActionMade = new NetServerActionMade();
    public bool playing = false;
    public PlayerSO playerSO;

    [SerializeField] private bool Start = false;
    [SerializeField] private float maxTime;
    private float timer;
    private NetTimer netTimer = new NetTimer();
    [SerializeField] private Text Timer;
    public void StartGame()
    {
        CanvasSwitcher.Instance.SwitchCanvas(modifyCanvas.chatScreen);

        playerList = new GameObject[NetworkManager.Instance.players.Count];

        for (int i = 0; i < NetworkManager.Instance.players.Count; i++)
        {
            playerList[i] = Instantiate(playerSpawn[i].PlayerPrefab, playerSpawn[i].SpawnPoint.position, Quaternion.identity, gameObject.transform);
            playerList[i].GetComponent<PlayerController>().id = i;

            if (i == NetworkManager.Instance.playerData.id && !NetworkManager.Instance.isServer)
            {
                playerList[i].AddComponent<PlayerMovement>();
                playerList[i].AddComponent<PlayerShoot>();
                playerList[i].GetComponent<PlayerController>().AddComponents();
            }
        }

        StartTimer();
    }

    public void MovePlayer(Vector3 position, int id)
    {
        if (NetworkManager.Instance.playerData.id != id || NetworkManager.Instance.isServer)
        {
            playerList[id].gameObject.transform.position = position;
        }
    }

    public void RotatePlayer(Quaternion rotation, int id)
    {
        if (NetworkManager.Instance.playerData.id != id || NetworkManager.Instance.isServer)
        {
            playerList[id].gameObject.transform.rotation = rotation;
        }
    }

    public void HitPlayer(Vector3 position, int id)
    {
        if (NetworkManager.Instance.playerData.id != id || NetworkManager.Instance.isServer)
        {
            playerList[id].gameObject.GetComponent<PlayerController>().health = (int)position.x;
        }
    }

    public void ShootPlayer(Vector3 position, int id)
    {
        if (NetworkManager.Instance.playerData.id != id || NetworkManager.Instance.isServer)
        {
            Instantiate(playerSO.shootPrefab, position, playerList[id].gameObject.transform.rotation);
        }
    }

    public void KillPlayer(int id)
    {
        playerList[id].gameObject.SetActive(false);
        playerList[id].gameObject.GetComponent<PlayerController>().isAlive = false;
        checkWinCondition();
    }

    private void checkWinCondition()
    {
        int playersAlive = 0;
        int playerIdWin = 0;

        for (int i = 0; i < playerList.Length; i++)
        {
            if (playerList[i].GetComponent<PlayerController>().isAlive)
            {
                playersAlive++;
                playerIdWin = i;
            }
        }

        if (playersAlive == 1)
        {
            EndGame(playerIdWin);
        }
    }


    public void ShowWin(int id)
    {
        Debug.Log(NetworkManager.Instance.players[id].clientId);
        Debug.Log(id);
        WinScreen.Instance.StartTimer(NetworkManager.Instance.players[id].clientId);
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Start && NetworkManager.Instance.isServer)
        {
            if (timer >= 0)
            {
                UpdateTimer();
            }
            else
            {
                ActionMade.data.Item1 = ServerActionMade.EndGame;

                int bestPlayer = 0;
                for (int i = 0; i < playerList.Length; i++)
                {
                    if (bestPlayer < playerList[i].GetComponent<PlayerController>().health)
                    {
                        bestPlayer = playerList[i].GetComponent<PlayerController>().health;
                    }
                }

                ActionMade.data.Item2 = bestPlayer;

                EndGame(bestPlayer);
            }
        }
    }

    private void EndGame(int playerIdWin)
    {
        ShowWin(playerIdWin);
        ActionMade.data.Item1 = ServerActionMade.EndGame;
        ActionMade.data.Item2 = playerIdWin;
        NetworkManager.Instance.Broadcast(ActionMade.Serialize());
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

    private void StartTimer()
    {
        Start = true;
        timer = maxTime;

        netTimer.data = timer;
        NetworkManager.Instance.Broadcast(netTimer.Serialize());
    }
}

[Serializable]
public class PlayerSpawn
{
    public Transform SpawnPoint;
    public GameObject PlayerPrefab;
}
using System;
using UnityEngine;

public class GameManager : MonoBehaviourSingleton<GameManager>
{
    [SerializeField] private PlayerSpawn[] playerSpawn;
    [SerializeField] private GameObject[] playerList;
    public bool playing = false;

    public void StartGame()
    {
        CanvasSwitcher.Instance.SwitchCanvas(modifyCanvas.chatScreen);

        for (int i = 0; i < NetworkManager.Instance.players.Count; i++)
        {
            playerList[i] = Instantiate(playerSpawn[i].PlayerPrefab, playerSpawn[i].SpawnPoint.position, Quaternion.identity, gameObject.transform);

            if (i == NetworkManager.Instance.playerData.id && !NetworkManager.Instance.isServer)
            {
                playerList[i].AddComponent<PlayerMovement>();
                playerList[i].GetComponent<PlayerMovement>().id = i;
            }
        }
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
}

[Serializable]
public class PlayerSpawn
{
    public Transform SpawnPoint;
    public GameObject PlayerPrefab;
}
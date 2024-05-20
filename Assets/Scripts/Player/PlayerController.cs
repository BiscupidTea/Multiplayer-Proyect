using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerSO playerSO;
    private PlayerShoot playerShoot;
    private PlayerMovement playerMovement;
    public int health = 3;
    private NetPlayerActionMade playerActionMade = new NetPlayerActionMade();
    public int id;
    public bool isAlive = true;

    public void AddComponents()
    {
        playerShoot = GetComponent<PlayerShoot>();
        playerMovement = GetComponent<PlayerMovement>();

        if (playerShoot != null && playerMovement != null)
        {
            playerShoot.ShootPrefab = playerSO.shootPrefab;
            playerShoot.shootPosition = playerSO.shootPosition;
            playerShoot.shootSpeed = playerSO.shootSpeed;
            playerShoot.id = id;

            playerMovement.moveSpeed = playerSO.moveSpeed;
            playerMovement.rotationSpeed = playerSO.rotateSpeed;
            playerMovement.id = id;
        }
    }

    public void DecreaseLive()
    {
        health--;

        if (health <= 0 && NetworkManager.Instance.isServer)
        {
            isAlive = false;
            playerActionMade.data.Item1 = PlayerActionMade.Death;
            playerActionMade.data.Item2 = id;
            playerActionMade.data.Item3 = Vector3.zero;
            NetworkManager.Instance.Broadcast(playerActionMade.Serialize());
            GameManager.Instance.KillPlayer(id);
        }
        else
        {
            playerActionMade.data.Item1 = PlayerActionMade.hit;
            playerActionMade.data.Item2 = id;
            playerActionMade.data.Item3 = new Vector3(health, 0, 0);
            NetworkManager.Instance.Broadcast(playerActionMade.Serialize());
        }
    }
}

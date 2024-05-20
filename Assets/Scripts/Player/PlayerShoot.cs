using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public int id;
    public float shootSpeed;
    public GameObject ShootPrefab;
    public Vector3 shootPosition;
    private NetPlayerActionMade playerActionMade = new NetPlayerActionMade();
    private bool canShoot = true;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (canShoot)
            {
                canShoot = false;
                Shoot();
                StartCoroutine(ShootDelay());
            }
        }
    }

    IEnumerator ShootDelay()
    {
        yield return new WaitForSeconds(shootSpeed);
        canShoot = true;
    }

    private void Shoot()
    {
        playerActionMade.data.Item1 = PlayerActionMade.Shoot;
        playerActionMade.data.Item2 = id;
        playerActionMade.data.Item3 = transform.position + transform.forward * shootPosition.x + transform.up * shootPosition.y;
        NetworkManager.Instance.SendToServer(playerActionMade.Serialize());
    }
}

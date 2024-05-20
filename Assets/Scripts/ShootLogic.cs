using System.Collections;
using UnityEngine;

public class ShootLogic : MonoBehaviour
{
    [SerializeField] private float shootSpeed;
    public int id;

    private void Start()
    {
        StartCoroutine(LifeTime());
    }

    private void Update()
    {
        transform.position += transform.forward * shootSpeed * Time.deltaTime;
    }

    IEnumerator LifeTime()
    {
        yield return new WaitForSeconds(6);
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        gameObject.GetComponent<Collider>().enabled = false;
        PlayerController enemy = other.gameObject.GetComponent<PlayerController>();
        if (enemy)
        {
            enemy.DecreaseLive();
        }

        Destroy(gameObject);
    }
}

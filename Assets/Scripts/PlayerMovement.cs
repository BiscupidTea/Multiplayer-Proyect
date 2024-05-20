using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public int id;
    [SerializeField] private int moveSpeed = 5;
    [SerializeField] private int rotationSpeed = 45;
    private NetVector3 position = new NetVector3();
    private NetQuaternion rotation = new NetQuaternion();
    private Vector2 movement;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        float moveVertical = Input.GetAxis("Vertical");
        float turn = Input.GetAxis("Horizontal");

        if (moveVertical != 0 || turn != 0)
        {
            MovePlayer(moveVertical, turn);
        }
    }

    private void MovePlayer(float moveVertical, float turn)
    {
        Vector3 movement = transform.forward * moveVertical * moveSpeed * Time.deltaTime;
        rb.MovePosition(rb.position + movement);

        float turnAngle = turn * rotationSpeed * Time.deltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, turnAngle, 0f);

        rb.MoveRotation(rb.rotation * turnRotation);

        rotation.data.Item1 = transform.rotation;
        rotation.data.Item2 = id;
        NetworkManager.Instance.SendToServer(rotation.Serialize());

        position.data.Item1 = transform.position;
        position.data.Item2 = id;
        NetworkManager.Instance.SendToServer(position.Serialize());
    }
}

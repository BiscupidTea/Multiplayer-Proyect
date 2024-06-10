using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerData", menuName = "ScriptableObjects/PlayerData")]
public class PlayerSO : ScriptableObject
{
    public float moveSpeed;
    public float rotateSpeed;

    public GameObject shootPrefab;
    public Vector3 shootPosition;
    public float shootSpeed;
}

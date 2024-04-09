using System.Net;
using UnityEngine;

public class MessageManager : MonoBehaviour
{
    private void Start()
    {
        NetworkManager.Instance.OnReceiveEvent += CheckMessageType;
    }

    private void CheckMessageType(byte[] data, IPEndPoint ip)
    {

    }
}

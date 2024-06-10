using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class NetworkScreen : MonoBehaviour
{
    public Button connectBtn;
    public Button startServerBtn;
    public InputField portInputField;
    public InputField addressInputField;
    public InputField playerName;

    public GameObject server;
    public GameObject client;

    private void OnEnable()
    {
        connectBtn.onClick.AddListener(OnConnectBtnClick);
        startServerBtn.onClick.AddListener(OnStartServerBtnClick);
        server.SetActive(false);
        client.SetActive(false);
    }

    private void OnDisable()
    {
        connectBtn.onClick.RemoveListener(OnConnectBtnClick);
        startServerBtn.onClick.RemoveListener(OnStartServerBtnClick);
    }

    void OnConnectBtnClick()
    {
        Debug.Log(portInputField.text);

        client.GetComponent<ClientNetManager>().Initialize(
        System.Convert.ToInt32(portInputField.text),
            IPAddress.Parse(addressInputField.text),
            new Player(playerName.text, -1));
        client.SetActive(true);

        gameObject.SetActive(false);
    }

    void OnStartServerBtnClick()
    {
        server.GetComponent<ServerNetManager>().Initialize(
            System.Convert.ToInt32(portInputField.text),
            IPAddress.Parse(addressInputField.text),
            new Player("server", -1));
        server.SetActive(true);

        gameObject.SetActive(false);
    }
}

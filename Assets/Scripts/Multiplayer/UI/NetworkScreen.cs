using System.Net;
using UnityEditor.Experimental.GraphView;
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

    public ChannelSO<int> switchCanvas;

    private void OnEnable()
    {
        connectBtn.onClick.AddListener(OnConnectBtnClick);
        startServerBtn.onClick.AddListener(OnStartServerBtnClick);
        server.SetActive(false);
        client.SetActive(false);
    }

    void OnConnectBtnClick()
    {
        switchCanvas.RaiseEvent((int)modifyCanvas.loadingScreen);

        LoadingScreen.Instance.SwitchToLoadingScreen(addressInputField, portInputField, playerName);

        client.SetActive(true);
        client.GetComponent<ClientNetManager>().Initialize(
        System.Convert.ToInt32(portInputField.text),
            IPAddress.Parse(addressInputField.text));
    }

    void OnStartServerBtnClick()
    {
        switchCanvas.RaiseEvent((int)modifyCanvas.lobby);

        server.SetActive(true);
        server.GetComponent<ServerNetManager>().Initialize(
            System.Convert.ToInt32(portInputField.text),
            IPAddress.Parse(addressInputField.text),
            new Player("server", -1));
    }
}

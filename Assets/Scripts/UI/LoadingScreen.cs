using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviourSingleton<LoadingScreen>
{
    public Text messages;
    public Button backToMenuBtn;

    IPAddress ipAddress;
    string playerNameString;
    int port;

    protected override void Initialize()
    {
        backToMenuBtn.onClick.AddListener(BackToMenu);

        this.gameObject.SetActive(false);
    }

    public void SwitchToLoadingScreen(InputField addressInputField, InputField portInputField, InputField playerName)
    {
        messages.gameObject.SetActive(false);
        backToMenuBtn.gameObject.SetActive(false);

        ipAddress = IPAddress.Parse(addressInputField.text);
        port = System.Convert.ToInt32(portInputField.text);
        playerNameString = playerName.text;

        NetworkManager.Instance.StartClient(ipAddress, port, playerNameString);
    }

    public void connectToChat()
    {
        Debug.Log("Connect Chat");

        SwitchToChatScreen();
    }

    public void SwitchToChatScreen()
    {
        ChatScreen.Instance.gameObject.SetActive(true);
        this.gameObject.SetActive(false);
    }

    public void ShowBackToMenu()
    {
        messages.gameObject.SetActive(true);
        backToMenuBtn.gameObject.SetActive(true);
    }

    public void BackToMenu()
    {
        NetworkScreen.Instance.gameObject.SetActive(true);
        this.gameObject.SetActive(false);
    }
}

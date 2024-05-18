using UnityEngine.UI;

public class NetworkScreen : MonoBehaviourSingleton<NetworkScreen>
{
    public Button connectBtn;
    public Button startServerBtn;
    public InputField portInputField;
    public InputField addressInputField;
    public InputField playerName;

    protected override void Initialize()
    {
        connectBtn.onClick.AddListener(OnConnectBtnClick);
        startServerBtn.onClick.AddListener(OnStartServerBtnClick);
    }

    void OnConnectBtnClick()
    {
        gameObject.SetActive(false);
        LoadingScreen.Instance.gameObject.SetActive(true);
        LoadingScreen.Instance.SwitchToLoadingScreen(addressInputField, portInputField, playerName);
    }

    void OnStartServerBtnClick()
    {
        gameObject.SetActive(false);
        int port = System.Convert.ToInt32(portInputField.text);
        NetworkManager.Instance.StartServer(port);
        LoadingScreen.Instance.SwitchToChatScreen();
    }

    public void SwitchToNetworkScreen()
    {
        ChatScreen.Instance.gameObject.SetActive(false);
        this.gameObject.SetActive(true);
    }
}

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
        CanvasSwitcher.Instance.SwitchCanvas(modifyCanvas.loadingScreen);
        LoadingScreen.Instance.SwitchToLoadingScreen(addressInputField, portInputField, playerName);
    }

    void OnStartServerBtnClick()
    {
        int port = System.Convert.ToInt32(portInputField.text);
        NetworkManager.Instance.StartServer(port);
        CanvasSwitcher.Instance.SwitchCanvas(modifyCanvas.lobby);
    }
}

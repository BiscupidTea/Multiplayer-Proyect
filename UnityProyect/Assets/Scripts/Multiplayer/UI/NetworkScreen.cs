using UnityEngine;
using UnityEngine.UI;

public class NetworkScreen : MonoBehaviour
{
    public Button connectBtn;
    public InputField playerName;

    public GameObject client;

    private void OnEnable()
    {
        connectBtn.onClick.AddListener(OnConnectBtnClick);
        client.SetActive(false);
    }

    private void OnDisable()
    {
        connectBtn.onClick.RemoveListener(OnConnectBtnClick);
    }

    void OnConnectBtnClick()
    {
        client.SetActive(true);
        client.AddComponent<ClientMonoBehaviour>().OnStartClient(playerName.text);
    }
}

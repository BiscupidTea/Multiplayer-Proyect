using System.Net;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviourSingleton<LoadingScreen>
{
    public Text LoadingMessage;
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
        StartCoroutine(UpdateTimer());
    }

    IEnumerator UpdateTimer()
    {
        yield return new WaitForSeconds(0.7f);
        switch (LoadingMessage.text)
        {
            case "Loading.":
                LoadingMessage.text = "Loading..";
                break;

            case "Loading..":
                LoadingMessage.text = "Loading...";
                break;

            case "Loading...":
                LoadingMessage.text = "Loading.";
                break;
        }

        StartCoroutine(UpdateTimer());
    }

    public void ShowErrorMessage(string errorMessage)
    {
        messages.gameObject.SetActive(true);
        messages.text = errorMessage;
        Debug.LogWarning(errorMessage);
        backToMenuBtn.gameObject.SetActive(true);
    }

    public void BackToMenu()
    {
        CanvasSwitcher.Instance.SwitchCanvas(modifyCanvas.networkScreen);
    }
}

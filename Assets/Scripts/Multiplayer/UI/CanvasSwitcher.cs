using UnityEngine;

public enum modifyCanvas
{
    networkScreen,
    loadingScreen,
    lobby,
    chatScreen,
}

public class CanvasSwitcher : MonoBehaviourSingleton<CanvasSwitcher>
{
    [SerializeField] private GameObject networkScreen;
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject lobby;
    [SerializeField] private GameObject chatScreen;

    public void SwitchCanvas(modifyCanvas canvas)
    {
        switch (canvas)
        {
            case modifyCanvas.networkScreen:
                networkScreen.SetActive(true);
                loadingScreen.SetActive(false);
                lobby.SetActive(false);
                chatScreen.SetActive(false);
                break;

            case modifyCanvas.loadingScreen:
                networkScreen.SetActive(false);
                loadingScreen.SetActive(true);
                lobby.SetActive(false);
                chatScreen.SetActive(false);
                break;

            case modifyCanvas.lobby:
                networkScreen.SetActive(false);
                loadingScreen.SetActive(false);
                lobby.SetActive(true);
                chatScreen.SetActive(false);
                break;

            case modifyCanvas.chatScreen:
                networkScreen.SetActive(false);
                loadingScreen.SetActive(false);
                lobby.SetActive(false);
                chatScreen.SetActive(true);
                break;

        }
    }
}
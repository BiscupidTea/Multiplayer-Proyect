using System;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UI;

public class ChatScreen : MonoBehaviourSingleton<ChatScreen>
{
    public Text messages;
    public InputField inputMessage;

    protected override void Initialize()
    {
        inputMessage.onEndEdit.AddListener(OnEndEdit);

        this.gameObject.SetActive(false);
    }

    public void OnReceiveDataEvent(string message)
    {
        messages.text += message + System.Environment.NewLine;
    }

    public void OnEndEdit(string str)
    {
        if (inputMessage.text != "")
        {
            Debug.Log(str);

            inputMessage.ActivateInputField();
            inputMessage.Select();
            inputMessage.text = "";

            NetworkManager.Instance.();
        }
    }

}

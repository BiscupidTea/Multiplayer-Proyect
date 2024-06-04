using System;
using UnityEngine;
using UnityEngine.UI;

public class ChatScreen : MonoBehaviour
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
            inputMessage.ActivateInputField();
            inputMessage.Select();
            inputMessage.text = "";

            MessageManager.Instance.OnSendConsoleMessage(str);
        }
    }

}

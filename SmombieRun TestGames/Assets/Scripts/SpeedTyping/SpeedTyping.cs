using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Enums.SpeedTypingEnums;
using UnityEngine.InputSystem;

public class SpeedTyping : Game
{
    [Header("SpeedTyping specific")]
    public TextMeshProUGUI MessageText;
    public GameObject SentMessageBoxPrefab, ReceivedMessageBoxPrefab;
    public Transform Content;
    public RectTransform ActiveWindow;
    public Sprite WrongTexture;
    public Button SendButton, TextButton;
    private bool Replied;

    private string FullMessage = "";
    private string RequestedMessage = "";

    private List<string> MessageLibrary = new List<string>()
    {
        "Lorem Ipsum Dolor Sit Amet"
    };

    private void Start()
    {
        
    }

    protected override void OnGameStart()
    {
        RunningGame = StartCoroutine(ProcessGame());
    }

    protected override IEnumerator ProcessGame()
    {
        Header.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(1f);
        Header.gameObject.SetActive(false);
        while (true)
        {
            _ = OnNewMessage(MessageType.RECEIVED);
            SendButton.interactable = true;
            TextButton.interactable = true;
            Replied = false;
            while (!Replied)
            {
                yield return new WaitForEndOfFrame();
            }
            SendButton.interactable = false;
            TextButton.interactable = false;
            yield return new WaitForSeconds(1);
        }
    }

    protected override void OnGameExit()
    {

    }

    private GameObject OnNewMessage(MessageType messageType)
    {
        string NewMessage = "";
        GameObject NewMessageObject = null;

        switch (messageType)
        {
            case MessageType.RECEIVED:
                {
                    RequestedMessage = MessageLibrary[Random.Range(0, MessageLibrary.Count)];
                    NewMessage = RequestedMessage;
                    NewMessageObject = Instantiate(ReceivedMessageBoxPrefab, Content);
                    break;
                }
            case MessageType.SENT:
                {
                    NewMessage = FullMessage != string.Empty ? FullMessage : " ";
                    NewMessageObject = Instantiate(SentMessageBoxPrefab, Content);
                    break;
                }
        }
        
        TextMeshProUGUI MessageText = NewMessageObject.GetComponentInChildren<TextMeshProUGUI>();
        MessageText.text = NewMessage;
        AllObjects.Add(NewMessageObject);
        return NewMessageObject;
    }

    public void AddLetter(string Letter)
    {
        FullMessage += FullMessage == string.Empty ? Letter.ToUpper() : Letter.ToLower();
        UpdateMessageBox();
    }

    public void Backspace()
    {
        if (FullMessage.Length > 0)
        {
            FullMessage = FullMessage.Remove(FullMessage.Length - 1);
            UpdateMessageBox();
        }
    }

    public void Send()
    {
        Replied = true;
        GameObject Message = OnNewMessage(MessageType.SENT);

        // Todo: check if answer was correct
        if (FullMessage != RequestedMessage)
        {
            Message.GetComponent<Image>().sprite = WrongTexture;
            OnStrike();
        }

        ClearMessage();
    }

    private void ClearMessage()
    {
        FullMessage = "";
        UpdateMessageBox();
    }

    private void UpdateMessageBox()
    {
        MessageText.text = FullMessage;
        if (FullMessage == string.Empty)
        {
            MessageText.text = " ";
        }
    }

    public void OpenKeyboard()
    {
        StartCoroutine(CheckKeyboard());
    }

    private IEnumerator CheckKeyboard()
    {
        TouchScreenKeyboard keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);
        while (keyboard.active)
        {
            FullMessage = keyboard.text;
            float Height = ReturnKeyboardHeight();
            ActiveWindow.sizeDelta = new Vector2(873f, Screen.height - Height);
            yield return new WaitForEndOfFrame();
        }
        Debug.Log(FullMessage);
        UpdateMessageBox();
        ActiveWindow.sizeDelta = new Vector2(873f, Screen.height);
    }

    private int ReturnKeyboardHeight()
    {
        using (AndroidJavaClass UnityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject View = UnityClass.GetStatic<AndroidJavaObject>("currentActivity").Get<AndroidJavaObject>("mUnityPlayer").Call<AndroidJavaObject>("getView");

            using (AndroidJavaObject Rct = new AndroidJavaObject("android.graphics.Rect"))
            {
                View.Call("getWindowVisibleDisplayFrame", Rct);

                return Screen.height - Rct.Call<int>("height");
            }
        }
    }
}

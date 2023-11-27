using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Enums.SpeedTypingEnums;
using UnityEngine.InputSystem;
using System.Linq;

public class SpeedTyping : Game
{
    [Header("SpeedTyping specific")]
    public TextMeshProUGUI MessageText;
    public GameObject SentMessageBoxPrefab, ReceivedMessageBoxPrefab;
    public Transform Content, Queue;
    public RectTransform ActiveWindow;
    public Button SendButton, TextButton;
    public float TimePerMessage;
    public int MaxMessages;

    private List<TextMessage> AllMessages = new List<TextMessage>();
    private TextMessage ActiveMessage;
    private int QueueLength;

    private string FullMessage = "";
    private string RequestedMessage = "";
    private int CurrentChar = 0;

    private const string StandardMessage = "<i><color=#939393>Type here...</color></i>";

    private Dictionary<string, string> MessageLibrary = new Dictionary<string, string>()
    {
        {"Answer this with \"OK!\"", "OK!"},
        {"Answer this with \"ok\"", "ok"},
        {"Answer this with \"Yup\"", "Yup"},
        {"Answer this with \"Good\"", "Good"},
    };

    private Coroutine NewMessageCoroutine;

    [Header("Editor specific")]
    public TMP_InputField EditorInputField;
    public bool InputFieldOpen;

    public override bool Started { get; protected set; }

    protected override void OnGameStart()
    {
        ActiveWindow.sizeDelta = new Vector2(873f, Screen.height);
        RunningGame = StartCoroutine(ProcessGame());
    }

    protected override IEnumerator ProcessGame()
    {
        SendButton.interactable = false;
        Header.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(1f);
        Header.gameObject.SetActive(false);
        Started = true;

        AddMessageToQueue(); // Start with one
        int KnownQueueLength = 1; // Will always start at one
        NewMessageCoroutine = StartCoroutine(AddNewMessages());
        while (true)
        {
            // Set active message to most recent, move to the content page
            ActiveMessage = AllMessages[KnownQueueLength - 1];
            ActiveMessage.MessageObject.transform.SetParent(Content, true);
            RequestedMessage = ActiveMessage.Answer;
            ToggleCurrentMessageButton(true);

            // Wait until replied
            while (!ActiveMessage.Replied)
            {
                yield return new WaitForEndOfFrame();
            }

            // Replied to message, check its content
            if (FullMessage != RequestedMessage) // Wrong answer, strike
            {
                OnStrike();
            }
            ClearMessageBox();

            while (KnownQueueLength == QueueLength)
            {
                // Wait for next message available
                yield return new WaitForEndOfFrame();
            }
            // On to the next
            KnownQueueLength = QueueLength;
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void ToggleCurrentMessageButton(bool Enable)
    {
        SendButton.interactable = Enable;
        TextButton.interactable = Enable;
    }

    private void Send()
    {
        ActiveMessage.Replied = true;
        ToggleCurrentMessageButton(false);

        // Add response
        GameObject NewResponse = Instantiate(SentMessageBoxPrefab, Content);
        NewResponse.GetComponentInChildren<TextMeshProUGUI>().text = FullMessage;
        AllObjects.Add(NewResponse);
    }

    private IEnumerator AddNewMessages()
    {
        while (true)
        {
            // Reset the timer for each message
            float currentTimeBetweenMessages = 0;

            // Wait until next message to queue
            while (currentTimeBetweenMessages < TimePerMessage)
            {
                currentTimeBetweenMessages += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            
            // Reached, add new message
            AddMessageToQueue();
            Debug.Log("New question");
            yield return new WaitForEndOfFrame();

            if (QueueLength == MaxMessages)
            {
                break;
            }
        }

        Debug.Log("All questions asked");
    }

    private void AddMessageToQueue()
    {
        KeyValuePair<string, string> NewEntry = MessageLibrary.ElementAt(Random.Range(0 ,MessageLibrary.Count));
        GameObject NewObject = Instantiate(ReceivedMessageBoxPrefab, Queue);
        TextMessage NewTextMessage = NewObject.AddComponent<TextMessage>();
        NewTextMessage.MessageObject = NewObject;
        NewTextMessage.Init(NewEntry.Key, NewEntry.Value);
        AllMessages.Add(NewTextMessage);
        AllObjects.Add(NewObject);
        QueueLength++;
    }

    protected override void OnGameExit()
    {
        if (NewMessageCoroutine != null) StopCoroutine(NewMessageCoroutine);
        ClearMessageBox();
        AllMessages.Clear();
        QueueLength = 0;
    }

    private void ClearMessageBox()
    {
        FullMessage = "";
        UpdateMessageBox();
    }

    private void UpdateMessageBox()
    {
        MessageText.text = FullMessage;
        CurrentChar = FullMessage.Count();
        if (CurrentChar == 0)
        {
            MessageText.text = StandardMessage;
        }
    }

    public void OpenKeyboard()
    {
        if (Application.isEditor && Application.isPlaying)
        {
            StartCoroutine(CheckKeyboardEditor());
        }
        else
        {
            StartCoroutine(CheckKeyboardBuild());
        }
    }

    private IEnumerator CheckKeyboardEditor()
    {
        InputFieldOpen = true;
        EditorInputField.gameObject.SetActive(true);
        SendButton.interactable = false;
        while (InputFieldOpen)
        {
            FullMessage = EditorInputField.text;
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSecondsRealtime(0.1f);
        Debug.Log(FullMessage);
        UpdateMessageBox();
        ActiveWindow.sizeDelta = new Vector2(873f, Screen.height);
        SendButton.interactable = true;
    }

    public void CloseInputField()
    {
        InputFieldOpen = false;
        EditorInputField.gameObject.SetActive(false);
    }

    private IEnumerator CheckKeyboardBuild()
    {
        SendButton.interactable = false;
        TouchScreenKeyboard keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);
        while (keyboard.active)
        {
            FullMessage = keyboard.text;
            float Height = ReturnKeyboardHeight();
            ActiveWindow.sizeDelta = new Vector2(873f, Screen.height - Height);
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSecondsRealtime(0.1f);
        Debug.Log(FullMessage);
        UpdateMessageBox();
        ActiveWindow.sizeDelta = new Vector2(873f, Screen.height);
        SendButton.interactable = true;
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

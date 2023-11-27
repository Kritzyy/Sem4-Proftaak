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
    public Transform Content;
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

    private const string StandardMessage = "<i><color=#939393>Typ hier...</color></i>";

    private Dictionary<string, string> MessageLibrary = new Dictionary<string, string>()
    {
        {"Answer this with \"OK!\"", "OK!"},
        {"Answer this with \"ok\"", "ok"},
        {"Answer this with \"Yup\"", "Yup"},
        {"Answer this with \"Good\"", "Good"},
    };

    private Coroutine NewMessageCoroutine;

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


        AddMessageToQueue(); // Start with one
        int KnownQueueLength = 1; // Will always start at one
        NewMessageCoroutine = StartCoroutine(AddNewMessages());
        while (true)
        {
            // Set active message to most recent
            ActiveMessage = AllMessages[KnownQueueLength - 1];

            // Wait until replied
            while (!ActiveMessage.Replied)
            {
                yield return new WaitForEndOfFrame();
            }

            // Replied to message, check its content
            if (FullMessage != ActiveMessage.Answer) // Wrong answer, strike
            {
                OnStrike();
            }

            while (KnownQueueLength == QueueLength)
            {
                // Wait for next message available
                yield return new WaitForEndOfFrame();
            }
            // On to the next
            yield return new WaitForEndOfFrame();
        }
    }

    private void Send()
    {
        ActiveMessage.Replied = true;
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

            if (QueueLength == MaxMessages)
            {
                break;
            }
            // Reached, add new message
            AddMessageToQueue();
            Debug.Log("New question");
            yield return new WaitForEndOfFrame();
        }

        Debug.Log("All questions asked");
    }

    private void AddMessageToQueue()
    {
        KeyValuePair<string, string> NewEntry = MessageLibrary.ElementAt(Random.Range(0 ,MessageLibrary.Count));
        TextMessage NewTextMessage = Instantiate(ReceivedMessageBoxPrefab, Content).AddComponent<TextMessage>();
        NewTextMessage.Init(NewEntry.Key, NewEntry.Value);
        AllMessages.Add(NewTextMessage);
    }

    protected override void OnGameExit()
    {
        if (NewMessageCoroutine != null) StopCoroutine(NewMessageCoroutine);
    }

    private void UpdateMessageBox()
    {
        MessageText.text = FullMessage;
        if (CurrentChar == 0)
        {
            MessageText.text = StandardMessage;
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
        CurrentChar = FullMessage.Count();
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

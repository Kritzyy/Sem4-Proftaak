using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityTime = UnityEngine.Time;
//using Time = StartTime.Time;
using Random = UnityEngine.Random;
//using StartTime;
using System;
using Unity.Netcode;
using System.Linq;
using Enums.Match;
using UnityEngine.SceneManagement;

public class MatchHandler : NetworkBehaviour
{
    public string PlayerName;

    [Header("Game settings")]
    //public bool MatchStarted;
    public MatchType Match;
    public DifficultyLevel Difficulty;
    private float timer;
    public float TimePerMicrogame;
    private int StartTimeInSeconds;
    private bool Finished;

    [Header("Game list")]
    public List<Game> AllGames;
    public Game CurrentGame;

    [Header("Strike UI")]
    public Sprite Strike;
    public Image[] AllStrikes = new Image[3];
    private int StrikeCount = 0;
    public TextMeshProUGUI DemoStrikeText;

    [Header("Timer UI")]
    public TextMeshProUGUI CountdownText;
    public TextMeshProUGUI TimerText;

    [Header("Game over screen")]
    [SerializeField]
    protected GameOverScreen GameOverScreen;

    [Header("Game Input")]
    public PlayerInput GameInputs;

    [Header("Animator")]
    public Animator Animator;
    public Animator GameOverAnim;
    public float AnimationLength;
    private float AnimMultiplier;

    [Header("Waiting lobby")]
    public Canvas WaitingLobby;

    [Header("Server")]
    public TestServerStuff Network;

    // Misc
    private Coroutine Game;
    private bool DemoMatchStarted;

    private void Start()
    {
        List<TestServerStuff> AllServerObjects = FindObjectsByType<TestServerStuff>(sortMode: FindObjectsSortMode.None).ToList();
        foreach (TestServerStuff serverObject in AllServerObjects)
        {
            if (serverObject.IsOwner)
            {
                Network = FindFirstObjectByType<TestServerStuff>();
                break;
            }
        }

        if (Match == MatchType.RELEASE_GAME) StartCoroutine(WaitingRoom());
    }

    public void SetDifficulty(int Index)
    {
        switch (Index)
        {
            case 0:
                {
                    Difficulty = DifficultyLevel.EASY;
                    break;
                }
            case 1:
                {
                    Difficulty = DifficultyLevel.NORMAL;
                    break;
                }
            case 2:
                {
                    Difficulty = DifficultyLevel.HARD;
                    break;
                }
        }
    }

    #region Playtest scene
    public void StartDemoGame(Game game)
    {
        // Start game
        CurrentGame = game;
        CurrentGame.StartGame();
    }

    public void EndDemoGame()
    {
        CurrentGame.ExitGame();
        StrikeCount = 0;
        DemoStrikeText.text = "x" + StrikeCount.ToString();
    }

    public void ExitDemo()
    {
        if (Match == MatchType.RELEASE_PLAYTEST)
        {
            SceneManager.LoadScene("MenuScene");
        }
        else if (Match == MatchType.DEMO_PLAYTEST)
        {
            SceneManager.LoadScene("DemoMenu");
        }
    }
    #endregion

    #region Waiting room
    private IEnumerator WaitingRoom()
    {
        if (Match == MatchType.RELEASE_GAME)
        {
            // Countdown
            while (true)
            {
                if (!TestServerStuff.StartTime.Value.TimeSet)
                {
                    CountdownText.text = "Waiting for start time set...";
                    yield return new WaitForSeconds(TestServerStuff.RefreshTime);
                    continue;
                }

                DateTime Now = DateTime.Now;
                TimeSpan TimeLeft = TestServerStuff.StartTime.Value.GetDateTime().Subtract(Now);
                StartTimeInSeconds = (TimeLeft.Hours * 3600) + (TimeLeft.Minutes * 60) + TimeLeft.Seconds;
                if (TestServerStuff.MatchStarted)
                {
                    break;
                }
                else if (StartTimeInSeconds > 0)
                {
                    CountdownText.text = TimeLeft.Hours.ToString("00") + ":" + TimeLeft.Minutes.ToString("00") + ":" + TimeLeft.Seconds.ToString("00");
                }
                else if (StartTimeInSeconds <= 0)
                {
                    // Timer reached
                    CountdownText.text = "Waiting for game start...";
                }
                yield return new WaitForEndOfFrame();
            }

            // Start!
            Debug.Log("Game start!");
            StartMatch();
        }
    }

    public void StartMatch()
    {
        WaitingLobby.gameObject.SetActive(false);
        AnimMultiplier = 1 / AnimationLength;
        Animator.SetFloat("AnimSpeed", AnimMultiplier);
        Game = StartCoroutine(HandleGame());
    }
    #endregion

    #region Gameplay
    private IEnumerator HandleGame()
    {
        Animator.SetTrigger("NewGame");
        yield return new WaitForSecondsRealtime(AnimationLength);
        while (!Finished)
        {
            // Reset to start
            Animator.ResetTrigger("NewGame");
            CurrentGame = AllGames[Random.Range(0, AllGames.Count)];
            CurrentGame.StartGame();
            timer = TimePerMicrogame;

            // Wait until game start
            while (!CurrentGame.Started)
            {
                yield return new WaitForEndOfFrame();
            }
            while (timer > 0)
            {
                timer -= UnityTime.deltaTime;
                TimerText.text = timer.ToString("00");
                yield return new WaitForEndOfFrame();
            }
            TimerText.text = "00";
            CurrentGame.ExitGame();
            Animator.SetTrigger("NewGame");
            yield return new WaitForSecondsRealtime(AnimationLength);
        }

        // Todo: add OnFinish
    }

    public void AddStrike()
    {
        StrikeCount++;
        switch (Match)
        {
            case MatchType.DEMO_GAME:
            case MatchType.RELEASE_GAME:
                switch (StrikeCount)
                {
                    case 1:
                        {
                            AllStrikes[0].sprite = Strike;
                            break;
                        }
                    case 2:
                        {
                            AllStrikes[1].sprite = Strike;
                            break;
                        }
                    case 3:
                        {
                            AllStrikes[2].sprite = Strike;
                            GameOver();
                            break;
                        }
                }
                if (Match == MatchType.RELEASE_GAME)
                {
                    Network.UpdateStrikes_ServerRpc(StrikeCount);
                }
                break;
            case MatchType.RELEASE_PLAYTEST:
                DemoStrikeText.text = "x" + StrikeCount.ToString();
                Debug.Log("Playtest strike handed");
                break;
        }
    }

    public void GameOver()
    {
        StopCoroutine(Game);
        TimerText.text = "XX";
        GameInputs.DeactivateInput();
        Animator.Play("GameOver");
        CurrentGame.ExitGame();
    }
    #endregion
}
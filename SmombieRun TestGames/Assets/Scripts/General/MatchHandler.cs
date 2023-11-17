using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MatchHandler : MonoBehaviour
{
    [Header("Game settings")]
    public bool MatchStarted;
    private float timer;
    public float TimePerMicrogame;

    [Header("Game list")]
    public List<Game> AllGames;
    public Game CurrentGame;

    [Header("Strike UI")]
    public Sprite Strike;
    public Image[] AllStrikes = new Image[3];
    private int StrikeCount = 0;

    [Header("Timer UI")]
    public TextMeshProUGUI TimerText;

    [Header("Game over screen")]
    [SerializeField]
    protected GameOverScreen GameOverScreen;

    [Header("Game Input")]
    public PlayerInput GameInputs;

    [Header("Animator")]
    public Animator Animator;
    public float AnimationLength;
    private float AnimMultiplier;

    // Misc
    private Coroutine Match;

    // Start is called before the first frame update
    private void Start()
    {
        TimerText.text = timer.ToString("00.00");
    }
    public void StartMatch()
    {
        MatchStarted = true;
        Match = StartCoroutine(HandleGame());
        AnimMultiplier = 1 / AnimationLength;
        Animator.SetFloat("AnimSpeed", AnimMultiplier);
    }

    private IEnumerator HandleGame()
    {
        while (MatchStarted)
        {
            Animator.ResetTrigger("NewGame");
            CurrentGame = AllGames[Random.Range(0, AllGames.Count)];
            CurrentGame.StartGame();
            timer = TimePerMicrogame;
            while (timer > 0)
            {
                timer -= Time.deltaTime;
                TimerText.text = timer.ToString("00.00");
                yield return new WaitForEndOfFrame();
            }
            TimerText.text = "00.00";
            CurrentGame.ExitGame();
            Animator.SetTrigger("NewGame");
            yield return new WaitForSecondsRealtime(AnimationLength);
        }
    }

    public void AddStrike()
    {
        StrikeCount++;
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
    }

    public void GameOver()
    {
        StopCoroutine(Match);
        TimerText.text = "XX:XX";
        CurrentGame.ExitGame();
        GameInputs.DeactivateInput();
        GameOverScreen.gameObject.SetActive(true);
    }
}

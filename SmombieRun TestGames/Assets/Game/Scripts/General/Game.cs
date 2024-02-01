using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public abstract class Game : MonoBehaviour
{
    [SerializeField]
    protected MatchHandler Match;
    [SerializeField]
    protected TextMeshProUGUI Header;
    [SerializeField]
    protected Image Backdrop;
    [SerializeField]
    protected Sprite BackgroundImage;

    public Coroutine RunningGame;
    protected List<GameObject> AllObjects = new List<GameObject>();
    public abstract bool Started { get; protected set; }

    public void StartGame()
    {
        gameObject.SetActive(true);
        switch (Match.Match)
        {
            case Enums.Match.MatchType.RELEASE_GAME:
            case Enums.Match.MatchType.DEMO_GAME:
                Backdrop.sprite = BackgroundImage;
                break;
        }
        OnGameStart();
    }
    public void ExitGame()
    {
        // Clear objects
        foreach (GameObject item in AllObjects)
        {
            Destroy(item);
        }
        AllObjects.Clear();

        // Game specific Exits
        OnGameExit();
        Backdrop.gameObject.SetActive(true);
        gameObject.SetActive(false);
        StopCoroutine(RunningGame);
        Started = false;
    }

    protected abstract void OnGameStart();
    protected abstract IEnumerator ProcessGame();
    protected abstract void OnGameExit();

    public void OnStrike()
    {
        Match.AddStrike();
    }
}

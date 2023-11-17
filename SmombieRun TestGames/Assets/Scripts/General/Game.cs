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
    public Coroutine RunningGame;
    protected List<GameObject> AllObjects = new List<GameObject>();

    public void StartGame()
    {
        gameObject.SetActive(true);
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
        StopCoroutine(RunningGame);
        gameObject.SetActive(false);
    }
    protected abstract void OnGameStart();
    protected abstract IEnumerator ProcessGame();
    protected abstract void OnGameExit();

    public void OnStrike()
    {
        Match.AddStrike();
    }
}

using Enums.Match.ClayshootingEnums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using System;
using Random = UnityEngine.Random;

public class Clayshooting : Game
{
    [Header("Clayshooting specific")]
    public GameObject Reticle;
    public GameObject ClayModel;
    public Transform BorderObject;
    private InputAction Shoot, Touch;
    public Animator Animator;

    public override bool Started { get; protected set; }

    public float MaxReticleUptime;
    private float ReticleUptime;

    protected override void OnGameStart()
    {
        Shoot = Match.GameInputs.actions["Shoot"];
        Touch = Match.GameInputs.actions["Touch"];
        Touch.performed += Touch_performed;
        Touch.Disable();
        
        RunningGame = StartCoroutine(ProcessGame());
    }

    protected override IEnumerator ProcessGame()
    {
        Header.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(1f);
        Header.gameObject.SetActive(false);
        Touch.Enable();
        Started = true;

        StartCoroutine(DespawnReticle());

        (float, float) MinMaxTimePerClay;
        switch (Match.Difficulty)
        {
            case Enums.Match.DifficultyLevel.EASY:
                MinMaxTimePerClay = (1.75f, 2.25f);
                break;
            case Enums.Match.DifficultyLevel.NORMAL:
                MinMaxTimePerClay = (1.25f, 2f);
                break;
            case Enums.Match.DifficultyLevel.HARD:
                MinMaxTimePerClay = (0.75f, 1.50f);
                break;
            default:
                MinMaxTimePerClay = (0.75f, 2.25f);
                break;
        }

        float TimeAfterLastClay = 0;
        float NewSpawnTime = 0;
        while (true)
        {
            // Spawn new clay pigeon and reset timers
            SpawnClayPigeon();
            TimeAfterLastClay = 0;
            NewSpawnTime = Random.Range(MinMaxTimePerClay.Item1, MinMaxTimePerClay.Item2);
            while (TimeAfterLastClay < NewSpawnTime)
            {
                TimeAfterLastClay += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForEndOfFrame();
        }
    }

    private void Touch_performed(InputAction.CallbackContext obj)
    {
        ReticleUptime = MaxReticleUptime;
        var Tapping = obj.ReadValue<float>();
        if (Tapping == 1)
        {
            Reticle.SetActive(true);
            Animator.Play("Shoot");

            var TapLocation = Shoot.ReadValue<Vector2>();

            Vector3 ShotLocation = Camera.main.ScreenToWorldPoint(TapLocation);
            ShotLocation.z = -1;
            Reticle.transform.position = ShotLocation;
            Collider2D ObjectHit = Physics2D.OverlapPoint(ShotLocation);
            if (ObjectHit != null)
            {
                bool IsClayPigeon = ObjectHit.TryGetComponent(out ClayPigeon HitObject);
                if (IsClayPigeon && !HitObject.Hit)
                {
                    Debug.LogFormat("Hit {0}", HitObject.name);
                    StartCoroutine(HitObject.Destroy());
                }
            }
        }
    }

    private IEnumerator DespawnReticle()
    {
        ReticleUptime = 0;
        while (true)
        {
            ReticleUptime -= Time.deltaTime;
            if (ReticleUptime <= 0)
            {
                ReticleUptime = 0;
                Reticle.SetActive(false);
            }
            yield return new WaitForEndOfFrame();
        }
    }

    private void SpawnClayPigeon()
    {
        GameObject NewObject = Instantiate(ClayModel, BorderObject);
        ClayPigeon ClayPigeon  = NewObject.GetComponent<ClayPigeon>();
        ClayPigeon.OnMiss = new UnityEngine.Events.UnityEvent();
        ClayPigeon.OnMiss.AddListener(OnStrike);

        // Choose spawn side
        SpawnSide spawnSide = (SpawnSide)Random.Range(0, 2);
        float SpawnHeight = Random.Range(-10, 7.5f);
        switch (spawnSide)
        {
            case SpawnSide.LEFT:
                {
                    NewObject.transform.localPosition = new Vector3(-16, SpawnHeight, 0);
                    break;
                }
            case SpawnSide.RIGHT:
                {
                    NewObject.transform.localPosition = new Vector3(16, SpawnHeight, 0);
                    break; 
                }
        }
        AllObjects.Add(ClayPigeon.gameObject);
        ClayPigeon.Launch(spawnSide, this);
    }
    protected override void OnGameExit()
    {
        ReticleUptime = 0;
        Reticle.SetActive(false);
        Touch.performed -= Touch_performed;
        Touch.Disable();
    }
}

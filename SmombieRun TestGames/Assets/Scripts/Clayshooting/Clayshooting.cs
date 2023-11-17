using Enums.ClayshootingEnums;
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
    public GameObject TestReticle;
    public GameObject ClayModel;
    public Transform BorderObject;
    private InputAction Shoot, Touch;

    protected override void OnGameStart()
    {
        Shoot = Match.GameInputs.actions["Shoot"];
        Touch = Match.GameInputs.actions["Touch"];
        Touch.performed += Touch_performed;
        
        RunningGame = StartCoroutine(ProcessGame());
    }

    private void Touch_performed(InputAction.CallbackContext obj)
    {
        var Tapping = obj.ReadValue<float>();
        if (Tapping == 1)
        {
            var TapLocation = Shoot.ReadValue<Vector2>();

            Vector3 ShotLocation = Camera.main.ScreenToWorldPoint(TapLocation);
            ShotLocation.z = -1;
            TestReticle.transform.position = ShotLocation;
            Collider2D HitObject = Physics2D.OverlapPoint(ShotLocation);
            if (HitObject != null)
            {
                Debug.LogFormat("Hit {0}", HitObject.name);
                Destroy(HitObject.gameObject);
            }
        }
    }

    protected override IEnumerator ProcessGame()
    {
        Header.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(1f);
        Header.gameObject.SetActive(false);
        (float, float) MinMaxTimePerClay = (0.75f, 2.25f);
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

    private void SpawnClayPigeon()
    {
        GameObject NewObject = Instantiate(ClayModel, BorderObject);
        ClayPigeon ClayPigeon  = NewObject.GetComponent<ClayPigeon>();
        ClayPigeon.OnMiss = new UnityEngine.Events.UnityEvent();
        ClayPigeon.OnMiss.AddListener(OnStrike);

        // Choose spawn side
        SpawnSide spawnSide = (SpawnSide)Random.Range(0, 2);
        switch (spawnSide)
        {
            case SpawnSide.LEFT:
                {
                    NewObject.transform.localPosition = new Vector3(-16, -10f, 0);
                    break;
                }
            case SpawnSide.RIGHT:
                {
                    NewObject.transform.localPosition = new Vector3(16, -10f, 0);
                    break; 
                }
        }
        AllObjects.Add(ClayPigeon.gameObject);
        ClayPigeon.Launch(spawnSide, this);
    }
    protected override void OnGameExit()
    {
        
    }
}

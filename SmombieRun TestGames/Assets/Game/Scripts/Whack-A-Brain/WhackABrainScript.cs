using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WhackABrainScript : Game
{
    public override bool Started { get; protected set; }

    private InputAction Shoot, Touch;

    private const int RowLength = 2, ColumnLength = 3;
    private WhackZombie[,] ZombieGrid = new WhackZombie[RowLength,ColumnLength];
    public List<WhackZombie> ZombieArray;

    protected override void OnGameStart()
    {
        int Index = 0;
        for (int i = 0; i < RowLength; i++)
        {
            for (int j = 0; j < ColumnLength; j++)
            {
                ZombieGrid[i, j] = ZombieArray[Index];
                Index++;
            }
        }

        Shoot = Match.GameInputs.actions["Shoot"];
        Touch = Match.GameInputs.actions["Touch"];
        Touch.performed += Touch_performed;
        Touch.Disable();

        RunningGame = StartCoroutine(ProcessGame());
    }

    protected override void OnGameExit()
    {
        foreach (WhackZombie zombie in ZombieArray)
        {
            zombie.transform.localScale = Vector3.zero;
            zombie.CanBeHit = false;
            zombie.IsUp = false;
        }
        Touch.performed -= Touch_performed;
        Touch.Disable();
    }

    protected override IEnumerator ProcessGame()
    {
        Header.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(1f);
        Header.gameObject.SetActive(false);
        Touch.Enable();
        Started = true;

        (float, float) TimePerZombie = (1, 2);
        float TimeAfterLastZombie = 0;
        float NewSpawnTime = 0;

        while (true)
        {
            TimeAfterLastZombie = 0;
            NewSpawnTime = Random.Range(TimePerZombie.Item1, TimePerZombie.Item2);
            while (TimeAfterLastZombie < NewSpawnTime)
            {
                TimeAfterLastZombie += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            SpawnZombie();
        }
    }

    private void SpawnZombie()
    {
        (int, int) LocationSpawner = (Random.Range(0, RowLength), Random.Range(0, ColumnLength));
        while (IsZombieUp(LocationSpawner.Item1, LocationSpawner.Item2))
        {
            LocationSpawner = (Random.Range(0, RowLength), Random.Range(0, ColumnLength));
        }
        ZombieGrid[LocationSpawner.Item1, LocationSpawner.Item2].MoveUp();
    }

    private bool IsZombieUp(int Row, int Column)
    {
        if (ZombieGrid[Row,Column].IsUp)
        {
            return true;
        }
        return false;
    }

    private void Touch_performed(InputAction.CallbackContext obj)
    {
        var Tapping = obj.ReadValue<float>();
        if (Tapping == 1)
        {
            var TapLocation = Shoot.ReadValue<Vector2>();

            Vector3 ShotLocation = Camera.main.ScreenToWorldPoint(TapLocation);
            ShotLocation.z = -1;
            Collider2D ObjectHit = Physics2D.OverlapPoint(ShotLocation);
            if (ObjectHit != null)
            {
                bool IsZombie = ObjectHit.TryGetComponent(out WhackZombie HitObject);
                if (IsZombie && HitObject.CanBeHit)
                {
                    Debug.LogFormat("Hit {0}", HitObject.name);
                    HitObject.OnHit();
                }
            }
        }
    }
}
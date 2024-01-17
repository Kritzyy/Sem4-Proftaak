using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using StartTime;
using System;
using Enums.UI;

public class TestServerStuff : NetworkBehaviour
{
    public UIElements UI;

    public static NetworkVariable<TimeClass> StartTime = new NetworkVariable<TimeClass>(new TimeClass(-1,-1,-1,false));
    public static bool MatchStarted;

    public static float RefreshTime = 1;

    private MenuHandler Menu;

    public override void OnNetworkSpawn()
    {
        DontDestroyOnLoad(gameObject);
        if (!IsOwner)
        {
            return;
        }

        if (IsServer)
        {
            UI = GameObject.FindWithTag("UI").GetComponent<UIElements>();
            StartCoroutine(WaitForStartSignal());
        }
        else if (IsClient)
        {
            Debug.Log("IS CLIENT, SWITCH TO NEW SCENE");
            Menu = FindObjectOfType<MenuHandler>();
            Menu.CancelConnectTimeout();
            GoToWaitingRoom();
        }
    }

    private void GoToWaitingRoom()
    {
        SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }

    private IEnumerator WaitForStartSignal()
    {
        while (!StartTime.Value.TimeSet)
        {
            yield return new WaitForEndOfFrame();
        }
        Debug.LogWarningFormat("Initial start time was updated to: {0}:{1}:{2}", StartTime.Value.Hours.ToString("00"), StartTime.Value.Minutes.ToString("00"), StartTime.Value.Seconds.ToString("00"));

        while (true)
        {
            DateTime Now = DateTime.Now;
            DateTime Start = StartTime.Value.GetDateTime();
            float diffInSeconds = (float)(Start - Now).TotalSeconds;
            if (diffInSeconds <= 0)
            {
                // Wait time's up!
                UI.ToggleButton(ButtonType.SAT, false);
                UI.ToggleButton(ButtonType.IMPORT, false);
                break;
            }
            yield return new WaitForEndOfFrame();
        }

        // Start!
        UI.SetStatusText("GAME STARTED");
        UI.SetHostFooter("The game has started now. Start time and player list cannot be changed, and no one can join anymore.");
        SendStartSignal_ClientRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateStrikes_ServerRpc(int Strikes, ServerRpcParams rpcParams = default)
    {
        Debug.Log($"ID {rpcParams.Receive.SenderClientId} has {Strikes} strikes now.");
        UI = GameObject.FindWithTag("UI").GetComponent<UIElements>();
        SetStrikeFromID(rpcParams.Receive.SenderClientId, Strikes);
    }

    public void SetStrikeFromID(ulong ID, int Strikes)
    {
        Entry entry = GetEntry(ID);
        UI.Internal_SetStrike(entry, Strikes);
    }

    public void SetStrikeFromName(string Name, int Strikes)
    {
        Entry entry = GetEntry(Name);
        UI.Internal_SetStrike(entry, Strikes);
    }

    private Entry GetEntry(ulong ID)
    {
        foreach (Entry entry in UI.AllPlayerEntries)
        {
            if (entry.ID == ID)
            {
                return entry;
            }
        }
        return null;
    }

    private Entry GetEntry(string Name)
    {
        foreach (Entry entry in UI.AllPlayerEntries)
        {
            if (entry.Name == Name)
            {
                return entry;
            }
        }
        return null;
    }

    public void UpdateTime(TimeClass newTime)
    {
        StartTime.Value = newTime;
    }

    [ClientRpc]
    public void SendStartSignal_ClientRpc()
    {
        MatchStarted = true;
    }
}

namespace StartTime
{
    [Serializable]
    public struct TimeClass : INetworkSerializable
    {
        public bool TimeSet;
        public int Hours;
        public int Minutes;
        public int Seconds;

        public TimeClass(int hours, int minutes, int seconds, bool timeSet)
        {
            Hours = hours;
            Minutes = minutes;
            Seconds = seconds;
            TimeSet = timeSet;
        }

        public DateTime GetDateTime()
        {
            return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, Hours, Minutes, Seconds);
        }

        public bool IsTimeChanged()
        {
            return (Hours != -1) && (Minutes != -1) && (Seconds != -1);
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Hours);
            serializer.SerializeValue(ref Minutes);
            serializer.SerializeValue(ref Seconds);
            serializer.SerializeValue(ref TimeSet);
        }
    }
}
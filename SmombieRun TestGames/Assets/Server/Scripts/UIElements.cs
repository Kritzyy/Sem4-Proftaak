using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Enums.Server.UI;
using TMPro;
using System;
using ExcelReader_NS;

public class UIElements : MonoBehaviour
{
    [Header("UI views")]
    [SerializeField]
    private GameObject RoomCodeView;
    [SerializeField]
    private GameObject PlayerView;
    [SerializeField]
    private GameObject SATView;
    [SerializeField]
    private GameObject ImportView;

    [Header("Buttons")]
    [SerializeField]
    private Button SATButton;
    [SerializeField]
    private Button ImportButton;

    [Header("Strike elements")]
    public Sprite _strike;
    public static Sprite Strike;
    public Sprite _noStrike;
    public static Sprite NoStrike;

    [Header("Status text")]
    [SerializeField]
    private TextMeshProUGUI StatusTextMesh;
    public TextMeshProUGUI HostAt;
    [SerializeField]
    private TextMeshProUGUI SATLabel;
    [SerializeField]
    private TextMeshProUGUI HostAtFooter;

    [Header("SAT text input")]
    public TMP_InputField StartTimeText;

    [Header("Player entry data")]
    [SerializeField]
    private GameObject PlayerEntryTemplate;
    [SerializeField]
    private Transform PlayersContent;
    public ServerMenuHandler ServerCallback;

    [Header("ExcelReader")]
    public TMP_InputField FilePathBox;

    private void Start()
    {
        Strike = _strike;
        NoStrike = _noStrike;
    }

    #region Entries
    public bool CreateEntry(string Name, string StartNumber, out PlayerEntry Entry)
    {
        if (!FindEntry(StartNumber, out PlayerEntry Found))
        {
            // Entry does not exist, add new
            Entry MenuEntry = Instantiate(PlayerEntryTemplate, PlayersContent).GetComponent<Entry>();
            MenuEntry.gameObject.SetActive(false);
            PlayerEntry New = new PlayerEntry(MenuEntry);

            New.Name = Name;
            New.StartNumber = StartNumber;
            New.SetMenuText();

            Entry = New;
            return true;
        }
        else
        {
            Entry = Found;
            return false;
        }
    }

    public void AssignEntry(PlayerEntry entry, ulong ID)
    {
        entry.ID = ID;
        entry.MenuEntry.IDText.color = Color.black;
        entry.Joined = true;
    }

    public void SetToDisconnected(ulong ID)
    {
        PlayerEntry FoundEntry = null;
        foreach (PlayerEntry entry in ServerCallback.RegisteredPlayers)
        {
            if (entry.ID == ID)
            {
                FoundEntry = entry;
                break;
            }
        }

        if (FoundEntry == null)
        {
            Debug.LogWarningFormat("Could not find player with ID {0} in the list.", ID);
            return;
        }

        FoundEntry.MenuEntry.IDText.color = Color.red;
    }

    public bool FindEntry(string StartNumber, out PlayerEntry Found)
    {
        foreach (PlayerEntry entry in ServerCallback.RegisteredPlayers)
        {
            if (entry.StartNumber == StartNumber)
            {
                Found = entry;
                return true;
            }
        }

        Found = null;
        return false;
    }
    #endregion

    #region Display code
    public void ToggleView(View view, bool SetActive)
    {
        switch (view)
        {
            case View.ROOMCODE:
                RoomCodeView.SetActive(SetActive);
                break;
            case View.SAT:
                SATView.SetActive(SetActive);
                break;
            case View.PLAYER:
                PlayerView.SetActive(SetActive);
                break;
            case View.IMPORT:
                ImportView.SetActive(SetActive);
                break;
        }
    }

    public void SetStatusText(string Text)
    {
        StatusTextMesh.text = "Status: " + Text;
    }

    public bool ToggleButton(ButtonType Type, bool Enable, Color OverrideColor = default)
    {
        Button button;
        switch (Type)
        {
            case ButtonType.SAT:
                button = SATButton;
                break;
            case ButtonType.IMPORT:
                button = ImportButton;
                break;
            default:
                return false;
        }
        button.interactable = Enable;

        if (!Enable)
        {
            button.GetComponentInChildren<TextMeshProUGUI>().color = OverrideColor == default ? Color.red : OverrideColor;
        }
        else
        {
            button.GetComponentInChildren<TextMeshProUGUI>().color = OverrideColor == default ? Color.black : OverrideColor;
        }

        return true;
    }

    public bool ToggleButton(ButtonType Type, bool Enable)
    {
        Button button;
        switch (Type)
        {
            case ButtonType.SAT:
                button = SATButton;
                break;
            case ButtonType.IMPORT:
                button = ImportButton;
                break;
            default:
                return false;
        }
        button.interactable = Enable;

        if (!Enable)
        {
            button.GetComponentInChildren<TextMeshProUGUI>().color = Color.red;
        }
        else
        {
            button.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;
        }

        return true;
    }

    public void UpdateStrikes(PlayerEntry entry)
    {
        entry.MenuEntry.SetStrikes(entry.StrikeCount);
    }

    public void ShowSATError()
    {
        SATLabel.gameObject.SetActive(true);
    }
    public void HideSATError()
    {
        SATLabel.gameObject.SetActive(false);
    }

    public void SetHostFooter(string Text)
    {
        HostAtFooter.text = Text;
    }
    #endregion
}
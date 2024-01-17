using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Enums.UI;
using TMPro;

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

    [Header("ExcelReader")]
    public TMP_InputField FilePathBox;

    public List<Entry> AllPlayerEntries = new List<Entry>();

    private void Start()
    {
        Strike = _strike;
        NoStrike = _noStrike;
    }

    public Entry AddToEntryList(ulong ID, string Name, string StartNumber)
    {
        Entry NewEntry = Instantiate(PlayerEntryTemplate, PlayersContent).GetComponent<Entry>();
        NewEntry.Name = Name;
        NewEntry.StartNumber = StartNumber;
        NewEntry.ID = ID;
        NewEntry.SetText();

        AllPlayerEntries.Add(NewEntry);
        return NewEntry;
    }

    public void RemoveFromEntryList(ulong ID)
    {
        Entry FoundEntry = null;
        foreach (Entry entry in AllPlayerEntries)
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

        Destroy(FoundEntry.gameObject);
        AllPlayerEntries.Remove(FoundEntry);
    }

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

    public void Internal_SetStrike(Entry entry, int Strikes)
    {
        entry.SetStrikes(Strikes);
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
}
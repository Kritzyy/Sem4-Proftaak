using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Entry : MonoBehaviour
{
    [Header("Client properties")]
    public ulong ID;
    public string Name;
    public string StartNumber;
    public int StrikeCount;

    [Header("Entry objects")]
    public TextMeshProUGUI IDText;
    public TextMeshProUGUI StartNumberText;
    public List<Image> Strikes;

    public void SetText()
    {
        IDText.text = Name.ToString();
        StartNumberText.text = StartNumber;
    }

    public void SetStrikes(int NewStrike)
    {
        StrikeCount = NewStrike;
        switch (StrikeCount)
        {
            case 0:
                {
                    Strikes[0].sprite = UIElements.Strike;
                    Strikes[1].sprite = UIElements.NoStrike;
                    Strikes[2].sprite = UIElements.NoStrike;
                    break;
                }
            case 1:
                {
                    Strikes[0].sprite = UIElements.Strike;
                    Strikes[1].sprite = UIElements.NoStrike;
                    Strikes[2].sprite = UIElements.NoStrike;
                    break;
                }
            case 2:
                {
                    Strikes[0].sprite = UIElements.Strike;
                    Strikes[1].sprite = UIElements.Strike;
                    Strikes[2].sprite = UIElements.NoStrike;
                    break;
                }
            case 3:
                {
                    Strikes[0].sprite = UIElements.Strike;
                    Strikes[1].sprite = UIElements.Strike;
                    Strikes[2].sprite = UIElements.Strike;
                    break;
                }
        }
    }
}
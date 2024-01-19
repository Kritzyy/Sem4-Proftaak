using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ExcelReader_NS;

/// <summary>
/// The visible data entry in the PlayerList of the server
/// </summary>
public class Entry : MonoBehaviour
{
    [Header("Entry objects")]
    public TextMeshProUGUI IDText;
    public TextMeshProUGUI StartNumberText;
    public List<Image> Strikes;

    /// <summary>
    /// Sets the name and start number text on the entry. This does not modify <paramref name="Name"/> or <paramref name="StartNumber"/>
    /// </summary>
    /// <param name="Name">The name that shows up on the left of the entry</param>
    /// <param name="StartNumber">The player's start number</param>
    public void SetText(string Name, string StartNumber)
    {
        IDText.text = Name;
        StartNumberText.text = StartNumber;
    }

    /// <summary>
    /// Update the strike count on the entry depending on <paramref name="NewStrike"/>. This does not modify <see cref="PlayerEntry.StrikeCount"/>
    /// </summary>
    /// <param name="NewStrike">The amount of strikes to display on this entry.</param>
    public void SetStrikes(int NewStrike)
    {
        switch (NewStrike)
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
            case > 2:
                {
                    Strikes[0].sprite = UIElements.Strike;
                    Strikes[1].sprite = UIElements.Strike;
                    Strikes[2].sprite = UIElements.Strike;
                    break;
                }
            default:
                {
                    throw new System.ArgumentException("");
                }
        }
    }
}
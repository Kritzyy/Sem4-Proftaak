using Enums.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIElements : MonoBehaviour
{
    [SerializeField]
    private GameObject LobbyView;
    [SerializeField]
    private GameObject PlayerView;
    [SerializeField]
    private GameObject SATView;
    [SerializeField]
    private GameObject StatusView;
    [SerializeField]
    private Button CreateButton;

    public void ToggleView(View view, bool Enable)
    {
        switch (view)
        {
            case View.LOBBY:
                LobbyView.SetActive(Enable);
                break;
            case View.SAT:
                SATView.SetActive(Enable);
                break;
            case View.PLAYER:
                PlayerView.SetActive(Enable);
                break;
            case View.STATUS:
                StatusView.SetActive(Enable);
                break;
            case View.CREATEBUTTON:
                CreateButton.gameObject.SetActive(Enable);
                break;
            default:
                break;
        }
    }
}

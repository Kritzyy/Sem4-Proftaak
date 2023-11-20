using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Client : MonoBehaviour
{
    [Header("Client properties")]
    public string ID;
    public int StrikeCount;
    public TcpClient Socket;
    public NetworkStream Stream;

    [Header("Entry objects")]
    public TextMeshProUGUI StartNumberText;
    public Image[] Strikes;

    public void CreateClient(string id, TcpClient socket, NetworkStream stream)
    {
        ID = id;
        Socket = socket;
        Stream = stream;
    }

    public void UpdateClientView()
    {
        StartNumberText.text = ID;
    }

    public void UpdateStrikeView(int TotalStrikes)
    {
        switch (TotalStrikes)
        {
            case 0:
                {
                    Strikes[0].sprite = Resources.Load("NoStrike") as Sprite;
                    Strikes[1].sprite = Resources.Load("NoStrike") as Sprite;
                    Strikes[2].sprite = Resources.Load("NoStrike") as Sprite;
                    break;
                }
            case 1:
                {
                    Strikes[0].sprite = Resources.Load("Strike") as Sprite;
                    Strikes[1].sprite = Resources.Load("NoStrike") as Sprite;
                    Strikes[2].sprite = Resources.Load("NoStrike") as Sprite;
                    break;
                }
            case 2:
                {
                    Strikes[0].sprite = Resources.Load("Strike") as Sprite;
                    Strikes[1].sprite = Resources.Load("Strike") as Sprite;
                    Strikes[2].sprite = Resources.Load("NoStrike") as Sprite;
                    break;
                }
            case 3:
                {
                    Strikes[0].sprite = Resources.Load("Strike") as Sprite;
                    Strikes[1].sprite = Resources.Load("Strike") as Sprite;
                    Strikes[2].sprite = Resources.Load("Strike") as Sprite;
                    break;
                }
        }
    }
}

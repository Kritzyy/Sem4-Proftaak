using System.Collections;
using TMPro;
using UnityEngine;

public class TextMessage : MonoBehaviour
{
    public GameObject MessageObject;
    public string Text;
    public string Answer;
    public TextMeshProUGUI MessageTextMesh;
    public bool Replied;

    public void Init(string TextMessage, string answer)
    {
        Text = TextMessage;
        Answer = answer;
        MessageTextMesh = GetComponentInChildren<TextMeshProUGUI>();
        MessageTextMesh.text = Text;
    }
}
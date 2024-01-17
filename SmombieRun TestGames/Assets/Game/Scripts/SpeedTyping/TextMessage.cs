using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextMessage : MonoBehaviour
{
    public GameObject MessageObject;
    public string Text;
    public List<string> Answers;
    public TextMeshProUGUI MessageTextMesh;
    public bool Replied;

    public void Init(string TextMessage, List<string> answers)
    {
        Text = TextMessage;
        Answers = answers;
        MessageTextMesh = GetComponentInChildren<TextMeshProUGUI>();
        MessageTextMesh.text = Text;
    }
}
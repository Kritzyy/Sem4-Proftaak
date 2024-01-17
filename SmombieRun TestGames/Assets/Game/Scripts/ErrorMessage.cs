using System.Collections;
using TMPro;
using UnityEngine;

public class ErrorMessage : MonoBehaviour
{
    [Header("Main canvas")]
    [SerializeField]
    private Canvas MainCanvas;

    [Header("Text fields")]
    [SerializeField]
    private TextMeshProUGUI ErrorTextMesh;

    private string ErrorText;

    public ErrorMessage SetText(string Text)
    {
        ErrorText = "<u>Error message:</u>\n\n" + Text;
        return this;
    }

    public void ShowError()
    {
        MainCanvas.gameObject.SetActive(true);
        ErrorTextMesh.text = ErrorText;
    }

    public void HideError()
    {
        MainCanvas.gameObject.SetActive(false);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using EditLayout = UnityEditor.EditorGUILayout;
using static UnityEngine.GraphicsBuffer;
using Unity.Netcode;

[CustomEditor(typeof(MenuHandler))]
public class MenuEditor : Editor
{
    SerializedProperty ConnectButton;
    SerializedProperty DemoButton;
    SerializedProperty ErrorExitButton;
    SerializedProperty RoomCode;
    SerializedProperty StartNumber;
    SerializedProperty IP;
    SerializedProperty ErrorCanvas;

    private void OnEnable()
    {
        ConnectButton = serializedObject.FindProperty("ConnectButton");
        DemoButton = serializedObject.FindProperty("DemoButton");
        ErrorExitButton = serializedObject.FindProperty("ErrorExitButton");
        RoomCode = serializedObject.FindProperty("RoomCode");
        StartNumber = serializedObject.FindProperty("StartNumber");
        IP = serializedObject.FindProperty("IP");
        ErrorCanvas = serializedObject.FindProperty("Error");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditLayout.LabelField("Menu text fields", EditorStyles.boldLabel);
        EditLayout.PropertyField(RoomCode);
        EditLayout.PropertyField(StartNumber);
        EditLayout.Space(5);

        EditLayout.LabelField("Buttons", EditorStyles.boldLabel);
        EditLayout.PropertyField(ConnectButton);
        EditLayout.PropertyField(DemoButton);
        EditLayout.PropertyField(ErrorExitButton);
        EditLayout.Space(5);
        
        EditLayout.LabelField("Error UI", EditorStyles.boldLabel);
        EditLayout.PropertyField(ErrorCanvas);
        EditLayout.Space(5);

        serializedObject.ApplyModifiedProperties();
        //base.OnInspectorGUI();
    }
}

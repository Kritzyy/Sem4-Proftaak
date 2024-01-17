using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using EditLayout = UnityEditor.EditorGUILayout;
using static UnityEngine.GraphicsBuffer;
using Unity.Netcode;

[CustomEditor(typeof(ServerMenuHandler))]
public class ServerMenuEditor : Editor
{
    SerializedProperty UI;
    SerializedProperty RegisteredPlayers;
    SerializedProperty Server;
    SerializedProperty ServerType;

    private void OnEnable()
    {
        UI = serializedObject.FindProperty("UI");
        RegisteredPlayers = serializedObject.FindProperty("RegisteredPlayers");
        Server = serializedObject.FindProperty("Server");
        ServerType = serializedObject.FindProperty("ServerType");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditLayout.LabelField("Server", EditorStyles.boldLabel);
        EditLayout.PropertyField(Server);
        EditLayout.PropertyField(ServerType);
        EditLayout.PropertyField(UI);
        EditLayout.Space(5);

        EditLayout.LabelField("Registered player list", EditorStyles.boldLabel);
        EditLayout.PropertyField(RegisteredPlayers);
        EditLayout.Space(5);

        serializedObject.ApplyModifiedProperties();
        //base.OnInspectorGUI();
    }
}

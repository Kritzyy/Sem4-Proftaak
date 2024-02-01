using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuHandler : MonoBehaviour
{
    public Button ConnectButton, ErrorExitButton, DemoButton;

    protected string IP;
    [SerializeField]
    protected TMP_InputField RoomCode, StartNumber;
    public UnityTransport transport;

    public ErrorMessage Error;

    protected ActionDelay ConnectAction;

    public GameObject EventSystem;

    private NetworkManager m_NetworkManager;

    private void Start()
    {
        // If connection timed out, play this
        Action ErrorEvent = () =>
        {
            Debug.LogError("Could not connect to server because it could not be found on this network");
            Error.SetText("Could not establish connection to server. Make sure you are connected to the same network.").ShowError();
            ConnectButton.interactable = true;
            DemoButton.interactable = true;
        };
        ConnectAction = new ActionDelay(ErrorEvent);

        // Get objects from Object Scene
        if (FindAnyObjectByType(typeof(EventSystem)) == null)
        {
            Debug.Log("No Objects found yet, loading objects.");
            SceneManager.LoadScene("ObjectsScene", LoadSceneMode.Additive);
        }

        // Delay get Network Manager for disconnect reason
        Action NetworkManagerAction = () =>
        {
            m_NetworkManager = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkManager>();
            if (m_NetworkManager != null)
            {
                m_NetworkManager.OnClientDisconnectCallback += OnClientDisconnectCallback;
            }
        };
        ActionDelay GetNetworkManager = new ActionDelay(NetworkManagerAction);
        StartCoroutine(GetNetworkManager.WaitThenDoAction(1f));
    }

    public void Connect()
    {
        if (!SetIpAddress(RoomCode.text, IsServer: false)) // Set the Ip to the above address
        {
            Debug.LogError("Could not connect to server because the room code was invalid");
            Error.SetText("The room code is invalid.").ShowError();
            ConnectButton.interactable = true;
            DemoButton.interactable = true;
            return;
        }

        ConnectButton.interactable = false;
        DemoButton.interactable = false;
        Debug.LogFormat("connect at IP: {0}", transport.ConnectionData.Address);
        Debug.LogFormat("Connect at port: {0}", transport.ConnectionData.Port);
        Debug.LogFormat("Connect at ListenAddress: {0}", transport.ConnectionData.ServerListenAddress);

        float SecondsPerAttempt = transport.ConnectTimeoutMS / 1000.00f;
        float maxConnectAttempts = transport.MaxConnectAttempts;
        float WaitTime = SecondsPerAttempt * maxConnectAttempts;

        NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes(StartNumber.text);
        NetworkManager.Singleton.StartClient();


        Debug.LogWarning("WAITING");
        StartCoroutine(ConnectAction.WaitThenDoAction(WaitTime));
    }

    /// <summary>
    /// Set the IP address that will be used to connect to the server
    /// </summary>
    /// <param name="Code">The room code</param>
    /// <param name="IsServer">The parameter used to distinguish <see cref="MenuHandler"/> from <see cref="ServerMenuHandler"/></param>
    /// <returns><see langword="true"/> if the room code is valid, otherwise <see langword="false"/></returns>
    public bool SetIpAddress(string Code, bool IsServer)
    {
        if (!IsServer)
        {
            if (!Regex.IsMatch(Code, @"[0-9]{1,}[.][0-9]{1,}"))
            {
                Debug.LogError("Given room code format is incorrect!");
                return false;
            }
            IP = "192.168." + Code;
        }
        else
        {
            IP = Code;
        }
        transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(IP, 7777, "0.0.0.0");
        Debug.LogFormat("Set IP to: {0}", transport.ConnectionData.Address);
        Debug.LogFormat("Set port to: {0}", transport.ConnectionData.Port);
        Debug.LogFormat("Set ListenAddress to: {0}", transport.ConnectionData.ServerListenAddress);
        return true;
    }

    /// <summary>
    /// If connection is successfull, cancel the timeout delay.
    /// </summary>
    public void CancelConnectTimeout()
    {
        ConnectAction.Cancel();
    }

    public void SwitchToDemo()
    {
        SceneManager.LoadScene("PlaytestScene");
    }

    /// <summary>
    /// Error handling if the connection was unsuccessfull
    /// </summary>
    /// <param name="ID">The ID of the player</param>
    private void OnClientDisconnectCallback(ulong ID)
    {
        Debug.Log("Disconnect");
        ConnectAction.Cancel();
        if (!m_NetworkManager.IsServer && m_NetworkManager.DisconnectReason != string.Empty)
        {
            if (m_NetworkManager.DisconnectReason == "StartCodeFormatError")
            {
                Debug.LogError("Could not connect to server because the room code was not correctly formatted.");
                Error.SetText($"The start number {StartNumber.text} is invalid. Check if you've entered the correct number.").ShowError();
            }
            else if (m_NetworkManager.DisconnectReason == "StartCodeNotRecognized")
            {
                Debug.LogError("Could not connect to server because the room code was not recognized as a valid player.");
                Error.SetText($"The start number {StartNumber.text} could not be found. Contact a staff member if you think this is incorrect.").ShowError();
            }
            else
            {
                Debug.Log($"Error message: {m_NetworkManager.DisconnectReason}");
            }
        }
        else
        {
            Debug.Log("No error message given");
        }

        ConnectButton.interactable = true;
        DemoButton.interactable = true;
    }
}

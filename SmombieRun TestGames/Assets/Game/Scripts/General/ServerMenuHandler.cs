using StartTime;
using System;
using System.Net.Sockets;
using System.Net;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using System.Text;
using Enums.Server.UI;
using ExcelReader_NS;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class ServerMenuHandler : MenuHandler
{
    public static TimeClass startTime_Static = new TimeClass(-1, -1, -1, false);
   
    public UIElements UI;

    public List<PlayerEntry> RegisteredPlayers;
    private ExcelReader ExcelReader = new ExcelReader();

    public TestServerStuff Server;

    [SerializeField]
    private ServerType ServerType;

    /// <summary>
    /// DO NOT MODIFY THIS. This is suppressing <see cref="MenuHandler.Connect"/> for the server. Do not call
    /// </summary>
    public new void Connect()
    {
        Debug.LogError("You're not the client!");
        return;
    }

    void Start()
    {
        UI.SetStatusText("NO GAME");
        IP = GetLocalIPAddress();
        UI.SetHostFooter("To create a game, press the green button above here. Make sure you're connected to a wifi point.");
        
        // Get objects from Object Scene
        if (FindAnyObjectByType(typeof(EventSystem)) == null)
        {
            Debug.Log("No Objects found yet, loading objects.");
            SceneManager.LoadScene("ObjectsScene", LoadSceneMode.Additive);
        }
    }

    #region Import Excel
    /// <summary>
    /// Used by the ImportButton to call <see cref="ExcelReader.GetListFromExcel(string)"/> and save the list in <see cref="RegisteredPlayers"/>
    /// </summary>
    public void ImportStartList()
    {
        try
        {
            UI.ToggleButton(ButtonType.IMPORT, false, Color.green);
            RegisteredPlayers = ExcelReader.GetListFromExcel(@UI.FilePathBox.text);
            Debug.LogFormat("List of start numbers has been updated successfully");
        }
        catch (Exception Error)
        {
            Debug.LogErrorFormat("An error has occurred\n\nException: {1}: {0}", Error.Message, Error.GetType().Name);
            UI.ToggleButton(ButtonType.IMPORT, true, default);
        }
    }
    #endregion

    #region Server creation
    public void OpenHost()
    {
        this.AsHost().CreateLobby();
    }

    public void OpenServer()
    {
        this.AsServer().CreateLobby();
    }

    private ServerMenuHandler AsServer()
    {
        ServerType = ServerType.SERVER;
        return this;
    }

    private ServerMenuHandler AsHost()
    {
        ServerType = ServerType.HOST;
        return this;
    }

    private void CreateLobby()
    {
        // Get time
        if (UI.StartTimeText == null)
        {
            Debug.LogError("You forgot to assign the Start Time Text Mesh!");
            return;
        }

        // Settings
        NetworkManager.Singleton.ConnectionApprovalCallback = NewClientApproval;
        NetworkManager.Singleton.OnClientConnectedCallback += Server_ClientConnect;
        NetworkManager.Singleton.OnClientDisconnectCallback += Server_ClientDisconnect;

        // Start
        SetIpAddress(IP, IsServer: true);

        switch (ServerType)
        {
            case ServerType.SERVER: // Server
                NetworkManager.Singleton.StartServer();
                break;
            case ServerType.HOST: // Host
                NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes("0000");
                NetworkManager.Singleton.StartHost();
                break;
            default:
                break;
        }

        UI.HostAt.text = GetRoomCodeFromIP(IP.ToString());
        UI.SetHostFooter("To join this match, connect to the smombierun network, open the app, and enter the room code and start ID.");

        // Toggle UI
        //UI.ToggleView(View.ROOMCODE, true);
        UI.ToggleView(View.PLAYER, true);
        UI.ToggleView(View.SAT, true);
        UI.ToggleView(View.IMPORT, true);
        UI.SetStatusText("GAME NOT RUNNING");

        // Debug
        Debug.LogFormat("Server at IP: {0}", transport.ConnectionData.Address);
        Debug.LogFormat("Server at port: {0}", transport.ConnectionData.Port);
        Debug.LogFormat("Server at ListenAddress: {0}", transport.ConnectionData.ServerListenAddress);

        List<TestServerStuff> AllServerObjects = FindObjectsByType<TestServerStuff>(sortMode: FindObjectsSortMode.None).ToList();
        foreach (TestServerStuff serverObject in AllServerObjects)
        {
            if (serverObject.IsOwner)
            {
                Server = FindFirstObjectByType<TestServerStuff>();
                break;
            }
        }
    }
    #endregion

    #region Server settings
    public void StartAtTime()
    {
        if (!SetTime())
        {
            Debug.LogErrorFormat("Game could not be started! Time set was invalid");
            return;
        }
        UI.SetStatusText("WAITING FOR START");
    }

    public bool SetTime()
    {
        bool Check = CheckTimeText(UI.StartTimeText.text, out TimeClass startTime);
        if (!Check)
        {
            Debug.LogErrorFormat("The given time format of {0} is invalid!", UI.StartTimeText.text);
            return false;
        }

        // Set static start time for server
        startTime_Static = startTime;
        startTime_Static.TimeSet = true;
        Debug.Log("Time sucessfully set.");
        return true;
    }

    public string GetRoomCodeFromIP(string IP)
    {
        if (!IP.Contains("192.168."))
        {
            Debug.LogError("Game is not hosted on a private network! Cannot create code!");
            throw new Exception("IP address is invalid.");
        }
        return IP.Replace("192.168.", "");
    }

    public bool CheckTimeText(string In, out TimeClass startTime)
    {
        bool T = DateTime.TryParse(In, out DateTime newTime);
        if (!T)
        {
            Debug.LogError("The given time was not in a valid format!");
            startTime = new TimeClass(-1, -1, -1, false);
            UI.ShowSATError();
            return false;
        }

        UI.HideSATError();
        TimeClass newTimeClass = new TimeClass(newTime.Hour, newTime.Minute, newTime.Second, true);
        startTime = newTimeClass;
        Server.UpdateTime(startTime);
        return true;
    }

    public string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new System.Exception("No network adapters with an IPv4 address in the system!");
    }
    #endregion

    #region Handle connections
    private void NewClientApproval(NetworkManager.ConnectionApprovalRequest Request, NetworkManager.ConnectionApprovalResponse Response)
    {
        ulong ID = Request.ClientNetworkId;
        var T = Request.Payload;
        string StartNumber = Encoding.ASCII.GetString(T);
        Response.CreatePlayerObject = true;
        
        if (StartNumber.Length != 4 || !IsDigitsOnly(StartNumber))
        {
            Debug.LogWarningFormat("A request for number {0} was denied because it did not meet standards", StartNumber);
            Response.Reason = "StartCodeFormatError";
            Response.Approved = false;
            Response.Pending = false;
            return;
        }
        else if (IsNameRegistered(StartNumber, out PlayerEntry Entry))
        {
            Debug.LogWarningFormat("A request for ID {0} was approved", Entry.Name);
            Entry.MenuEntry.gameObject.SetActive(true);
            if (Entry.Joined && TestServerStuff.MatchStarted)
            {
                JoinLate(Entry);
            }
            UI.AssignEntry(Entry, ID);
            Response.Approved = true;
            Response.Pending = false;
        }
        else
        {
            Debug.LogWarningFormat("A request for number {0} was denied because it wasn't found in the list", StartNumber);
            Response.Reason = "StartCodeNotRecognized";
            Response.Approved = false;
            Response.Pending = false;
            return;
        }
    }

    bool IsDigitsOnly(string str)
    {
        foreach (char c in str)
        {
            if (c < '0' || c > '9')
                return false;
        }

        return true;
    }

    private bool IsNameRegistered(string StartNumber, out PlayerEntry Entry)
    {
        foreach (PlayerEntry entry in RegisteredPlayers)
        {
            if (entry.StartNumber == StartNumber)
            {
                Entry = entry;
                return true;
            }
        }
        Entry = null;
        return false;
    }

    public void JoinLate(PlayerEntry Entry, ServerRpcParams rpcParams = default)
    {
        foreach (PlayerEntry ListEntry in RegisteredPlayers)
        {
            if (Entry == ListEntry)
            {
                Entry.MenuEntry.SetText(Entry.Name + " {!}", Entry.StartNumber);
                return;
            }
        }
    }
    #endregion

    #region Server triggers
    private void Server_ClientConnect(ulong ID)
    {
        Debug.LogFormat("Device with ID {0} connected", ID);
    }
    private void Server_ClientDisconnect(ulong ID)
    {
        Debug.LogFormat("Device with ID {0} disconnected", ID);
        UI.SetToDisconnected(ID);
    }
    #endregion
}

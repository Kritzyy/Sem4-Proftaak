using StartTime;
using System;
using System.Net.Sockets;
using System.Net;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using System.Text;
using Enums.UI;
using ExcelReader_NS;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class ServerMenuHandler : MenuHandler
{
    public static TimeClass startTime_Static = new TimeClass(-1, -1, -1, false);
   
    public UIElements UI;

    public List<PlayerEntry> RegisteredPlayers;
    private ExcelReader ExcelReader = new ExcelReader();

    public TestServerStuff Server;

    [SerializeField]
    private ServerType ServerType;

    void Start()
    {
        UI.SetStatusText("NO GAME");
        IP = GetLocalIPAddress();
        UI.SetHostFooter("To create a game, press the green button above here. Make sure you're connected to a wifi point.");

        SceneManager.LoadScene("ObjectsScene", LoadSceneMode.Additive);
    }

    public new void Connect()
    {
        Debug.LogError("You're not the client!");
        return;
    }

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
        }
        return IP.Replace("192.168.", "");
    }

    private void NewClientApproval(NetworkManager.ConnectionApprovalRequest Request, NetworkManager.ConnectionApprovalResponse Response)
    {
        ulong ID = Request.ClientNetworkId;
        var T = Request.Payload;
        string Message = Encoding.ASCII.GetString(T);
        Response.CreatePlayerObject = true;
        
        if (Message.Length != 4 || !IsDigitsOnly(Message))
        {
            Debug.LogWarningFormat("A request for number {0} was denied because it did not meet standards", Message);
            Response.Reason = "StartCodeFormatError";
            Response.Approved = false;
            Response.Pending = false;
            return;
        }
        else if (IsNameRegistered(Message, out PlayerEntry Entry))
        {
            Debug.LogWarningFormat("A request for ID {0} was approved", Entry.Name);
            UI.AddToEntryList(ID, Entry.Name, Message);
            Response.Approved = true;
            Response.Pending = false;
        }
        else
        {
            Debug.LogWarningFormat("A request for number {0} was denied because it wasn't found in the list", Message);
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

    //[ServerRpc]
    //public void UpdateStrikes_ServerRpc(ulong ID, int Strikes)
    //{

    //}

    private void Server_ClientConnect(ulong ID)
    {
        Debug.LogFormat("Device with ID {0} connected", ID);
    }
    private void Server_ClientDisconnect(ulong ID)
    {
        Debug.LogFormat("Device with ID {0} disconnected", ID);
        UI.RemoveFromEntryList(ID);
    }

}

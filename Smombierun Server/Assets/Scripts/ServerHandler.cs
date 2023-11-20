using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

using Enums.UI;
using System.Text;
using System;
using Unity.VisualScripting;
using IP = System.Net.IPAddress;

public class ServerHandler : MonoBehaviour
{
    // Server
    private IP IP;
    private TcpListener Server;
    private int Port;
    private bool Started = false;
    private Coroutine ListenCoroutine;

    [Header("Settings")]
    public GameObject DefaultClientBox;
    public Transform ClientContent;
    public float MaxTimeoutTimer = 5f;
    public char SOL;
    public char EOL;

    [Header("Client list")]
    public List<Client> AllClients = new List<Client>();

    [Header("Handlers")]
    [SerializeField]
    private UIElements UI;

    private void Start()
    {
        IP = IP.Any;
        Port = 8888;
        Server = new TcpListener(IP, Port);
    }

    public void CreateServer()
    {
        try // open server
        {
            Server.Start();
            Started = true;
        }
        catch (SocketException Ex)
        {
            // Failed
            Debug.LogErrorFormat("Could not start server. Error: {0}", Ex.Message);
            return;
        }

        // Succeeded
        Debug.LogFormat("Server successfully started at {0}:{1}", IP, Port);

        UI.ToggleView(View.LOBBY, true);
        UI.ToggleView(View.STATUS, true);
        UI.ToggleView(View.PLAYER, true);
        UI.ToggleView(View.SAT, true);

        UI.ToggleView(View.CREATEBUTTON, false);

        ListenCoroutine = StartCoroutine(CheckConnections());
    }

    private IEnumerator CheckConnections()
    {
        while (true)
        {
            float Timer = MaxTimeoutTimer;
            bool Fail = false;

            while (!Server.Pending())
            {
                yield return new WaitForEndOfFrame();
            }

            // New client
            TcpClient TestClient = Server.AcceptTcpClient();
            NetworkStream TestStream = TestClient.GetStream();

            // Handle check
            while (TestClient.Available <= 0 && !Fail)
            {
                Timer -= Time.deltaTime;
                if (Timer <= 0)
                {
                    Fail = true;
                }
                yield return new WaitForEndOfFrame();
            }

            // If failed, drop communication and wait for next.
            if (Fail) 
            {
                continue;
            }

            // Todo: Add protocol check
            string StringIN = "";
            for (int i = 0; i < TestClient.Available; i++)
            {
                byte ByteIN = (byte)TestStream.ReadByte();
                char CharIN = Convert.ToChar(ByteIN);
                if (i == 0 && CharIN != SOL)
                {
                    Fail = true;
                    break;
                }
                else if (CharIN == EOL)
                {
                    break;
                }
                else
                {
                    StringIN += CharIN;
                }
            }

            // Checks passed. Create new client
            Client client = Instantiate(DefaultClientBox, ClientContent).GetComponent<Client>();
            client.CreateClient(StringIN, TestClient, TestStream);
            AllClients.Add(client);
            UpdateClientView(client);

            yield return new WaitForEndOfFrame();
        }
    }

    private void UpdateClientView(Client client)
    {
        client.UpdateClientView();
    }

    private void OnApplicationQuit()
    {
        if (Started)
        {
            StopCoroutine(ListenCoroutine);
            Server.Stop();
        }
    }
}

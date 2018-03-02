using UnityEngine;
using System.Net.Sockets;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;


public struct Parameters
{
    public string AcademyName;
    public string apiNumber;
    public string logPath;
    public List<string> brainNames;
    public List<string> externalBrainNames;
}


public class ConnectPy : MonoBehaviour
{
    const string api = "API-2";
    Socket sender;
    byte[] messageHolder;
    const int messageLength = 12000;

    bool init = false;
    int index = 0;
    void OnGUI()
    {
        if (GUI.Button(new Rect(20, 20, 120, 60), "CONNECT"))
        {
            if (!init)
                Initial();
            else
                Debug.Log("Socket has Initial");
        }
        if (GUI.Button(new Rect(20, 130, 120, 60), "HELLO"))
        {
            SendString("hello world " + index++);
        }
        if (GUI.Button(new Rect(20, 240, 120, 60), "ACTION"))
        {
            SendString("ACTION");
            Receive();
        }
        if (GUI.Button(new Rect(20, 350, 120, 60), "EXIT"))
        {
            SendString("EXIT");
        }
    }


    void Initial()
    {
        init = true;
        messageHolder = new byte[messageLength];

        // Create a TCP/IP  socket
        sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        sender.Connect("localhost", 5006);
        Parameters paramerters = new Parameters();
        paramerters.brainNames = new List<string>();
        paramerters.brainNames.Add("hello world");
        paramerters.brainNames.Add("test job");
        paramerters.externalBrainNames = new List<string>();
        paramerters.externalBrainNames.Add("externalBrainNames");
        paramerters.apiNumber = api;
        paramerters.logPath = "/tmp/unity.log";
        paramerters.AcademyName = gameObject.name;
        SendParameters(paramerters);
        Debug.Log("***   socket is init   ***");
    }


    private void SendParameters(Parameters envParams)
    {
        string envMessage = JsonConvert.SerializeObject(envParams, Formatting.Indented);
        Debug.Log(envMessage);
        sender.Send(Encoding.ASCII.GetBytes(envMessage));
    }
    
    private void SendString(string str)
    {
        try
        {
            Debug.Log("send:" + str);
            sender.Send(AppendLength(Encoding.ASCII.GetBytes(str)));
        }
        catch(SocketException e)
        {
            Debug.LogWarning(e.Message);
        }
    }

    private byte[] AppendLength(byte[] input)
    {
        byte[] newArray = new byte[input.Length + 4];
        input.CopyTo(newArray, 4);
        System.BitConverter.GetBytes(input.Length).CopyTo(newArray, 0);
        return newArray;
    }

    private void Receive()
    {
        int location = sender.Receive(messageHolder);
        string message = Encoding.ASCII.GetString(messageHolder, 0, location);
        Debug.Log("recv: "+message);
    }

    private void OnApplicationQuit()
    {
        Debug.Log("Socket is closing");
        sender.Close();
    }

}

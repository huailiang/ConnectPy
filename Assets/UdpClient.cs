using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;



public class UdpClient : MonoBehaviour
{
    private static Socket client;
    private static IPAddress ipaddr;
    public static BlockingCollection<string> queue = new BlockingCollection<string>();
    public static bool enable = false;

    public void Start()
    {
        enable = true;
        ipaddr = IPAddress.Parse("127.0.0.1");
        client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        client.Bind(new IPEndPoint(ipaddr, 8900));
        Thread threadReciveMsg = new Thread(ReciveMsg);
        threadReciveMsg.Start();
    }


    private void OnGUI()
    {
        if (GUI.Button(new Rect(50,40,100,80),"SEND"))
        {
            SendMsg("HELLO WORLD C#");
        }
    }

    private void OnDestroy()
    {
        try
        {
            client.Disconnect(false);
            client.Dispose();
        }
        catch (SocketException e)
        {
            Debug.Log(e);
        }
    }

    public void SendMsg(string msg)
    {
        EndPoint point = new IPEndPoint(ipaddr, 8901);
        client.SendTo(Encoding.UTF8.GetBytes(msg), point);
    }

    public void Disable()
    {
        if (client != null)
        {
            client.Dispose();
        }
        enable = false;
    }



    void ReciveMsg()
    {
        while (enable)
        {
            EndPoint point = new IPEndPoint(IPAddress.Any, 0); //用来保存发送方的ip和端口号
            byte[] buffer = new byte[10240];
            int length = client.ReceiveFrom(buffer, ref point); //接收数据报
            string message = Encoding.UTF8.GetString(buffer, 0, length); //将接收到的数据转换成字符串类型
            Debug.Log(DateTime.Now + " Msg From: " + point + ":" + message); //控制台打印出来
            if (queue.Count < 120) queue.Add(message);
        }
    }
}
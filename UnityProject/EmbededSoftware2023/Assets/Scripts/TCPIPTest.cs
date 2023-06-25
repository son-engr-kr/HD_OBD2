using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class TCPIPTest : MonoBehaviour
{
    static TCPIPTest _Instance;
    string ServerIP;
    int Port;
    // Start is called before the first frame update
    void Start()
    {
        _Instance = this;
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public static void SetIPAndPort(string ip, int port)
    {
        _Instance.ServerIP = ip;
        _Instance.Port = port;
    }
    public static void StartTCPIP()
    {
        _Instance.StartTCPIPThread();
    }
    void StartTCPIPThread()
    {
        Thread thread = new Thread(new ThreadStart(TCPIP));
        thread.Start();
    }
    void TCPIP()
    {
        while (true)
        {
            TcpClient socket = new TcpClient();
            HDOBD2MainUI.PrintlnDetailDebugLabel($"tcpip BeginConnect:{ServerIP},{Port}");

            //IAsyncResult ar = socket.BeginConnect("127.0.0.1", 3333, null, null);
            IAsyncResult ar = socket.BeginConnect(ServerIP, Port, null, null);
            System.Threading.WaitHandle wh = ar.AsyncWaitHandle;
            try
            {
                HDOBD2MainUI.PrintlnDetailDebugLabel($"tcpip WaitOne");

                if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(3), false))
                {
                    socket.Close();
                    throw new TimeoutException();
                }

                socket.EndConnect(ar);
                NetworkStream stream = socket.GetStream();
                while (true)
                {

                    //if (stream.CanWrite)
                    //{
                    //    stream.Write(buffer, 0, buffer.Length);
                    //}
                    if (stream.DataAvailable && stream.CanRead)
                    {
                        Byte[] buffer = new Byte[5];

                        stream.Read(buffer, 0, buffer.Length);
                        UTF8Encoding encoding = new UTF8Encoding();
                        HDOBD2MainUI.PrintlnDetailDebugLabel($"tcpip client:{encoding.GetString(buffer)}");
                    }
                }
            }
            catch (Exception ex)
            {
                socket = null;
                //Debug.Log($"TCP fail ip: {ip}, port: {port}, exception: {ex}");
                HDOBD2MainUI.PrintlnDebugLabel($"tcpip exception:{ex.Message}");

            }
            finally
            {
                wh.Close();
            }
            Thread.Sleep(300);
        }
    }
}

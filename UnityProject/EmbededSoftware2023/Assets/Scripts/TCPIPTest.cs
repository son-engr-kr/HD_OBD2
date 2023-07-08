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
    Thread tcpClientThread;
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
        tcpClientThread = new Thread(new ThreadStart(TCPIP));
        tcpClientThread.Start();
    }
    bool ThreadStopFlag = false;
    private void OnApplicationQuit()
    {
        ThreadStopFlag = true;
        tcpClientThread?.Join();
        //tcpClientThread?.Abort();
    }
    void TCPIP()
    {
        while (!ThreadStopFlag)
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
                HDOBD2MainUI.PrintlnDetailDebugLabel($"tcpip connected to server");

                socket.EndConnect(ar);
                NetworkStream stream = socket.GetStream();
                stream.Write(Encoding.UTF8.GetBytes("Im client"));
                int idx = 0;
                while (!ThreadStopFlag)
                {
                    idx++;
                    //if (stream.CanWrite)
                    //{
                    //    stream.Write(buffer, 0, buffer.Length);
                    //}
                    if (stream.CanWrite && idx % 100 == 0)
                    {
                        UTF8Encoding encoding = new UTF8Encoding();
                        //HDOBD2MainUI.PrintlnDetailDebugLabel($"tcpip send");
                        stream.Write(Encoding.UTF8.GetBytes("Im client"));


                        Thread.Sleep(1000);

                    }
                    if (stream.DataAvailable && stream.CanRead)
                    {
                        Byte[] buffer = new Byte[4];

                        stream.Read(buffer, 0, buffer.Length);
                        //UTF8Encoding encoding = new UTF8Encoding();
                        //HDOBD2MainUI.PrintlnDetailDebugLabel($"tcpip receive from client :{Encoding.UTF8.GetString(buffer)}");
                        int bodySize = BitConverter.ToInt32(buffer, 0);
                        HDOBD2MainUI.PrintlnDetailDebugLabel($"tcpip receive from client : bodySize: {bodySize}");
                        if(bodySize != 1234567)
                        {
                            HDOBD2MainUI.PrintlnDetailDebugLabel($"tcpip receive from client : {Encoding.UTF8.GetString(buffer)}");
                        }
                    }
                    Thread.Sleep(1);
                }
                socket.Close();
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
            Thread.Sleep(5000);
        }
    }
}

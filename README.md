# NetWorkClient
the net work client with c# iocp mode

[code]`
using UnityEngine;
using System.Collections;
using NetIOCPClient;
using NetIOCPClient.Log;
using NetIOCPClient.NetWork;
using NetIOCPClient.NetWork.Data;
using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
public class testNetIOCPClient_Script : MonoBehaviour
{

    // Use this for initialization
    void Start() {
#if UNITY_WEBPLAYER
        Security.PrefetchSocketPolicy(UnityEngine.Network.player.ipAddress, 843);
		Security.PrefetchSocketPolicy(ip, port);
#endif
        Net_Client nc = new Net_Client("u3d iocp client");
        this.m_Client = nc;
        //121.40.174.130,12588
        //121.40.189.10,12588
        //string ip = "121.40.189.10";
        //int port = 12588;
        //
        nc.OnConnectBegin += nc_OnConnectBegin;
        nc.OnConncetEnd += nc_OnConncetEnd;
        nc.OnDisconnect += nc_OnDisconnect;
        nc.OnRecvData += nc_OnRecvData;
        nc.OnRecvDataBytes += nc_OnRecvDataBytes;
        nc.OnRecvError += nc_OnRecvError;
        nc.OnSend += nc_OnSend;
        nc.OnSendError += nc_OnSendError;

        //Console.WriteLine(NetIOCPClient.Util.Utility.ToHex(0, login, 0, login.Length));

        //nc.BeginConnect(ip, port);
        nc.Connect(System.Net.IPAddress.Parse(ip),port);
        //
    }
    Net_Client m_Client;
    System.Diagnostics.Stopwatch sw;
    //121.40.174.130,12588
    //121.40.189.10,12588
    string ip = "121.40.189.10";
    int port = 12588;
    static bool haslogin = false;
    // Update is called once per frame
    void Update() {
        //if (m_Client != null) {
        Net_Client nc = m_Client;
        //
        if (nc.IsConnected) {
            if (!haslogin) {
                haslogin = true;
                //login 
                byte[] login = createLoginPacket();
                //m_WaitObj.WaitOne(1000 * 10);

                nc.Send(login, 0, login.Length, true);
                sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                nc.StartHeatbeat();
            }
            nc.ProcessSendAndRecivedPackets();
            if (sw.ElapsedMilliseconds > 1000 * 60) {
                Debug.Log("60秒接收数据：-------------");
                //
                Debug.Log(nc.GetNetworkInfo());
                sw.Reset();
                sw.Start();
            }
        }
        else {
            //System.Threading.Thread.Sleep(1000 * 3);
            //yield return new WaitForSeconds(3);
            //System.Threading.Monitor.Enter(_syncObj);
            //nc.BeginConnect(ip, port);
            //m_WaitObj.WaitOne(1000 * 10);
            //login 
            //byte[] login = createLoginPacket();
            // nc.Send(login, 0, login.Length, true);
            //nc.StartHeatbeat();
        }
        //System.Threading.Thread.Sleep(10);
        //}

    }



    //static readonly object _syncObj = new object();
    //static System.Threading.ManualResetEvent m_WaitObj = new System.Threading.ManualResetEvent(true);
     void nc_OnSendError(object sender, Exception e) {
        Debug.LogWarning("send error:" + e.ToString());
    }

     void nc_OnSend(NetClientBase sender, Packet p) {
        Debug.Log("send:" + (p == null ? "byte[]" : p.ToString()));
    }

     void nc_OnRecvError(object sender, Exception e) {
        Debug.LogWarning("recived error:" + e.ToString());
    }

     void nc_OnRecvDataBytes(NetClientBase sender, BufferSegment recvData, int size) {
        Debug.Log("Recived Data:");
        Debug.Log(NetIOCPClient.Util.Utility.ToHex(0, recvData.Buffer.Array, recvData.Offset, size));
    }

     void nc_OnRecvData(NetClientBase sender, Packet p) {
        Debug.Log("recived packet:" + p.ToString());
      
    }

     void nc_OnDisconnect(object sender, EventArgs e) {
        Debug.LogWarning("断开连接");
        haslogin = false;
    }

     void nc_OnConncetEnd(System.Net.Sockets.SocketError obj) {
        Debug.Log("连接结果：" + obj.ToString());
        //m_WaitObj.Set();
    }

     void nc_OnConnectBegin(object sender, EventArgs e) {
        Debug.Log("开始连接...");
    }
    const int PACKETLENBYTES = 2;
    const int PACKETLENPOS = 2;
    static byte[] createLoginPacket() {
        //1  1  37 0  7  49 46 53 46 49 
        //46 56 3  115 103 100 16 217 53 145
        //189 247 134 14 30 78 226 252 167 153
        //145 18 21  192 168 1 206  
        // 包结构:
        //ushort  Id
        //ushort  len
        //        data
        //总长度为 len+4
        byte[] buf = null;
        using (MemoryStream ms = new MemoryStream()) {
            using (BinaryWriter bw = new BinaryWriter(ms)) {
                ushort MSG_Login = (ushort)(1 << 8) + 1;
                bw.Write(MSG_Login);
                bw.Write((ushort)0);
                byte[] ver = Encoding.UTF8.GetBytes("1.5.1.8");
                bw.Write((byte)ver.Length);
                bw.Write(ver, 0, ver.Length);
                byte[] name = Encoding.UTF8.GetBytes("hztest");
                bw.Write((byte)name.Length);
                bw.Write(name, 0, name.Length);
                byte[] pwd = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes("1234"));
                bw.Write((byte)pwd.Length);
                bw.Write(pwd, 0, pwd.Length);
                System.Net.IPAddress mask = null;
                long ip = Lidgren.Network.NetUtility.GetMyAddress(out mask).Address;//GetLocalIP()[0].Address;
                bw.Write(ip);
                buf = ms.ToArray();
                byte[] len = System.BitConverter.GetBytes((ushort)(ms.Length) - 4);
                buf[2] = len[0];
                buf[3] = len[1];

            }
        }
        return buf;
    
    }

    public class Net_Client : NetClientBase
    {
        public Net_Client(string name)
            : base(name) {
        }

        protected override void _InitClient() {
            //初始化
            //#if DEBUG
            //发送分片尺寸
            SendQueue.CoalesceBufferSize = 1024 * 1;
            this.IsPacketMode = false;
            Logs.ConfigLogFile(UnityEngine.Application.streamingAssetsPath+"netiocpclient.log", LogMessageType.MSG_INFO);
            Logs.AddDebugAppender();
            //#endif
        }
        static ClientPacketHandler[] PacketHandles = new ClientPacketHandler[65535];


        protected override void HandleReceviedPackets(NetClientBase client, NetIOCPClient.NetWork.Packet recivedPacket) {
            //关联包处理
            ProfInstance pi = Profile.StartProf();
            //ToDo:读取内容                   

            if (PacketHandles[recivedPacket.PacketID] != null) {
                PacketHandles[recivedPacket.PacketID](0L, recivedPacket);
            }
            else {
                //Debug.Assert(false, p.ToString() + "没有相应的解析委托");
                //Console.WriteLine("包" + recivedPacket.PacketID + "没有相应的解析委托");
                Logs.Warn("包" + recivedPacket.PacketID + "没有相应的解析委托");
            }
            pi.EndProf(recivedPacket.ToString(), 1f);
        }
    }
    //
}
`
[/code]

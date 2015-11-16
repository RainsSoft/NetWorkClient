using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;
using NetIOCPClient.Core;
using NetIOCPClient;
using NetIOCPClient.Network;

namespace U3DNetWorkClient_Test
{
       /// <summary>
    /// 游戏的启动项目，同时也是一个服务器状态的监视窗口
    /// </summary>
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main() {
            //string fileName = "a/c/dd.txt";
            //string dirStr = new FileInfo(fileName).FullName;
            //DirectoryInfo dir = new DirectoryInfo(Path.GetDirectoryName(fileName));
            //Process thisProcess = Process.GetCurrentProcess();
            NetIOCPClient.Log.Logs.AddConsoleAppender();

            Net_Client nc = new Net_Client();
            //121.40.174.130,12588
            //121.40.189.10,12588
            string ip = "121.40.189.10";
            int port=12588;
            //
            nc.OnConnectBegin += nc_OnConnectBegin;
            nc.OnConncetEnd += nc_OnConncetEnd;
            nc.OnDisconnect += nc_OnDisconnect;
            nc.OnRecvData += nc_OnRecvData;
            nc.OnRecvDataBytes += nc_OnRecvDataBytes;
            nc.OnRecvError += nc_OnRecvError;
            nc.OnSend += nc_OnSend;
            nc.OnSendError += nc_OnSendError;
            //
            //login 
            byte[] login=createLoginPacket();
            Console.WriteLine(NetIOCPClient.Util.Utility.ToHex(0, login, 0, login.Length));
            
            nc.BeginConnect(ip,port);
            //
            m_WaitObj.WaitOne(1000 * 10);
          
            nc.Send(login,0,login.Length);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            nc.StartHeatbeat();
            while (true) {
                if (nc.IsConnected) {
                    nc.ProcessSendAndRecivedPackets();
                    if (sw.ElapsedMilliseconds > 1000 * 60) {
                        Console.WriteLine("60秒接收数据：" + nc.ReceivedBytes.ToString());
                        //
                        Console.WriteLine(nc.GetNetworkInfo());
                        sw.Reset();
                        sw.Start();
                    }
                }
                else { 
                      System.Threading.Thread.Sleep(1000*5);
                      System.Threading.Monitor.Enter(_syncObj);
                      nc.BeginConnect(ip, port);
                      m_WaitObj.WaitOne(1000 * 10);
                      nc.Send(login, 0, login.Length);
                      nc.StartHeatbeat();
                }
                System.Threading.Thread.Sleep(10);
                
            }
        }
        static readonly object _syncObj = new object();
        static System.Threading.ManualResetEvent m_WaitObj = new System.Threading.ManualResetEvent(true);
        static void nc_OnSendError(object sender, Exception e) {
            Console.WriteLine("send error:"+e.ToString());
        }

        static void nc_OnSend(NetClientBase sender, Packet p) {
            Console.WriteLine("send:"+(p==null?"byte[]":p.ToString()));
        }

        static void nc_OnRecvError(object sender, Exception e) {
            Console.WriteLine("recived error:"+e.ToString());
        }

        static void nc_OnRecvDataBytes(NetClientBase sender, BufferSegment recvData, int size) {
            Console.WriteLine("Recived Data:");
            Console.WriteLine(NetIOCPClient.Util.Utility.ToHex(0, recvData.Buffer.Array, recvData.Offset, size));
        }

        static void nc_OnRecvData(NetClientBase sender, Packet p) {
            Console.WriteLine("recived packet:" + p.ToString());
        }

        static void nc_OnDisconnect(object sender, EventArgs e) {
            Console.WriteLine("断开连接");
        }

        static void nc_OnConncetEnd(System.Net.Sockets.SocketError obj) {
            Console.WriteLine("连接结果："+obj.ToString());
            m_WaitObj.Set();
        }

        static void nc_OnConnectBegin(object sender, EventArgs e) {
            Console.WriteLine("开始连接...");
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
            byte[] buf=null;
            using (MemoryStream ms = new MemoryStream()) {
                using (BinaryWriter bw = new BinaryWriter(ms)) { 
                     ushort MSG_Login = (ushort)(1 << 8) + 1;
                     bw.Write(MSG_Login);
                     bw.Write((ushort)0);
                     byte[] ver = Encoding.UTF8.GetBytes("1.5.1.8");
                     bw.Write((byte)ver.Length);
                     bw.Write(ver, 0, ver.Length);
                     byte[]name=Encoding.UTF8.GetBytes("sgd");
                     bw.Write((byte)name.Length);
                     bw.Write(name,0,name.Length);                     
                     byte[] pwd = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes("1234"));
                     bw.Write((byte)pwd.Length);
                     bw.Write(pwd,0,pwd.Length);
                     long ip = GetLocalIP()[0].Address;
                     bw.Write(ip);
                     buf = ms.ToArray();
                     byte[] len = System.BitConverter.GetBytes((ushort)(ms.Length)-4);
                     buf[2] = len[0];
                     buf[3] = len[1];
                     
                }
            }
            return buf;
             //unsafe protected virtual void WriteBegin(byte* buf, ref int offset) {
            //IRQHelper.WriteUShort(buf, ref offset, this.PacketID);
            //跳过两字节,留个空位用来保存包的总体长度
            //offset += PACKETLENBYTES;
            //IRQHelper.WriteString1(buf, ref offset, ver, ServerConfig.TxtEncoding);
            //IRQHelper.WriteString1(buf, ref offset, name, ServerConfig.TxtEncoding);
            //IRQHelper.WriteBytes1(buf, ref offset, pwd);
            //IRQHelper.WriteLong64(buf, ref offset, localip);
        }

        
        public static List<IPAddress> GetLocalIP() {
            List<IPAddress> ret = new List<IPAddress>();
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces()) {
                foreach (var uni in ni.GetIPProperties().UnicastAddresses) {
                    IPAddress ip = uni.Address;
                    if (IPAddress.IsLoopback(ip) || ip.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork || (uni.PrefixOrigin != PrefixOrigin.Manual && uni.PrefixOrigin != PrefixOrigin.Dhcp)) {
                        continue;
                    }
                    ret.Add(ip);
                }
            }
            return ret;
        }
    }
}

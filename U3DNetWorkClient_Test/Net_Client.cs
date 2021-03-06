﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NetIOCPClient;
using NetIOCPClient.NetWork;

namespace U3DNetWorkClient_Test
{
    //using (StreamWriter sw = new StreamWriter("_server.lst", false, Encoding.UTF8)) {
    //        sw.WriteLine("阿里云,121.40.174.130,12588");
    //        sw.WriteLine("联通,124.160.148.203,21");
    //        sw.WriteLine("电信,115.236.77.80,81");
    //        sw.WriteLine("电信_zj,115.236.77.80,12589");
    //        sw.WriteLine("1.3公网测试,115.236.77.80,12589");
    //        sw.WriteLine("机房版,192.168.1.102,12588");
    //        sw.WriteLine("内部开发,192.168.1.81,12588");
    //        sw.WriteLine("czj,192.168.1.88,12588");
    //        sw.WriteLine("sgd,192.168.1.3,12588");
    //    }
    public class Net_Client : NetClientBase
    {


        protected override void _InitClient() {
            //初始化
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
                Console.WriteLine("包" + recivedPacket.PacketID + "没有相应的解析委托");
            }
            pi.EndProf(recivedPacket.ToString(), 1f);
        }
    }
}

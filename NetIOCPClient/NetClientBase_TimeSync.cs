using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using NetIOCPClient.Core;
using NetIOCPClient.Log;
using NetIOCPClient.Network;

namespace NetIOCPClient
{
    abstract partial class NetClientBase
    {

        #region 心跳包处理
        int HEATBEATTIME = 15;//15秒心跳一次


        private Stopwatch m_HeatbeatTimer = new Stopwatch();
        private bool m_HeatBeatStart = false;
        private void SendHeatBeat() {
            if (m_HeatBeatStart && m_HeatbeatTimer.Elapsed.Seconds > HEATBEATTIME) {
                //HeatbeatPacket p = Packet.GetPacket<HeatbeatPacket>(HeatbeatPacket._PacketID);
                //HeatbeatPacket p = new HeatbeatPacket(true);
                HeatbeatPacket p = this.PacketCreatorMgr.GetPacketCreator(HeatbeatPacket._PacketID).CreatePacket() as HeatbeatPacket;
                if (IsPrepareModel) {
                    //Send(0, p, true);
                }
                else {
                    // Send(p, true);
                    Send(p);
                }
                m_HeatbeatTimer.Reset();
                m_HeatbeatTimer.Start();
            }
        }

        /// <summary>
        /// 开始发送心跳包
        /// </summary>
        public void StartHeatbeat() {
            m_HeatBeatStart = true;
            m_HeatbeatTimer.Start();
            //开始同步时间           
        }
        /// <summary>
        /// 停止发心跳包 等待服务器移除当前连接
        /// </summary>
        public void StopHeatBeat() {
            m_HeatBeatStart = false;
            m_HeatbeatTimer.Stop();
            Logs.Info("StopHeatBeat");

        }
        #endregion

        #region 时间同步

        private readonly static int TimeSynInterval = 1000 * 30;//15秒进行一次时间同步
        private Stopwatch m_SynTimer = Stopwatch.StartNew();
        //private int m_TimeSynThreshold = 10;/
        //上一次执行同步的时刻.
        private long m_SynTimeStamp = 0;
        private int m_Ping = 0;
        private long m_TimeSpanWithServer;

        /// <summary>
        /// 与服务器的ping值.单位:ms
        /// </summary>
        public int Ping {
            get {
                return m_Ping;
            }
        }
        /// <summary>
        /// 和服务器的绝对时间差.单位:ms
        /// </summary>
        public long TimeSpanWithServer {
            get {
                return m_TimeSpanWithServer;
            }
        }
        private void ProcessTimeSyn() {

            if (m_SynTimer.ElapsedMilliseconds - m_SynTimeStamp > TimeSynInterval || m_SynTimeStamp == 0) {
                m_SynTimeStamp = m_SynTimer.ElapsedMilliseconds;
                //TimeSynPacket p = Packet.GetPacket<TimeSynPacket>(TimeSynPacket._PacketID);// new TimeSynPacket(m_SynTimeStamp);
                TimeSynPacket p = this.PacketCreatorMgr.GetPacketCreator(TimeSynPacket._PacketID).CreatePacket() as TimeSynPacket;//new TimeSynPacket(m_SynTimeStamp);
                p.ClinetTimeStamp = m_SynTimeStamp;
                p.ServerTimeStamp = DateTime.Now.ToBinary();
                p.UsePacketAgain();
                //p.Write();
                if (IsPrepareModel) {
                    // Send(0,p, true);
                }
                else {
                    Send(p);
                }
            }
            else {
                int k = 0;
            }
        }

        private void OnRecvTimeSyn(Packet packet) {
            TimeSynPacket p = packet as TimeSynPacket;
            m_Ping = (int)(m_SynTimer.ElapsedMilliseconds - p.ClinetTimeStamp);
            m_TimeSpanWithServer = (long)((DateTime.FromBinary(p.ServerTimeStamp) - DateTime.Now).TotalMilliseconds);
            //回收 应该不再这里做
            //this.PacketCreatorMgr.GetPacketCreator(TimeSynPacket._PacketID).RecylePacket(p);
        }

        #endregion
    }
}
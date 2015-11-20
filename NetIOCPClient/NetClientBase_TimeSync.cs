using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using NetIOCPClient.Core;
using NetIOCPClient.Log;
using NetIOCPClient.NetWork;


namespace NetIOCPClient
{
    abstract partial class NetClientBase
    {

        #region ����������
        int HEATBEATTIME = 15;//15������һ��


        private Stopwatch m_HeatbeatTimer = new Stopwatch();
        private bool m_HeatBeatStart = false;
        private void SendHeatBeat() {
            if (m_HeatBeatStart && m_HeatbeatTimer.Elapsed.Seconds > HEATBEATTIME) {
                //HeatbeatPacket p = Packet.GetPacket<HeatbeatPacket>(HeatbeatPacket._PacketID);
                //HeatbeatPacket p = new HeatbeatPacket(true);
                HeatbeatPacket p = this.PacketCreatorMgr_Send.GetPacketCreator(HeatbeatPacket._PacketID).CreatePacket() as HeatbeatPacket;
                if (IsPrepareModel) {
                    //Send(0, p, true);
                }
                else {
                    // Send(p, true);
#if DEBUG
                    Logs.Info("Send HeatBeat:"+p.PacketID.ToString());
                    Console.WriteLine("���������Ϣ:" + this.PacketCreatorMgr_Send.GetPacketCreator(HeatbeatPacket._PacketID).GetPoolInfo().ToString());

#endif
                    Send(p);
                }
                m_HeatbeatTimer.Reset();
                m_HeatbeatTimer.Start();
            }
        }

        /// <summary>
        /// ��ʼ����������
        /// </summary>
        public void StartHeatbeat() {
            m_HeatBeatStart = true;
            m_HeatbeatTimer.Start();
            //��ʼͬ��ʱ��        
            Logs.Info("Start HeatBeat");
        }
        /// <summary>
        /// ֹͣ�������� �ȴ��������Ƴ���ǰ����
        /// </summary>
        public void StopHeatBeat() {
            m_HeatBeatStart = false;
            m_HeatbeatTimer.Stop();
            Logs.Info("Stop HeatBeat");

        }
        #endregion

        #region ʱ��ͬ��

        private readonly static int TimeSynInterval = 1000 * 30;//15�����һ��ʱ��ͬ��
        private Stopwatch m_SynTimer = Stopwatch.StartNew();
        //private int m_TimeSynThreshold = 10;/
        //��һ��ִ��ͬ����ʱ��.
        private long m_SynTimeStamp = 0;
        private int m_Ping = 0;
        private long m_TimeSpanWithServer;

        /// <summary>
        /// ���������pingֵ.��λ:ms
        /// </summary>
        public int Ping {
            get {
                return m_Ping;
            }
        }
        /// <summary>
        /// �ͷ������ľ���ʱ���.��λ:ms
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
                TimeSynPacket p = this.PacketCreatorMgr_Send.GetPacketCreator(TimeSynPacket._PacketID).CreatePacket() as TimeSynPacket;//new TimeSynPacket(m_SynTimeStamp);
                p.ClinetTimeStamp = m_SynTimeStamp;
                p.ServerTimeStamp = DateTime.Now.ToBinary();
                p.Write();
                //p.Write();
                if (IsPrepareModel) {
                    // Send(0,p, true);
                }
                else {
#if DEBUG
                    Logs.Info("Send TimeSyn:"+p.PacketID.ToString());
                    Console.WriteLine("���������Ϣ:" + this.PacketCreatorMgr_Send.GetPacketCreator(TimeSynPacket._PacketID).GetPoolInfo().ToString());
#endif
                    Send(p);
                }
            }
            else {
                int k = 0;
            }
        }

        private void OnRecvTimeSyn(Packet packet) {
#if DEBUG
            Logs.Info("Recived TimeSyn:"+packet.PacketID.ToString());
#endif
            TimeSynPacket p = packet as TimeSynPacket;
            m_Ping = (int)(m_SynTimer.ElapsedMilliseconds - p.ClinetTimeStamp);
            m_TimeSpanWithServer = (long)((DateTime.FromBinary(p.ServerTimeStamp) - DateTime.Now).TotalMilliseconds);
            //����,��Ϊֱ�������ﴦ���� 
            this.PacketCreatorMgr_Recived.GetPacketCreator(TimeSynPacket._PacketID).RecylePacket(p);
        }

        #endregion
    }
}
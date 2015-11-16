using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetIOCPClient.Core;

namespace NetIOCPClient.NetWork
{
    /// <summary>
    ///  包创建管理器
    ///  包结构 ： 包ID(ushort)+包内容长度(ushort)
    ///  根据包结构 对接收到的数据进行解析或者发送
    /// </summary>
    public class PacketCreatorManager
    {
        //static Dictionary<ushort, PacketCreator> m_Buf = new Dictionary<ushort, PacketCreator>();
        PacketCreator[] m_Buf = new PacketCreator[65535];
        public void RegistePacket(ushort packetID, PacketCreator creator) {
            //if (!m_Buf.ContainsKey(packetID)) {
            //    m_Buf.Add(packetID, creator);
            //}
            if (m_Buf[packetID] != null) {
                throw new Exception(packetID.ToString() + " 对应的包构造器已经存在,它是:" + m_Buf[packetID].ToString());
            }
            else {
                m_Buf[packetID] = creator;
            }
        }

        public void Remove(ushort packetID) {
            m_Buf[packetID] = null;
        }

        public PacketCreator GetPacketCreator(ushort packetID) {
            //PacketCreator p = null;
            // m_Buf.TryGetValue(packetID, out p);
#if DEBUG
            if (m_Buf[packetID] == null) {

                Console.WriteLine("未注册的包类型.packetId:{0} ", packetID);

            }
#endif
            return m_Buf[packetID];
        }
        public PacketCreatorManager() {
            //基础类型
            m_Buf[100] = new HeatbeatPacketCreator();
            m_Buf[101] = new TimeSynPacketCreator();
            m_Buf[99] = new CustomPacketCreator();
        }
    }


}

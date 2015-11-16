using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetIOCPClient.Network
{
    /// <summary>
    ///  包创建器
    ///  包结构 ： 包ID(ushort)+包内容长度(ushort)
    ///  根据包结构 对接收到的数据进行解析或者发送
    /// </summary>

    public class PacketWriteException : Exception
    {
        public Packet ErrPacket;
        public PacketErrorLevel ErrLevel;
        public PacketWriteException(Packet p, PacketErrorLevel errLevel, Exception inner)
            : base("写Packet错误", inner) {

            ErrPacket = p;
            ErrLevel = errLevel;
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.Append("Error Msg:");
            sb.Append("写Packet错误");
            sb.Append("\n");
            sb.Append("Inner Error:");
            sb.Append(this.InnerException.ToString());
            sb.Append("Packet:");
            sb.Append(ErrPacket.ToString());
            return sb.ToString();
        }
    }

    public class PacketReadException : Exception
    {
        public Packet ErrPacket;
        public PacketErrorLevel ErrLevel;
        public PacketReadException(Packet p, PacketErrorLevel errLevel, Exception inner)
            : base("读packet错误", inner) {

            ErrPacket = p;
            ErrLevel = errLevel;
        }
        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.Append("Error Msg:");
            sb.Append("读Packet错误");
            sb.Append("\n");
            sb.Append("Inner Error:");
            sb.Append(this.InnerException.ToString());
            sb.Append("Packet:");
            sb.Append(ErrPacket.ToString());
            return sb.ToString();
        }

    }

}

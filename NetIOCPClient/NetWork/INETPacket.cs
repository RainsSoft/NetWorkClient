using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetIOCPClient.Network
{
    /// <summary>
    /// net包 接口
    /// </summary>
    public interface INETPacket
    {
        /// <summary>
        /// packet id是不在长度之内的
        /// </summary>
        ushort PacketID { get; set; }

        /// <summary>
        /// 整个包的大小 通常是值等于DataLen+4
        /// </summary>
        int PacketBufLen { get; set; }
        /// <summary>
        /// 包内数据长度
        /// </summary>
        ushort DataLen { get; set; }
        /// <summary>
        /// int long 等基础数据大小端
        /// </summary>
        Endian Endian { get; set; }
        /// <summary>
        /// 对数据内容加密
        /// </summary>
        void Encode(byte[]buf,int offset,int len);
        /// <summary>
        /// 对数据内容解密
        /// </summary>
        void Decode(byte[]buf,int offset,int len);
    }
}

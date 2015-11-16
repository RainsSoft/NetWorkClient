using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NetIOCPClient.Pool;

namespace NetIOCPClient.Network
{
    /// <summary>
    ///  包创建器
    ///  包结构 ： 包ID(ushort)+包内容长度(ushort)
    ///  根据包结构 对接收到的数据进行解析或者发送
    /// </summary>
    public abstract class PacketCreator
    {

        /// <summary>
        /// 通过 唯一的ID标识 来缓存数据包
        /// </summary>    
        protected const string _packetPoolNameFormat = "NetPacket_{0}";
        //不带参数
        //在TCP的情况,不知道什么时候才会有一个完整的包.
        //所以Packet的数据填充交给Packet的Read方法去做.
        public abstract Packet CreatePacket();
        protected abstract void _initPacketPool();
        /// <summary>
        /// 创建的 与回收的 packetid必须一样，通过ID来关联创建器与 packet对象
        /// </summary>
        /// <param name="p"></param>
        public abstract void RecylePacket(Packet p);//{
        //    if (_packetPool == null) {
        //        _packetPool = new ObjectPool<Packet>(128, 1024, string.Format(_packetPoolNameFormat,p.PacketID));
        //    }
        //    _packetPool.ReleaseContent(p);//通过缓存池，就没必要每次都new出来了，packet.buffer要动态关联片段
        //}
    }


}

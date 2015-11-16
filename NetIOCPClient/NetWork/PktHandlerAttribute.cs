using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetIOCPClient.NetWork
{
    /// <summary>
    /// 用来标记包处理函数.
    /// 可以对包处理函数的注册起到辅助作用
    /// </summary>
    public class PktHandlerAttribute : Attribute
    {
        /// <summary>
        /// 包ID
        /// </summary>
        public ushort PacketID;
        /// <summary>
        /// 备注
        /// </summary>
        public string Memo;
        /// <summary>
        /// 包构造器
        /// </summary>
        public PacketCreator PacketCreator;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="packetId">包ID</param>
        /// <param name="creatorType">包构造器类型</param>
        /// <param name="memo">备注</param>
        public PktHandlerAttribute(ushort packetId, Type creatorType, string memo) {
            if (packetId <= 0) {
                throw new ArgumentException("packetId必须大于0", "packetid");
            }
            if (creatorType == null) {
                throw new ArgumentNullException("creatorType不能为空", "creatorType");
            }
            PacketID = packetId;
            Memo = memo;
            PacketCreator = Activator.CreateInstance(creatorType) as PacketCreator;
        }

    }


}

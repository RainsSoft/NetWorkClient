using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetIOCPClient.NetWork;
using NetIOCPClient.Pool;

namespace NetIOCPClient.Core
{
    /// <summary>
    /// 自定义包，内部只有ID 加byte[]
    /// </summary>
    public class CustomPacket : Packet
    {
        internal static readonly ushort _PacketID = 99;
        public CustomPacket()
            : base() {
            // base create buffer
        }

        internal unsafe CustomPacket(bool fake)
            : base(100) {
            //
            this.Write();
        }
        /// <summary>
        /// 长度不能超过64k,windows系统一次最大能发送96K，
        /// 为了保险起见64K为最大值一个包
        /// </summary>
        public byte[] MsgData;


        public override unsafe void Read(BufferSegment msg) {
            int offset = 0;
            ReadBegin(msg, ref offset);
            ReadEnd(msg);
        }

        public override unsafe void Write() {
            int offset = 0;
            //fixed (byte* buf = PacketBuf) {
            //    WriteBegin(buf, ref offset);
            //    WriteEnd(buf, ref offset);
            //}
            this.WriteBegin(this.Buffer, ref offset);
            if (MsgData != null) {
                PacketHelper.Writer.WriteUShort(this.Buffer, ref offset, (ushort)0);
            }
            else {
                System.Diagnostics.Debug.Assert((MsgData.Length + HeadSize) < 1024 * 64);
                PacketHelper.Writer.WriteBytes2(this.Buffer, ref offset, MsgData);
            }
            this.WriteEnd(this.Buffer, ref offset);
        }

        public override ushort PacketID {
            get { return _PacketID; }
        }
        protected override void _initBuffer() {
            this.Buffer = BufferManager.Large.CheckOut();
        }

    }

    class CustomPacketCreator : PacketCreator
    {

        public override Packet CreatePacket() {
            _initPacketPool();
            CustomPacket packet = _packetPool.AcquireContent();
            return packet;
            //return Packet.GetPacket<HeatbeatPacket>(HeatbeatPacket._PacketID);
        }

        public override void RecylePacket(Packet p) {
            _initPacketPool();
            _packetPool.ReleaseContent(p as CustomPacket);
        }
        //
        ObjectPool<CustomPacket> _packetPool;
        protected override void _initPacketPool() {
            if (_packetPool == null) {
                _packetPool = new ObjectPool<CustomPacket>(2, 128, string.Format(_packetPoolNameFormat, CustomPacket._PacketID));
            }
        }
        public override PoolInfo GetPoolInfo() {
            _initPacketPool();
            return _packetPool.GetPoolInfo();
        }
    }


}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetIOCPClient.NetWork;
using NetIOCPClient.Pool;

namespace NetIOCPClient.Core
{
    public class HeatbeatPacket : Packet
    {
        internal static readonly ushort _PacketID = 100;
        public HeatbeatPacket()
            : base() {
            // base create buffer

        }


        internal unsafe HeatbeatPacket(bool fake)
            : base(100) {
            //
            this.Write();
        }

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
            this.WriteEnd(this.Buffer, ref offset);
        }

        public override ushort PacketID {
            get { return _PacketID; }
        }
        protected override void _initBuffer() {
            this.Buffer = BufferManager.Tiny.CheckOut();
        }

    }

    class HeatbeatPacketCreator : PacketCreator
    {

        public override Packet CreatePacket() {
            _initPacketPool();
            HeatbeatPacket packet = _packetPool.AcquireContent();
            return packet;
            //return Packet.GetPacket<HeatbeatPacket>(HeatbeatPacket._PacketID);
        }

        public override void RecylePacket(Packet p) {
            _initPacketPool();
            _packetPool.ReleaseContent(p as HeatbeatPacket);
        }
        //
        ObjectPool<HeatbeatPacket> _packetPool;
        protected override void _initPacketPool() {
            if (_packetPool == null) {
                _packetPool = new ObjectPool<HeatbeatPacket>(4, 64, string.Format(_packetPoolNameFormat, HeatbeatPacket._PacketID));
            }
        }

        public override PoolInfo GetPoolInfo() {
            _initPacketPool();
            return _packetPool.GetPoolInfo();
        }
    }


}

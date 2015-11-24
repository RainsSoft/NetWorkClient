using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetIOCPClient.NetWork;
using NetIOCPClient.Pool;

namespace NetIOCPClient.Core
{
    public class TimeSynPacket : Packet
    {
        internal static readonly ushort _PacketID = 101;
        internal long ServerTimeStamp;
        //internal long ServerRecvTimeStamp;
        internal long ClinetTimeStamp;
        public unsafe TimeSynPacket()
            : base() {
            // base create bufffer
        }
        internal unsafe TimeSynPacket(long clientTimeStamp)
            : base() {
            ClinetTimeStamp = clientTimeStamp;
            ServerTimeStamp = DateTime.Now.ToBinary();
            //
            this.Write();
        }
        internal unsafe TimeSynPacket(long clientTimeStamp, long serverTime)
            : base() {
            ClinetTimeStamp = clientTimeStamp;
            ServerTimeStamp = serverTime;
            //
            this.Write();
        }


        public override unsafe void Read(BufferSegment msg) {
            int offset = 0;
            ReadBegin(msg, ref offset);
            ClinetTimeStamp = PacketHelper.Reader.ReadLong64(msg, ref offset);// msg.ReadLong64();
            //ServerRecvTimeStamp = msg.ReadLong64();
            ServerTimeStamp = PacketHelper.Reader.ReadLong64(msg, ref offset);//msg.ReadLong64();
            ReadEnd(msg);
        }

        public override unsafe void Write() {
            int offset = 0;
            // fixed (byte* buf = PacketBuf) {
            BufferSegment buf = this.Buffer;
            WriteBegin(buf, ref offset);

            PacketHelper.Writer.WriteLong64(buf, ref offset, ClinetTimeStamp);
            PacketHelper.Writer.WriteLong64(buf, ref offset, ServerTimeStamp);
            //IRQHelper.WriteLong64(buf, ref offset, ServerSendTimeStamp);
            WriteEnd(buf, ref offset);
            //}
        }

        public override ushort PacketID {
            get { return _PacketID; }
        }


        protected override void _initBuffer() {
            this.Buffer = BufferManager.Tiny.CheckOut();
        }

        public override void ClearContent() {
           //todo:清理额外的对象内容缓存,防止内存溢出
        }
    }
    internal class TimeSynPacketCreator : PacketCreator
    {

        public override Packet CreatePacket() {
            _initPacketPool();
            TimeSynPacket packet = _packetPool.AcquireContent();
            if (packet.Buffer != null) {
                System.Diagnostics.Debug.Assert(packet.Buffer.Uses == 1);
                packet.Buffer.DecrementUsage();
            }
            packet.Buffer = BufferManager.Tiny.CheckOut();
            return packet;
            //return Packet.GetPacket<HeatbeatPacket>(HeatbeatPacket._PacketID);
        }

        public override void RecylePacket(Packet p) {
            _initPacketPool();
            System.Diagnostics.Debug.Assert(p.Buffer.Uses == 1);
            p.Buffer.DecrementUsage();
            if (p.Buffer.Uses == 0) {
                p.Buffer = null;
            }
            p.ClearContent();//防止内存溢出
            _packetPool.ReleaseContent(p as TimeSynPacket);
        }
        public override IPoolInfo _Pool {
            get { return _packetPool; }
        }
         ObjectPool<TimeSynPacket> _packetPool;
        
       
        protected override void _initPacketPool() {
            if (_packetPool == null) {
                _packetPool = new ObjectPool<TimeSynPacket>(2, 64, string.Format(_packetPoolNameFormat, TimeSynPacket._PacketID));
            }
        }
        public override PoolInfo GetPoolInfo() {
            _initPacketPool();
            return _packetPool.GetPoolInfo();
        }
        
    }


}

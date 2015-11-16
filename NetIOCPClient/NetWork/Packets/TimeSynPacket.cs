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
        }
        internal unsafe TimeSynPacket(long clientTimeStamp)
            : base() {
            ClinetTimeStamp = clientTimeStamp;
            ServerTimeStamp = DateTime.Now.ToBinary();
            int offset = 0;
            //fixed (byte* buf = PacketBuf) {
            BufferSegment buf = this.Buffer;
            WriteBegin(buf, ref offset);

            PacketHelper.Writer.WriteLong64(buf, ref offset, ClinetTimeStamp);
            PacketHelper.Writer.WriteLong64(buf, ref offset, ServerTimeStamp);
            //IRQHelper.WriteLong64(buf, ref offset, ServerSendTimeStamp);
            WriteEnd(buf, ref offset);
            //}
        }
        internal unsafe TimeSynPacket(long clientTimeStamp, long serverTime)
            : base() {
            ClinetTimeStamp = clientTimeStamp;
            ServerTimeStamp = serverTime;
            int offset = 0;
            //fixed (byte* buf = PacketBuf) {
            BufferSegment buf = this.Buffer;
            WriteBegin(buf, ref offset);

            PacketHelper.Writer.WriteLong64(buf, ref offset, ClinetTimeStamp);
            //IRQHelper.WriteLong64(buf, ref offset, ServerRecvTimeStamp);
            PacketHelper.Writer.WriteLong64(buf, ref offset, ServerTimeStamp);
            WriteEnd(buf, ref offset);
            // }
        }
        //public override unsafe void UsePacketAgain() {
        //    int offset = 0;
        //    //fixed (byte* buf = PacketBuf) {
        //    BufferSegment buf = this.Buffer;
        //        WriteBegin(buf, ref offset);

        //        PacketHelper.Writer.WriteLong64(buf, ref offset, ClinetTimeStamp);
        //        //IRQHelper.WriteLong64(buf, ref offset, ServerRecvTimeStamp);
        //        PacketHelper.Writer.WriteLong64(buf, ref offset, ServerTimeStamp);
        //        WriteEnd(buf, ref offset);
        //    //}
        //}


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
        /// <summary>
        /// 重复使用，重新写入新数据
        /// </summary>
        public override unsafe void UsePacketAgain() {
            int offset = 0;
            //fixed (byte* buf = PacketBuf) {
            BufferSegment buf = this.Buffer;
            WriteBegin(buf, ref offset);
            PacketHelper.Writer.WriteLong64(buf, ref offset, ClinetTimeStamp);
            //IRQHelper.WriteLong64(buf, ref offset, ServerRecvTimeStamp);
            PacketHelper.Writer.WriteLong64(buf, ref offset, ServerTimeStamp);
            WriteEnd(buf, ref offset);
            // }
        }

        protected override void _initBuffer() {
            this.Buffer = BufferManager.Small.CheckOut();
        }
    }
    internal class TimeSynPacketCreator : PacketCreator
    {

        public override Packet CreatePacket() {
            //return new TimeSynPacket();
            //return Packet.GetPacket<TimeSynPacket>(TimeSynPacket._PacketID);
            _initPacketPool();
            TimeSynPacket tp = _packetPool.AcquireContent();
            return tp;
        }

        public override void RecylePacket(Packet p) {
            _initPacketPool();
            _packetPool.ReleaseContent(p as TimeSynPacket);
        }
        ObjectPool<TimeSynPacket> _packetPool;
        protected override void _initPacketPool() {
            if (_packetPool == null) {
                _packetPool = new ObjectPool<TimeSynPacket>(4, 64, string.Format(_packetPoolNameFormat, TimeSynPacket._PacketID));
            }
        }
    }


}

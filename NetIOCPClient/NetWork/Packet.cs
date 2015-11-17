using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NetIOCPClient.Log;

namespace NetIOCPClient.NetWork
{
    public enum PacketErrorLevel
    {
        ERR_OK = 0,
        ERR_DEATH = 1,
        ERR_CONTINUE = 2,

    }

    /// <summary>
    /// 网络数据处理委托
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="state">状态</param>
    /// <param name="msgData">数据包</param>
    /// <returns>处理结果</returns>
    public delegate object PacketHandler(Packet packet, object arg, out PacketErrorLevel level);
    /// <summary>
    /// 接收包处理委托
    /// </summary>
    /// <param name="connIndex"></param>
    /// <param name="packet"></param>
    public delegate void ClientPacketHandler(long connIndex, Packet packet);
    public delegate void SCPacketHandler(Packet packet);
    public delegate void ServerPacketHandler(long connIndex, BufferSegment msg);
    /// <summary>
    ///  包结构 ： 包ID(ushort)+包内容长度(ushort)
    ///  根据包结构 对接收到的数据进行解析或者发送
    /// </summary>
    public abstract class Packet : PacketBase
    {
        /// <summary>
        /// 包长占字节数2
        /// </summary>
        private const int PACKETLENBYTES = 2;
        /// <summary>
        /// 包长度开始位置索引2
        /// </summary>
        private const int PACKETLENPOS = 2;
        /// <summary>
        /// 包头（ID2+长度2）占字节数4
        /// </summary>
        public static readonly int HeadSize = 4;
        /// <summary>
        /// PacketID高位是包类型,只读,在PacketID被赋予时确定
        /// </summary>
        public byte PacketType {
            get {
                return (byte)(this.PacketID >> 8);
            }
        }
        /// <summary>
        /// PacketID低位是命令字,只读,在PacketID被赋予时确定
        /// </summary>
        public byte PacketCmd {
            get {
                return (byte)(this.PacketID & 0xFFFF);
            }
        }
        /// <summary>
        /// 包前置数据
        /// 前置数据的处理方式:该部分数据不写入PacketBuf.
        /// 如果需要发送这前置数据,则它必须位于包ID之前.
        /// </summary>
        [Obsolete()]
        public long PacketPrepareData;

        public Packet() {
            _initBuffer();
        }
        protected abstract void _initBuffer();
        public Packet(ushort packetid)
            : this() {
            m_PacketID = packetid;
        }
        //
        #region
        protected ushort m_PacketID;
        /// <summary>
        /// packet id是不在DataLen长度之内的[唯一的]
        /// </summary>
        public override ushort PacketID {
            get { return m_PacketID; }
            set {
                throw new NotSupportedException("PacketID must assgin by m_PacketID");
            }
        }

        /// <summary>
        /// 整个包的大小 通常是值等于DataLen+HeadSize { PacketID(2)长度+长度(2) }
        /// </summary>
        public override int PacketBufLen {
            get { return DataLen + HeadSize; }
            set {
                throw new NotSupportedException("PacketBufLen cann't set");
            }
        }

        /// <summary>
        /// int long 等基础数据大小端
        /// </summary>
        public override Endian Endian {
            get {
                //使用小端模式，写包的时候注意 int long等基础数据转byte的顺序，保证很服务器端的大小端一致
                return Endian.LITTLE_ENDIAN;
            }
        }
        /// <summary>
        /// 关联数据,作为额外的对象地址关联
        /// </summary>
        public object Token;
       
        #endregion
        /// <summary>
        /// 如果当前os为大端模式，发送数据生成包的时候需要对 int long等基础数据转换为byte
        /// 使用BitConverter
        /// </summary>
        /// <returns></returns>
        Endian GetLocalOS_Endian() {
            return BitConverter.IsLittleEndian ? Endian.LITTLE_ENDIAN : Endian.BIG_ENDIAN;

        }
        public BufferSegment Get(int bufferLen) {
            BufferSegment buf = BufferManager.GetSegment(bufferLen);
            return buf;
        }
        //public SegmentStream GetStream(int bufferLen) {
        //    SegmentStream buf = BufferManager.GetSegmentStream(bufferLen);
        //    return buf;
        //}
        public override string ToString() {
            return string.Format("NetPacket({0})", PacketID);
        }
        public override void Dispose() {

        }


        unsafe protected ushort ReadBegin(BufferSegment msg, ref int offset) {
            ushort packid = PacketHelper.Reader.ReadUShort(msg, ref offset);
            System.Diagnostics.Debug.Assert(packid == this.PacketID); //检验包ID是否正确            //
            ushort len = PacketHelper.Reader.ReadUShort(msg, ref offset);
            //this.BufLen = len + 4;
            this.DataLen = len;
            return len;
        }
        unsafe protected void ReadEnd(BufferSegment msg) {
            //检查长度是否合法
        }

        /// <summary>
        /// 读包,读取当前包内容 注意：一个buffersegment内只有一个包
        /// </summary>
        /// <param name="msg"></param>
        public unsafe abstract void Read(BufferSegment msg);
        /// <summary>
        /// 写包,写入当前包内容 注意：一个buffersegment内只有一个包
        /// </summary>
        public unsafe abstract void Write();
        unsafe protected virtual void WriteBegin(BufferSegment buf, ref int offset) {

            PacketHelper.Writer.WriteUShort(buf, ref offset, this.PacketID);
            //跳过两字节,留个空位用来保存包的总体长度
            offset += PACKETLENBYTES;
        }
        /// <summary>
        /// 在第2个字节后插入长度
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="offset"></param>
        unsafe protected virtual void WriteEnd(BufferSegment buf, ref int offset) {
#if DEBUG11
            IRQHelper.WriteLong64(buf, ref offset, this.ExtendData);
#endif
            //插入长度
            int len = PACKETLENPOS;
            PacketHelper.Writer.WriteUShort(buf, ref len, (ushort)(offset - 4));
            //this.BufLen = offset;//
            this.DataLen = (ushort)(offset - 4);
            // Debug.Assert(this.BufLen <= this.PacketBuf.Length, "写入包的数据尺寸 " + this.BufLen.ToString() + "超过PacketBuf的最大尺寸 " + this.PacketBuf.Length.ToString());
            Debug.Assert(offset <= this.Buffer.Length);
            //if (this.BufLen > this.PacketBuf.Length) {
            if (offset > this.Buffer.Length) {
                Logs.Error("Packet WriteEnd.Packet:" + this.ToString() +
                     " \n写入包的数据尺寸 " + offset.ToString() +
                     "超过PacketBuf的最大尺寸 " + this.Buffer.Length.ToString());
                throw new PacketWriteException(this, PacketErrorLevel.ERR_DEATH, null);
            }
#if DEBUG11
            WriteOK = true;
#endif
        }

        //

    }


}

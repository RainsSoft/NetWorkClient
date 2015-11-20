

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using NetIOCPClient.NetWork;
using NetIOCPClient.Log;
using NetIOCPClient.Core;
//using NLog;

namespace NetIOCPClient
{
    public delegate void NetworkExceptionHandler(object sender, NetException e);
    /// <summary>
    /// 收到数据包
    /// </summary>
    /// <param name="connIndex">连接ID</param>
    /// <param name="p"></param>
    public delegate void ClientRecvDataHandler(NetClientBase sender, Packet p);

    public delegate void RecvDataBytesHandler(NetClientBase sender, BufferSegment recvData, int size);

    public delegate void SendEventHandler(NetClientBase sender, Packet p);

    public enum ClientNetworkStatus
    {
        Connected = 1,
        BeginDisConnect = 5,
        DisConnected = 6,
    }
    /// <summary>
    /// Base class for all clients.
    /// </summary>
    /// <seealso cref="ServerBase"/>
    public abstract partial class NetClientBase : IClient
    {
        public event EventHandler OnConnectBegin;//开始连接
        public event Action<SocketError> OnConncetEnd;//连接结果
        //public event EventHandler OnConncetTimeOut;//连接失败

        //private NetworkExceptionHandler m_OnSendError = null;
        /// <summary>
        /// 发送失败事件，可能会在线程中引发，不保证线程安全
        /// </summary>
        public event NetworkExceptionHandler OnSendError;
        public event NetworkExceptionHandler OnRecvError;
        /// <summary>
        /// 断开事件，主动调用DisConnect或者Close，不会引发该事件。
        /// </summary>
        public event EventHandler OnDisconnect;
        public event SendEventHandler OnSend;
        /// <summary>
        /// 接收到包数据时引发的事件,IsPacketMode时有效
        /// 比如广播包，在执行事件的时候必须根据广播包的ID来执行相应的操作
        /// </summary>
        //public static PacketHasNoRPCCallbackHandler OnRecivedBCPacketEvent;
        public event ClientRecvDataHandler OnRecvData;
        public ClientNetworkStatus Status;
        /// <summary>
        /// 接收到包数据时引发的事件,IsPacketMode=false时有效
        /// </summary>
        public event RecvDataBytesHandler OnRecvDataBytes;

        /// <summary>
        /// 是否采用包模式,默认为true
        /// 如果不是采用包模式.
        /// </summary>
        public bool IsPacketMode = true;
        public string Name = string.Empty;
        /// <summary>
        /// 是否启用前置处理.
        /// 用来处理包id前的标识性数据
        /// </summary>
        public bool IsPrepareModel = false;
        /// <summary>
        /// 前置处理的数据长度
        /// </summary>
        internal int PrepareDataSize = 0;
        /// <summary>
        /// 发送字节总数G/M/K/B
        /// </summary>
        public static string SendTotalByteData {
            get {
                string sm = "{0}(G),{1}(M),{2}(K),{3}(B)";
                int b = (int)(TotalBytesSent % 1024);
                int k = (int)(TotalBytesSent / 1024 % 1024);
                int m = (int)(TotalBytesSent / (1024 * 1024) % 1024);
                int g = (int)(TotalBytesSent / (1024 * 1024 * 1024) % 1024);
                return string.Format(sm, g, m, k, b);
            }
        }
        /// <summary>
        /// 接收字节总数G/M/K/B
        /// </summary>
        public static string RecivedTotalByteData {
            get {
                string rm = "{0}(G),{1}(M),{2}(K),{3}(B)";
                int b = (int)(TotalBytesReceived % 1024);
                int k = (int)(TotalBytesReceived / 1024 % 1024);
                int m = (int)(TotalBytesReceived / (1024 * 1024) % 1024);
                int g = (int)(TotalBytesReceived / (1024 * 1024 * 1024) % 1024);
                return string.Format(rm, g, m, k, b);
            }
        }
        /// <summary>
        /// 发送字节总数G/M/K/B
        /// </summary>
        public  string SendBytesData {
            get {
                string sm = "{0}(G),{1}(M),{2}(K),{3}(B)";
                int b = (int)(this.SentBytes % 1024);
                int k = (int)(this.SentBytes / 1024 % 1024);
                int m = (int)(this.SentBytes / (1024 * 1024) % 1024);
                int g = (int)(this.SentBytes / (1024 * 1024 * 1024) % 1024);
                return string.Format(sm, g, m, k, b);
            }
        }
        /// <summary>
        /// 接收字节总数G/M/K/B
        /// </summary>
        public  string RecivedBytesData {
            get {
                string rm = "{0}(G),{1}(M),{2}(K),{3}(B)";
                int b = (int)(this.ReceivedBytes % 1024);
                int k = (int)(this.ReceivedBytes / 1024 % 1024);
                int m = (int)(this.ReceivedBytes / (1024 * 1024) % 1024);
                int g = (int)(this.ReceivedBytes / (1024 * 1024 * 1024) % 1024);
                return string.Format(rm, g, m, k, b);
            }
        }
        public string GetNetworkInfo() {
            // string str = "模式:{0}\n接收:{0}字节 发送:{1}字节\n接收缓冲:{";
            StringBuilder sb = new StringBuilder();

            sb.Append("运行时间:");
            TimeSpan ts = m_SynTimer.Elapsed;
            sb.Append(string.Format("{0}天{1}小时{2}分{3}秒", ts.Days, ts.Hours, ts.Minutes, ts.Seconds));
            sb.Append("\n");

            sb.Append("模式:");
            sb.Append(IsPacketMode ? "分包" : "流");
            sb.Append("\n");

            sb.Append("前置数据:");
            sb.Append(IsPrepareModel ? "开启" : "关闭");
            if (IsPrepareModel) {
                sb.Append(" 前置数据长度:");
                sb.Append(PrepareDataSize);
            }
            sb.Append("\n");

            sb.Append("接收:");
            sb.Append(_bytesReceived);
            sb.Append(" 发送:");
            sb.Append(_bytesSent);
            sb.Append(" 单位(字节)\n");

            //sb.Append("接收缓冲:");
            //sb.Append(m_RecvBuf.ToString());
            //sb.Append(" 发送缓冲:");
            //sb.Append(m_SendBuf.ToString());
            sb.Append(" 申请缓存: " + (BufferManager.GlobalAllocatedMemory / 1024).ToString() + " K");
            sb.Append("\n");

            return sb.ToString();
        }
        private bool m_NoDelay = true;
        public bool NoDelay {
            get {
                return m_NoDelay;
            }
            set {
                if (_tcpSocket != null) {
                    _tcpSocket.NoDelay = value;
                }
                m_NoDelay = value;
            }
        }
        /// <summary>
        /// 是否采用缓冲发送的形式 
        /// </summary>
        private bool m_IsBuffered = false;
        /// <summary>
        /// 是否采用缓冲发送的形式 
        /// 只读
        /// </summary>
        public bool IsBuffered {
            get {
                return m_IsBuffered;
            }
        }
        /// <summary>
        ///接收网络包packet构造器
        /// </summary>
        public PacketCreatorManager PacketCreatorMgr_Recived = new PacketCreatorManager();
        /// <summary>
        /// 发送包构造器
        /// </summary>
        public PacketCreatorManager PacketCreatorMgr_Send = new PacketCreatorManager();
        /// <summary>
        /// 组件管理器 
        /// </summary>
        public ComponentManager ComponentMgr = new ComponentManager();
        /// <summary>
        /// 设定包的最大尺寸1k,大于该尺寸的包应该分包处理
        /// </summary>
        public const int MaxPacketSize = 1024 * 1;

        //private static readonly Logger Logs ;//= LogManager.GetCurrentClassLogger();

        //public const int MinBufferSize = 1024;
        /// <summary>
        ///const 8KB 默认
        /// </summary>
        public const int BufferSize = NetIOCPClientDef.MAX_PBUF_SEGMENT_SIZE;
        /// <summary>
        /// 8KB数据片段提供器
        /// </summary>
        private static readonly BufferManager Buffers = BufferManager.Default;

        /// <summary>
        /// Total number of bytes that have been received by all clients.
        /// 所有client接受的总字节数
        /// </summary>
        private static long _totalBytesReceived;

        /// <summary>
        /// Total number of bytes that have been sent by all clients.
        /// 所有client发送的总字节数
        /// </summary>
        private static long _totalBytesSent;

        /// <summary>
        /// Gets the total number of bytes sent to all clients.
        /// 所有client发送的总字节数
        /// </summary>
        public static long TotalBytesSent {
            get { return _totalBytesSent; }
        }

        /// <summary>
        /// Gets the total number of bytes received by all clients.
        /// 所有client接受的总字节数
        /// </summary>
        public static long TotalBytesReceived {
            get { return _totalBytesReceived; }
        }

        #region Private variables

        /// <summary>
        /// Number of bytes that have been received by this client.
        /// 当前client接收的字节数
        /// </summary>
        private uint _bytesReceived;

        /// <summary>
        /// Number of bytes that have been sent by this client.
        /// 当前client发送的字节数
        /// </summary>
        private uint _bytesSent;

        /// <summary>
        /// The socket containing the TCP connection this client is using.
        /// 当前client使用的socket 连接
        /// </summary>
        protected Socket _tcpSocket = null;//new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        /// <summary>
        /// Pointer to the server this client is connected to.
        /// </summary>
        //protected ServerBase _server;

        /// <summary>
        /// The port the client should receive UDP datagrams on.
        /// </summary>
        [Obsolete()]
        protected IPEndPoint _udpEndpoint;



        #endregion
        #region



        #endregion
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="server">The server this client is connected to.</param>
        protected NetClientBase(/*ServerBase server*/) {
            //_server = server;
            _bufferSegment = Buffers.CheckOut();
            //m_Buf[100] = new HeatbeatPacketCreator();
            //m_Buf[101] = new TimeSynPacketCreator();
            //m_Buf[99] = new CustomPacketCreator();
            //下面为默认构造器
            this.PacketCreatorMgr_Send.RegistePacket((ushort)100, new HeatbeatPacketCreator());
            this.PacketCreatorMgr_Send.RegistePacket((ushort)101, new TimeSynPacketCreator());
            this.PacketCreatorMgr_Send.RegistePacket((ushort)99, new CustomPacketCreator());
            //
            this.PacketCreatorMgr_Recived.RegistePacket((ushort)100, new HeatbeatPacketCreator());
            this.PacketCreatorMgr_Recived.RegistePacket((ushort)101, new TimeSynPacketCreator());
            this.PacketCreatorMgr_Recived.RegistePacket((ushort)99, new CustomPacketCreator());
            _InitClient();
        }
        public NetClientBase(string name)
            : this() {
            this.Name = name;
        }
        /// <summary>
        /// 客户端的基本初始化
        /// </summary>
        protected abstract void _InitClient();
        #region Public properties

        //public ServerBase Server
        //{
        //    get { return _server; }
        //}
        /// <summary>
        /// Gets the IP address of the client.
        /// 当前client IP
        /// </summary>
        public IPAddress ClientAddress {
            get {
                return (_tcpSocket != null && _tcpSocket.RemoteEndPoint != null) ?
                    ((IPEndPoint)_tcpSocket.RemoteEndPoint).Address : null;
            }
        }

        /// <summary>
        /// Gets the port the client is communicating on.
        /// 当前client 端口
        /// </summary>
        public int Port {
            get {
                return (_tcpSocket != null && _tcpSocket.RemoteEndPoint != null) ?
                    ((IPEndPoint)_tcpSocket.RemoteEndPoint).Port : -1;
            }
        }

        /// <summary>
        /// Gets the port the client should receive UDP datagrams on.
        /// </summary>
        //[Obsolete()]
        //public IPEndPoint UdpEndpoint {
        //    get { return _udpEndpoint; }
        //    set { _udpEndpoint = value; }
        //}

        /// <summary>
        /// Gets/Sets the socket this client is using for TCP communication.
        /// 当前tcp socket
        /// </summary>
        public Socket @Socket {
            get { return _tcpSocket; }
            set {
                if (_tcpSocket != null && _tcpSocket.Connected) {
                    _tcpSocket.Shutdown(SocketShutdown.Both);
                    _tcpSocket.Close();
                }

                if (value != null) {
                    _tcpSocket = value;
                }
            }
        }
        /// <summary>
        /// 当前client接收的字节数
        /// </summary>
        public uint ReceivedBytes {
            get { return _bytesReceived; }
        }
        /// <summary>
        /// 当前client发送的字节数
        /// </summary>
        public uint SentBytes {
            get { return _bytesSent; }
        }
        /// <summary>
        /// 当前client socket的连接状态,true连接
        /// </summary>
        public bool IsConnected {
            get { return _tcpSocket != null && _tcpSocket.Connected; }
        }
        private bool m_Connected = false;
        #endregion
        public void Close() {
            Dispose();
        }
        /// <summary>
        /// Begins asynchronous TCP receiving for this client.
        /// 开始 异步TCP 接收数据
        /// </summary>
        protected void BeginReceive() {
            ResumeReceive();
        }

        /// <summary>
        /// Resumes asynchronous TCP receiving for this client.
        ///  重新开始 异步TCP 接收数据
        /// </summary>
        private void ResumeReceive() {  //内部循环调用? 
            if (_tcpSocket != null && _tcpSocket.Connected) {
                SocketAsyncEventArgs socketArgs = SocketHelpers.AcquireSocketArg();
                int offset = this._offset + this._remainingLength;

                socketArgs.SetBuffer(this._bufferSegment.Buffer.Array, this._bufferSegment.Offset + offset, BufferSize - offset);
                socketArgs.UserToken = this;
                socketArgs.Completed += ReceiveAsyncComplete;

                bool willRaiseEvent = _tcpSocket.ReceiveAsync(socketArgs);
                //返回结果：Type: System.Boolean
                //如果 I/O 操作挂起，将返回 true。操作完成时，将引发 e 参数的 SocketAsyncEventArgs.Completed 事件。
                //如果 I/O 操作同步完成，将返回 false。在这种情况下，将不会引发 e 参数的 SocketAsyncEventArgs.Completed 事件，并且可能在方法调用返回后立即检查作为参数传递的 e 对象以检索操作的结果。

                if (!willRaiseEvent) {//返回false 命令还没有执行,直接处理？
                    ReceiveAsyncComplete(_tcpSocket, socketArgs);
                }
            }
        }
        /// <summary>
        /// 处理异步接受的数据
        /// </summary>
        /// <param name="args"></param>
        private void ProcessRecieve(SocketAsyncEventArgs args) {
            try {
                int bytesReceived = args.BytesTransferred;

                if (bytesReceived == 0)
                //if (args.SocketError != SocketError.Success)
				{ //ToDo:这里可能已经断开连接了
                    // no bytes means the client disconnected, so clean up!
                    //_server.DisconnectClient(this, true);
                    if (args.SocketError != SocketError.Success) {
                        Logs.Error("网络连接错误.错误码:" + args.SocketError.ToString());
                        this.DisConnect();
                        if (OnRecvError != null) {
                            OnRecvError(this, new NetException("接收错误，网络连接错误.错误码:" + args.SocketError.ToString()));
                        }
                    }
                    else {
                        ResumeReceive();
                    }
                }
                else {
                    // increment our counters
                    unchecked {
                        _bytesReceived += (uint)bytesReceived;
                    }

                    Interlocked.Add(ref _totalBytesReceived, bytesReceived);

                    this._remainingLength += bytesReceived;
                    if (OnRecvDataBytes != null) {
                        OnRecvDataBytes(this, this._bufferSegment, bytesReceived);//引发接收数据事件
                    }
                    //长度最大为64k
                    ushort needlargeLen = 0;
                    if (DoReceive(this._bufferSegment, out needlargeLen)) {   //处理完成
                        // packet processed entirely
                        _offset = 0;
                        this._bufferSegment.DecrementUsage();//回收
                        this._bufferSegment = Buffers.CheckOut();//获取新的片段
                    }
                    else {   //还没处理完 
                        if (needlargeLen == 0) {
                            EnsureBuffer();//把没有处理完的数据 放入新的片段开始
                        }
                        else {
                            throw new NetException("packet包长度异常");
                        }
                    }

                    ResumeReceive();
                }
            }
            catch (ObjectDisposedException ee1) {
                //_server.DisconnectClient(this, true);
                Logs.Error("接收数据时发生已释放对象错误：" + ee1.ToString());
                this.DisConnect();
                if (OnRecvError != null) {
                    OnRecvError(this, new NetException("接收数据时发生已释放对象错误:" + ee1.ToString()));
                }
            }
            catch (Exception ee2) {
                //_server.Warning(this, e);
                //_server.DisconnectClient(this, true);
                Logs.Error("接收数据时发生错误：" + ee2.ToString());
                this.DisConnect();
                if (OnRecvError != null) {
                    OnRecvError(this, new NetException("接收数据时发生错误:" + ee2.ToString()));
                }
            }
            finally {
                args.Completed -= ReceiveAsyncComplete;
                SocketHelpers.ReleaseSocketArg(args);
            }
        }

        private void ReceiveAsyncComplete(object sender, SocketAsyncEventArgs args) {
            ProcessRecieve(args);
        }

        /// <summary>
        /// Makes sure the underlying buffer is big enough (but will never exceed BufferSize)
        /// </summary>
        /// <param name="size"></param>
        protected void EnsureBuffer() //(int size)
        {
            //if (size > BufferSize - _offset)
            {
                // not enough space left in buffer: Copy to new buffer
                var newSegment = Buffers.CheckOut();
                Array.Copy(_bufferSegment.Buffer.Array,
                    _bufferSegment.Offset + _offset,
                    newSegment.Buffer.Array,
                    newSegment.Offset,
                    _remainingLength);
                _bufferSegment.DecrementUsage();
                _bufferSegment = newSegment;
                _offset = 0;
            }
        }

        /// <summary>
        /// The buffer containing the data received.
        /// 接收数据片段,默认是8kb
        /// </summary>
        protected BufferSegment _bufferSegment;

        /// <summary>
        /// The offset in the buffer to write at.
        /// 当前buffer写入的偏移位置
        /// </summary>
        protected int _offset;
        /// <summary>
        ///当前_bufferSegment 接受到的长度
        /// </summary>
        protected int _remainingLength;
        /// <summary>
        /// Called when a packet has been received and needs to be processed.
        /// </summary>
        /// <param name="numBytes">The size of the packet in bytes.</param>
        protected virtual bool DoReceive(BufferSegment segment, out ushort needLargeLen) {
            needLargeLen = 0;
            //处理包
            byte[] recvBuffer = segment.Buffer.Array;
            int i = 1;
            //
            do {
                if (_remainingLength < Packet.HeadSize) {
                    //小于包头长度，取不到包的大小,继续收取数据
                    return false;
                }
                int offset = segment.Offset + this._offset;//总偏移位置
                int headSize = Packet.HeadSize;
                int packetFullLength = 0;//包的总尺寸
                ushort packetId = BitConverter.ToUInt16(recvBuffer, offset);//PacketHelper.Reader.PeekUShort(segment, 0);
                ushort packetlen = BitConverter.ToUInt16(recvBuffer, offset + 2);
                packetFullLength = packetlen + headSize;
                //判断
                if (packetFullLength > BufferSize) {
                    //如果包总长度>默认缓存 8K，则需要建立一个新包，用于下次完整的接收包
                    //但是我们的包不可能超过该长度
                    // packet is just too big        
                    Logs.Error(string.Format("接收到的包:{0}长度:{1} >8k,应该不会出现这个大的包,数据异常，客户端主动断开..."));
                    //包不正常了，断开连接就好 
                    this.DisConnect();
                    if (OnRecvError != null) {
                        OnRecvError(this, new NetException(string.Format("包:{0} 长度不正常.长度:{1}", packetId, packetFullLength)));
                    }

                    return false;
                }
                if (_remainingLength < packetFullLength) {
                    //收到的数据小于包的尺寸,里面不能接满
                    return false;
                }
                try {
                    PacketCreator creator = this.PacketCreatorMgr_Recived.GetPacketCreator(packetId);
                    if (creator == null) {
                        Debug.Assert(false, string.Format("未注册的包类型.packetId:{0} ", packetId));
                        Logs.Error(string.Format("不存在相应的包构造器.packetId:{0} ", packetId));
                    }
                    else {
#if DEBUG
                        Console.WriteLine("包缓存池信息:" + creator.GetPoolInfo().ToString());
#endif
                        //接收包处理
                        Packet p = creator.CreatePacket();
                        BufferSegment oldseg = p.Buffer;
                        int oldLen = (oldseg == null) ? 0 : oldseg.Length;
                        //保证包的片段能存入比原始内容长的数据
                        p.Buffer = BufferManager.GetSegment(packetFullLength > oldLen ? packetFullLength : oldLen);//取新的片段，根据长度
                        //把当前包的数据复制到关联片段内
                        p.Buffer.CopyStartFromBytes(0, recvBuffer, offset, packetFullLength);
                        if (oldseg != null) {
                            oldseg.DecrementUsage();//回收老片段,因为同样的包大小一般是一致的，所以其利用率还是比较高的
                        }
                        //心跳包只发送不返回的，所以接收数据里不需要处理
                        switch (p.PacketID) {
                            case 101:
                                p.Read(p.Buffer);
                                OnRecvTimeSyn(p);
                                break;
                            case 100:
                                break;
                            default:
                                m_RecivePackets.Enqueue(p);//加入到处理列队
                                break;

                        }
                        //if (p.PacketID == 101) {//时间同步 直接处理
                        //    p.Read(p.Buffer);
                        //    OnRecvTimeSyn(p);
                        //}
                        //else {
                        //    m_RecivePackets.Enqueue(p);//加入到处理列队

                        //}
                    }
                }
                catch {
                    Logs.Error(string.Format("包构造器.packetId:{0} 超出范围", packetId));
                }
                //继续处理下一个包
                _remainingLength -= packetFullLength;
                _offset += packetFullLength;
                i++;
            } while (this._remainingLength > 0);
            return true;
        }


        #region 基础异步IOCP发送,一般不使用
        /// <summary>
        /// Asynchronously sends a packet of data to the client.
        /// </summary>
        /// <param name="packet">An array of bytes containing the packet to be sent.</param>
        internal void Send(byte[] packet) {
            Send(packet, 0, packet.Length);
        }

        private void SendCopy(byte[] packet) {
            var copy = new byte[packet.Length];
            Array.Copy(packet, copy, packet.Length);
            Send(copy, 0, copy.Length);
        }

        private void Send(BufferSegment segment, int length) {
            Send(segment.Buffer.Array, segment.Offset, length);
        }



        /// <summary>
        /// Asynchronously sends a packet of data to the client.
        /// 单独直接发送指定的 byte[]数据内容到服务端，这里不对byte[] packet进行包装或者修改,外面保证其内容的合法性
        /// </summary>
        /// <param name="packet">An array of bytes containing the packet to be sent.</param>
        /// <param name="length">长度大小有限制，不能超64K。The number of bytes to send starting at offset.</param>
        /// <param name="offset">The offset into packet where the sending begins.</param>
        public virtual void Send(byte[] packet, int offset, int length) {
            if (_tcpSocket != null && _tcpSocket.Connected) {
                SocketAsyncEventArgs args = SocketHelpers.AcquireSocketArg();
                if (args != null) {
                    args.Completed += SendAsyncComplete;
                    //if packet内容太多，需要分包发送
                    args.SetBuffer(packet, offset, length);
                    //BufferSegment sendseg = BufferManager.GetSegment(length);
                    //sendseg.CopyStartFromBytes(0, packet, offset, length);
                    //PacketCreator creater=PacketCreatorMgr.GetPacketCreator(CustomPacket._PacketID);
                    //CustomPacket sp = null;
                    //if (creater != null) {
                    //    sp =creater.CreatePacket() as CustomPacket;
                    //    sp.Token = this;
                    //    sp.MsgData = packet;
                    //    sp.Write();
                    //    args.UserToken = sp;
                    //}
                    ////开始发送
                    if (OnSend != null) {
                        OnSend(this, null);
                    }
                    bool doResult = false;
                    try {
                        doResult = _tcpSocket.SendAsync(args);
                    }
                    catch {
                        int edoResult = 0;
                    }
                    if (!doResult && _tcpSocket != null) {
                        SendAsyncComplete(_tcpSocket, args);
                    }
                    unchecked {
                        _bytesSent += (uint)length;
                    }

                    Interlocked.Add(ref _totalBytesSent, length);
                }
                else {
                    Logs.Error(string.Format("Client {0}'s SocketArgs are null", this._tcpSocket.ToString()));
                }
            }
        }
        #endregion

        #region 基于包Packet列队 异步IOCP模式发送

        /// <summary>
        /// 把要发送的数据包加入到发送缓存里面
        /// </summary>
        /// <param name="packet"></param>
        public void Send(Packet packet) {

            if (IsPrepareModel) {
                //ToDo:
            }
            else {
                bool needSend = (m_SendPackets.Count == 0);
                m_SendPackets.Enqueue(packet);
                //如果在此之前，数据空了，那就发一次
                if (needSend) {
                    PeekSend();
                }
            }
        }
        /// <summary>
        /// 获取指定ID类型的包，内部数据需要外面去写入,这样有利于包的重复利用以及
        /// 节约反复内存申请的开支
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="packetId"></param>
        /// <returns></returns>
        protected T CreateRecivedPacket<T>(ushort packetId) where T : Packet {
            PacketCreator creator = this.PacketCreatorMgr_Recived.GetPacketCreator(packetId);
            if (creator == null) {
                Debug.Assert(false, string.Format("未注册的包类型.packetId:{0} ", packetId));
                Logs.Error(string.Format("不存在相应的包构造器.packetId:{0} ", packetId));
            }
            else {
                //接收包处理
                Packet p = creator.CreatePacket();
                //P内的数据还没有写入 外面注意写入
                return p as T;
            }
            return null;
        }
        public T CreatePacketToSend<T>(ushort packetId) where T : Packet {
            PacketCreator creator = this.PacketCreatorMgr_Send.GetPacketCreator(packetId);
            if (creator == null) {
                Debug.Assert(false, string.Format("未注册的包类型.packetId:{0} ", packetId));
                Logs.Error(string.Format("不存在相应的包构造器.packetId:{0} ", packetId));
            }
            else {
#if DEBUG
                Console.WriteLine("包缓存池信息:"+creator.GetPoolInfo().ToString());
#endif
                //接收包处理
                Packet p = creator.CreatePacket();
                //P内的数据还没有写入 外面注意写入
                return p as T;
            }
            return null;
        }
        /// <summary>
        /// 把要发送的数据包加入到发送缓存里面
        /// </summary>
        public void AddPacketToSend(Packet packet) {
            this.m_SendPackets.Enqueue(packet);
        }
        /// <summary>
        /// 把要发送的数据包加入到发送缓存里面
        /// </summary>
        public void AddPacketToSend(System.Collections.Generic.ICollection<Packet> packets) {
            foreach (var v in packets) {
                this.m_SendPackets.Enqueue(v);
            }
        }
        /// <summary>
        /// 即时异步发送数据包
        /// </summary>
        /// <param name="packet"></param>
        protected void SendPacketImmediate(Packet packetdata) {
            if (_tcpSocket != null && _tcpSocket.Connected) {
                var args = SocketHelpers.AcquireSocketArg();
                if (args != null) {
                    byte[] packet = packetdata.Buffer.Buffer.Array;
                    int offset = packetdata.Buffer.Offset;
                    int length = packetdata.PacketBufLen;
                    args.Completed += SendAsyncComplete;
                    //packet关联的数据段 大小应该有限制的
                    args.SetBuffer(packet, offset, length);
                    packetdata.Token = this;//可能使用的
                    args.UserToken = packetdata;//发送完即可回收包
                    //开始发送
                    if (OnSend != null) {
                        OnSend(this, packetdata);
                    }
                    bool doResult = _tcpSocket.SendAsync(args);
                    if (!doResult) {
                        SendAsyncComplete(_tcpSocket, args);
                    }
                    unchecked {
                        _bytesSent += (uint)length;
                    }

                    Interlocked.Add(ref _totalBytesSent, length);
                }
                else {
                    Logs.Error(string.Format("Client {0}'s SocketArgs are null", this._tcpSocket.ToString()));
                }
            }

        }

        /// <summary>
        /// 发送包列队
        /// </summary>
        NetQueue<Packet> m_SendPackets = new NetQueue<Packet>(1024);
        /// <summary>
        /// 接收包列队
        /// </summary>
        NetQueue<Packet> m_RecivePackets = new NetQueue<Packet>(1024);
        protected void PeekSend() {
            if (m_IsClosed || !m_Connected)
                return;
            if (m_SendPackets.Count > 0) {
                Packet packet = null;
                m_SendPackets.TryDequeue(out packet);
                if (packet != null) {
                    SendPacketImmediate(packet);
                }
            }
        }
        /// <summary>
        /// 每一帧中处理的包数量上限
        /// </summary>
        public int ProcessPacketCountInOneFrame = 1000000;
        /// <summary>
        /// 发送/接收缓存包 处理驱动
        /// </summary>
        public void ProcessSendAndRecivedPackets() {
            if (m_IsClosed || !m_Connected) {
                return;
            }
            if (!IsConnected) return;//没有连接 直接返回
            this.ProcessTimeSyn();//事件同步
            this.SendHeatBeat();  //保持心跳
            int pp1f = 0;
            while (m_SendPackets.Count > 0) {
                pp1f++;
                if (pp1f >= ProcessPacketCountInOneFrame) {
                    break;//每次最大发送数量
                }
                //对处理速度限制
                PeekSend();
            }
            //对接收包处理
            while (m_RecivePackets.Count > 0) {
                Packet p = null;//这里处理包的回收
                int pid = -1;
                try {
                    m_RecivePackets.TryDequeue(out p);//处理包
                    if (p != null && OnRecvData != null) {
                        pid = p.PacketID;
#if DEBUG
                        ProfInstance pi = Profile.StartProf();
                        OnRecvData(this, p);//引发数据包接收事件 ,内容还没有读取的                    
                        pi.EndProf(p.ToString(), 1f);
#else
                        OnRecvData(this, p);//引发数据包接收事件 ,内容还没有读取的     
#endif
                        //if (PacketHandles[p.PacketID] != null) {
                        //    PacketHandles[p.PacketID](0L, p);
                        //}
                        //else {
                        //    //Debug.Assert(false, p.ToString() + "没有相应的解析委托");
                        //    Console.WriteLine("包" + p.PacketID + "没有相应的解析委托");
                        //}
                    }
                    this.HandleReceviedPackets(this, p);//子类里具体操作对包的处理方式
                }
                catch {
                    Logs.Error("处理包:" + pid.ToString() + "  过程中发生错误！");
                }
                finally {
                    //回收包
                    if (p != null) {
                        this.PacketCreatorMgr_Recived.GetPacketCreator(p.PacketID).RecylePacket(p);
                    }
                }

            }
        }
        protected abstract void HandleReceviedPackets(NetClientBase client, Packet recivedPacket);
        #endregion


        private void SendAsyncComplete(object sender, SocketAsyncEventArgs args) {

            args.Completed -= SendAsyncComplete;
            try {
                //发送失败怎么处理？
                if (args.BytesTransferred == 0) {//说明连接断开了
                    if (args.SocketError != SocketError.Success) {
                        Logs.Error("send error:" + args.SocketError.ToString());

                        Logs.Warn("ClientNetwork Name:" + this.Name + " 因网络问题,数据尚未发送即失败." + args.SocketError.ToString());
                        this.DisConnect();
                        if (OnSendError != null) {
                            OnSendError((args.UserToken as Packet), new NetException("socket sendAsyn 错误号:" + args.SocketError.ToString()));
                        }
                    }
                    //  发送传输为0，目标方应该断开连接了，这里进入断开环节。
                    //isSending = false;
                    //CloseSocket();
                    return;
                }
                switch (args.LastOperation) {
                    case SocketAsyncOperation.Connect:
                        //不会出现该状态
                        break;
                    case SocketAsyncOperation.Send:
                    case SocketAsyncOperation.SendTo:
                        //发送长度检查
                        Packet packet = (args.UserToken as Packet);
                        if (packet != null && args.BytesTransferred < packet.PacketBufLen) {
                            //TODO:当前缓存没有发送完成
                            int i = 0; ;
                        }
                        else {
                        }
                        break;
                    default:
                        //不会出现该状态
                        if (args.SocketError != SocketError.Success) {
                            //tOdO:发送失败
                            Logs.Error("send error:" + args.SocketError.ToString());
                            this.DisConnect();
                            if (OnSendError != null) {
                                OnSendError((args.UserToken as Packet), new NetException("socket sendAsyn 错误号:" + args.SocketError.ToString()));
                            }

                        }

                        break;
                }
            }
            finally {
                Packet packet = (args.UserToken as Packet);
                if (packet != null) {
                    //回收包,以便重复利用
                    (packet.Token as NetClientBase).PacketCreatorMgr_Send.GetPacketCreator(packet.PacketID).RecylePacket(packet);
                }
                SocketHelpers.ReleaseSocketArg(args);
            }
        }


        ///// <summary>
        ///// Connects the client to the server at the specified address and port.
        ///// </summary>
        ///// <remarks>This function uses IPv4.</remarks>
        ///// <param name="addr">The IP address of the server to connect to.</param>
        ///// <param name="port">The port to use when connecting to the server.</param>
        //[Obsolete()]
        //public void Connect(IPAddress addr, int port) {
        //    if (_tcpSock != null) {
        //        if (_tcpSock.Connected) {
        //            _tcpSock.Disconnect(true);
        //        }
        //        _tcpSock.Connect(addr, port);

        //        BeginReceive();
        //    }
        //}

        #region

        /// <summary>
        /// 接收数据上下文对象
        /// </summary>
        internal SocketAsyncEventArgs ConnectEventArgs { get; private set; }
        void OnConnectCompleted(object sender, SocketAsyncEventArgs e) {
            isBeginConnect = false;
            Socket Socket = _tcpSocket;
            if (e.SocketError != SocketError.Success) {
                if (e.AcceptSocket != Socket) {
                    Logs.Error("Socket 重复连接错误");
                    Socket = e.AcceptSocket;
                }
                try {
                    m_Connected = false;
                    if (OnConncetEnd != null) {
                        OnConncetEnd(e.SocketError);
                    }
                    //  连接失败
                    //if (SocketConnect != null) {//ToDo: 引发没有连接成功事件
                    //    try {
                    //        SocketConnect(this, new SocketConnectEventArgs<T> {
                    //            IsConnected = false,
                    //            SocketError = e.SocketError,
                    //            Session = this
                    //        });
                }
                catch (Exception ex) {
                    Logs.Error("OnConnectCompleted event error.", ex);
                }
                m_IsClosed = false;
                //没有连接成功 返回
                return;
            }
            m_Connected = Socket.Connected;
            if (OnConncetEnd != null) {
                OnConncetEnd(e.SocketError);
            }
            if (m_Connected) {
                m_SynTimer.Reset();
                m_SynTimer.Start();
                Status = ClientNetworkStatus.Connected;
                //StartHeatbeat();//开启心跳包 ，连接成功后
            }
            BeginReceive();

        }

        private bool isBeginConnect = false;
        private bool m_IsClosed = false;
        public IPEndPoint ServerEndPoint {
            get;
            private set;
        }
        /// <summary>
        /// 先服务器发起一个连接
        /// 这是一个异步方法
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public void BeginConnect(string ip, int port) {

            if (isBeginConnect && this._tcpSocket != null) {
                Logs.Error("replace connecting...");
                //已经处于连接状态，不能再连接
                return;
            }
            isBeginConnect = true;
            if (_tcpSocket != null) {
                try {
                    _tcpSocket.Shutdown(SocketShutdown.Both);
                }
                catch {
                }
                try {
                    _tcpSocket.Close(1);
                }
                catch {
                }
                Thread.Sleep(100);
                this.m_Connected = false;
            }
            _InitSocket();

            IPAddress address;
            if (IPAddress.TryParse(ip, out address)) {
                if (ConnectEventArgs == null)   //  如果之前连接被关闭了，这里会重新对连接的数据做初始化
                {
                    // Init();
                    ConnectEventArgs = new SocketAsyncEventArgs();
                    ConnectEventArgs.Completed += OnConnectCompleted;
                }
                if (OnConnectBegin != null) {
                    OnConnectBegin(this, EventArgs.Empty);
                }
                this.ServerEndPoint = new IPEndPoint(address, port);
                ConnectEventArgs.RemoteEndPoint = this.ServerEndPoint;//new IPEndPoint(address, port);
                bool doResult = _tcpSocket.ConnectAsync(ConnectEventArgs);
                if (!doResult) {
                    OnConnectCompleted(_tcpSocket, ConnectEventArgs);
                }
            }
            else {
                isBeginConnect = false;//IP不正确
                Logs.Error("ip is error: " + ip);
                m_Connected = false;
                if (OnConncetEnd != null) {
                    OnConncetEnd(SocketError.AddressNotAvailable);
                }
            }
        }
        /// <summary>
        /// 初始化
        /// </summary>
        private void _InitSocket() {
            this._tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _tcpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, false);
            _tcpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, false);
            LingerOption lo = new LingerOption(true, 5);
            _tcpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, lo);
            _tcpSocket.NoDelay = m_NoDelay;
            _offset = 0;
            _remainingLength = 0;
            this._bufferSegment.DecrementUsage();//回收
            this._bufferSegment = Buffers.CheckOut();//获取新的片段
        }

        /// <summary>
        /// 客户端主动立即与服务器断开连接
        /// </summary>
        public void DisConnect() {
            Status = ClientNetworkStatus.DisConnected;
            if (_tcpSocket != null) {
                if (_tcpSocket.Connected) {
                    try {
                        _tcpSocket.Disconnect(true);
                    }
                    catch { }
                    try {
                        _tcpSocket.Shutdown(SocketShutdown.Both);
                    }
                    catch {
                    }
                }
                //断开连接时不要释放_tcpSocket
                //因为可能在接收或发送数据中途移除需要断开，后续的操作可能还会用到_tcpSocket
                //如果释放，可能会造成异常
                // _tcpSock.Close();               
                m_Connected = false;
                //m_IsClosed = true;
                StopHeatBeat();
                //
                ClearSendAndRecivedQuene();
            }
            if (OnDisconnect != null) {
                OnDisconnect(this, EventArgs.Empty);
            }
        }
        /// <summary>
        /// 回收没有发送与接收的包
        /// </summary>
        void ClearSendAndRecivedQuene() {
            while (m_SendPackets.Count > 0) {
                Packet p = null;
                m_SendPackets.TryDequeue(out p);
                if (p != null) {
                    this.PacketCreatorMgr_Send.GetPacketCreator(p.PacketID).RecylePacket(p);
                }
            }
            while (m_RecivePackets.Count > 0) {
                Packet p = null;
                m_RecivePackets.TryDequeue(out p);
                if (p != null) {
                    this.PacketCreatorMgr_Recived.GetPacketCreator(p.PacketID).RecylePacket(p);
                }
            }
        }
        #endregion
        #region IDisposable

        ~NetClientBase() {
            Dispose(false);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            m_IsClosed = true;
            Status = ClientNetworkStatus.DisConnected;
            m_Connected = false;
            ClearSendAndRecivedQuene();
            _bufferSegment.DecrementUsage();
            if (_tcpSocket != null) {
                try {
                    if (_tcpSocket.Connected) {
                        LingerOption lo = new LingerOption(true, 2);
                        try {
                            _tcpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, lo);
                        }
                        catch { }
                        _tcpSocket.Shutdown(SocketShutdown.Both);
                    }
                    _tcpSocket.Close();
                    _tcpSocket = null;
                }
                catch (SocketException/* exception*/) {
                    // TODO: Check what exceptions we need to handle
                }

            }
        }

        #endregion

        public override string ToString() {
            return
                (this.Socket == null || !this.Socket.Connected
                     ? "<disconnected client>"
                     : (this.Socket.RemoteEndPoint ?? (object)"<unknown client>")).ToString();
        }
    }

}
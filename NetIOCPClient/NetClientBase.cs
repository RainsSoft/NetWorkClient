

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using NetIOCPClient.Network;
using NetIOCPClient.Log;
using NetIOCPClient.Core;
//using NLog;

namespace NetIOCPClient
{
    public delegate void NetworkExceptionHandler(object sender, Exception e);
    /// <summary>
    /// �յ����ݰ�
    /// </summary>
    /// <param name="connIndex">����ID</param>
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
        public event EventHandler OnConnectBegin;//��ʼ����
        public event Action<SocketError> OnConncetEnd;//���ӽ��
        //public event EventHandler OnConncetTimeOut;//����ʧ��

        //private NetworkExceptionHandler m_OnSendError = null;
        /// <summary>
        /// ����ʧ���¼������ܻ����߳�������������֤�̰߳�ȫ
        /// </summary>
        public event NetworkExceptionHandler OnSendError;
        public event NetworkExceptionHandler OnRecvError;
        /// <summary>
        /// �Ͽ��¼�����������DisConnect����Close�������������¼���
        /// </summary>
        public event EventHandler OnDisconnect;
        public event SendEventHandler OnSend;
        /// <summary>
        /// ���յ�������ʱ�������¼�,IsPacketModeʱ��Ч
        /// ����㲥������ִ���¼���ʱ�������ݹ㲥����ID��ִ����Ӧ�Ĳ���
        /// </summary>
        //public static PacketHasNoRPCCallbackHandler OnRecivedBCPacketEvent;
        public event ClientRecvDataHandler OnRecvData;
        public ClientNetworkStatus Status;
        /// <summary>
        /// ���յ�������ʱ�������¼�,IsPacketMode=falseʱ��Ч
        /// </summary>
        public event RecvDataBytesHandler OnRecvDataBytes;

        /// <summary>
        /// �Ƿ���ð�ģʽ,Ĭ��Ϊtrue
        /// ������ǲ��ð�ģʽ.
        /// </summary>
        public bool IsPacketMode = true;
        public string Name = string.Empty;
        /// <summary>
        /// �Ƿ�����ǰ�ô���.
        /// ���������idǰ�ı�ʶ������
        /// </summary>
        public bool IsPrepareModel = false;
        /// <summary>
        /// ǰ�ô�������ݳ���
        /// </summary>
        internal int PrepareDataSize = 0;
        /// <summary>
        /// �����ֽ�����G/M/K/B
        /// </summary>
        public string SendTotalByteData {
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
        /// �����ֽ�����G/M/K/B
        /// </summary>
        public string RecivedTotalByteData {
            get {
                string rm = "{0}(G),{1}(M),{2}(K),{3}(B)";
                int b = (int)(TotalBytesReceived % 1024);
                int k = (int)(TotalBytesReceived / 1024 % 1024);
                int m = (int)(TotalBytesReceived / (1024 * 1024) % 1024);
                int g = (int)(TotalBytesReceived / (1024 * 1024 * 1024) % 1024);
                return string.Format(rm, g, m, k, b);
            }
        }
        public string GetNetworkInfo() {
            // string str = "ģʽ:{0}\n����:{0}�ֽ� ����:{1}�ֽ�\n���ջ���:{";
            StringBuilder sb = new StringBuilder();

            sb.Append("����ʱ��:");
            TimeSpan ts = m_SynTimer.Elapsed;
            sb.Append(string.Format("{0}��{1}Сʱ{2}��{3}��", ts.Days, ts.Hours, ts.Minutes, ts.Seconds));
            sb.Append("\n");

            sb.Append("ģʽ:");
            sb.Append(IsPacketMode ? "�ְ�" : "��");
            sb.Append("\n");

            sb.Append("ǰ������:");
            sb.Append(IsPrepareModel ? "����" : "�ر�");
            if (IsPrepareModel) {
                sb.Append(" ǰ�����ݳ���:");
                sb.Append(PrepareDataSize);
            }
            sb.Append("\n");

            sb.Append("����:");
            sb.Append(_bytesReceived);
            sb.Append(" ����:");
            sb.Append(_bytesSent);
            sb.Append(" ��λ(�ֽ�)\n");

            //sb.Append("���ջ���:");
            //sb.Append(m_RecvBuf.ToString());
            //sb.Append(" ���ͻ���:");
            //sb.Append(m_SendBuf.ToString());
            sb.Append(" ���뻺��: " + (BufferManager.GlobalAllocatedMemory / 1024).ToString() + " K");
            sb.Append("\n");

            return sb.ToString();
        }
        private bool m_NoDelay = true;
        public bool NoDelay {
            get {
                return m_NoDelay;
            }
            set {
                if (_tcpSock != null) {
                    _tcpSock.NoDelay = value;
                }
                m_NoDelay = value;
            }
        }
        /// <summary>
        /// �Ƿ���û��巢�͵���ʽ 
        /// </summary>
        private bool m_IsBuffered = false;
        /// <summary>
        /// �Ƿ���û��巢�͵���ʽ 
        /// ֻ��
        /// </summary>
        public bool IsBuffered {
            get {
                return m_IsBuffered;
            }
        }
        /// <summary>
        /// �����packet������
        /// </summary>
        public PacketCreatorManager PacketCreatorMgr = new PacketCreatorManager();
        /// <summary>
        /// ���������
        /// </summary>
        public ComponentManager ComponentMgr = new ComponentManager();
        /// <summary>
        /// �趨�������ߴ�1k,���ڸóߴ�İ�Ӧ�÷ְ�����
        /// </summary>
        public const int MaxPacketSize = 1024 * 1;

        //private static readonly Logger Logs ;//= LogManager.GetCurrentClassLogger();

        //public const int MinBufferSize = 1024;
        /// <summary>
        ///const 8KB Ĭ��
        /// </summary>
        public const int BufferSize = NetIOCPClientDef.MAX_PBUF_SEGMENT_SIZE;
        /// <summary>
        /// 8KB����Ƭ���ṩ��
        /// </summary>
        private static readonly BufferManager Buffers = BufferManager.Default;

        /// <summary>
        /// Total number of bytes that have been received by all clients.
        /// ����client���ܵ����ֽ���
        /// </summary>
        private static long _totalBytesReceived;

        /// <summary>
        /// Total number of bytes that have been sent by all clients.
        /// ����client���͵����ֽ���
        /// </summary>
        private static long _totalBytesSent;

        /// <summary>
        /// Gets the total number of bytes sent to all clients.
        /// ����client���͵����ֽ���
        /// </summary>
        public static long TotalBytesSent {
            get { return _totalBytesSent; }
        }

        /// <summary>
        /// Gets the total number of bytes received by all clients.
        /// ����client���ܵ����ֽ���
        /// </summary>
        public static long TotalBytesReceived {
            get { return _totalBytesReceived; }
        }

        #region Private variables

        /// <summary>
        /// Number of bytes that have been received by this client.
        /// ��ǰclient���յ��ֽ���
        /// </summary>
        private uint _bytesReceived;

        /// <summary>
        /// Number of bytes that have been sent by this client.
        /// ��ǰclient���͵��ֽ���
        /// </summary>
        private uint _bytesSent;

        /// <summary>
        /// The socket containing the TCP connection this client is using.
        /// ��ǰclientʹ�õ�socket ����
        /// </summary>
        protected Socket _tcpSock = null;//new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

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
        }
        public NetClientBase(string name)
            : this() {
            this.Name = name;
        }
        #region Public properties

        //public ServerBase Server
        //{
        //    get { return _server; }
        //}
        /// <summary>
        /// Gets the IP address of the client.
        /// ��ǰclient IP
        /// </summary>
        public IPAddress ClientAddress {
            get {
                return (_tcpSock != null && _tcpSock.RemoteEndPoint != null) ?
                    ((IPEndPoint)_tcpSock.RemoteEndPoint).Address : null;
            }
        }

        /// <summary>
        /// Gets the port the client is communicating on.
        /// ��ǰclient �˿�
        /// </summary>
        public int Port {
            get {
                return (_tcpSock != null && _tcpSock.RemoteEndPoint != null) ?
                    ((IPEndPoint)_tcpSock.RemoteEndPoint).Port : -1;
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
        /// ��ǰtcp socket
        /// </summary>
        public Socket @Socket {
            get { return _tcpSock; }
            set {
                if (_tcpSock != null && _tcpSock.Connected) {
                    _tcpSock.Shutdown(SocketShutdown.Both);
                    _tcpSock.Close();
                }

                if (value != null) {
                    _tcpSock = value;
                }
            }
        }
        /// <summary>
        /// ��ǰclient���յ��ֽ���
        /// </summary>
        public uint ReceivedBytes {
            get { return _bytesReceived; }
        }
        /// <summary>
        /// ��ǰclient���͵��ֽ���
        /// </summary>
        public uint SentBytes {
            get { return _bytesSent; }
        }
        /// <summary>
        /// ��ǰclient socket������״̬,true����
        /// </summary>
        public bool IsConnected {
            get { return _tcpSock != null && _tcpSock.Connected; }
        }
        private bool m_Connected = false;
        #endregion
        public void Close() {
            Dispose();
        }
        /// <summary>
        /// Begins asynchronous TCP receiving for this client.
        /// ��ʼ �첽TCP ��������
        /// </summary>
        protected void BeginReceive() {
            ResumeReceive();
        }

        /// <summary>
        /// Resumes asynchronous TCP receiving for this client.
        ///  ���¿�ʼ �첽TCP ��������
        /// </summary>
        private void ResumeReceive() {  //�ڲ�ѭ������? 
            if (_tcpSock != null && _tcpSock.Connected) {
                SocketAsyncEventArgs socketArgs = SocketHelpers.AcquireSocketArg();
                int offset = this._offset + this._remainingLength;

                socketArgs.SetBuffer(this._bufferSegment.Buffer.Array, this._bufferSegment.Offset + offset, BufferSize - offset);
                socketArgs.UserToken = this;
                socketArgs.Completed += ReceiveAsyncComplete;

                bool willRaiseEvent = _tcpSock.ReceiveAsync(socketArgs);
                //���ؽ����Type: System.Boolean
                //��� I/O �������𣬽����� true���������ʱ�������� e ������ SocketAsyncEventArgs.Completed �¼���
                //��� I/O ����ͬ����ɣ������� false������������£����������� e ������ SocketAsyncEventArgs.Completed �¼������ҿ����ڷ������÷��غ����������Ϊ�������ݵ� e �����Լ��������Ľ����

                if (!willRaiseEvent) {//����false ���û��ִ��,ֱ�Ӵ���
                    ReceiveAsyncComplete(_tcpSock, socketArgs);
                }
            }
        }
        /// <summary>
        /// �����첽���ܵ�����
        /// </summary>
        /// <param name="args"></param>
        private void ProcessRecieve(SocketAsyncEventArgs args) {
            try {
                int bytesReceived = args.BytesTransferred;

                if (bytesReceived == 0)
                //if (args.SocketError != SocketError.Success)
				{ //ToDo:��������Ѿ��Ͽ�������
                    // no bytes means the client disconnected, so clean up!
                    //_server.DisconnectClient(this, true);
                    if (args.SocketError != SocketError.Success) {
                        Logs.Error("�������Ӵ���.������:" + args.SocketError.ToString());
                        this.DisConnect();
                        if (OnRecvError != null) {
                            OnRecvError(this, new Exception("���մ����������Ӵ���.������:" + args.SocketError.ToString()));
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
                        OnRecvDataBytes(this, this._bufferSegment, bytesReceived);//�������������¼�
                    }
                    //�������Ϊ64k
                    ushort needlargeLen = 0;
                    if (OnReceive(this._bufferSegment, out needlargeLen)) {   //�������
                        // packet processed entirely
                        _offset = 0;
                        this._bufferSegment.DecrementUsage();//����
                        this._bufferSegment = Buffers.CheckOut();//��ȡ�µ�Ƭ��
                    }
                    else {   //��û������ 
                        if (needlargeLen == 0) {
                            EnsureBuffer();//��û�д���������� �����µ�Ƭ�ο�ʼ
                        }
                        else {
                            throw new Exception("packet�������쳣");
                        }
                    }

                    ResumeReceive();
                }
            }
            catch (ObjectDisposedException ee1) {
                //_server.DisconnectClient(this, true);
                Logs.Error("��������ʱ�������ͷŶ������" + ee1.ToString());
                this.DisConnect();
                if (OnRecvError != null) {
                    OnRecvError(this, new Exception("��������ʱ�������ͷŶ������:" + ee1.ToString()));
                }
            }
            catch (Exception ee2) {
                //_server.Warning(this, e);
                //_server.DisconnectClient(this, true);
                Logs.Error("��������ʱ��������" + ee2.ToString());
                this.DisConnect();
                if (OnRecvError != null) {
                    OnRecvError(this, new Exception("��������ʱ��������:" + ee2.ToString()));
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
        /// ��������Ƭ��,Ĭ����8kb
        /// </summary>
        protected BufferSegment _bufferSegment;

        /// <summary>
        /// The offset in the buffer to write at.
        /// ��ǰbufferд���ƫ��λ��
        /// </summary>
        protected int _offset;
        /// <summary>
        ///��ǰ_bufferSegment ���ܵ��ĳ���
        /// </summary>
        protected int _remainingLength;
        /// <summary>
        /// Called when a packet has been received and needs to be processed.
        /// </summary>
        /// <param name="numBytes">The size of the packet in bytes.</param>
        protected virtual bool OnReceive(BufferSegment segment, out ushort needLargeLen) {
            needLargeLen = 0;
            //�����
            byte[] recvBuffer = segment.Buffer.Array;
            int i = 1;
            //
            do {
                if (_remainingLength < Packet.HeadSize) {
                    //С�ڰ�ͷ���ȣ�ȡ�������Ĵ�С,������ȡ����
                    return false;
                }
                int offset = segment.Offset + this._offset;//��ƫ��λ��
                int headSize = Packet.HeadSize;
                int packetFullLength = 0;//�����ܳߴ�
                ushort packetId = BitConverter.ToUInt16(recvBuffer, offset);//PacketHelper.Reader.PeekUShort(segment, 0);
                ushort packetlen = BitConverter.ToUInt16(recvBuffer, offset + 2);
                packetFullLength = packetlen + headSize;
                //�ж�
                if (packetFullLength > BufferSize) {
                    //������ܳ���>Ĭ�ϻ��� 8K������Ҫ����һ���°��������´������Ľ��հ�
                    //�������ǵİ������ܳ����ó���
                    // packet is just too big        
                    Logs.Error(string.Format("���յ��İ�:{0}����:{1} >8k,Ӧ�ò�����������İ�,�����쳣���ͻ��������Ͽ�..."));
                    //���������ˣ��Ͽ����Ӿͺ� 
                    DisConnect();
                    if (OnRecvError != null) {
                        OnRecvError(this, new Exception(string.Format("��:{0} ���Ȳ�����.����:{1}", packetId, packetFullLength)));
                    }

                    return false;
                }
                if (_remainingLength < packetFullLength) {
                    //�յ�������С�ڰ��ĳߴ�,���治�ܽ���
                    return false;
                }
                try {
                    PacketCreator creator = this.PacketCreatorMgr.GetPacketCreator(packetId);
                    if (creator == null) {
                        Debug.Assert(false, string.Format("δע��İ�����.packetId:{0} ", packetId));
                        Logs.Error(string.Format("��������Ӧ�İ�������.packetId:{0} ", packetId));
                    }
                    else {
                        //���հ�����
                        Packet p = creator.CreatePacket();
                        BufferSegment oldseg = p.Buffer;
                        p.Buffer = BufferManager.GetSegment(packetFullLength);//ȡ�µ�Ƭ�Σ����ݳ���
                        //�ѵ�ǰ�������ݸ��Ƶ�����Ƭ����
                        p.Buffer.CopyStartFromBytes(0, recvBuffer, offset, packetFullLength);
                        if (oldseg != null) {
                            oldseg.DecrementUsage();//������Ƭ��,��Ϊͬ���İ���Сһ����һ�µģ������������ʻ��ǱȽϸߵ�
                        }
                        if (p.PacketID == 101) {
                            OnRecvTimeSyn(p);
                        }
                        else {
                            m_RecivePackets.Enqueue(p);

                        }
                    }
                }
                catch {
                    Logs.Error(string.Format("��������.packetId:{0} ������Χ", packetId));
                }
                //����������һ����
                _remainingLength -= packetFullLength;
                _offset += packetFullLength;
                i++;
            } while (this._remainingLength > 0);
            return true;
        }

        #region �����첽IOCP����,һ�㲻ʹ��
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
        /// </summary>
        /// <param name="packet">An array of bytes containing the packet to be sent.</param>
        /// <param name="length">���ȴ�С�����ƣ����ܳ�64K��The number of bytes to send starting at offset.</param>
        /// <param name="offset">The offset into packet where the sending begins.</param>
        public virtual void Send(byte[] packet, int offset, int length) {
            if (_tcpSock != null && _tcpSock.Connected) {
                var args = SocketHelpers.AcquireSocketArg();
                if (args != null) {
                    args.Completed += SendAsyncComplete;
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
                    ////��ʼ����
                    if (OnSend != null) {
                        OnSend(this, null);
                    }
                    bool doResult = false;
                    try {
                        doResult = _tcpSock.SendAsync(args);
                    }
                    catch {
                        int edoResult = 0;
                    }
                    if (!doResult && _tcpSock != null) {
                        SendAsyncComplete(_tcpSock, args);
                    }
                    unchecked {
                        _bytesSent += (uint)length;
                    }

                    Interlocked.Add(ref _totalBytesSent, length);
                }
                else {
                    Logs.Error(string.Format("Client {0}'s SocketArgs are null", this._tcpSock.ToString()));
                }
            }
        }
        #endregion

        #region ���ڰ�Packet�ж� �첽IOCPģʽ����

        /// <summary>
        /// ��Ҫ���͵����ݰ����뵽���ͻ�������
        /// </summary>
        /// <param name="packet"></param>
        public void Send(Packet packet) {
            ////��ʼ����
            if (OnSend != null) {
                OnSend(this, packet);
            }
            if (IsPrepareModel) {
                //ToDo:
            }
            else {
                bool needSend = (m_SendPackets.Count == 0);
                m_SendPackets.Enqueue(packet);
                //����ڴ�֮ǰ�����ݿ��ˣ��Ǿͷ�һ��
                if (needSend) {
                    PeekSend();
                }
            }
        }
        /// <summary>
        /// ��ʱ�첽�������ݰ�
        /// </summary>
        /// <param name="packet"></param>
        protected void SendPacketImmediate(Packet packetdata) {
            if (_tcpSock != null && _tcpSock.Connected) {
                var args = SocketHelpers.AcquireSocketArg();
                if (args != null) {
                    byte[] packet = packetdata.Buffer.Buffer.Array;
                    int offset = packetdata.Buffer.Offset;
                    int length = packetdata.PacketBufLen;
                    args.Completed += SendAsyncComplete;
                    //packet���������ݶ� ��СӦ�������Ƶ�
                    args.SetBuffer(packet, offset, length);
                    packetdata.Token = this;//����ʹ�õ�
                    args.UserToken = packetdata;//�����꼴�ɻ��հ�
                    bool doResult = _tcpSock.SendAsync(args);
                    if (!doResult) {
                        SendAsyncComplete(_tcpSock, args);
                    }
                    unchecked {
                        _bytesSent += (uint)length;
                    }

                    Interlocked.Add(ref _totalBytesSent, length);
                }
                else {
                    Logs.Error(string.Format("Client {0}'s SocketArgs are null", this._tcpSock.ToString()));
                }
            }

        }

        /// <summary>
        /// ���Ͱ��ж�
        /// </summary>
        NetQueue<Packet> m_SendPackets = new NetQueue<Packet>(1024);
        /// <summary>
        /// ���հ��ж�
        /// </summary>
        NetQueue<Packet> m_RecivePackets = new NetQueue<Packet>(1024);
        protected void PeekSend() {
            if (m_IsClosed)
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
        /// ÿһ֡�д���İ���������
        /// </summary>
        public int ProcessPacketCountInOneFrame = 1000000;
        /// <summary>
        /// �������
        /// </summary>
        public void ProcessSendAndRecivedPackets() {
            if (m_IsClosed || !m_Connected) {
                return;
            }
            if (!IsConnected) return;//û������ ֱ�ӷ���
            this.ProcessTimeSyn();//�¼�ͬ��
            this.SendHeatBeat();  //��������
            int pp1f = 0;
            while (m_SendPackets.Count > 0) {
                pp1f++;
                if (pp1f >= ProcessPacketCountInOneFrame) {
                    break;//ÿ�����������
                }
                //�Դ����ٶ�����
                PeekSend();
            }
            //�Խ��հ�����
            while (m_RecivePackets.Count > 0) {
                Packet p = null;//���ﴦ����Ļ���
                int pid = -1;
                try {
                    m_RecivePackets.TryDequeue(out p);//�����
                    if (p != null && OnRecvData != null) {
                        pid = p.PacketID;
                        //ProfInstance pi = Profile.StartProf();
                        OnRecvData(this, p);//�������ݰ������¼�
                        //pi.EndProf(p.ToString(), 1f);
                    }
                }
                catch {
                    Logs.Error("�����:" + pid.ToString() + "  �����з�������");
                }
                finally {
                    //���հ�
                    if (p != null) {
                        this.PacketCreatorMgr.GetPacketCreator(p.PacketID).RecylePacket(p);
                    }
                }

            }
        }
        #endregion


        private void SendAsyncComplete(object sender, SocketAsyncEventArgs args) {

            args.Completed -= SendAsyncComplete;
            try {
                //����ʧ����ô����
                if (args.BytesTransferred == 0) {//˵�����ӶϿ���
                    if (args.SocketError != SocketError.Success) {
                        Logs.Error("send error:" + args.SocketError.ToString());

                        Logs.Warn("ClientNetwork Name:" + this.Name + " ����������,������δ���ͼ�ʧ��." + args.SocketError.ToString());
                        this.DisConnect();
                        if (OnSendError != null) {
                            OnSendError((args.UserToken as Packet), new Exception("socket sendAsyn �����:" + args.SocketError.ToString()));
                        }
                    }
                    //  ���ʹ���Ϊ0��Ŀ�귽Ӧ�öϿ������ˣ��������Ͽ����ڡ�
                    //isSending = false;
                    //CloseSocket();
                    return;
                }
                switch (args.LastOperation) {
                    case SocketAsyncOperation.Connect:
                        //������ָ�״̬
                        break;
                    case SocketAsyncOperation.Send:
                    case SocketAsyncOperation.SendTo:
                        //���ͳ��ȼ��
                        Packet packet = (args.UserToken as Packet);
                        if (packet != null && args.BytesTransferred < packet.PacketBufLen) {
                            //TODO:��ǰ����û�з������
                            int i = 0; ;
                        }
                        else {
                        }
                        break;
                    default:
                        //������ָ�״̬
                        if (args.SocketError != SocketError.Success) {
                            //tOdO:����ʧ��
                            Logs.Error("send error:" + args.SocketError.ToString());
                            this.DisConnect();
                            if (OnSendError != null) {
                                OnSendError((args.UserToken as Packet), new Exception("socket sendAsyn �����:" + args.SocketError.ToString()));
                            }

                        }

                        break;
                }
            }
            finally {
                Packet packet = (args.UserToken as Packet);
                if (packet != null) {
                    //���հ�,�Ա��ظ�����
                    (packet.Token as NetClientBase).PacketCreatorMgr.GetPacketCreator(packet.PacketID).RecylePacket(packet);
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
        /// �������������Ķ���
        /// </summary>
        internal SocketAsyncEventArgs ConnectEventArgs { get; private set; }
        void OnConnectCompleted(object sender, SocketAsyncEventArgs e) {
            isBeginConnect = false;
            Socket Socket = _tcpSock;
            if (e.SocketError != SocketError.Success) {
                if (e.AcceptSocket != Socket) {
                    Logs.Error("Socket �ظ����Ӵ���");
                    Socket = e.AcceptSocket;
                }
                try {
                    m_Connected = false;
                    if (OnConncetEnd != null) {
                        OnConncetEnd(e.SocketError);
                    }
                    //  ����ʧ��
                    //if (SocketConnect != null) {//ToDo: ����û�����ӳɹ��¼�
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
                //û�����ӳɹ� ����
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
                //StartHeatbeat();//���������� �����ӳɹ���
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
        /// �ȷ���������һ������
        /// ����һ���첽����
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public void BeginConnect(string ip, int port) {

            if (isBeginConnect && this._tcpSock != null) {
                Logs.Error("replace connecting...");
                //�Ѿ���������״̬������������
                return;
            }
            isBeginConnect = true;
            if (_tcpSock != null) {
                _tcpSock.Close(1);
                Thread.Sleep(100);
                this.m_Connected = false;
            }
            _InitSocket();

            IPAddress address;
            if (IPAddress.TryParse(ip, out address)) {
                if (ConnectEventArgs == null)   //  ���֮ǰ���ӱ��ر��ˣ���������¶����ӵ���������ʼ��
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
                bool doResult = _tcpSock.ConnectAsync(ConnectEventArgs);
                if (!doResult) {
                    OnConnectCompleted(_tcpSock, ConnectEventArgs);
                }
            }
            else {
                isBeginConnect = false;//IP����ȷ
                Logs.Error("ip is error: " + ip);
                m_Connected = false;
                if (OnConncetEnd != null) {
                    OnConncetEnd(SocketError.AddressNotAvailable);
                }
            }
        }
        /// <summary>
        /// ��ʼ��
        /// </summary>
        private void _InitSocket() {
            this._tcpSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _tcpSock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, false);
            _tcpSock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, false);
            LingerOption lo = new LingerOption(true, 5);
            _tcpSock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, lo);
            _tcpSock.NoDelay = m_NoDelay;
            _offset = 0;
            _remainingLength = 0;
            this._bufferSegment.DecrementUsage();//����
            this._bufferSegment = Buffers.CheckOut();//��ȡ�µ�Ƭ��
        }

        /// <summary>
        /// �ͻ�������������������Ͽ�����
        /// </summary>
        public void DisConnect() {
            Status = ClientNetworkStatus.DisConnected;
            if (_tcpSock != null) {
                if (_tcpSock.Connected) {
                    _tcpSock.Disconnect(true);
                }
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
        /// ����û�з�������յİ�
        /// </summary>
        void ClearSendAndRecivedQuene() {
            while (m_SendPackets.Count > 0) {
                Packet p = null;
                m_SendPackets.TryDequeue(out p);
                if (p != null) {
                    this.PacketCreatorMgr.GetPacketCreator(p.PacketID).RecylePacket(p);
                }
            }
            while (m_RecivePackets.Count > 0) {
                Packet p = null;
                m_RecivePackets.TryDequeue(out p);
                if (p != null) {
                    this.PacketCreatorMgr.GetPacketCreator(p.PacketID).RecylePacket(p);
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
            if (_tcpSock != null) {
                try {
                    if (_tcpSock.Connected) {
                        LingerOption lo = new LingerOption(true, 2);
                        try {
                            _tcpSock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, lo);
                        }
                        catch { }
                        _tcpSock.Shutdown(SocketShutdown.Both);
                    }
                    _tcpSock.Close();
                    _tcpSock = null;
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
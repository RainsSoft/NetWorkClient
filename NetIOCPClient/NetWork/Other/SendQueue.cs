

using System;
using System.Collections.Generic;
using NetIOCPClient.Util.Collections;




namespace NetIOCPClient.NetWork.Data
{
    /// <summary>
    /// 发送byte[]队列 分段发送器,缓冲慢才能发送，这样有个问题，当发送频率不高的时候，一部分数据始终取不出来
    /// </summary>
    public class SendQueue
    {
        /// <summary>
        /// 单位数据(克?) 
        /// </summary>
        public class Gram
        {
            private static Stack<Gram> _pool = new Stack<Gram>();
            /// <summary>
            /// 获得
            /// </summary>
            /// <returns></returns>
            public static Gram Acquire() {
                lock (_pool) {
                    Gram gram;

                    if (_pool.Count > 0) {
                        gram = _pool.Pop();
                    }
                    else {
                        gram = new Gram();
                    }

                    gram._buffer = AcquireBuffer();
                    gram._length = 0;

                    return gram;
                }
            }

            private byte[] _buffer;
            private int _length;
            /// <summary>
            /// 缓冲区的字节
            /// </summary>
            public byte[] Buffer {
                get {
                    return _buffer;
                }
            }
            /// <summary>
            /// 已经写入的数据长度
            /// </summary>
            public int Length {
                get {
                    return _length;
                }
            }
            /// <summary>
            /// 剩下可用长度
            /// </summary>
            public int Available {
                get {
                    return (_buffer.Length - _length);
                }
            }
            /// <summary>
            /// 是否满了
            /// </summary>
            public bool IsFull {
                get {
                    return (_length == _buffer.Length);
                }
            }

            private Gram() {
            }
            /// <summary>
            /// 写入数据
            /// </summary>
            /// <param name="buffer">要写入的数据</param>
            /// <param name="offset">要写入数据的起始位置</param>
            /// <param name="length">写入的长度</param>
            /// <returns>返回写入成功的长度</returns>
            public int Write(byte[] buffer, int offset, int length) {
                int write = Math.Min(length, this.Available);

                System.Buffer.BlockCopy(buffer, offset, _buffer, _length, write);

                _length += write;

                return write;
            }
            /// <summary>
            /// 回收
            /// </summary>
            public void Release() {
                lock (_pool) {
                    _pool.Push(this);
                    ReleaseBuffer(_buffer);
                }
            }
        }
        /// <summary>
        /// 字节池一段数据长度
        /// </summary>
        private static int m_CoalesceBufferSize = 512;
        /// <summary>
        /// 申请 2048个 512字节大小的缓冲池
        /// </summary>
        private static BufferPool m_UnusedBuffers = new BufferPool("Coalesced", 2048, m_CoalesceBufferSize);
        /// <summary>
        /// 修改缓存池m_UnusedBuffers字节大小
        /// </summary>
        public static int CoalesceBufferSize {
            get {
                return m_CoalesceBufferSize;
            }
            set {
                if (m_CoalesceBufferSize == value)
                    return;

                if (m_UnusedBuffers != null)
                    m_UnusedBuffers.Free();

                m_CoalesceBufferSize = value;
                m_UnusedBuffers = new BufferPool("Coalesced", 2048, m_CoalesceBufferSize);
            }
        }
        /// <summary>
        /// 申请
        /// </summary>
        /// <returns></returns>
        public static byte[] AcquireBuffer() {
            return m_UnusedBuffers.AcquireBuffer();
        }
        /// <summary>
        /// 回收
        /// </summary>
        /// <param name="buffer"></param>
        public static void ReleaseBuffer(byte[] buffer) {
            if (buffer != null && buffer.Length == m_CoalesceBufferSize) {
                m_UnusedBuffers.ReleaseBuffer(buffer);
            }
        }
        /// <summary>
        /// 待定
        /// </summary>
        private ConcurrentQueue<Gram> _pending;
        /// <summary>
        /// 
        /// </summary>
        private Gram _buffered;
        /// <summary>
        /// 发送队列是否发生完毕
        /// </summary>
        public bool IsFlushReady {
            get {
                return (_pending.Count == 0 && _buffered != null);
            }
        }
        /// <summary>
        /// 发送队列是否为空
        /// </summary>
        public bool IsEmpty {
            get {
                return (_pending.Count == 0 && _buffered == null);
            }
        }

        public SendQueue() {
            _pending = new ConcurrentQueue<Gram>();
        }
        /// <summary>
        /// 检测是否发生完毕
        /// </summary>
        /// <returns></returns>
        public Gram CheckFlushReady() {
            Gram gram = null;

            if (_pending.Count == 0 && _buffered != null) {
                gram = _buffered;

                _pending.Enqueue(_buffered);
                _buffered = null;
            }

            return gram;
        }
        /// <summary>
        /// 取出要发送的数据
        /// </summary>
        /// <returns></returns>
        public Gram Dequeue() {
            Gram gram = null;

            if (_pending.Count > 0) {
                //_pending.Dequeue().Release();
                if (_pending.TryDequeue(out gram)) {
                    gram.Release();//???why
                }
                gram = null;
                if (_pending.Count > 0) {
                    //gram = _pending.Peek();
                    _pending.TryPeek(out gram);
                }
            }
            
            return gram;
        }
        /// <summary>
        /// 系统一次发生的最大数据量96k 最好还是小点好 
        /// </summary>
        private const int PendingCap = 96 * 1024;
        /// <summary>
        /// 压入要发送的数据
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public Gram Enqueue(byte[] buffer, int length) {
            return Enqueue(buffer, 0, length);
        }
        /// <summary>
        /// 压入要发送的数据,并返回第一个
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public Gram Enqueue(byte[] buffer, int offset, int length) {
            if (buffer == null) {
                throw new ArgumentNullException("buffer");
            }
            else if (!(offset >= 0 && offset < buffer.Length)) {
                throw new ArgumentOutOfRangeException("offset", offset, "Offset must be greater than or equal to zero and less than the size of the buffer.");
            }
            else if (length < 0 || length > buffer.Length) {
                throw new ArgumentOutOfRangeException("length", length, "Length cannot be less than zero or greater than the size of the buffer.");
            }
            else if ((buffer.Length - offset) < length) {
                throw new ArgumentException("Offset and length do not point to a valid segment within the buffer.");
            }

            int existingBytes = (_pending.Count * m_CoalesceBufferSize) + (_buffered == null ? 0 : _buffered.Length);

            if ((existingBytes + length) > PendingCap) {
                throw new CapacityExceededException();
            }

            Gram gram = null;

            while (length > 0) {
                if (_buffered == null) { // nothing yet buffered
                    _buffered = Gram.Acquire();
                }

                int bytesWritten = _buffered.Write(buffer, offset, length);

                offset += bytesWritten;
                length -= bytesWritten;

                if (_buffered.IsFull) {
                    if (_pending.Count == 0) {
                        gram = _buffered;
                    }

                    _pending.Enqueue(_buffered);
                    _buffered = null;
                }
            }

            return gram;
        }
        /// <summary>
        /// 清理
        /// </summary>
        public void Clear() {
            if (_buffered != null) {
                _buffered.Release();
                _buffered = null;
            }

            while (_pending.Count > 0) {
                //_pending.Dequeue().Release();
                Gram gram = null;
                if (_pending.TryDequeue(out gram)) {
                    gram.Release();
                }
            }
        }
    }

    public sealed class CapacityExceededException : Exception
    {
        public CapacityExceededException()
            : base("send Queue: Too much data pending.") {
        }
    }

    ///// <summary>
    ///// 
    ///// </summary>
    //public struct SendBuffer
    //{


    //    public readonly static SendBuffer NullBuffer = new SendBuffer(null);



    //    /// <summary>
    //    /// 私有的构造类
    //    /// </summary>
    //    private SendBuffer(byte[] buffer) {
    //        m_Buffer = buffer;
    //        m_Length = 0;
    //    }


    //    /// <summary>
    //    /// 缓冲区的字节
    //    /// </summary>
    //    private byte[] m_Buffer;

    //    /// <summary>
    //    /// 缓冲区的字节
    //    /// </summary>
    //    public byte[] Buffer {
    //        get { return m_Buffer; }
    //    }


    //    /// <summary>
    //    /// 缓冲区的长度
    //    /// </summary>
    //    private long m_Length;

    //    /// <summary>
    //    /// 缓冲区已写入的长度
    //    /// </summary>
    //    public long Length {
    //        get { return m_Length; }
    //    }

    //    /// <summary>
    //    /// 缓冲区的剩余的有效空间
    //    /// </summary>
    //    public long SpareSpace {
    //        get { return (m_Buffer.Length - m_Length); }
    //    }

    //    /// <summary>
    //    /// 缓冲区是否已经满了
    //    /// </summary>
    //    public bool IsFull {
    //        get { return (m_Length >= m_Buffer.Length); }
    //    }

    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public bool IsNull {
    //        get { return m_Buffer == null; }
    //    }


    //    #region
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <param name="byteBuffer"></param>
    //    /// <param name="iOffset"></param>
    //    /// <param name="iLength"></param>
    //    /// <returns></returns>
    //    public long Write(byte[] byteBuffer, long lOffset, long lLength) {
    //        // 获取可以写当前缓冲区的字节数
    //        long iWrite = Math.Min(lLength, this.SpareSpace);

    //        // 写入数据
    //        System.Buffer.BlockCopy(byteBuffer, (int)lOffset, m_Buffer, (int)m_Length, (int)iWrite);

    //        // 跟新缓冲区的长度
    //        m_Length += iWrite;

    //        return iWrite;
    //    }

    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public void Release() {
    //        if (m_Buffer == null)
    //            return;

    //        // 把数据返回进内存池
    //        SendQueue.ReleaseBuffer(m_Buffer);

    //        m_Buffer = null;
    //        m_Length = 0;
    //    }
    //    #endregion

    //    #region
    //    /// <summary>
    //    /// 请求Gram类
    //    /// </summary>
    //    /// <returns></returns>
    //    public static SendBuffer Instance() {
    //        return new SendBuffer(SendQueue.AcquireBuffer());
    //    }
    //    #endregion
    //}

    ///// <summary>
    ///// 数据输出包的缓冲区,如果数据过长就等处理缓存发送时发送数据( 小于 512K 时 )
    ///// 之所以小于 512K 是因为TCP-IP发送数据时总是发送至客户端彼此确认完毕后才通知返回的,如果过大网络延迟时会严重影响网络的通讯.
    ///// </summary>
    ////[MultiThreadedSupport( "zh-CHS", "当前的类所有成员都可锁定,支持多线程操作" )]
    //public class SendQueue
    //{
    //    #region zh-CHS 私有成员变量 | en Private Member Variables

    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    private readonly static int QUEUE_CAPACITY_SIZE = 1024;

    //    /// <summary>
    //    /// 当Flush发出的数据(最后发出的数据块)
    //    /// </summary>
    //    private SendBuffer m_FlushBuffer = SendBuffer.NullBuffer;
    //    /// <summary>
    //    /// 等待需要发出的数据
    //    /// </summary>
    //    private Queue<SendBuffer> m_PendingBuffer = new Queue<SendBuffer>(QUEUE_CAPACITY_SIZE);
    //    /// <summary>
    //    /// 等待需要发出的数据锁
    //    /// </summary>
    //    private readonly object m_LockFlushAndPending = new object();
    //    //private SpinLock m_LockFlushAndPending = new SpinLock();
    //    #endregion

    //    #region zh-CHS 属性 | en Properties
    //    /// <summary>
    //    /// 当前需发送的数据是否空的
    //    /// </summary>
    //    public bool IsEmpty {
    //        get { return (m_PendingBuffer.Count <= 0 && m_FlushBuffer.IsNull == true); }
    //    }


    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    private long m_WaitSendSize = 0;

    //    /// <summary>
    //    /// 缓冲区内还有多少没有发送的数据
    //    /// </summary>
    //    public long WaitSendSize {
    //        get { return m_WaitSendSize; }
    //    }
    //    #endregion

    //    #region zh-CHS 静态属性 | en Static Properties

    //    /// <summary>
    //    /// 缓冲的数据大小
    //    /// </summary>
    //    private static int s_CoalesceBufferSize = 512;

    //    /// <summary>
    //    /// 合并缓冲的数据大小
    //    /// </summary>
    //    public static int CoalesceBufferSize {
    //        get { return s_CoalesceBufferSize; }
    //        set {
    //            if (s_CoalesceBufferSize == value)
    //                return;

    //            if (s_CoalesceBufferSize <= 0)
    //                return;

    //            s_CoalesceBufferSize = value;

    //            BufferPool tempBuffers = s_UnusedBuffers;
    //            s_UnusedBuffers = new BufferPool("SendQueue - CoalescedBuffer", INITIAL_CAPACITY, s_CoalesceBufferSize);

    //            if (tempBuffers != null)
    //                tempBuffers.Free();
    //        }
    //    }
    //    #endregion

    //    #region zh-CHS 方法 | en Method

    //    /// <summary>
    //    /// 可能是windows操作系统的最大可发送的字节数
    //    /// </summary>
    //    private const int PENDING_MAX_BUFFER = 96 * 1024;

    //    /// <summary>
    //    /// 如果数据满了,且缓冲区内的数据是空的则返回需要发送的数据
    //    /// 调用Enqueue(...)后调用Dequeue(...),不能直接调用Dequeue(...)
    //    /// </summary>
    //    /// <param name="byteBuffer"></param>
    //    /// <param name="iOffset"></param>
    //    /// <param name="iLength"></param>
    //    /// <returns></returns>
    //    public void Enqueue(byte[] byteBuffer, long iOffset, long iLength) {
    //        if (byteBuffer == null)
    //            throw new Exception("SendQueue.Enqueue(...) - byteBuffer == null error!");

    //        if (iOffset < 0 || iOffset >= byteBuffer.Length)
    //            throw new Exception("SendQueue.Enqueue(...) - iOffset < 0 || iOffset >= byteBuffer.Length error!");

    //        if (iLength < 0 || iLength > byteBuffer.Length) // 如果iLength == 0就返回空,如果iLength == 0就跳过
    //            throw new Exception("SendQueue.Enqueue(...) - iLength < 0 || iLength > byteBuffer.Length error!");

    //        if ((byteBuffer.Length - iOffset) < iLength)
    //            throw new Exception("SendQueue.Enqueue(...) - ( byteBuffer.Length - iOffset ) < iLength error!");
    //        //bool locktoken = false;
    //        //m_LockFlushAndPending.Enter(ref locktoken);
    //        Monitor.Enter(m_LockFlushAndPending);

    //        do {
    //            if (m_FlushBuffer.IsNull == true) {
    //                // nothing yet buffered
    //                m_FlushBuffer = SendBuffer.Instance();

    //                if (m_FlushBuffer.IsNull == true)
    //                    throw new Exception("SendQueue.Enqueue(...) - m_FlushBuffer.IsNull == true error!");
    //            }

    //            // 当前已经写入的字节
    //            long iBytesWritten = m_FlushBuffer.Write(byteBuffer, iOffset, iLength);

    //            iOffset += iBytesWritten;
    //            iLength -= iBytesWritten;

    //            // 写入需要发送的数据的大小
    //            m_WaitSendSize += iBytesWritten;

    //            // 如果数据没有满,且数据写入完毕则退出,返回空,不添加到集合内
    //            if (m_FlushBuffer.IsFull == true) {
    //                // 如果满了添加到集合内的尾处
    //                m_PendingBuffer.Enqueue(m_FlushBuffer);
    //                m_FlushBuffer = SendBuffer.NullBuffer;　// 置空再次请求缓存
    //            }

    //        } while (iLength > 0L);



    //        //m_LockFlushAndPending.Exit();
    //        Monitor.Exit(m_LockFlushAndPending);
    //    }

    //    /// <summary>
    //    /// 获取当前的数据
    //    /// </summary>
    //    /// <returns></returns>
    //    public SendBuffer Dequeue() {
    //        SendBuffer sendGram = SendBuffer.NullBuffer;
    //        //bool locktoken = false;
    //        //m_LockFlushAndPending.Enter(ref locktoken);
    //        Monitor.Enter(m_LockFlushAndPending);
    //        //{
    //        if (m_PendingBuffer.Count > 0) {
    //            sendGram = m_PendingBuffer.Dequeue();   // 再给出数据
    //        }
    //        else if (m_FlushBuffer.IsNull == false) {
    //            sendGram = m_FlushBuffer;               // 再给出数据
    //            m_FlushBuffer = SendBuffer.NullBuffer;
    //        }

    //        // 移去已发送的数据的大小
    //        m_WaitSendSize -= sendGram.Length;
    //        //}
    //        //m_LockFlushAndPending.Exit();
    //        Monitor.Exit(m_LockFlushAndPending);
    //        return sendGram;
    //    }

    //    /// <summary>
    //    /// 清除数据
    //    /// </summary>
    //    public void Clear() {
    //        //bool locktoken = false;
    //        //m_LockFlushAndPending.Enter(ref locktoken);
    //        Monitor.Enter(m_LockFlushAndPending);

    //        while (m_PendingBuffer.Count > 0) {
    //            m_PendingBuffer.Dequeue().Release();
    //        }
    //        if (m_FlushBuffer.IsNull == false) {
    //            m_FlushBuffer.Release();
    //            m_FlushBuffer = SendBuffer.NullBuffer;
    //        }

    //        // 清空
    //        m_WaitSendSize = 0;

    //        //m_LockFlushAndPending.Exit();
    //        Monitor.Exit(m_LockFlushAndPending);
    //    }
    //    #endregion

    //    #region  en Private Static Method


    //    /// <summary>
    //    /// 1024
    //    /// </summary>
    //    private readonly static int INITIAL_CAPACITY = 1024;

    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    private static BufferPool s_UnusedBuffers = new BufferPool("SendQueue - CoalescedBuffer", INITIAL_CAPACITY, s_CoalesceBufferSize);

    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <returns></returns>
    //    internal static byte[] AcquireBuffer() {
    //        return s_UnusedBuffers.AcquireBuffer();
    //    }

    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <param name="byteBuffer"></param>
    //    internal static void ReleaseBuffer(byte[] byteBuffer) {
    //        Debug.Assert(byteBuffer != null, "SendQueue.ReleaseBuffer(...) - byteBuffer == null error!");

    //        if (byteBuffer.Length == s_CoalesceBufferSize) {
    //            // 可能修改过m_CoalesceBufferSize如果不同就抛弃它
    //            s_UnusedBuffers.ReleaseBuffer(byteBuffer);
    //        }
    //    }


    //    #endregion
    //}
}

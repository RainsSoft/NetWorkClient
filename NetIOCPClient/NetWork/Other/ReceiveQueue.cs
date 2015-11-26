

using System;
using System.Threading;
using NetIOCPClient.Core;


namespace NetIOCPClient.NetWork.Data
{
    /// <summary>
    /// 环形缓冲区,适用于接受数据缓存
    /// </summary>
    public class ByteQueue
    {
        /// <summary>
        /// 字节头位置
        /// </summary>
        private int m_Head;
        /// <summary>
        /// 字节尾位置
        /// </summary>
        private int m_Tail;
        /// <summary>
        /// 剩下字节大小
        /// </summary>
        private int m_Size;
        /// <summary>
        /// 长度为2048
        /// </summary>
        private byte[] m_Buffer;
        private readonly ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim(); 
        public int Length { get { return m_Size; } }

        public ByteQueue() {
            m_Buffer = new byte[2048];
        }
        /// <summary>
        /// 清理数据
        /// </summary>
        public void Clear() {
            m_lock.EnterWriteLock();
            m_Head = 0;
            m_Tail = 0;
            m_Size = 0;
            m_lock.ExitWriteLock();
        }
        /// <summary>
        /// 修改环形缓冲byte[]大小
        /// </summary>
        /// <param name="capacity"></param>
        private void SetCapacity(int capacity) {
            byte[] newBuffer = new byte[capacity];

            if (m_Size > 0) {
                if (m_Head < m_Tail) {
                    Buffer.BlockCopy(m_Buffer, m_Head, newBuffer, 0, m_Size);
                }
                else {
                    Buffer.BlockCopy(m_Buffer, m_Head, newBuffer, 0, m_Buffer.Length - m_Head);
                    Buffer.BlockCopy(m_Buffer, 0, newBuffer, m_Buffer.Length - m_Head, m_Tail);
                }
            }

            m_Head = 0;
            m_Tail = m_Size;
            m_Buffer = newBuffer;
        }
        /// <summary>
        /// 包ID  USHORT
        /// </summary>
        /// <returns></returns>
        public ushort GetPacketID() {
            m_lock.EnterReadLock();
            ushort pid = ushort.MaxValue;           
            if (m_Size >= 2) {
                //pid= BitConverter.ToUInt16(m_Buffer, m_Head);
                //return m_Buffer[m_Head];//old
                pid=   (ushort)((m_Buffer[(m_Head ) % m_Buffer.Length] << 8) | m_Buffer[(m_Head + 1) % m_Buffer.Length]);
            }
            m_lock.ExitReadLock();
            return pid;
        }
        /// <summary>
        /// 包结构是 packetid(ushort2)+packetLength(ushort2)
        /// </summary>
        /// <returns></returns>
        public ushort GetPacketLength() {  
            m_lock.EnterReadLock();
            ushort len = 0;
            if (m_Size >= 4) {
                //return (m_Buffer[(m_Head + 1) % m_Buffer.Length] << 8) | m_Buffer[(m_Head + 2) % m_Buffer.Length];
                len= (ushort)((m_Buffer[(m_Head + 2) % m_Buffer.Length] << 8) | m_Buffer[(m_Head + 3) % m_Buffer.Length]);
                //return BitConverter.ToUInt16(m_Buffer, m_Head + 2);
            }
            m_lock.ExitReadLock();
            return len;
        }
        /// <summary>
        /// 取出指定大小数据
        /// </summary>
        /// <param name="buffer">目标数据</param>
        /// <param name="offset">目标数据开始位置</param>
        /// <param name="size">取出的数据长度</param>
        /// <returns>返回取出成功的字节长度</returns>
        public int Dequeue(byte[] buffer, int offset, int size) {
            try {
                m_lock.EnterReadLock();
                if (size > m_Size)
                    size = m_Size;//不能超过最大长度

                if (size == 0)
                    return 0;

                if (m_Head < m_Tail) {
                    Buffer.BlockCopy(m_Buffer, m_Head, buffer, offset, size);//尾部指针没没有超过起始指针
                }
                else {
                    int rightLength = (m_Buffer.Length - m_Head);//剩下的长度
                    //对尾部指针大于或等于开始指针的处理
                    if (rightLength >= size) {
                        Buffer.BlockCopy(m_Buffer, m_Head, buffer, offset, size);//剩下的长度能完全取出指定长度
                    }
                    else {
                        Buffer.BlockCopy(m_Buffer, m_Head, buffer, offset, rightLength);//复制剩下的长度
                        Buffer.BlockCopy(m_Buffer, 0, buffer, offset + rightLength, size - rightLength);//把尾部部分复制出来
                    }
                }

                m_Head = (m_Head + size) % m_Buffer.Length;//修改头位置，以buffer的长度来取
                m_Size -= size;

                if (m_Size == 0) {//数据取完 ，指针移到开始的位置
                    m_Head = 0;
                    m_Tail = 0;
                }

                return size;
            }
            finally {
                m_lock.ExitReadLock();
            }
        }
        /// <summary>
        /// 压入数据指定数据 
        /// </summary>
        /// <param name="buffer">指定数据</param>
        /// <param name="offset">指定数据的偏移位置</param>
        /// <param name="size">压入的数据长度</param>
        public void Enqueue(byte[] buffer, int offset, int size) {
            try {
                m_lock.ExitWriteLock();
                if ((m_Size + size) > m_Buffer.Length)
                    SetCapacity((m_Size + size + 2047) & ~2047);//总是以2048的倍数来增大字节数

                if (m_Head < m_Tail) {//一般情况
                    int rightLength = (m_Buffer.Length - m_Tail);

                    if (rightLength >= size) {//剩下的长度能完全容纳
                        Buffer.BlockCopy(buffer, offset, m_Buffer, m_Tail, size);
                    }
                    else {
                        Buffer.BlockCopy(buffer, offset, m_Buffer, m_Tail, rightLength);
                        Buffer.BlockCopy(buffer, offset + rightLength, m_Buffer, 0, size - rightLength);
                    }
                }
                else {
                    Buffer.BlockCopy(buffer, offset, m_Buffer, m_Tail, size);
                }

                m_Tail = (m_Tail + size) % m_Buffer.Length;
                m_Size += size;
            }
            finally {
                m_lock.ExitWriteLock();
            }
        }
    }
    //    /// <summary>
    //    /// 接受到的数据,使用环绕缓冲区来解决粘包和半包的处理
    //    /// </summary>
    //    public class ReceiveQueue
    //    {
    //        #region zh-CHS 类常量 | en Class Constants
    //        /// <summary>
    //        /// 字节默认的大小
    //        /// </summary>
    //        public const int BUFFER_SIZE = 1024 * 512;
    //        #endregion

    //        #region zh-CHS 私有成员变量 | en Private Member Variables
    //        /// <summary>
    //        /// 字节的头位置
    //        /// </summary>
    //        private long m_Head;
    //        /// <summary>
    //        /// 字节的尾位置
    //        /// </summary>
    //        private long m_Tail;
    //        /// <summary>
    //        /// 字节的数组
    //        /// </summary>
    //        private byte[] m_Buffer = new byte[BUFFER_SIZE];
    //        /// <summary>
    //        /// 
    //        /// </summary>
    //        private object m_LockBuffer = new object();
    //        #endregion

    //        #region zh-CHS 属性 | en Properties
    //        #region zh-CHS 私有成员变量 | en Private Member Variables
    //        /// <summary>
    //        /// 字节的大小
    //        /// </summary>
    //        private long m_Size;
    //        #endregion
    //        /// <summary>
    //        /// 环绕缓冲区内的数据大小
    //        /// </summary>
    //        public long Length
    //        {
    //            get { return m_Size; }
    //        }


    //        #endregion

    //        #region zh-CHS 方法 | en Method
    //        /// <summary>
    //        /// 给出使用环绕缓冲区内的数据
    //        /// </summary>
    //        /// <param name="byteBuffer">要复制到的数据的数组</param>
    //        /// <param name="iOffset">要复制到数组的长度偏移量</param>
    //        /// <param name="iSize">要复制多少长度的数据</param>
    //        /// <returns>返回实际读取到的字节数</returns>
    //        public long Dequeue( byte[] byteBuffer, long iOffset, long iSize )
    //        {
    //            if ( byteBuffer == null )
    //                throw new ArgumentNullException( "byteBuffer", "ReceiveQueue.Dequeue(...) - byteBuffer == null error!" );

    //            if ( iOffset < 0 || iOffset >= byteBuffer.Length )
    //                throw new Exception( "ReceiveQueue.Dequeue(...) - iOffset < 0 || iOffset >= byteBuffer.Length error!" );

    //            if ( iSize < 0 || iSize > byteBuffer.Length ) // 如果iLength == 0就返回空,如果iLength == 0就跳过
    //                throw new Exception( string.Format("ReceiveQueue.Dequeue(...) - iSize < 0 || iSize > byteBuffer.Length error! iSize={0}", iSize ));

    //            if ( ( byteBuffer.Length - iOffset ) < iSize )
    //                throw new Exception( "ReceiveQueue.Dequeue(...) - ( byteBuffer.Length - iOffset ) < iSize error!" );

    //            if ( iSize == 0 )
    //                return 0;

    //            lock( m_LockBuffer )
    //            {
    //                if ( iSize > m_Size )
    //                    iSize = m_Size;

    //                if ( m_Head < m_Tail )
    //                    Buffer.BlockCopy( m_Buffer, (int)m_Head, byteBuffer, (int)iOffset, (int)iSize );
    //                else
    //                {
    //                    long rightLength = m_Buffer.Length - m_Head;

    //                    if ( rightLength >= iSize )
    //                        Buffer.BlockCopy( m_Buffer, (int)m_Head, byteBuffer, (int)iOffset, (int)iSize );
    //                    else
    //                    {
    //                        Buffer.BlockCopy( m_Buffer, (int)m_Head, byteBuffer, (int)iOffset, (int)rightLength );
    //                        Buffer.BlockCopy( m_Buffer, 0, byteBuffer, (int)( iOffset + rightLength ), (int)( iSize - rightLength ) );
    //                    }
    //                }

    //                m_Head = ( m_Head + iSize ) % m_Buffer.Length;
    //                m_Size -= iSize;

    //                if ( m_Size == 0 )
    //                {
    //                    m_Head = 0;
    //                    m_Tail = 0;
    //                }
    //            }

    //            return iSize;
    //        }

    //        /// <summary>
    //        /// 压入数据至环绕缓冲区内
    //        /// </summary>
    //        /// <param name="byteBuffer"></param>
    //        /// <param name="iOffset"></param>
    //        /// <param name="iSize"></param>
    //        public void Enqueue( byte[] byteBuffer, long iOffset, long iSize )
    //        {
    //            if ( byteBuffer == null )
    //                throw new ArgumentNullException( "byteBuffer", "ReceiveQueue.Enqueue(...) - byteBuffer == null error!" );

    //            if ( iOffset < 0 || iOffset >= byteBuffer.Length )
    //                throw new Exception( "ReceiveQueue.Enqueue(...) - iOffset < 0 || iOffset >= byteBuffer.Length error!" );

    //            if ( iSize < 0 || iSize > byteBuffer.Length ) // 如果iLength == 0就返回空,如果iLength == 0就跳过
    //                throw new Exception( "ReceiveQueue.Enqueue(...) - iSize < 0 || iSize > byteBuffer.Length error!" );

    //            if ( ( byteBuffer.Length - iOffset ) < iSize )
    //                throw new Exception( "ReceiveQueue.Enqueue(...) - ( byteBuffer.Length - iOffset ) < iSize error!" );

    //            lock( m_LockBuffer )
    //            {
    //                if ( ( m_Size + iSize ) >= m_Buffer.Length )
    //                    SetCapacityInLock( ( m_Size + iSize + 2047 ) & ~2047 ); // 总是以2048的倍数来增大字节数, :( 弄得我老半天才明白原理呢!

    //                if ( m_Head < m_Tail )
    //                {
    //                    long rightLength = m_Buffer.Length - m_Tail;

    //                    if ( rightLength >= iSize )
    //                        Buffer.BlockCopy( byteBuffer, (int)iOffset, m_Buffer, (int)m_Tail, (int)iSize );
    //                    else
    //                    {
    //                        Buffer.BlockCopy( byteBuffer, (int)iOffset, m_Buffer, (int)m_Tail, (int)rightLength );
    //                        Buffer.BlockCopy( byteBuffer, (int)( iOffset + rightLength ), m_Buffer, 0, (int)( iSize - rightLength ) );
    //                    }
    //                }
    //                else
    //                    Buffer.BlockCopy( byteBuffer, (int)iOffset, m_Buffer, (int)m_Tail, (int)iSize );

    //                m_Tail = ( m_Tail + iSize ) % m_Buffer.Length;
    //                m_Size += iSize;
    //            }
    //        }

    //        /// <summary>
    //        /// 清除数据的信息,不清除数据缓冲,用于下次使用
    //        /// </summary>
    //        public void Clear()
    //        {
    //#if !UNITY_IPHONE
    //            lock (m_LockBuffer)
    //#endif
    //            {
    //                m_Head = 0;
    //                m_Tail = 0;
    //                m_Size = 0;
    //            }

    //        }

    //        /// <summary>
    //        /// 
    //        /// </summary>
    //        private Endian m_Endian = Endian.BIG_ENDIAN;

    //        /// <summary>
    //        /// 包的长度
    //        /// </summary>
    //        public const int PacketLengthSize = 4;

    //        /// <summary>
    //        /// 给出数据包的长度
    //        /// </summary>
    //        /// <returns></returns>
    //        public int GetPacketLength()
    //        {
    //            int iReturn = 0;

    //#if !UNITY_IPHONE
    //            lock (m_LockBuffer)
    //#endif
    //            {
    //                if (PacketLengthSize > m_Size)
    //                    return 0;

    //                if (m_Head + PacketLengthSize < m_Buffer.Length)
    //                {
    //                    //  保证要读取的数据在字节数组里
    //                    var index = m_Head;

    //                    //  读四字节长度
    //                    if (m_Endian == Endian.LITTLE_ENDIAN)
    //                        return (m_Buffer[index] << 24) | (m_Buffer[index + 1] << 16) | (m_Buffer[index + 2] << 8) | m_Buffer[index + 3];
    //                    else
    //                        return m_Buffer[index] | (m_Buffer[index + 1] << 8) | (m_Buffer[index + 2] << 16) | (m_Buffer[index + 3] << 24);

    //                    //if (m_Endian == Endian.LITTLE_ENDIAN)
    //                    //    return (m_Buffer[index] << 8) | (m_Buffer[index + 1]);
    //                    //else
    //                    //    return m_Buffer[index] | (m_Buffer[index + 1] << 8);
    //                }
    //            }

    //            return iReturn;
    //        }

    //        #endregion

    //        #region zh-CHS 私有方法 | en Private Method
    //        /// <summary>
    //        /// 扩大缓冲数据的大小(当前都在锁中操作，因此不需要锁定的)
    //        /// </summary>
    //        /// <param name="iCapacity"></param>
    //        private void SetCapacityInLock( long iCapacity )
    //        {
    //            byte[] newBuffer = new byte[iCapacity];

    //            if ( m_Size > 0 )
    //            {
    //                if ( m_Head < m_Tail )
    //                    Buffer.BlockCopy( m_Buffer, (int)m_Head, newBuffer, 0, (int)m_Size );
    //                else
    //                {
    //                    long rightLength = m_Buffer.Length - m_Head;

    //                    Buffer.BlockCopy( m_Buffer, (int)m_Head, newBuffer, 0, (int)rightLength );
    //                    Buffer.BlockCopy( m_Buffer, 0, newBuffer, (int)rightLength, (int)m_Tail );
    //                }
    //            }

    //            m_Head = 0;
    //            m_Tail = m_Size;
    //            m_Buffer = newBuffer;
    //        }
    //        #endregion
    //    }
}

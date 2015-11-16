using System;


namespace NetIOCPClient.NetWork
{
    /// <summary>
    /// Defines a wrapper for a chunk of memory that may be split into smaller, logical segments.
    /// 定义一个可拆分为较小的，逻辑段的内存块的包装器.
    /// </summary>
    public class ArrayBuffer
    {
        //protected static DefultLog logs;// = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// 缓存管理器，根据初始化方法不同，可能为null
        /// </summary>
        private BufferManager m_mgr;
        //private readonly int BufferSize;
        /// <summary>
        /// 缓存数据段
        /// </summary>
        public readonly byte[] Array;

        /// <summary>
        /// Creates an ArrayBuffer that is wrapping a pre-existing buffer.
        /// </summary>
        /// <param name="arr">the buffer to wrap</param>
        internal ArrayBuffer(byte[] arr) {
            Array = arr;
            // BufferSize = arr.Length;
        }

        /// <summary>
        /// Creates an ArrayBuffer and allocates a new buffer for usage.
        /// </summary>
        /// <param name="mgr">the <see cref="BufferManager" /> which allocated this array</param>
        internal ArrayBuffer(BufferManager mgr, int bufferSize) {
            m_mgr = mgr;
            // BufferSize = bufferSize;
            Array = new byte[bufferSize];
        }
        /// <summary>
        /// 把一个缓存分段放入当前所属bufferManager的缓存池
        /// </summary>
        /// <param name="segment"></param>
        protected internal void CheckIn(BufferSegment segment) {
            if (m_mgr != null) {
                m_mgr.CheckIn(segment);
            }
        }
    }
}
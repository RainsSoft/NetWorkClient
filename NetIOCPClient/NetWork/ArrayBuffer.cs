using System;


namespace NetIOCPClient.NetWork
{
    /// <summary>
    /// Defines a wrapper for a chunk of memory that may be split into smaller, logical segments.
    /// ����һ���ɲ��Ϊ��С�ģ��߼��ε��ڴ��İ�װ��.
    /// </summary>
    public class ArrayBuffer
    {
        //protected static DefultLog logs;// = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// ��������������ݳ�ʼ��������ͬ������Ϊnull
        /// </summary>
        private BufferManager m_mgr;
        //private readonly int BufferSize;
        /// <summary>
        /// �������ݶ�
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
        /// ��һ������ֶη��뵱ǰ����bufferManager�Ļ����
        /// </summary>
        /// <param name="segment"></param>
        protected internal void CheckIn(BufferSegment segment) {
            if (m_mgr != null) {
                m_mgr.CheckIn(segment);
            }
        }
    }
}
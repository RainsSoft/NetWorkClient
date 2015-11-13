using System;
using System.Threading;

namespace NetIOCPClient.Network
{
    /// <summary>
    /// 缓存分段,用于操作ArrayBuffer
    /// </summary>
	public class BufferSegment
	{
        /// <summary>
        /// [readonly]数据存储段
        /// </summary>
		public readonly ArrayBuffer Buffer;
        /// <summary>
        /// [readpnly] Buffer上的起始偏移位置
        /// </summary>
		public readonly int Offset;
        /// <summary>
        ///  [readpnly] Buffer上的有效数据长度
        /// </summary>
		public readonly int Length;
        /// <summary>
        /// 使用计数
        /// </summary>
		internal int m_uses;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer">关联ArrayBuffer</param>
        /// <param name="offset">buffer中的起始位置</param>
        /// <param name="length">数据长度</param>
        /// <param name="id">编号</param>
		public BufferSegment(ArrayBuffer buffer, int offset, int length, int id)
		{
			Buffer = buffer;
			Offset = offset;
			Length = length;
            Number = id;
		}

        /// <summary>
        /// Returns the byte as the given index within this segment.
        /// 返回给定索引在该段中的字节。
        /// </summary>
        /// <param name="i">the offset in this segment to go</param>
        /// <returns>the byte at the index, or 0 if the index is out-of-bounds</returns>
		public byte this[int i]
		{
			get { return Buffer.Array[Offset + i]; }
            set { Buffer.Array[Offset + i] = value; }
		}

        /// <summary>
        /// Returns a deep-copy of the data mapped to this segment.
        /// 返回映射到该段的数据的深拷贝。可结合CreateSegment（byte[]）使用,建立不通过DecrementUsage()主动回收，而自己管理的片段
        /// </summary>
		public byte[] SegmentData
		{
			get
			{
				var bytes = new byte[Length];
				System.Buffer.BlockCopy(Buffer.Array, Offset, bytes, 0, Length);

				return bytes;
			}
		}

        /// <summary>
        /// The number of users still using this segment.
        ///仍在使用该段的计数。
        /// </summary>
		public int Uses
		{
			get
			{
				return m_uses;
			}
		}

        /// <summary>
        /// Unique segment identifier.
        /// 唯一段标识。
        /// </summary>
        public int Number
        {
            get;
            internal set;
        }

		/// <summary>
		/// Copies the contents of the given array into this segment at the given offset.
		/// </summary>
		/// <param name="bytes">the buffer to read from</param>
		/// <param name="offset">the offset to start reading from</param>
        /// <exception cref="ArgumentException">an ArgumentException will be thrown if offset is greater than
        /// the length of the buffer</exception>
		public void CopyFrom(byte[] bytes, int offset)
		{
            System.Buffer.BlockCopy(bytes, offset, Buffer.Array, Offset /*+ offset*/, (bytes.Length - offset));
		}
        /// <summary>
        /// 填充数据，外面保证不要出现越界问题,这里不做检测
        /// </summary>
        /// <param name="startpos">当前缓存开始位置</param>
        /// <param name="bytes">数据源</param>
        /// <param name="offset">数据源偏移位置</param>
        /// <param name="count">辅助数据字节数</param>
	    public void CopyStartFromBytes(int startpos,byte[] bytes, int offset,int count)
		{
            System.Diagnostics.Debug.Assert(startpos + count <= this.Length &&count<=bytes.Length);
            System.Buffer.BlockCopy(bytes, offset, Buffer.Array, Offset + startpos, count);
		}
        /// <summary>
        /// Copys the data in this segment to another <see cref="BufferSegment" />.
        /// </summary>
        /// <param name="segment">the <see cref="BufferSegment" /> instance to copy to</param>
        /// <param name="length">the amount of bytes to copy from this segment</param>
        /// <exception cref="ArgumentException">an ArgumentException will be thrown if length is greater than
        /// the length of the segment</exception>
		public void CopyTo(BufferSegment segment, int length)
		{
			System.Buffer.BlockCopy(Buffer.Array, Offset, segment.Buffer.Array, segment.Offset, length);
		}
        public void CopyToBytes(int startpos,byte[] buf,int offset,int count) {
            System.Buffer.BlockCopy(Buffer.Array, Offset+startpos, buf,offset, count);
        }
        /// <summary>
        /// Increments the usage counter of this segment.
        /// 递增此段的使用次数计数器。
        /// </summary>
		public void IncrementUsage()
		{
            Interlocked.Increment(ref m_uses);
		}

		/// <summary>
		/// Decrements the usage counter of this segment.
        /// 递减此段的使用次数计数器。并回收该分片
		/// </summary>
        /// <remarks>When the usage counter reaches 0, the segment will be 
        /// returned to the buffer pool.</remarks>
		public void DecrementUsage()
		{
			if (Interlocked.Decrement(ref m_uses) == 0)
			{
				Buffer.CheckIn(this);//回收
			}
		}

        /// <summary>
        /// Creates a new BufferSegment for the given buffer.
        /// 这样创建的 BufferSegment 不受管理,不会通过 DecrementUsage()自动回收，用于外面自己管理的BufferSegment
        /// </summary>
        /// <param name="bytes">the buffer to wrap</param>
        /// <returns>a new BufferSegment wrapping the given buffer</returns>
        /// <remarks>This will also create an underlying ArrayBuffer to pin the buffer
        /// for the BufferSegment.  The ArrayBuffer will be disposed when the segment
        /// is released.</remarks>
        public static BufferSegment CreateSegment(byte[] bytes)
        {
            return CreateSegment(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Creates a new BufferSegment for the given buffer.
        /// 这样创建的 BufferSegment 不受管理,不可通过DecrementUsage()自动回收，用于外面自己管理的BufferSegment
        /// </summary>
        /// <param name="bytes">the buffer to wrap</param>
        /// <param name="offset">the offset of the buffer to read from</param>
        /// <param name="length">the length of the data to read</param>
        /// <returns>a new BufferSegment wrapping the given buffer</returns>
        /// <remarks>This will also create an underlying ArrayBuffer to pin the buffer
        /// for the BufferSegment.  The ArrayBuffer will be disposed when the segment
        /// is released.</remarks>
        public static BufferSegment CreateSegment(byte[] bytes, int offset, int length)
        {
            return new BufferSegment(new ArrayBuffer(bytes), offset, length, -1);
        }
	}
}
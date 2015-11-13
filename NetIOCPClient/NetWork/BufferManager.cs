/*************************************************************************
 *
 *   file		: BufferManager.cs
 *   copyright		: (C) 
 *   email		: 
 *   last changed	: $LastChangedDate:  $
 
 *   revision		: $Rev: 1208 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Threading;

using NetIOCPClient.Util.Collections;
using NetIOCPClient.Log;
using NetIOCPClient.Core;

namespace NetIOCPClient.Network
{
	/// <summary>
	/// Manages a pool of small buffers allocated from large, contiguous chunks of memory.
    /// 管理一个由大、连续的内存块分配的小缓冲池。
	/// </summary>
	/// <remarks>
	/// When used in an async network call, a buffer is pinned. Large numbers of pinned buffers
	/// cause problem with the GC (in particular it causes heap fragmentation).
    /// 当使用异步network socket，一个缓冲区是固定的。用GC回收（特别是它引起堆碎片）的大量固定的缓冲区产生问题。
	///
	/// This class maintains a set of large segments and gives clients pieces of these
	/// segments that they can use for their buffers. The alternative to this would be to
	/// create many small arrays which it then maintained. This methodology should be slightly
	/// better than the many small array methodology because in creating only a few very
	/// large objects it will force these objects to be placed on the LOH. Since the
	/// objects are on the LOH they are at this time not subject to compacting which would
	/// require an update of all GC roots as would be the case with lots of smaller arrays
	/// that were in the normal heap.
	/// </remarks>
	public class BufferManager //: IDisposable
	{
		//protected static DefultLog Logs;// = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// 总 BufferManager 列表
        /// </summary>
		public static readonly List<BufferManager> Managers = new List<BufferManager>();

		/// <summary>
		/// Default BufferManager for small buffers with up to 128 bytes length
        /// 最小片段管理器 128字节
		/// </summary>
		public static readonly BufferManager Tiny = new BufferManager(1024, 128);

		/// <summary>
		/// Default BufferManager for small buffers with up to 1kb length
        /// 小片段管理器 1kb
		/// </summary>
		public static readonly BufferManager Small = new BufferManager(1024, 1024);

		/// <summary>
		/// Default BufferManager for default-sized buffers (usually up to 8kb)
        /// 默认片段管理器 8kb,大部分包都在当前范围内
		/// </summary>
		public static readonly BufferManager Default = new BufferManager(NetIOCPClientDef.PBUF_SEGMENT_COUNT, NetIOCPClientDef.MAX_PBUF_SEGMENT_SIZE);
        /// <summary>
        /// 32kb
        /// </summary>
        public static readonly BufferManager Normal = new BufferManager(256, 32 * 1024);
		/// <summary>
		/// Large BufferManager for buffers up to 64kb size
        /// 大片段管理器 64kb
		/// </summary>
		public static readonly BufferManager Large = new BufferManager(128, 64 * 1024);

		/// <summary>
		/// Extra Large BufferManager holding 512kb buffers
        /// 巨大片段管理器 512k
		/// </summary>
		//public static readonly BufferManager LargeExtra = new BufferManager(16, 512 * 1024);

		/// <summary>
		/// Super Large BufferManager holding 1MB buffers
        /// 超大片段管理器 1M
		/// </summary>
		//public static readonly BufferManager SuperSized = new BufferManager(8, 1024 * 1024);
        /// <summary>
        /// 2m 
        /// </summary>
        //public static readonly BufferManager SuperExtra2Sized = new BufferManager(4, 2048 * 1024);
        /// <summary>
        /// 4M 建立4M的包应该很少见吧
        /// </summary>
        //public static readonly BufferManager SuperExtra4Sized = new BufferManager(2, 4096 * 1024);
        /// <summary>
        /// 8M 建立8M的包应该不存在
        /// </summary>
        //public static readonly BufferManager SuperExtra8Sized = new BufferManager(2, 8192 * 1024);
        /// <summary>
        /// 可能是windows操作系统的最大可发送的字节数
        /// </summary>
        private const int PENDING_MAX_BUFFER = 96 * 1024;
		/// <summary>
		/// Holds the total amount of memory allocated by all buffer managers.
        /// 保存所有缓冲区管理者分配的内存总数。
		/// </summary>
		public static long GlobalAllocatedMemory = 0;

		/// <summary>
		/// Count of segments per Buffer
        /// 片段数
		/// </summary>
		private readonly int _segmentCount;
		/// <summary>
		/// Segment size
        /// 片段尺寸
		/// </summary>
		private readonly int _segmentSize;
		/// <summary>
		/// Total count of segments in all buffers
        /// 总片段数
		/// </summary>
		private int _totalSegmentCount;
        /// <summary>
        /// 片段号，唯一，递增
        /// </summary>
		private volatile static int _segmentId;
        /// <summary>
        /// 数据存储对象列表
        /// </summary>
		private readonly List<ArrayBuffer> _buffers;
        /// <summary>
        /// 当前可用的片段列队
        /// </summary>
		private readonly LockfreeQueue<BufferSegment> _availableSegments;

		/// <summary>
		/// The number of currently available segments
        /// 当前可用的段数
		/// </summary>
		public int AvailableSegmentsCount
		{
			get { return _availableSegments.Count; } //do we really care about volatility here?
		}
        /// <summary>
        /// 是否使用中
        /// </summary>
		public bool InUse
		{
			get { return _availableSegments.Count < _totalSegmentCount; }
		}
        /// <summary>
        /// 使用的片段数
        /// </summary>
		public int UsedSegmentCount
		{
			get { return _totalSegmentCount - _availableSegments.Count; }
		}

		/// <summary>
		/// The total number of currently allocated buffers.
        /// 当前分配的缓冲区总数。
		/// </summary>
		public int TotalBufferCount
		{
			get { return _buffers.Count; } //do we really care about volatility here?
		}

		/// <summary>
		/// The total number of currently allocated segments.
        /// 当前分配的段总数。
		/// </summary>
		public int TotalSegmentCount
		{
			get { return _totalSegmentCount; } //do we really care about volatility here?
		}

		/// <summary>
		/// The total amount of all currently allocated buffers.
        /// 所有当前分配的缓冲区的总分配内存。
		/// </summary>
		public int TotalAllocatedMemory
		{
			get { return _buffers.Count * (_segmentCount * _segmentSize); } // do we really care about volatility here?
		}

		/// <summary>
		/// The size of a single segment
        /// 单段的大小
		/// </summary>
		public int SegmentSize
		{
			get { return _segmentSize; }
		}

		#region Constructors

		/// <summary>
		/// Constructs a new <see cref="Default"></see> object
		/// </summary>
		/// <param name="segmentCount">The number of chunks tocreate per segment每段创造的块的数量</param>
		/// <param name="segmentSize">The size of a chunk in bytes</param>
		public BufferManager(int segmentCount, int segmentSize)
		{
			_segmentCount = segmentCount;
			_segmentSize = segmentSize;
			_buffers = new List<ArrayBuffer>();
			_availableSegments = new LockfreeQueue<BufferSegment>();
			Managers.Add(this);
		}

		#endregion

		/// <summary>
		/// Checks out a segment, creating more if the pool is empty.
        /// 检查出一段，如果池是空的，多创建多个。
		/// </summary>
		/// <returns>a BufferSegment object from the pool</returns>
		public BufferSegment CheckOut()
		{
			BufferSegment segment;

			if (!_availableSegments.TryDequeue(out segment))
			{
				lock (_buffers)
				{
					while (!_availableSegments.TryDequeue(out segment))
					{
						CreateBuffer();
					}
				}
			}

			// this doubles up with what CheckIn() looks for, but no harm in that, really.
			if (segment.m_uses > 1)
			{
				Logs.Error(string.Format("Checked out segment (Size: {0}, Number: {1}) that is already in use! Queue contains: {2}, Buffer amount: {3}",
					segment.Length, segment.Number, _availableSegments.Count, _buffers.Count));
			}

			// set initial usage to 1
			segment.m_uses = 1;

			return segment;
		}

		/// <summary>
		/// Checks out a segment, and wraps it with a SegmentStream, creating more if the pool is empty.
		/// </summary>
		/// <returns>a SegmentStream object wrapping the BufferSegment taken from the pool</returns>
		public SegmentStream CheckOutStream()
		{
			return new SegmentStream(CheckOut());
		}

		/// <summary>
		/// Requeues a segment into the buffer pool.
        /// 重新加入到缓冲池。
		/// </summary>
		/// <param name="segment">the segment to requeue</param>
		public void CheckIn(BufferSegment segment)
		{
			if (segment.m_uses > 1)
			{
				Logs.Error(string.Format("Checked in segment (Size: {0}, Number: {1}) that is already in use! Queue contains: {2}, Buffer amount: {3}",
					segment.Length, segment.Number, _availableSegments.Count, _buffers.Count));
			}

			_availableSegments.Enqueue(segment);
		}

		/// <summary>
		/// Creates a new buffer and adds the segments to the buffer pool.
		/// </summary>
		private void CreateBuffer()
		{
			// create a new buffer 
			var newBuf = new ArrayBuffer(this, _segmentCount * _segmentSize);

			// create segments from the buffer
			for (int i = 0; i < _segmentCount; i++)
			{
				_availableSegments.Enqueue(new BufferSegment(newBuf, i * _segmentSize, _segmentSize, _segmentId++));
			}

			// increment our total count
			_totalSegmentCount += _segmentCount;

			// hold a ref to our new buffer
			_buffers.Add(newBuf);

			// update global alloc'd memory
			Interlocked.Add(ref GlobalAllocatedMemory, _segmentCount * _segmentSize);
		}

		/// <summary>
		/// Returns a BufferSegment that is at least of the given size.
        /// 返回一个buffersegment是根据给定的至少尺寸。
		/// </summary>
		/// <param name="payloadSize"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException">In case that the payload exceeds the SegmentSize of the largest buffer available.</exception>
		public static BufferSegment GetSegment(int payloadSize)
		{
			if (payloadSize <= Tiny.SegmentSize)
			{
				return Tiny.CheckOut();
			}
			if (payloadSize <= Small.SegmentSize)
			{
				return Small.CheckOut();
			}
			if (payloadSize <= Default.SegmentSize)
			{
				return Default.CheckOut();
			}
            if (payloadSize <= Normal.SegmentSize) {
                return Normal.CheckOut();
            }
			if (payloadSize <= Large.SegmentSize)
			{
				return Large.CheckOut();
			}
            //if (payloadSize <= LargeExtra.SegmentSize)
            //{
            //    return LargeExtra.CheckOut();
            //}
            //if (payloadSize <= SuperSized.SegmentSize) {
            //    return SuperSized.CheckOut();
            //}
            //if (payloadSize <= SuperExtra2Sized.SegmentSize) {
            //    return SuperExtra2Sized.CheckOut();
            //}
            //if (payloadSize <= SuperExtra4Sized.SegmentSize) {
            //    return SuperExtra4Sized.CheckOut();
            //}
            //if (payloadSize <= SuperExtra8Sized.SegmentSize) {
            //    return SuperExtra8Sized.CheckOut();
            //}
			throw new ArgumentOutOfRangeException("Required buffer is way too big: " + payloadSize);
		}

		/// <summary>
		/// Returns a SegmentStream that is at least of the given size.
		/// </summary>
		/// <param name="payloadSize"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException">In case that the payload exceeds the SegmentSize of the largest buffer available.</exception>
		public static SegmentStream GetSegmentStream(int payloadSize)
		{
			return new SegmentStream(GetSegment(payloadSize));
		}

		#region IDisposable Members

		~BufferManager()
		{
			Dispose(false);
		}
        /// <summary>
        /// 销毁，非回收
        /// </summary>
		public void Dispose()
		{
			GC.SuppressFinalize(this);
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			// clear the segment queue
			BufferSegment segment;
			while (_availableSegments.TryDequeue(out segment)) ;

			// clean up buffers

			_buffers.Clear();
		}

		#endregion
	}
}
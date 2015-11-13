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
    /// ����һ���ɴ��������ڴ������С����ء�
	/// </summary>
	/// <remarks>
	/// When used in an async network call, a buffer is pinned. Large numbers of pinned buffers
	/// cause problem with the GC (in particular it causes heap fragmentation).
    /// ��ʹ���첽network socket��һ���������ǹ̶��ġ���GC���գ��ر������������Ƭ���Ĵ����̶��Ļ������������⡣
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
        /// �� BufferManager �б�
        /// </summary>
		public static readonly List<BufferManager> Managers = new List<BufferManager>();

		/// <summary>
		/// Default BufferManager for small buffers with up to 128 bytes length
        /// ��СƬ�ι����� 128�ֽ�
		/// </summary>
		public static readonly BufferManager Tiny = new BufferManager(1024, 128);

		/// <summary>
		/// Default BufferManager for small buffers with up to 1kb length
        /// СƬ�ι����� 1kb
		/// </summary>
		public static readonly BufferManager Small = new BufferManager(1024, 1024);

		/// <summary>
		/// Default BufferManager for default-sized buffers (usually up to 8kb)
        /// Ĭ��Ƭ�ι����� 8kb,�󲿷ְ����ڵ�ǰ��Χ��
		/// </summary>
		public static readonly BufferManager Default = new BufferManager(NetIOCPClientDef.PBUF_SEGMENT_COUNT, NetIOCPClientDef.MAX_PBUF_SEGMENT_SIZE);
        /// <summary>
        /// 32kb
        /// </summary>
        public static readonly BufferManager Normal = new BufferManager(256, 32 * 1024);
		/// <summary>
		/// Large BufferManager for buffers up to 64kb size
        /// ��Ƭ�ι����� 64kb
		/// </summary>
		public static readonly BufferManager Large = new BufferManager(128, 64 * 1024);

		/// <summary>
		/// Extra Large BufferManager holding 512kb buffers
        /// �޴�Ƭ�ι����� 512k
		/// </summary>
		//public static readonly BufferManager LargeExtra = new BufferManager(16, 512 * 1024);

		/// <summary>
		/// Super Large BufferManager holding 1MB buffers
        /// ����Ƭ�ι����� 1M
		/// </summary>
		//public static readonly BufferManager SuperSized = new BufferManager(8, 1024 * 1024);
        /// <summary>
        /// 2m 
        /// </summary>
        //public static readonly BufferManager SuperExtra2Sized = new BufferManager(4, 2048 * 1024);
        /// <summary>
        /// 4M ����4M�İ�Ӧ�ú��ټ���
        /// </summary>
        //public static readonly BufferManager SuperExtra4Sized = new BufferManager(2, 4096 * 1024);
        /// <summary>
        /// 8M ����8M�İ�Ӧ�ò�����
        /// </summary>
        //public static readonly BufferManager SuperExtra8Sized = new BufferManager(2, 8192 * 1024);
        /// <summary>
        /// ������windows����ϵͳ�����ɷ��͵��ֽ���
        /// </summary>
        private const int PENDING_MAX_BUFFER = 96 * 1024;
		/// <summary>
		/// Holds the total amount of memory allocated by all buffer managers.
        /// �������л����������߷�����ڴ�������
		/// </summary>
		public static long GlobalAllocatedMemory = 0;

		/// <summary>
		/// Count of segments per Buffer
        /// Ƭ����
		/// </summary>
		private readonly int _segmentCount;
		/// <summary>
		/// Segment size
        /// Ƭ�γߴ�
		/// </summary>
		private readonly int _segmentSize;
		/// <summary>
		/// Total count of segments in all buffers
        /// ��Ƭ����
		/// </summary>
		private int _totalSegmentCount;
        /// <summary>
        /// Ƭ�κţ�Ψһ������
        /// </summary>
		private volatile static int _segmentId;
        /// <summary>
        /// ���ݴ洢�����б�
        /// </summary>
		private readonly List<ArrayBuffer> _buffers;
        /// <summary>
        /// ��ǰ���õ�Ƭ���ж�
        /// </summary>
		private readonly LockfreeQueue<BufferSegment> _availableSegments;

		/// <summary>
		/// The number of currently available segments
        /// ��ǰ���õĶ���
		/// </summary>
		public int AvailableSegmentsCount
		{
			get { return _availableSegments.Count; } //do we really care about volatility here?
		}
        /// <summary>
        /// �Ƿ�ʹ����
        /// </summary>
		public bool InUse
		{
			get { return _availableSegments.Count < _totalSegmentCount; }
		}
        /// <summary>
        /// ʹ�õ�Ƭ����
        /// </summary>
		public int UsedSegmentCount
		{
			get { return _totalSegmentCount - _availableSegments.Count; }
		}

		/// <summary>
		/// The total number of currently allocated buffers.
        /// ��ǰ����Ļ�����������
		/// </summary>
		public int TotalBufferCount
		{
			get { return _buffers.Count; } //do we really care about volatility here?
		}

		/// <summary>
		/// The total number of currently allocated segments.
        /// ��ǰ����Ķ�������
		/// </summary>
		public int TotalSegmentCount
		{
			get { return _totalSegmentCount; } //do we really care about volatility here?
		}

		/// <summary>
		/// The total amount of all currently allocated buffers.
        /// ���е�ǰ����Ļ��������ܷ����ڴ档
		/// </summary>
		public int TotalAllocatedMemory
		{
			get { return _buffers.Count * (_segmentCount * _segmentSize); } // do we really care about volatility here?
		}

		/// <summary>
		/// The size of a single segment
        /// ���εĴ�С
		/// </summary>
		public int SegmentSize
		{
			get { return _segmentSize; }
		}

		#region Constructors

		/// <summary>
		/// Constructs a new <see cref="Default"></see> object
		/// </summary>
		/// <param name="segmentCount">The number of chunks tocreate per segmentÿ�δ���Ŀ������</param>
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
        /// ����һ�Σ�������ǿյģ��ഴ�������
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
        /// ���¼��뵽����ء�
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
        /// ����һ��buffersegment�Ǹ��ݸ��������ٳߴ硣
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
        /// ���٣��ǻ���
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
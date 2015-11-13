using System;

namespace NetIOCPClient.Pool
{
    /// <summary>
    /// Interface for an object pool.
    /// </summary>
    /// <remarks>
    /// An object pool holds reusable objects. See zzObjectPoolMgr for more details.
    /// </remarks>
    public interface IObjectPool
    {
        /// <summary>
        /// Amount of available objects in pool
        /// 池中可用对象的数量
        /// </summary>
        int AvailableCount {
            get;
        }

        /// <summary>
        /// Amount of objects that have been obtained but not recycled.
        /// 已获得但不回收的对象的数量。
        /// </summary>
        int ObtainedCount {
            get;
        }

        ///// <summary>
        ///// Enqueues an object in the pool to be reused.
        ///// 入队的池中的对象可以重复使用。
        ///// </summary>
        ///// <param name="obj">The object to be put back in the pool.</param>
        //void Recycle(object obj);

        ///// <summary>
        ///// Grabs an object from the pool.
        ///// 从池中抓取一个对象。
        ///// </summary>
        ///// <returns>An object from the pool.</returns>
        //object ObtainObj();
    }

    /// <summary>
    /// 对象池信息
    /// </summary>
    public interface IPoolInfo : IObjectPool
    {
        /// <summary>
        /// 对象池的名字
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 获得对象池信息
        /// </summary>
        /// <returns></returns>
        PoolInfo GetPoolInfo();
    }


    /// <summary>
    /// 对象池信息
    /// </summary>
    public struct PoolInfo
    {
        #region zh-CHS 共有属性 | en Public Properties

        /// <summary>
        /// 空闲数量
        /// </summary>
        public long FreeCount { get; internal set; }

        /// <summary>
        /// 申请的数量
        /// </summary>
        public long AcquireCount { get; internal set; }

        /// <summary>
        /// 释放的数量
        /// </summary>
        public long ReleaseCount { get; internal set; }

        /// <summary>
        /// 初始化池数量
        /// </summary>
        public long InitialCapacity { get; internal set; }

        /// <summary>
        /// 当前池数量
        /// </summary>
        public long CurrentCapacity { get; internal set; }

        /// <summary>
        /// 请求失败次数
        /// </summary>
        public long Misses { get; internal set; }

        /// <summary>
        /// 对象池的名字
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// 输出当前数据
        /// </summary>
        /// <returns></returns>
        public override string ToString() {

            return string.Format("{0} FreeCount={1} CurrentCapacity={2} Misses={3} freerate={4:f3}% AcquireCount={5} ReleaseCount={6}",
                Name, FreeCount, CurrentCapacity, Misses, FreeCount * 100f / CurrentCapacity,
                AcquireCount, ReleaseCount);
        }

        #endregion
    }

}
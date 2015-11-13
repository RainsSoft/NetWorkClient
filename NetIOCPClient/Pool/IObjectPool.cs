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
        /// ���п��ö��������
        /// </summary>
        int AvailableCount {
            get;
        }

        /// <summary>
        /// Amount of objects that have been obtained but not recycled.
        /// �ѻ�õ������յĶ����������
        /// </summary>
        int ObtainedCount {
            get;
        }

        ///// <summary>
        ///// Enqueues an object in the pool to be reused.
        ///// ��ӵĳ��еĶ�������ظ�ʹ�á�
        ///// </summary>
        ///// <param name="obj">The object to be put back in the pool.</param>
        //void Recycle(object obj);

        ///// <summary>
        ///// Grabs an object from the pool.
        ///// �ӳ���ץȡһ������
        ///// </summary>
        ///// <returns>An object from the pool.</returns>
        //object ObtainObj();
    }

    /// <summary>
    /// �������Ϣ
    /// </summary>
    public interface IPoolInfo : IObjectPool
    {
        /// <summary>
        /// ����ص�����
        /// </summary>
        string Name { get; }

        /// <summary>
        /// ��ö������Ϣ
        /// </summary>
        /// <returns></returns>
        PoolInfo GetPoolInfo();
    }


    /// <summary>
    /// �������Ϣ
    /// </summary>
    public struct PoolInfo
    {
        #region zh-CHS �������� | en Public Properties

        /// <summary>
        /// ��������
        /// </summary>
        public long FreeCount { get; internal set; }

        /// <summary>
        /// ���������
        /// </summary>
        public long AcquireCount { get; internal set; }

        /// <summary>
        /// �ͷŵ�����
        /// </summary>
        public long ReleaseCount { get; internal set; }

        /// <summary>
        /// ��ʼ��������
        /// </summary>
        public long InitialCapacity { get; internal set; }

        /// <summary>
        /// ��ǰ������
        /// </summary>
        public long CurrentCapacity { get; internal set; }

        /// <summary>
        /// ����ʧ�ܴ���
        /// </summary>
        public long Misses { get; internal set; }

        /// <summary>
        /// ����ص�����
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// �����ǰ����
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
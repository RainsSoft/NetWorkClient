using System;
using System.Collections.Generic;
using System.Linq;

namespace NetIOCPClient.Pool
{
      

    /// <summary>
    /// 所有对象池ObjectPool关联信息
    /// </summary>
    /// <remarks>
    /// </remarks>
    public static class ObjectPoolStateInfo
    {
        private static List<IPoolInfo> pools = new List<IPoolInfo>(32);

        internal static void Add(IPoolInfo pool) {
            pools.Add(pool);
        }

        /// <summary>
        /// 获得所有内存池的信息
        /// </summary>
        /// <returns></returns>
        public static PoolInfo[] GetPoolInfos() {
            return pools.Select(o => o.GetPoolInfo()).ToArray();
        }

        /// <summary>
        /// 获得指定名称的内存池信息
        /// </summary>
        /// <returns></returns>
        public static PoolInfo[] GetPoolInfos(string name) {
            return pools.Where(o => o.Name == name)
                .Select(o => o.GetPoolInfo()).ToArray();
        }
        public static PoolInfo[] GetPoolInfos(long  uniqueId) {
            return pools.Where(o => o.UniqueId ==uniqueId)
                .Select(o => o.GetPoolInfo()).ToArray();
        }
        public static IPoolInfo GetPoolByUniqueID(long  uniqueId) {
            return pools.Find(o => o.UniqueId == uniqueId);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetIOCPClient.Pool
{

    /// <summary>
    /// 带单例模式的对象池
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class StaticInstanceObjectPool<T> where T : new()
    {
        private readonly ObjectPool<T> instance = new ObjectPool<T>(4, 65536);

        /// <summary>
        /// 单例模式
        /// </summary>
        public ObjectPool<T> Instatnce {
            get { return instance; }
        }
    }
    /// <summary>
    /// 泛型的，静态唯一的对象池【线程安全，放心使用】
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class StaticObjectPool<T> where T : new()
    {
        private static readonly ObjectPool<T> pool = new ObjectPool<T>(4, 1024);
        /// <summary>
        /// ID
        /// </summary>
        public static long Pool_UniqueID { get { return pool.UniqueId; } }
        /// <summary>
        /// 内存池请求数据
        /// </summary>
        /// <returns></returns>
        public static T AcquireContent() {
            return pool.AcquireContent();
        }

        /// <summary>
        /// 回收内存
        /// </summary>
        /// <param name="contentT"></param>
        public static void ReleaseContent(T contentT) {
            pool.ReleaseContent(contentT);
        }
    }
}

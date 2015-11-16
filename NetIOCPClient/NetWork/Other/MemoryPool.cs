using System;
using System.Collections.Generic;
using NetIOCPClient.Util.Collections;

namespace NetIOCPClient.Network.Other
{
    /// <summary>
    /// (内存池)难得用一次泛型,C# 比 C++ 泛型好用好写多了 :)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    //[MultiThreadedSupport("zh-CHS", "当前的类所有成员都可锁定,支持多线程操作")]
    internal class MemoryPool<T> where T : new()
    {
        #region zh-CHS 共有的结构 | en Public Struct
        /// <summary>
        /// 
        /// </summary>
        public struct PoolInfo
        {
            #region zh-CHS 共有属性 | en Public Properties

            /// <summary>
            /// 
            /// </summary>
            private string m_strName;

            /// <summary>
            /// 
            /// </summary>
            public string Name {
                get { return m_strName; }
                internal set { m_strName = value; }
            }


            /// <summary>
            /// 
            /// </summary>
            private int m_iFreeCount;

            /// <summary>
            /// 
            /// </summary>
            public int FreeCount {
                get { return m_iFreeCount; }
                internal set { m_iFreeCount = value; }
            }


            /// <summary>
            /// 
            /// </summary>
            private int m_iInitialCapacity;

            /// <summary>
            /// 
            /// </summary>
            public int InitialCapacity {
                get { return m_iInitialCapacity; }
                internal set { m_iInitialCapacity = value; }
            }


            /// <summary>
            /// 
            /// </summary>
            private int m_iCurrentCapacity;

            /// <summary>
            /// 
            /// </summary>
            public int CurrentCapacity {
                get { return m_iCurrentCapacity; }
                internal set { m_iCurrentCapacity = value; }
            }


            /// <summary>
            /// 
            /// </summary>
            private int m_iMisses;

            /// <summary>
            /// 
            /// </summary>
            public int Misses {
                get { return m_iMisses; }
                internal set { m_iMisses = value; }
            }
            #endregion
        }
        #endregion

        #region zh-CHS 私有成员变量 | en Private Member Variables
        /// <summary>
        /// 内存池的名字
        /// </summary>
        private string m_Name = string.Empty;
        /// <summary>
        /// 内存池的容量
        /// </summary>
        private int m_InitialCapacity = 0;
        /// <summary>
        /// 内存池的容量不足时再次请求数据的次数
        /// </summary>
        private int m_Misses = 0;
        /// <summary>
        /// 内存池
        /// </summary>
        private ConcurrentQueue<T> m_FreePool = new ConcurrentQueue<T>();
        #endregion

        #region zh-CHS 构造和初始化和清理 | en Constructors and Initializers and Dispose
        /// <summary>
        /// 初始化内存池
        /// </summary>
        /// <param name="strName"></param>
        /// <param name="iInitialCapacity"></param>
        public MemoryPool(string strName, int iInitialCapacity) {
            m_Name = strName;

            m_InitialCapacity = iInitialCapacity;

            for (int iIndex = 0; iIndex < iInitialCapacity; ++iIndex)
                m_FreePool.Enqueue(new T());
        }
        #endregion

        #region zh-CHS 共有方法 | en Public Methods
        /// <summary>
        /// 内存池请求数据
        /// </summary>
        /// <returns></returns>
        public T AcquirePoolContent() {
            T returnT = default(T);

            do {
                if (m_FreePool.Count > 0) {
                    if (m_FreePool.TryDequeue(out returnT) == true)
                        break;
                }

                ++m_Misses;

                for (int iIndex = 0; iIndex < m_InitialCapacity; ++iIndex)
                    m_FreePool.Enqueue(new T());

                if (m_FreePool.TryDequeue(out returnT) == true)
                    break;
            } while (true);

            return returnT;
        }

        /// <summary>
        /// 内存池释放数据
        /// </summary>
        /// <param name="TContent"></param>
        public void ReleasePoolContent(T contentT) {
            if (contentT == null)
                throw new ArgumentNullException("TContent", "TContent == null");

            m_FreePool.Enqueue(contentT);
        }

        /// <summary>
        /// 释放内存池内全部的数据
        /// </summary>
        public void Free() {
            m_FreePool = new ConcurrentQueue<T>();
        }

        /// <summary>
        /// 给出内存池的详细信息
        /// </summary>
        /// <param name="strName"></param>
        /// <param name="iFreeCount"></param>
        /// <param name="iInitialCapacity"></param>
        /// <param name="iCurrentCapacity"></param>
        /// <param name="iMisses"></param>
        public MemoryPool<T>.PoolInfo GetPoolInfo() {
            MemoryPool<T>.PoolInfo poolInfo = new MemoryPool<T>.PoolInfo();

            // 可以不需要锁定的，因为只是给出没有修改数据
            poolInfo.Name = m_Name;
            poolInfo.FreeCount = m_FreePool.Count;
            poolInfo.InitialCapacity = m_InitialCapacity;
            poolInfo.CurrentCapacity = m_InitialCapacity * (1 + m_Misses); // m_Misses是从零开始计算的因此需加1
            poolInfo.Misses = m_Misses;

            return poolInfo;
        }
        #endregion
    }
}

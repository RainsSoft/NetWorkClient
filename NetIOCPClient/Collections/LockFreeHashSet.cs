/*************************************************************************
 *
 *   file			: LockfreeStack.cs
 *   copyright		: (C) 2004 Julian M Bucknall 
 *   last changed	: $LastChangedDate: 2008-11-25 11:16:45 +0100 (ti, 25 nov 2008) $
 
 *   revision		: $Rev: 686 $
 *
 *   Written by/rights held by Julian M Bucknall (boyet.com)
 *   http://www.boyet.com/Articles/LockfreeStack.html
 *   
 *   Modified by WCell
 *
 *************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace NetIOCPClient.Util.Collections
{
    /// <summary>
    /// 
    /// </summary>
    public class LockFreeHashSet<KeyT> : IEnumerable<KeyT>
    {
        #region zh-CHS ����ͳ�ʼ�������� | en Constructors and Initializers and Dispose
        /// <summary>
        /// 
        /// </summary>
        public LockFreeHashSet() {
        }

        /// <summary>
        /// 
        /// </summary>
        public LockFreeHashSet(IEnumerable<KeyT> collection) {
            m_HashSet = new HashSet<KeyT>(collection);
        }
        #endregion

        #region zh-CHS �������� | en Public Properties

        /// <summary>
        /// 
        /// </summary>
        public int Count {
            get { return m_HashSet.Count; }
        }

        #endregion

        #region zh-CHS ���з��� | en Public Methods
        #region zh-CHS ˽�г�Ա���� | en Private Member Variables
        /// <summary>
        /// 
        /// </summary>
        private HashSet<KeyT> m_HashSet = new HashSet<KeyT>();
        #endregion
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(KeyT key) {
            HashSet<KeyT> newHashSet = null;
            HashSet<KeyT> oldHashSet = null;

            do {
                oldHashSet = m_HashSet;

                newHashSet = new HashSet<KeyT>(oldHashSet);
                newHashSet.Add(key);

            } while (Interlocked.CompareExchange<HashSet<KeyT>>(ref m_HashSet, newHashSet, oldHashSet) != oldHashSet);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serial"></param>
        /// <param name="realm"></param>
        public void AddRange(IEnumerable<KeyT> collection) {
            HashSet<KeyT> newHashSet = null;
            HashSet<KeyT> oldHashSet = null;

            do {
                oldHashSet = m_HashSet;

                newHashSet = new HashSet<KeyT>(oldHashSet);

                foreach (KeyT key in collection)
                    newHashSet.Add(key);

            } while (Interlocked.CompareExchange<HashSet<KeyT>>(ref m_HashSet, newHashSet, oldHashSet) != oldHashSet);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serial"></param>
        public void Remove(KeyT key) {
            HashSet<KeyT> newHashSet = null;
            HashSet<KeyT> oldHashSet = null;

            do {
                oldHashSet = m_HashSet;

                newHashSet = new HashSet<KeyT>(oldHashSet);

                newHashSet.Remove(key);

            } while (Interlocked.CompareExchange<HashSet<KeyT>>(ref m_HashSet, newHashSet, oldHashSet) != oldHashSet);
        }

        /// <summary>
        /// �ӵ�ǰ LockFreeHashSet �������Ƴ�ָ�������е�����Ԫ�ء�
        /// </summary>
        /// <param name="other"></param>
        public void ExceptWith(IEnumerable<KeyT> other) {
            HashSet<KeyT> newHashSet = null;
            HashSet<KeyT> oldHashSet = null;

            do {
                oldHashSet = m_HashSet;

                newHashSet = new HashSet<KeyT>(oldHashSet);

                newHashSet.ExceptWith(other);

            } while (Interlocked.CompareExchange<HashSet<KeyT>>(ref m_HashSet, newHashSet, oldHashSet) != oldHashSet);
        }

        /// <summary>
        /// �� LockFreeHashSet �������Ƴ���ָ����ν���������������ƥ�������Ԫ�ء�
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public int RemoveWhere(Predicate<KeyT> match) {
            HashSet<KeyT> tempHashSet = m_HashSet;
            if (tempHashSet.Count <= 0)
                return 0;

            List<KeyT> removeList = new List<KeyT>(tempHashSet.Count);

            foreach (KeyT key in tempHashSet) {
                if (match(key) == true)
                    removeList.Add(key);
            }

            if (removeList.Count <= 0)
                return 0;

            int iRemoveCount = 0;

            HashSet<KeyT> newHashSet = null;
            HashSet<KeyT> oldHashSet = null;

            do {
                iRemoveCount = 0;

                oldHashSet = m_HashSet;

                newHashSet = new HashSet<KeyT>(oldHashSet);

                foreach (KeyT itemKey in removeList) {
                    if (newHashSet.Remove(itemKey) == true)
                        iRemoveCount++;
                }

            } while (Interlocked.CompareExchange<HashSet<KeyT>>(ref m_HashSet, newHashSet, oldHashSet) != oldHashSet);

            return iRemoveCount;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Contains(KeyT key) {
            return m_HashSet.Contains(key);
        }

        /// <summary>
        /// �޸ĵ�ǰ�� LockFreeHashSet �����Խ������ö����ָ�������д��ڵ�Ԫ�ء�
        /// </summary>
        /// <param name="other"></param>
        public void IntersectWith(IEnumerable<KeyT> other) {
            HashSet<KeyT> newHashSet = null;
            HashSet<KeyT> oldHashSet = null;

            do {
                oldHashSet = m_HashSet;

                newHashSet = new HashSet<KeyT>(oldHashSet);

                newHashSet.IntersectWith(other);

            } while (Interlocked.CompareExchange<HashSet<KeyT>>(ref m_HashSet, newHashSet, oldHashSet) != oldHashSet);
        }

        /// <summary>
        /// ȷ�� LockFreeHashSet �����Ƿ�Ϊָ�����ϵ����Ӽ���
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsProperSubsetOf(IEnumerable<KeyT> other) {
            return m_HashSet.IsProperSubsetOf(other);
        }

        /// <summary>
        /// ȷ�� LockFreeHashSet �����Ƿ�Ϊָ�����ϵ��泬����
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsProperSupersetOf(IEnumerable<KeyT> other) {
            return m_HashSet.IsProperSupersetOf(other);
        }

        /// <summary>
        /// ȷ�� LockFreeHashSet �����Ƿ�Ϊָ�����ϵ��Ӽ���
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsSubsetOf(IEnumerable<KeyT> other) {
            return m_HashSet.IsSubsetOf(other);
        }

        /// <summary>
        /// ȷ�� LockFreeHashSet �����Ƿ�Ϊָ�����ϵĳ�����
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsSupersetOf(IEnumerable<KeyT> other) {
            return m_HashSet.IsSupersetOf(other);
        }

        /// <summary>
        /// ȷ����ǰ�� LockFreeHashSet �����Ƿ���ָ���ļ����ص���
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Overlaps(IEnumerable<KeyT> other) {
            return m_HashSet.Overlaps(other);
        }

        /// <summary>
        /// ȷ�� LockFreeHashSet ������ָ���ļ������Ƿ������ͬ��Ԫ�ء�
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool SetEquals(IEnumerable<KeyT> other) {
            return m_HashSet.SetEquals(other);
        }

        /// <summary>
        /// �޸ĵ�ǰ�� LockFreeHashSet �����Խ������ö����ָ�������д��ڵ�Ԫ�أ�������ͬʱ���������е�Ԫ�أ���
        /// </summary>
        /// <param name="other"></param>
        public void SymmetricExceptWith(IEnumerable<KeyT> other) {
            HashSet<KeyT> newHashSet = null;
            HashSet<KeyT> oldHashSet = null;

            do {
                oldHashSet = m_HashSet;

                newHashSet = new HashSet<KeyT>(oldHashSet);

                newHashSet.SymmetricExceptWith(other);

            } while (Interlocked.CompareExchange<HashSet<KeyT>>(ref m_HashSet, newHashSet, oldHashSet) != oldHashSet);
        }

        /// <summary>
        /// �޸ĵ�ǰ�� LockFreeHashSet �����԰����ö������ָ�������д��ڵ�����Ԫ�ء�
        /// </summary>
        /// <param name="other"></param>
        public void UnionWith(IEnumerable<KeyT> other) {
            HashSet<KeyT> newHashSet = null;
            HashSet<KeyT> oldHashSet = null;

            do {
                oldHashSet = m_HashSet;

                newHashSet = new HashSet<KeyT>(oldHashSet);

                newHashSet.UnionWith(other);

            } while (Interlocked.CompareExchange<HashSet<KeyT>>(ref m_HashSet, newHashSet, oldHashSet) != oldHashSet);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Clear() {
            HashSet<KeyT> newHashSet = null;
            HashSet<KeyT> oldHashSet = null;

            do {
                oldHashSet = m_HashSet;

                newHashSet = new HashSet<KeyT>();

            } while (Interlocked.CompareExchange<HashSet<KeyT>>(ref m_HashSet, newHashSet, oldHashSet) != oldHashSet);
        }

        #region zh-CHS ˽�г�Ա���� | en Private Member Variables
        #region zh-CHS ˽�г��� | en Private Constants
        /// <summary>
        /// 
        /// </summary>
        private readonly static KeyT[] s_ZeroKeyArray = new KeyT[0];
        #endregion
        /// <summary>
        /// 
        /// </summary>
        private volatile KeyT[] m_KeyArray = s_ZeroKeyArray;
        /// <summary>
        /// 
        /// </summary>
        private HashSet<KeyT> m_CachedHashSet = null;
        /// <summary>
        /// 
        /// </summary>
        //private SpinLock m_LockCachedHashSet = new SpinLock();
          private readonly object m_LockCachedHashSet=new  object();
         
        #endregion

        #region zh-CHS ˽�з��� | en Private Methods
        /// <summary>
        /// 
        /// </summary>
        private KeyT[] InternalToArray() {
            HashSet<KeyT> newHashSet = null;
            HashSet<KeyT> oldHashSet = null;

            do {
                if (m_CachedHashSet == m_HashSet)
                    return m_KeyArray;

                oldHashSet = m_CachedHashSet;
                newHashSet = m_HashSet;

            } while (Interlocked.CompareExchange<HashSet<KeyT>>(ref m_CachedHashSet, newHashSet, oldHashSet) != oldHashSet);


            KeyT[] keyArray = new KeyT[newHashSet.Count];

            int iIndex = 0;
            foreach (KeyT key in newHashSet) {
                keyArray[iIndex] = key;
                ++iIndex;
            }

            // �������ǻ�ȡ���µ�����
            //bool localToken = false;
            //m_LockCachedHashSet.Enter(ref localToken);
            Monitor.Enter(m_LockCachedHashSet);
            {
                if (m_CachedHashSet == newHashSet)
                    m_KeyArray = keyArray;
            }
            //m_LockCachedHashSet.Exit();
            Monitor.Exit(m_LockCachedHashSet);

            return keyArray;
        }
        #endregion

        /// <summary>
        /// �ٶȱ�GetEnumerator(...)��
        /// </summary>
        /// <returns></returns>
        //[MultiThreadedWarning( "zh-CHS", "��ǰ���������б���ʱ������,���ܱ������������Ժ����:����!" )]
        public KeyT[] ToArray() {
            return InternalToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public KeyT[] ToArrayAndClear() {
            HashSet<KeyT> newHashSet = null;
            HashSet<KeyT> oldHashSet = null;

            do {
                oldHashSet = m_HashSet;

                newHashSet = new HashSet<KeyT>();

            } while (Interlocked.CompareExchange<HashSet<KeyT>>(ref m_HashSet, newHashSet, oldHashSet) != oldHashSet);

            // ��ʼ��ȡ����
            KeyT[] keyArray = new KeyT[newHashSet.Count];

            int iIndex = 0;
            foreach (KeyT key in oldHashSet) {
                keyArray[iIndex] = key;
                ++iIndex;
            }

            return keyArray;
        }
        #endregion

        #region zh-CHS �ӿ�ʵ�� | en Interface Implementation
        /// <summary>
        /// �ٶȱ�ToArray(...)��
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyT> GetEnumerator() {
            return m_HashSet.GetEnumerator();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        #endregion
    }

}
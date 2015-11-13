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
    /// ����ʲô���������ʹ��LockFreeMultiDictionary�ģ���Ȼ���õ��෴��Ч�������ܺܲ
    /// ���������ǳ��࣬д�ǳ��ǳ��١� 
    /// ���ַ����ĺô����ڻ�ȡ��ֵ��ʱ��û���κ�lock���Ӷ���������������
    /// </summary>
    /// <typeparam name="KeyT"></typeparam>
    /// <typeparam name="ValueT"></typeparam>
    public class LockFreeMultiDictionary<KeyT, ValueT> : IEnumerable<KeyValuePair<KeyT, IEnumerable<ValueT>>>
    {
        #region zh-CHS ����ͳ�ʼ�������� | en Constructors and Initializers and Dispose
        /// <summary>
        /// allowDuplicateValues == false
        /// </summary>
        /// <param name="iCapacity"></param>
        public LockFreeMultiDictionary() {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iCapacity"></param>
        public LockFreeMultiDictionary(bool allowDuplicateValues) {
            m_AllowDuplicateValues = allowDuplicateValues;
            m_MultiDictionary = new MultiDictionary<KeyT, ValueT>(m_AllowDuplicateValues);
        }
        #endregion

        #region zh-CHS �������� | en Public Properties

        /// <summary>
        /// 
        /// </summary>
        public int Count {
            get { return m_MultiDictionary.Count; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IEnumerable<ValueT> this[KeyT key] {
            get { return m_MultiDictionary[key]; }
            set { this.ReplaceMany(key, value); }
        }

        #endregion

        #region zh-CHS ���з��� | en Public Methods
        #region zh-CHS ˽�г�Ա���� | en Private Member Variables
        /// <summary>
        /// 
        /// </summary>
        private bool m_AllowDuplicateValues = false;
        /// <summary>
        /// 
        /// </summary>
        private MultiDictionary<KeyT, ValueT> m_MultiDictionary = new MultiDictionary<KeyT, ValueT>(false);
        #endregion
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(KeyT key, ValueT value) {
            MultiDictionary<KeyT, ValueT> newMultiDict = null;
            MultiDictionary<KeyT, ValueT> oldMultiDict = null;

            do {
                oldMultiDict = m_MultiDictionary;

                newMultiDict = new MultiDictionary<KeyT, ValueT>(m_AllowDuplicateValues);
                foreach (var item in oldMultiDict)
                    newMultiDict.AddMany(item.Key, item.Value);

                newMultiDict.Add(key, value);

            } while (Interlocked.CompareExchange<MultiDictionary<KeyT, ValueT>>(ref m_MultiDictionary, newMultiDict, oldMultiDict) != oldMultiDict);
        }

        /// <summary>
        /// �뾡���ܵ�ʹ��AddMany(...) �� Add(...) ��ӵ�����ö�
        /// </summary>
        /// <param name="serial"></param>
        /// <param name="realm"></param>
        public void AddMany(KeyT key, IEnumerable<ValueT> values) {
            MultiDictionary<KeyT, ValueT> newMultiDict = null;
            MultiDictionary<KeyT, ValueT> oldMultiDict = null;

            do {
                oldMultiDict = m_MultiDictionary;

                newMultiDict = new MultiDictionary<KeyT, ValueT>(m_AllowDuplicateValues);
                foreach (var item in oldMultiDict)
                    newMultiDict.AddMany(item.Key, item.Value);

                newMultiDict.AddMany(key, values);

            } while (Interlocked.CompareExchange<MultiDictionary<KeyT, ValueT>>(ref m_MultiDictionary, newMultiDict, oldMultiDict) != oldMultiDict);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serial"></param>
        public void Remove(KeyT key) {
            MultiDictionary<KeyT, ValueT> newMultiDict = null;
            MultiDictionary<KeyT, ValueT> oldMultiDict = null;

            do {
                oldMultiDict = m_MultiDictionary;

                newMultiDict = new MultiDictionary<KeyT, ValueT>(m_AllowDuplicateValues);
                foreach (var item in oldMultiDict)
                    newMultiDict.AddMany(item.Key, item.Value);

                newMultiDict.Remove(key);

            } while (Interlocked.CompareExchange<MultiDictionary<KeyT, ValueT>>(ref m_MultiDictionary, newMultiDict, oldMultiDict) != oldMultiDict);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serial"></param>
        public void Remove(KeyT key, ValueT value) {
            MultiDictionary<KeyT, ValueT> newMultiDict = null;
            MultiDictionary<KeyT, ValueT> oldMultiDict = null;

            do {
                oldMultiDict = m_MultiDictionary;

                newMultiDict = new MultiDictionary<KeyT, ValueT>(m_AllowDuplicateValues);
                foreach (var item in oldMultiDict)
                    newMultiDict.AddMany(item.Key, item.Value);

                newMultiDict.Remove(key, value);

            } while (Interlocked.CompareExchange<MultiDictionary<KeyT, ValueT>>(ref m_MultiDictionary, newMultiDict, oldMultiDict) != oldMultiDict);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serial"></param>
        public void RemoveMany(IEnumerable<KeyT> key) {
            MultiDictionary<KeyT, ValueT> newMultiDict = null;
            MultiDictionary<KeyT, ValueT> oldMultiDict = null;

            do {
                oldMultiDict = m_MultiDictionary;

                newMultiDict = new MultiDictionary<KeyT, ValueT>(m_AllowDuplicateValues);
                foreach (var item in oldMultiDict)
                    newMultiDict.AddMany(item.Key, item.Value);

                newMultiDict.RemoveMany(key);

            } while (Interlocked.CompareExchange<MultiDictionary<KeyT, ValueT>>(ref m_MultiDictionary, newMultiDict, oldMultiDict) != oldMultiDict);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serial"></param>
        public void RemoveMany(KeyT key, IEnumerable<ValueT> values) {
            MultiDictionary<KeyT, ValueT> newMultiDict = null;
            MultiDictionary<KeyT, ValueT> oldMultiDict = null;

            do {
                oldMultiDict = m_MultiDictionary;

                newMultiDict = new MultiDictionary<KeyT, ValueT>(m_AllowDuplicateValues);
                foreach (var item in oldMultiDict)
                    newMultiDict.AddMany(item.Key, item.Value);

                newMultiDict.RemoveMany(key, values);

            } while (Interlocked.CompareExchange<MultiDictionary<KeyT, ValueT>>(ref m_MultiDictionary, newMultiDict, oldMultiDict) != oldMultiDict);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public int RemoveAll(Predicate<KeyT, ValueT> match) {
            MultiDictionary<KeyT, ValueT> tempMultiDictionary = m_MultiDictionary;
            if (tempMultiDictionary.Count <= 0)
                return 0;

            List<KeyValuePair<KeyT, ValueT>> removeList = new List<KeyValuePair<KeyT, ValueT>>(tempMultiDictionary.Count);

            foreach (KeyValuePair<KeyT, ICollection<ValueT>> keyValuePair in tempMultiDictionary) {
                foreach (ValueT valueItem in keyValuePair.Value) {
                    if (match(keyValuePair.Key, valueItem) == true)
                        removeList.Add(new KeyValuePair<KeyT, ValueT>(keyValuePair.Key, valueItem));
                }
            }

            if (removeList.Count <= 0)
                return 0;

            int iRemoveCount = 0;

            MultiDictionary<KeyT, ValueT> newMultiDict = null;
            MultiDictionary<KeyT, ValueT> oldMultiDict = null;

            do {
                iRemoveCount = 0;

                oldMultiDict = m_MultiDictionary;

                newMultiDict = new MultiDictionary<KeyT, ValueT>(m_AllowDuplicateValues);
                foreach (var item in oldMultiDict)
                    newMultiDict.AddMany(item.Key, item.Value);

                foreach (KeyValuePair<KeyT, ValueT> keyValuePair in removeList) {
                    if (newMultiDict.Remove(keyValuePair.Key, keyValuePair.Value) == true)
                        iRemoveCount++;
                }

            } while (Interlocked.CompareExchange<MultiDictionary<KeyT, ValueT>>(ref m_MultiDictionary, newMultiDict, oldMultiDict) != oldMultiDict);

            return iRemoveCount;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        public void Replace(KeyT key, ValueT value) {
            MultiDictionary<KeyT, ValueT> newMultiDict = null;
            MultiDictionary<KeyT, ValueT> oldMultiDict = null;

            newMultiDict = new MultiDictionary<KeyT, ValueT>(m_AllowDuplicateValues);
            newMultiDict.Add(key, value);

            do {
                oldMultiDict = m_MultiDictionary;

            } while (Interlocked.CompareExchange<MultiDictionary<KeyT, ValueT>>(ref m_MultiDictionary, newMultiDict, oldMultiDict) != oldMultiDict);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        public void ReplaceMany(KeyT key, IEnumerable<ValueT> values) {
            MultiDictionary<KeyT, ValueT> newMultiDict = null;
            MultiDictionary<KeyT, ValueT> oldMultiDict = null;

            newMultiDict = new MultiDictionary<KeyT, ValueT>(m_AllowDuplicateValues);
            newMultiDict.AddMany(key, values);

            do {
                oldMultiDict = m_MultiDictionary;

            } while (Interlocked.CompareExchange<MultiDictionary<KeyT, ValueT>>(ref m_MultiDictionary, newMultiDict, oldMultiDict) != oldMultiDict);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serial"></param>
        public IEnumerable<ValueT> GetValues(KeyT key) {
            return m_MultiDictionary[key];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serial"></param>
        public bool TryGetValues(KeyT key, out IEnumerable<ValueT> values) {
            values = null;

            ICollection<ValueT> collection = m_MultiDictionary[key];
            if (collection == null)
                return false;

            if (collection.Count <= 0)
                return false;

            values = collection;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(KeyT key) {
            return m_MultiDictionary.ContainsKey(key);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool ContainsValue(KeyT key, ValueT value) {
            return m_MultiDictionary.Contains(key, value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public bool Find(Predicate<KeyT, ValueT> match, out KeyValuePair<KeyT, ValueT> findKeyValuePair) {
            findKeyValuePair = new KeyValuePair<KeyT, ValueT>(default(KeyT), default(ValueT));

            MultiDictionary<KeyT, ValueT> tempMultiDictionary = m_MultiDictionary;
            if (tempMultiDictionary.Count <= 0)
                return false;

            foreach (KeyValuePair<KeyT, ICollection<ValueT>> keyValuePair in tempMultiDictionary) {
                foreach (ValueT valueItem in keyValuePair.Value) {
                    if (match(keyValuePair.Key, valueItem) == true) {
                        findKeyValuePair = new KeyValuePair<KeyT, ValueT>(keyValuePair.Key, valueItem);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public KeyValuePair<KeyT, ValueT>[] FindAll(Predicate<KeyT, ValueT> match) {
            List<KeyValuePair<KeyT, ValueT>> keyValuePairList = new List<KeyValuePair<KeyT, ValueT>>();

            MultiDictionary<KeyT, ValueT> tempMultiDictionary = m_MultiDictionary;
            if (tempMultiDictionary.Count <= 0)
                return keyValuePairList.ToArray();
            else
                keyValuePairList.Capacity = tempMultiDictionary.Count;

            foreach (KeyValuePair<KeyT, ICollection<ValueT>> keyValuePair in tempMultiDictionary) {
                foreach (ValueT valueItem in keyValuePair.Value) {
                    if (match(keyValuePair.Key, valueItem) == true)
                        keyValuePairList.Add(new KeyValuePair<KeyT, ValueT>(keyValuePair.Key, valueItem));
                }
            }

            return keyValuePairList.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        public void ForEach(Action<KeyT, ValueT> action) {
            MultiDictionary<KeyT, ValueT> tempMultiDictionary = m_MultiDictionary;
            if (tempMultiDictionary.Count <= 0)
                return;

            foreach (KeyValuePair<KeyT, ICollection<ValueT>> keyValuePair in tempMultiDictionary) {
                foreach (ValueT valueItem in keyValuePair.Value)
                    action(keyValuePair.Key, valueItem);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public bool Exists(Predicate<KeyT, ValueT> match) {
            MultiDictionary<KeyT, ValueT> tempMultiDictionary = m_MultiDictionary;
            if (tempMultiDictionary.Count <= 0)
                return false;

            foreach (KeyValuePair<KeyT, ICollection<ValueT>> keyValuePair in tempMultiDictionary) {
                foreach (ValueT valueItem in keyValuePair.Value) {
                    if (match(keyValuePair.Key, valueItem) == true)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Clear() {
            MultiDictionary<KeyT, ValueT> newMultiDict = null;
            MultiDictionary<KeyT, ValueT> oldMultiDict = null;

            do {
                oldMultiDict = m_MultiDictionary;

                newMultiDict = new MultiDictionary<KeyT, ValueT>(m_AllowDuplicateValues);

            } while (Interlocked.CompareExchange<MultiDictionary<KeyT, ValueT>>(ref m_MultiDictionary, newMultiDict, oldMultiDict) != oldMultiDict);
        }

        #region zh-CHS ˽�г�Ա���� | en Private Member Variables
        #region zh-CHS ˽�г��� | en Private Constants
        /// <summary>
        /// 
        /// </summary>
        private readonly static KeyT[] s_ZeroKeyArray = new KeyT[0];
        /// <summary>
        /// 
        /// </summary>
        private readonly static ValueT[] s_ZeroValueArray = new ValueT[0];
        /// <summary>
        /// 
        /// </summary>
        private readonly static KeyValuePair<KeyT, ValueT[]>[] s_ZeroKeyValuePairArray = new KeyValuePair<KeyT, ValueT[]>[0];
        #endregion
        /// <summary>
        /// 
        /// </summary>
        private volatile KeyT[] m_KeyArray = s_ZeroKeyArray;
        /// <summary>
        /// 
        /// </summary>
        private volatile Dictionary<KeyT, ValueT[]> m_ValueDictionary = null;
        /// <summary>
        /// 
        /// </summary>
        private volatile KeyValuePair<KeyT, ValueT[]>[] m_KeyValuePairArray = s_ZeroKeyValuePairArray;
        /// <summary>
        /// 
        /// </summary>
        private MultiDictionary<KeyT, ValueT> m_CachedMultiDictionary = null;
        /// <summary>
        /// 
        /// </summary>
        //private SpinLock m_LockCachedDictionary = new SpinLock();
        private readonly object m_LockCachedDictionary = new object();
        #endregion

        #region zh-CHS ˽�з��� | en Private Methods
        /// <summary>
        /// 
        /// </summary>
        private KeyValuePair<KeyT, ValueT[]>[] InternalToArray() {
            MultiDictionary<KeyT, ValueT> newMultiDict = null;
            MultiDictionary<KeyT, ValueT> oldMultiDict = null;

            do {
                if (m_CachedMultiDictionary == m_MultiDictionary)
                    return m_KeyValuePairArray;

                oldMultiDict = m_CachedMultiDictionary;
                newMultiDict = m_MultiDictionary;

            } while (Interlocked.CompareExchange<MultiDictionary<KeyT, ValueT>>(ref m_CachedMultiDictionary, newMultiDict, oldMultiDict) != oldMultiDict);


            KeyT[] keyArray = new KeyT[newMultiDict.Count];
            Dictionary<KeyT, ValueT[]> valueArray = new Dictionary<KeyT, ValueT[]>(newMultiDict.Count);
            KeyValuePair<KeyT, ValueT[]>[] keyValuePairArray = new KeyValuePair<KeyT, ValueT[]>[newMultiDict.Count];

            int iIndex = 0;
            foreach (KeyValuePair<KeyT, ICollection<ValueT>> keyValuePair in newMultiDict) {
                keyArray[iIndex] = keyValuePair.Key;

                ValueT[] tempValueArray = new ValueT[keyValuePair.Value.Count];

                int iIndex2 = 0;
                foreach (ValueT value in keyValuePair.Value) {
                    tempValueArray[iIndex2] = value;

                    ++iIndex2;
                }

                valueArray.Add(keyValuePair.Key, tempValueArray);
                keyValuePairArray[iIndex] = new KeyValuePair<KeyT, ValueT[]>(keyValuePair.Key, tempValueArray);

                ++iIndex;
            }

            // �������ǻ�ȡ���µ�����
            //bool locktoken = false;
            //m_LockCachedDictionary.Enter(ref locktoken);
            Monitor.Enter(m_LockCachedDictionary);
            {
                if (m_CachedMultiDictionary == newMultiDict) {
                    m_KeyArray = keyArray;
                    m_ValueDictionary = valueArray;
                    m_KeyValuePairArray = keyValuePairArray;
                }
            }
            //m_LockCachedDictionary.Exit();
            Monitor.Exit(m_LockCachedDictionary);
            return keyValuePairArray;
        }

        /// <summary>
        /// 
        /// </summary>
        private ValueT[] InternalToArrayValuesByKey(KeyT key) {
            MultiDictionary<KeyT, ValueT> newMultiDict = null;
            MultiDictionary<KeyT, ValueT> oldMultiDict = null;

            do {
                if (m_CachedMultiDictionary == m_MultiDictionary) {
                    ValueT[] tempValues = s_ZeroValueArray;

                    Dictionary<KeyT, ValueT[]> tempValueDictionary = m_ValueDictionary;
                    if (tempValueDictionary != null)
                        tempValueDictionary.TryGetValue(key, out tempValues);

                    return tempValues;
                }

                oldMultiDict = m_CachedMultiDictionary;
                newMultiDict = m_MultiDictionary;

            } while (Interlocked.CompareExchange<MultiDictionary<KeyT, ValueT>>(ref m_CachedMultiDictionary, newMultiDict, oldMultiDict) != oldMultiDict);


            KeyT[] keyArray = new KeyT[newMultiDict.Count];
            Dictionary<KeyT, ValueT[]> valueDictionary = new Dictionary<KeyT, ValueT[]>(newMultiDict.Count);
            KeyValuePair<KeyT, ValueT[]>[] keyValuePairArray = new KeyValuePair<KeyT, ValueT[]>[newMultiDict.Count];

            int iIndex = 0;
            foreach (KeyValuePair<KeyT, ICollection<ValueT>> keyValuePair in newMultiDict) {
                keyArray[iIndex] = keyValuePair.Key;

                ValueT[] tempValueArray = new ValueT[keyValuePair.Value.Count];

                int iIndex2 = 0;
                foreach (ValueT value in keyValuePair.Value) {
                    tempValueArray[iIndex2] = value;

                    ++iIndex2;
                }

                valueDictionary.Add(keyValuePair.Key, tempValueArray);
                keyValuePairArray[iIndex] = new KeyValuePair<KeyT, ValueT[]>(keyValuePair.Key, tempValueArray);

                ++iIndex;
            }

            // �������ǻ�ȡ���µ�����
            //bool locktoken = false;
            //m_LockCachedDictionary.Enter(ref locktoken);
            Monitor.Enter(m_LockCachedDictionary);
            {
                if (m_CachedMultiDictionary == newMultiDict) {
                    m_KeyArray = keyArray;
                    m_ValueDictionary = valueDictionary;
                    m_KeyValuePairArray = keyValuePairArray;
                }
            }
           // m_LockCachedDictionary.Exit();
            Monitor.Exit(m_LockCachedDictionary);
            ValueT[] returnValues = s_ZeroValueArray;

            valueDictionary.TryGetValue(key, out returnValues);

            return returnValues;
        }

        /// <summary>
        /// 
        /// </summary>
        private KeyT[] InternalToArrayKeys() {
            MultiDictionary<KeyT, ValueT> newMultiDict = null;
            MultiDictionary<KeyT, ValueT> oldMultiDict = null;

            do {
                if (m_CachedMultiDictionary == m_MultiDictionary)
                    return m_KeyArray;

                oldMultiDict = m_CachedMultiDictionary;
                newMultiDict = m_MultiDictionary;

            } while (Interlocked.CompareExchange<MultiDictionary<KeyT, ValueT>>(ref m_CachedMultiDictionary, newMultiDict, oldMultiDict) != oldMultiDict);


            KeyT[] keyArray = new KeyT[newMultiDict.Count];
            Dictionary<KeyT, ValueT[]> valueDictionary = new Dictionary<KeyT, ValueT[]>(newMultiDict.Count);
            KeyValuePair<KeyT, ValueT[]>[] keyValuePairArray = new KeyValuePair<KeyT, ValueT[]>[newMultiDict.Count];

            int iIndex = 0;
            foreach (KeyValuePair<KeyT, ICollection<ValueT>> keyValuePair in newMultiDict) {
                keyArray[iIndex] = keyValuePair.Key;

                ValueT[] tempValueArray = new ValueT[keyValuePair.Value.Count];

                int iIndex2 = 0;
                foreach (ValueT value in keyValuePair.Value) {
                    tempValueArray[iIndex2] = value;

                    ++iIndex2;
                }

                valueDictionary.Add(keyValuePair.Key, tempValueArray);
                keyValuePairArray[iIndex] = new KeyValuePair<KeyT, ValueT[]>(keyValuePair.Key, tempValueArray);

                ++iIndex;
            }

            // �������ǻ�ȡ���µ�����
            //bool locktoken = false;
            //m_LockCachedDictionary.Enter(ref locktoken);
            Monitor.Enter(m_LockCachedDictionary);
            {
                if (m_CachedMultiDictionary == newMultiDict) {
                    m_KeyArray = keyArray;
                    m_ValueDictionary = valueDictionary;
                    m_KeyValuePairArray = keyValuePairArray;
                }
            }
            //m_LockCachedDictionary.Exit();
            Monitor.Exit(m_LockCachedDictionary);
            return keyArray;
        }
        #endregion
        /// <summary>
        /// �ٶȱ�GetEnumerator(...)��
        /// </summary>
        /// <returns></returns>
        //[MultiThreadedWarning("zh-CHS", "��ǰ���������б���ʱ������,���ܱ������������Ժ����:����!")]
        public KeyValuePair<KeyT, ValueT[]>[] ToArray() {
            return InternalToArray();
        }

        /// <summary>
        /// �ٶȱ�GetEnumerator(...)��
        /// </summary>
        /// <returns></returns>
        //[MultiThreadedWarning("zh-CHS", "��ǰ���������б���ʱ������,���ܱ������������Ժ����:����!")]
        public ValueT[] ToArrayValuesByKey(KeyT key) {
            return InternalToArrayValuesByKey(key);
        }

        /// <summary>
        /// �ٶȱ�GetEnumerator(...)��
        /// </summary>
        /// <returns></returns>
        //[MultiThreadedWarning("zh-CHS", "��ǰ���������б���ʱ������,���ܱ������������Ժ����:����!")]
        public KeyT[] ToArrayKeys() {
            return InternalToArrayKeys();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public KeyValuePair<KeyT, ValueT[]>[] ToArrayAndClear() {
            MultiDictionary<KeyT, ValueT> newMultiDict = null;
            MultiDictionary<KeyT, ValueT> oldMultiDict = null;

            do {
                oldMultiDict = m_MultiDictionary;

                newMultiDict = new MultiDictionary<KeyT, ValueT>(m_AllowDuplicateValues);

            } while (Interlocked.CompareExchange<MultiDictionary<KeyT, ValueT>>(ref m_MultiDictionary, newMultiDict, oldMultiDict) != oldMultiDict);

            // ��ʼ��ȡ����
            KeyValuePair<KeyT, ValueT[]>[] keyValuePairArray = new KeyValuePair<KeyT, ValueT[]>[newMultiDict.Count];

            int iIndex = 0;
            foreach (KeyValuePair<KeyT, ICollection<ValueT>> keyValuePair in oldMultiDict) {
                ValueT[] valueArray = new ValueT[keyValuePair.Value.Count];

                int iIndex2 = 0;
                foreach (ValueT value in keyValuePair.Value) {
                    valueArray[iIndex2] = value;

                    ++iIndex2;
                }

                keyValuePairArray[iIndex] = new KeyValuePair<KeyT, ValueT[]>(keyValuePair.Key, valueArray);

                ++iIndex;
            }

            return keyValuePairArray;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public ValueT[] ToArrayValuesByKeyAndClear(KeyT key) {
            MultiDictionary<KeyT, ValueT> newMultiDict = null;
            MultiDictionary<KeyT, ValueT> oldMultiDict = null;

            do {
                oldMultiDict = m_MultiDictionary;

                newMultiDict = new MultiDictionary<KeyT, ValueT>(m_AllowDuplicateValues);

            } while (Interlocked.CompareExchange<MultiDictionary<KeyT, ValueT>>(ref m_MultiDictionary, newMultiDict, oldMultiDict) != oldMultiDict);

            // ��ʼ��ȡ����
            ValueT[] valueArray = s_ZeroValueArray;

            EqualityComparer<KeyT> equalityComparer = EqualityComparer<KeyT>.Default;

            foreach (KeyValuePair<KeyT, ICollection<ValueT>> keyValuePair in oldMultiDict) {
                if (equalityComparer.Equals(keyValuePair.Key, key) == true) {
                    ValueT[] tempValueArray = new ValueT[keyValuePair.Value.Count];

                    int iIndex = 0;
                    foreach (ValueT value in keyValuePair.Value) {
                        tempValueArray[iIndex] = value;

                        ++iIndex;
                    }

                    valueArray = tempValueArray;
                    break;
                }
            }

            return valueArray;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public KeyT[] ToArrayKeysAndClear() {
            MultiDictionary<KeyT, ValueT> newMultiDict = null;
            MultiDictionary<KeyT, ValueT> oldMultiDict = null;

            do {
                oldMultiDict = m_MultiDictionary;

                newMultiDict = new MultiDictionary<KeyT, ValueT>(m_AllowDuplicateValues);

            } while (Interlocked.CompareExchange<MultiDictionary<KeyT, ValueT>>(ref m_MultiDictionary, newMultiDict, oldMultiDict) != oldMultiDict);

            // ��ʼ��ȡ����
            KeyT[] keyArray = new KeyT[newMultiDict.Count];

            int iIndex = 0;
            foreach (KeyValuePair<KeyT, ICollection<ValueT>> keyValuePair in oldMultiDict) {
                keyArray[iIndex] = keyValuePair.Key;

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
        public IEnumerator<KeyValuePair<KeyT, IEnumerable<ValueT>>> GetEnumerator() {
            MultiDictionary<KeyT, ValueT> tempMultiDict = m_MultiDictionary;
            foreach (var item in tempMultiDict)
                yield return new KeyValuePair<KeyT, IEnumerable<ValueT>>(item.Key, item.Value);

            yield break;
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
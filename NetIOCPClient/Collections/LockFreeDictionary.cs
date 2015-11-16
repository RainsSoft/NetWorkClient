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
    /// ����ʲô���������ʹ��LockFreeDictionary�ģ���Ȼ���õ��෴��Ч�������ܺܲ
    /// ���������ǳ��࣬д�ǳ��ǳ��١� 
    /// ���ַ����ĺô����ڻ�ȡ��ֵ��ʱ��û���κ�lock���Ӷ���������������
    /// </summary>
    public class LockFreeDictionary<KeyT, ValueT> : IEnumerable<KeyValuePair<KeyT, ValueT>>
    {
        #region zh-CHS ����ͳ�ʼ�������� | en Constructors and Initializers and Dispose
        /// <summary>
        /// 
        /// </summary>
        public LockFreeDictionary() {
        }

        /// <summary>
        /// 
        /// </summary>
        public LockFreeDictionary(IDictionary<KeyT, ValueT> dictionary) {
            m_Dictionary = new Dictionary<KeyT, ValueT>(dictionary);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iCapacity"></param>
        public LockFreeDictionary(int iCapacity) {
            m_Dictionary = new Dictionary<KeyT, ValueT>(iCapacity);
        }
        #endregion

        #region zh-CHS �������� | en Public Properties

        /// <summary>
        /// 
        /// </summary>
        public int Count {
            get { return m_Dictionary.Count; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public ValueT this[KeyT key] {
            get { return m_Dictionary[key]; }
            set { this.Add(key, value); }
        }

        #endregion

        #region zh-CHS ���з��� | en Public Methods
        #region zh-CHS ˽�г�Ա���� | en Private Member Variables
        /// <summary>
        /// 
        /// </summary>
        private Dictionary<KeyT, ValueT> m_Dictionary = new Dictionary<KeyT, ValueT>();
        #endregion
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(KeyT key, ValueT value) {
            Dictionary<KeyT, ValueT> newDict = null;
            Dictionary<KeyT, ValueT> oldDict = null;

            do {
                oldDict = m_Dictionary;

                newDict = new Dictionary<KeyT, ValueT>(oldDict);
                newDict[key] = value;

            } while (Interlocked.CompareExchange<Dictionary<KeyT, ValueT>>(ref m_Dictionary, newDict, oldDict) != oldDict);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serial"></param>
        /// <param name="realm"></param>
        public void AddRange(IDictionary<KeyT, ValueT> dictionary) {
            Dictionary<KeyT, ValueT> newDict = null;
            Dictionary<KeyT, ValueT> oldDict = null;

            do {
                oldDict = m_Dictionary;

                newDict = new Dictionary<KeyT, ValueT>(oldDict);

                foreach (KeyValuePair<KeyT, ValueT> keyValuePair in dictionary)
                    newDict[keyValuePair.Key] = keyValuePair.Value;

            } while (Interlocked.CompareExchange<Dictionary<KeyT, ValueT>>(ref m_Dictionary, newDict, oldDict) != oldDict);
        }

        /// <summary>
        /// �뾡���ܵ�ʹ��AddRange(...) �� Add(...) ��ӵ�����ö�
        /// </summary>
        /// <param name="serial"></param>
        /// <param name="realm"></param>
        public void AddRange(IEnumerable<KeyValuePair<KeyT, ValueT>> collection) {
            Dictionary<KeyT, ValueT> newDict = null;
            Dictionary<KeyT, ValueT> oldDict = null;

            do {
                oldDict = m_Dictionary;

                newDict = new Dictionary<KeyT, ValueT>(oldDict);

                foreach (KeyValuePair<KeyT, ValueT> keyValuePair in collection)
                    newDict[keyValuePair.Key] = keyValuePair.Value;

            } while (Interlocked.CompareExchange<Dictionary<KeyT, ValueT>>(ref m_Dictionary, newDict, oldDict) != oldDict);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serial"></param>
        public void Remove(KeyT key) {
            Dictionary<KeyT, ValueT> newDict = null;
            Dictionary<KeyT, ValueT> oldDict = null;

            do {
                oldDict = m_Dictionary;

                newDict = new Dictionary<KeyT, ValueT>(oldDict);

                newDict.Remove(key);

            } while (Interlocked.CompareExchange<Dictionary<KeyT, ValueT>>(ref m_Dictionary, newDict, oldDict) != oldDict);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public int RemoveAll(Predicate<KeyT, ValueT> match) {
            Dictionary<KeyT, ValueT> tempDictionary = m_Dictionary;
            if (tempDictionary.Count <= 0)
                return 0;

            List<KeyT> removeList = new List<KeyT>(tempDictionary.Count);

            foreach (KeyValuePair<KeyT, ValueT> itemT in tempDictionary) {
                if (match(itemT.Key, itemT.Value) == true)
                    removeList.Add(itemT.Key);
            }

            if (removeList.Count <= 0)
                return 0;

            int iRemoveCount = 0;

            Dictionary<KeyT, ValueT> newDict = null;
            Dictionary<KeyT, ValueT> oldDict = null;

            do {
                iRemoveCount = 0;

                oldDict = m_Dictionary;

                newDict = new Dictionary<KeyT, ValueT>(oldDict);

                foreach (KeyT itemKey in removeList) {
                    if (newDict.Remove(itemKey) == true)
                        iRemoveCount++;
                }

            } while (Interlocked.CompareExchange<Dictionary<KeyT, ValueT>>(ref m_Dictionary, newDict, oldDict) != oldDict);

            return iRemoveCount;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serial"></param>
        public ValueT GetValue(KeyT key) {
            return m_Dictionary[key];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serial"></param>
        public bool TryGetValue(KeyT key, out ValueT value) {
            return m_Dictionary.TryGetValue(key, out value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(KeyT key) {
            return m_Dictionary.ContainsKey(key);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool ContainsValue(ValueT value) {
            return m_Dictionary.ContainsValue(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public bool Find(Predicate<KeyT, ValueT> match, out KeyValuePair<KeyT, ValueT> findKeyValuePair) {
            findKeyValuePair = new KeyValuePair<KeyT, ValueT>(default(KeyT), default(ValueT));

            Dictionary<KeyT, ValueT> tempDictionary = m_Dictionary;
            if (tempDictionary.Count <= 0)
                return false;

            foreach (KeyValuePair<KeyT, ValueT> keyValuePair in tempDictionary) {
                if (match(keyValuePair.Key, keyValuePair.Value) == true) {
                    findKeyValuePair = keyValuePair;
                    return true;
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

            Dictionary<KeyT, ValueT> tempDictionary = m_Dictionary;
            if (tempDictionary.Count <= 0)
                return keyValuePairList.ToArray();
            else
                keyValuePairList.Capacity = tempDictionary.Count;

            foreach (KeyValuePair<KeyT, ValueT> keyValuePair in tempDictionary) {
                if (match(keyValuePair.Key, keyValuePair.Value) == true)
                    keyValuePairList.Add(keyValuePair);
            }

            return keyValuePairList.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        public void ForEach(Action<KeyT, ValueT> action) {
            Dictionary<KeyT, ValueT> tempDictionary = m_Dictionary;
            if (tempDictionary.Count <= 0)
                return;

            foreach (KeyValuePair<KeyT, ValueT> keyValuePair in tempDictionary)
                action(keyValuePair.Key, keyValuePair.Value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public bool Exists(Predicate<KeyT, ValueT> match) {
            Dictionary<KeyT, ValueT> tempDictionary = m_Dictionary;
            if (tempDictionary.Count <= 0)
                return false;

            foreach (KeyValuePair<KeyT, ValueT> keyValuePair in tempDictionary) {
                if (match(keyValuePair.Key, keyValuePair.Value) == true)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Clear() {
            Dictionary<KeyT, ValueT> newDict = null;
            Dictionary<KeyT, ValueT> oldDict = null;

            do {
                oldDict = m_Dictionary;

                newDict = new Dictionary<KeyT, ValueT>();

            } while (Interlocked.CompareExchange<Dictionary<KeyT, ValueT>>(ref m_Dictionary, newDict, oldDict) != oldDict);
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
        private readonly static KeyValuePair<KeyT, ValueT>[] s_ZeroKeyValuePairArray = new KeyValuePair<KeyT, ValueT>[0];
        #endregion
        /// <summary>
        /// 
        /// </summary>
        private volatile KeyT[] m_KeyArray = s_ZeroKeyArray;
        /// <summary>
        /// 
        /// </summary>
        private volatile ValueT[] m_ValueArray = s_ZeroValueArray;
        /// <summary>
        /// 
        /// </summary>
        private volatile KeyValuePair<KeyT, ValueT>[] m_KeyValuePairArray = s_ZeroKeyValuePairArray;
        /// <summary>
        /// 
        /// </summary>
        private Dictionary<KeyT, ValueT> m_CachedDictionary = null;
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
        private KeyValuePair<KeyT, ValueT>[] InternalToArray() {
            Dictionary<KeyT, ValueT> newDict = null;
            Dictionary<KeyT, ValueT> oldDict = null;

            do {
                if (m_CachedDictionary == m_Dictionary)
                    return m_KeyValuePairArray;

                oldDict = m_CachedDictionary;
                newDict = m_Dictionary;

            } while (Interlocked.CompareExchange<Dictionary<KeyT, ValueT>>(ref m_CachedDictionary, newDict, oldDict) != oldDict);


            KeyT[] keyArray = new KeyT[newDict.Count];
            ValueT[] valueArray = new ValueT[newDict.Count];
            KeyValuePair<KeyT, ValueT>[] keyValuePairArray = new KeyValuePair<KeyT, ValueT>[newDict.Count];

            int iIndex = 0;
            foreach (KeyValuePair<KeyT, ValueT> keyValuePair in newDict) {
                keyArray[iIndex] = keyValuePair.Key;
                valueArray[iIndex] = keyValuePair.Value;
                keyValuePairArray[iIndex] = keyValuePair;

                ++iIndex;
            }

            // �������ǻ�ȡ���µ�����
            //bool localToken = false;
            // m_LockCachedDictionary.Enter(ref localToken);
            Monitor.Enter(m_LockCachedDictionary);
            {
                if (m_CachedDictionary == newDict) {
                    m_KeyArray = keyArray;
                    m_ValueArray = valueArray;
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
        private ValueT[] InternalToArrayValues() {
            Dictionary<KeyT, ValueT> newDict = null;
            Dictionary<KeyT, ValueT> oldDict = null;

            do {
                if (m_CachedDictionary == m_Dictionary)
                    return m_ValueArray;

                oldDict = m_CachedDictionary;
                newDict = m_Dictionary;

            } while (Interlocked.CompareExchange<Dictionary<KeyT, ValueT>>(ref m_CachedDictionary, newDict, oldDict) != oldDict);


            KeyT[] keyArray = new KeyT[newDict.Count];
            ValueT[] valueArray = new ValueT[newDict.Count];
            KeyValuePair<KeyT, ValueT>[] keyValuePairArray = new KeyValuePair<KeyT, ValueT>[newDict.Count];

            int iIndex = 0;
            foreach (KeyValuePair<KeyT, ValueT> keyValuePair in newDict) {
                keyArray[iIndex] = keyValuePair.Key;
                valueArray[iIndex] = keyValuePair.Value;
                keyValuePairArray[iIndex] = keyValuePair;

                ++iIndex;
            }

            // �������ǻ�ȡ���µ�����
            //bool localToken = false;
            //m_LockCachedDictionary.Enter(ref localToken);
            Monitor.Enter(m_LockCachedDictionary);
            {
                if (m_CachedDictionary == newDict) {
                    m_KeyArray = keyArray;
                    m_ValueArray = valueArray;
                    m_KeyValuePairArray = keyValuePairArray;
                }
            }
            //m_LockCachedDictionary.Exit();
            Monitor.Exit(m_LockCachedDictionary);
            return valueArray;
        }

        /// <summary>
        /// 
        /// </summary>
        private KeyT[] InternalToArrayKeys() {
            Dictionary<KeyT, ValueT> newDict = null;
            Dictionary<KeyT, ValueT> oldDict = null;

            do {
                if (m_CachedDictionary == m_Dictionary)
                    return m_KeyArray;

                oldDict = m_CachedDictionary;
                newDict = m_Dictionary;

            } while (Interlocked.CompareExchange<Dictionary<KeyT, ValueT>>(ref m_CachedDictionary, newDict, oldDict) != oldDict);


            KeyT[] keyArray = new KeyT[newDict.Count];
            ValueT[] valueArray = new ValueT[newDict.Count];
            KeyValuePair<KeyT, ValueT>[] keyValuePairArray = new KeyValuePair<KeyT, ValueT>[newDict.Count];

            int iIndex = 0;
            foreach (KeyValuePair<KeyT, ValueT> keyValuePair in newDict) {
                keyArray[iIndex] = keyValuePair.Key;
                valueArray[iIndex] = keyValuePair.Value;
                keyValuePairArray[iIndex] = keyValuePair;

                ++iIndex;
            }

            // �������ǻ�ȡ���µ�����
            //bool localToken = false;
            //m_LockCachedDictionary.Enter(ref localToken);
            Monitor.Enter(m_LockCachedDictionary);
            {
                if (m_CachedDictionary == newDict) {
                    m_KeyArray = keyArray;
                    m_ValueArray = valueArray;
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
        //[MultiThreadedWarning( "zh-CHS", "��ǰ���������б���ʱ������,���ܱ������������Ժ����:����!" )]
        public KeyValuePair<KeyT, ValueT>[] ToArray() {
            return InternalToArray();
        }

        /// <summary>
        /// �ٶȱ�GetEnumerator(...)��
        /// </summary>
        /// <returns></returns>
        //[MultiThreadedWarning( "zh-CHS", "��ǰ���������б���ʱ������,���ܱ������������Ժ����:����!" )]
        public ValueT[] ToArrayValues() {
            return InternalToArrayValues();
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
        public KeyValuePair<KeyT, ValueT>[] ToArrayAndClear() {
            Dictionary<KeyT, ValueT> newDict = null;
            Dictionary<KeyT, ValueT> oldDict = null;

            do {
                oldDict = m_Dictionary;

                newDict = new Dictionary<KeyT, ValueT>();

            } while (Interlocked.CompareExchange<Dictionary<KeyT, ValueT>>(ref m_Dictionary, newDict, oldDict) != oldDict);

            // ��ʼ��ȡ����
            KeyValuePair<KeyT, ValueT>[] tempKeyValuePairArray = new KeyValuePair<KeyT, ValueT>[newDict.Count];

            int iIndex = 0;
            foreach (KeyValuePair<KeyT, ValueT> keyValuePair in oldDict) {
                tempKeyValuePairArray[iIndex] = keyValuePair;

                ++iIndex;
            }

            return tempKeyValuePairArray;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ValueT[] ToArrayValuesAndClear() {
            Dictionary<KeyT, ValueT> newDict = null;
            Dictionary<KeyT, ValueT> oldDict = null;

            do {
                oldDict = m_Dictionary;

                newDict = new Dictionary<KeyT, ValueT>();

            } while (Interlocked.CompareExchange<Dictionary<KeyT, ValueT>>(ref m_Dictionary, newDict, oldDict) != oldDict);

            // ��ʼ��ȡ����
            ValueT[] tempValueArray = new ValueT[newDict.Count];

            int iIndex = 0;
            foreach (KeyValuePair<KeyT, ValueT> keyValuePair in oldDict) {
                tempValueArray[iIndex] = keyValuePair.Value;

                ++iIndex;
            }

            return tempValueArray;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public KeyT[] ToArrayKeysAndClear() {
            Dictionary<KeyT, ValueT> newDict = null;
            Dictionary<KeyT, ValueT> oldDict = null;

            do {
                oldDict = m_Dictionary;

                newDict = new Dictionary<KeyT, ValueT>();

            } while (Interlocked.CompareExchange<Dictionary<KeyT, ValueT>>(ref m_Dictionary, newDict, oldDict) != oldDict);

            // ��ʼ��ȡ����
            KeyT[] tempKeyArray = new KeyT[newDict.Count];

            int iIndex = 0;
            foreach (KeyValuePair<KeyT, ValueT> keyValuePair in oldDict) {
                tempKeyArray[iIndex] = keyValuePair.Key;

                ++iIndex;
            }

            return tempKeyArray;
        }
        #endregion

        #region zh-CHS �ӿ�ʵ�� | en Interface Implementation
        /// <summary>
        /// �ٶȱ�ToArray(...)��Ķ�
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<KeyT, ValueT>> GetEnumerator() {
            return m_Dictionary.GetEnumerator();
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
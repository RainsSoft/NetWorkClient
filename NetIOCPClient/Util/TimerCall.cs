using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using NetIOCPClient.Log;

namespace NetIOCPClient.Util
{
    /// <summary>
    /// 周期性的执行相关工作
    /// </summary>
    public static class TimerCallWorker
    {
        static readonly int INTERVAL = 5;
        private static LinkedList<TimerCallItem> m_worklist = new LinkedList<TimerCallItem>();
        private static System.Timers.Timer m_Timer;
        private static Thread m_TimerThread;
        private static Stopwatch m_StopWatch;
        private static readonly object locker = new object();
        private static long LastTick = 0L;
        static TimerCallWorker() {
            lock (locker) {
                //m_Timer = new System.Timers.Timer();
                //m_Timer.Interval = INTERVAL;
                //m_Timer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimer);
                //m_Timer.Enabled = true;
                //m_Timer.Start();
                m_StopWatch = Stopwatch.StartNew();
                m_TimerThread = new Thread(OnTimer);
                m_TimerThread.Start();
            }
        }

        /// <summary>
        /// 增加一个
        /// </summary>
        /// <param name="item"></param>
        public static void Add(TimerCallItem item) {
            lock (m_worklist) {
                item.watch.Start();
                m_worklist.AddLast(item);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="callType"></param>
        /// <param name="delay">延时</param>
        /// <param name="callback"></param>
        public static void Add(string name, TimerCallType callType, int delay, Action<TimerCallItem> callback) {
            TimerCallItem item = new TimerCallItem { Name = name, CallType = callType, Timestamp = DateTime.Now, Delay = delay, Callback = callback };
            Add(item);
        }

        private static void OnTimer() {//object sender, System.Timers.ElapsedEventArgs e) {
            while (true) {
                while (true) {
                    float dt = ((float)((m_StopWatch.ElapsedTicks - LastTick) * (double)1) / Stopwatch.Frequency);
                    if (dt * 1000 < INTERVAL) {
                        System.Threading.Thread.Sleep((int)(1000 * dt));
                    }
                    else {
                        LastTick = m_StopWatch.ElapsedTicks;
                        break;
                    }
                }
                LinkedList<TimerCallItem> tmp;
                List<LinkedListNode<TimerCallItem>> toRemove = new List<LinkedListNode<TimerCallItem>>();
                lock (m_worklist) {
                    tmp = new LinkedList<TimerCallItem>(m_worklist);
                }


                LinkedListNode<TimerCallItem> cur = tmp.First;
                while (cur != null) {
                    LinkedListNode<TimerCallItem> next = cur.Next;

                    if (cur.Value.watch.ElapsedMilliseconds >= cur.Value.Delay) {
                        if (cur.Value.Callback != null) {
                            try {
                                cur.Value.Callback(cur.Value);
                            }
                            catch (Exception ee) {
                                Logs.Error("TimerCall回调出错 name:" + cur.Value.Name + ee.ToString());
                            }
                        }
                        if (cur.Value.CallType == TimerCallType.After) {
                            //m_worklist.Remove(cur);
                            toRemove.Add(cur);
                        }
                        else {
                            cur.Value.watch.Reset();
                            cur.Value.watch.Start();
                        }
                    }
                    else {
                        //cur.Value.Current += INTERVAL;
                    }
                    cur = next;
                }

                lock (locker) {
                    //这里有个遍历
                    foreach (var v in toRemove) {
                        m_worklist.Remove(v.Value);
                    }
                }

            }
        }
    }
    public class TimerCallItem
    {
        public string Name {
            get;
            set;
        }
        public DateTime Timestamp {
            get;
            internal set;
        }

        public TimerCallType CallType {
            get;
            set;
        }
        public int Delay {
            get;
            set;
        }
        public Action<TimerCallItem> Callback {
            get;
            set;
        }

        internal int Current {
            get;
            set;
        }

        internal Stopwatch watch = new Stopwatch();

    }
    public enum TimerCallType
    {
        /// <summary>
        /// 每隔多久做一次
        /// </summary>
        Every = 0,
        /// <summary>
        /// 多久之后做。
        /// </summary>
        After = 1
    }
}

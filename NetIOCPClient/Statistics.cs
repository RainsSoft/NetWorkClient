

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using NetIOCPClient.Log;
using NetIOCPClient.Util;

namespace NetIOCPClient.Core
{
    /// <summary>
    /// system info
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Statistics<T> : Statistics where T : Statistics, new()
    {
        protected static T instance;

        public static Statistics<T> Instance {
            get {
                if (instance == null) {
                    instance = new T();
                }
                return instance as Statistics<T>;
            }
        }

        /// <summary>
        /// The Statistic-timer update interval in seconds
        /// </summary>
        public int StatsPostInterval {
            get {
                return s_interval;
            }
            set {
                if (value > 0) {
                    instance.Change(value * 1000);
                }
                else {
                    instance.Change(Timeout.Infinite);
                }
                s_interval = value;
            }
        }

    }
#if UNITY_WEBPLAYER
 
#endif
    /// <summary>
    /// system info
    /// </summary>
    public abstract class Statistics
    {
        protected static int s_interval = 30 * 60 * 1000;

        protected static bool inited;

        //protected static Logger Logs = LogManager.GetCurrentClassLogger();
        protected Timer m_statTimer;
        protected long m_lastBytesSent, m_lastBytesReceived;
        protected DateTime m_lastUpdate = DateTime.Now;

        public readonly PerformanceCounter CPUPerfCounter, MemPerfCounter;


        protected Statistics() {
            try {
                m_lastUpdate = DateTime.Now;

                m_lastBytesSent = 0;
                m_lastBytesReceived = 0;

                var thisProcess = Process.GetCurrentProcess();

                CPUPerfCounter = new PerformanceCounter("Process", "% Processor Time", thisProcess.ProcessName);
                MemPerfCounter = new PerformanceCounter("Process", "Private Bytes", thisProcess.ProcessName);

                m_statTimer = new Timer(OnTick);
            }
            catch (Exception e) {
                Logs.Warn("Could not initialize Performance Counters." + e.ToString());
            }
        }

        public void Change(int seconds) {
            s_interval = seconds;
            if (seconds > 0) {
                seconds *= 1000;
            }
            m_statTimer.Change(seconds, seconds);
        }

        private void OnTick(object state) {
            var list = GetFullStats();
            foreach (var line in list) {
                Logs.Info(line);
            }
        }

        public abstract long TotalBytesSent {
            get;
        }

        public abstract long TotalBytesReceived {
            get;
        }

        public List<string> GetFullStats() {
            var list = new List<string>();
            list.Add("----------------- Statistics ------------------");
            GetStats(list);
            list.Add("-----------------------------------------------");
            return list;
        }

        public virtual void GetStats(ICollection<string> statLines) {
            //GC.Collect(2, GCCollectionMode.Optimized);
            Process thisProcess = Process.GetCurrentProcess();

            //var processUptime =DateTime.Now - thisProcess.StartTime;
            TimeSpan processUptime = new TimeSpan(Environment.TickCount * 10000000);
            var totalBytesSent = TotalBytesSent;
            var totalBytesRcvd = TotalBytesReceived;

            var averageThroughputUp = totalBytesSent / processUptime.TotalSeconds;
            var averageThroughputDown = totalBytesRcvd / processUptime.TotalSeconds;

            double currentUploadSpeed, currentDownloadSpeed;

            var delta = (DateTime.Now - m_lastUpdate).TotalSeconds;
            m_lastUpdate = DateTime.Now;

            currentUploadSpeed = (totalBytesSent - m_lastBytesSent) / delta;
            currentDownloadSpeed = (totalBytesRcvd - m_lastBytesReceived) / delta;
            m_lastBytesSent = totalBytesSent;
            m_lastBytesReceived = totalBytesRcvd;


            var cpuUsage = CPUPerfCounter.NextValue();
            var memUsage = MemPerfCounter.NextValue();

            statLines.Add(string.Format("+ CPU Usage: {0:0.00}% <-> Memory Usage: {1}", cpuUsage, Utility.FormatBytes(memUsage)));
            statLines.Add(string.Format("+ Upload: Total {0} - Avg {1}/s - Current {2}/s",
                                        Utility.FormatBytes(totalBytesSent), Utility.FormatBytes(averageThroughputUp),
                                        Utility.FormatBytes(currentUploadSpeed)));
            statLines.Add(string.Format("+ Download: Total: {0} - Avg: {1}/s - Current {2}/s",
                                        Utility.FormatBytes(totalBytesRcvd), Utility.FormatBytes(averageThroughputDown),
                                        Utility.FormatBytes(currentDownloadSpeed)));

            var gcCounts = new int[GC.MaxGeneration + 1];
            for (var i = 0; i <= GC.MaxGeneration; i++) {
                gcCounts[i] = GC.CollectionCount(i);
            }
#if UNITY_WEBPLAYER
            statLines.Add(string.Format("+ cpu Count: {0} ", Environment.ProcessorCount));
#else
            statLines.Add(string.Format("+ Thread Count: {0} - GC Counts: {1}", thisProcess.Threads.Count, ToString(gcCounts, ", ")));
#endif
        }
        public static string ToString<T>(IEnumerable<T> collection, string conj) {
            string vals;
            if (collection != null) {
                vals = string.Join(conj, ToStringArrT(collection));
            }
            else
                vals = "(null)";

            return vals;
        }
        public static string[] ToStringArrT<T>(IEnumerable<T> collection) {
            return ToStringArrT(collection, null);
        }

        public static string[] ToStringArr(IEnumerable collection) {
            var strs = new List<string>();
            var colEnum = collection.GetEnumerator();
            while (colEnum.MoveNext()) {
                var cur = colEnum.Current;
                if (cur != null) {
                    strs.Add(cur.ToString());
                }
            }
            return strs.ToArray();
        }
        public static string[] ToStringArrT<T>(IEnumerable<T> collection, Func<T, object> converter) {

            List<string> strArr = new List<string>();
            var colEnum = collection.GetEnumerator();
            //var i = 0;
            while (colEnum.MoveNext()) {
                var cur = colEnum.Current;
                if (!Equals(cur, default(T))) {
                    strArr.Add((converter != null ? converter(cur) : cur).ToString());
                }
            }
            return strArr.ToArray();
        }
    }
}
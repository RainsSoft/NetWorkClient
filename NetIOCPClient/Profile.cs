using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NetIOCPClient.Log;


namespace NetIOCPClient
{
    /// <summary>
    /// 性能计时
    /// </summary>
    public class Profile
    {
        private Stopwatch m_Sw = new Stopwatch();
        private long m_Last;
        public void Start() {
            if (m_Sw.IsRunning) {
                return;
            }
            m_Sw.Start();
        }
        public long GetElapsedSinceLastCall() {
            long kk = m_Sw.ElapsedMilliseconds;
            long ret = kk - m_Last;
            m_Last = kk;
            return ret;
        }

        public long GetElapsed() {
            return m_Sw.ElapsedMilliseconds;
        }

        private long m_EveryLast = 0;
        public bool IsEvery(int ms) {
            long kk = m_Sw.ElapsedMilliseconds;
            if (kk - m_EveryLast > ms) {
                m_EveryLast = kk;
                return true;
            }
            else {
                return false;
            }
        }


        public static bool EnableProf = false;
        internal static Stopwatch m_ProfSW = Stopwatch.StartNew();
        public static ProfInstance StartProf() {
            if (EnableProf == false) {
                return ProfInstance.Empty;
            }
            return new ProfInstance(m_ProfSW.ElapsedTicks);
        }
    }
    /// <summary>
    /// 性能计时
    /// </summary>
    public struct ProfInstance
    {
        public static readonly ProfInstance Empty = new ProfInstance(-1);
        internal long StartTicks;
        internal ProfInstance(long start) {
            StartTicks = start;
        }
        /// <summary>
        /// 记录一次Profile
        /// </summary>
        /// <param name="name">文本</param>
        /// <param name="ms">大于ms才记录,单位毫秒</param>
        public float EndProf(string name, float ms) {
            if (Profile.EnableProf == false) {
                return -1f;
            }
            float f = ((Profile.m_ProfSW.ElapsedTicks - StartTicks) * 1000f / Stopwatch.Frequency);
            if (f > ms) {
                Logs.Warn(name + " " + f.ToString());
            }
            return f;
        }
    }
}

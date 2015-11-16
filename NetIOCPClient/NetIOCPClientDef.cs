using System;

namespace NetIOCPClient.Core
{
    /// <summary>
    /// Global constants for the Cell framework.
    /// </summary>
    public static class NetIOCPClientDef
    {
        /// <summary>
        /// File name for the Cell framework error file.
        /// </summary>
        public const string CORE_LOG_FNAME = "NetIOCPClient_Core.log";

        /// <summary>
        /// Internal version string.
        /// </summary>
        public const string SVER = "NetIOCPClient v1.0 ALPHA";

        /// <summary>
        /// Internal version number.
        /// </summary>
        public const float VER = 1.0f;

        /// <summary>
        /// Maximum size of a packet buffer segment
        /// 8kb
        /// </summary>
        public const int MAX_PBUF_SEGMENT_SIZE = 8192; //8kb

        /// <summary>
        /// Maximum size of a packet buffer segment.
        /// 512¸ö
        /// </summary>
        public const int PBUF_SEGMENT_COUNT = 512;
    }
}
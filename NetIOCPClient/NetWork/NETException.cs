using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetIOCPClient.NetWork
{
    /// <summary>
    /// Exception thrown in the Lidgren Network Library
    /// </summary>
    public sealed class NetException : Exception
    {
        /// <summary>
        /// NetException constructor
        /// </summary>
        public NetException()
            : base() {
        }

        /// <summary>
        /// NetException constructor
        /// </summary>
        public NetException(string message)
            : base(message) {
        }

        /// <summary>
        /// NetException constructor
        /// </summary>
        public NetException(string message, Exception inner)
            : base(message, inner) {
        }

        /// <summary>
        /// Throws an exception, in DEBUG only, if first parameter is false
        /// </summary>
        //[Conditional("DEBUG")]
        public static void Assert(bool isOk, string message) {
            if (!isOk)
                throw new NetException(message);
        }

        /// <summary>
        /// Throws an exception, in DEBUG only, if first parameter is false
        /// </summary>
        //[Conditional("DEBUG")]
        public static void Assert(bool isOk) {
            if (!isOk)
                throw new NetException();
        }
    }
}

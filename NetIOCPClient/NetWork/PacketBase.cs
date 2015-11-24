using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace NetIOCPClient.NetWork
{
    public interface IClearPacketContent:IDisposable {
        void ClearContent();
    }

    /// <summary>
    ///基础类
    /// </summary>
    public abstract class PacketBase : INETPacket, IClearPacketContent
    {

        /// <summary>
        /// 关联片段
        /// </summary>        
        public BufferSegment Buffer;

   
        #region

        /// <summary>
        /// packet id是不在DataLen长度之内的
        /// </summary>
        public virtual ushort PacketID { get; set; }

        /// <summary>
        /// 整个包的大小 通常是值等于DataLen+4
        /// </summary>
        public virtual int PacketBufLen {
            get;
            set;
        }
        /// <summary>
        /// 包内数据长度(不包括PacketID)
        /// </summary>
        public ushort DataLen { get; set; }
        /// <summary>
        /// int long 等基础数据大小端
        /// </summary>
        public virtual Endian Endian { get; set; }
        #endregion
        //读取数据
        /// <summary>
        /// Reverses the contents of an array
        /// </summary>
        /// <typeparam name="T">type of the array</typeparam>
        /// <param name="buffer">array of objects to reverse</param>
        protected static void Reverse<T>(T[] buffer) {
            Reverse(buffer, buffer.Length);
        }

        /// <summary>
        /// Reverses the contents of an array
        /// </summary>
        /// <typeparam name="T">type of the array</typeparam>
        /// <param name="buffer">array of objects to reverse</param>
        /// <param name="length">number of objects in the array</param>
        protected static void Reverse<T>(T[] buffer, int length) {
            for (int i = 0; i < length / 2; i++) {
                T temp = buffer[i];
                buffer[i] = buffer[length - i - 1];
                buffer[length - i - 1] = temp;
            }
        }


      

        /// <summary>
        ///数据加密，最快最简单的加密方式 请使用XOR加密方式
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="offset"></param>
        /// <param name="len"></param>
        public virtual void  Encode(byte[] buf, int offset, int len) {
            
        }
        /// <summary>
        /// 数据解密，最快最简单的加密方式 请使用XOR加密方式
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="offset"></param>
        /// <param name="len"></param>
        public virtual void Decode(byte[] buf, int offset, int len) {
            
        }
        public abstract void Dispose();
        public abstract void ClearContent();
    }

}

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
        /*
public	static 	ushort 	PKG_MARK    	=	0xAAEE;
	public	static 	int		HEADER_LENGTH	=	14;
	public	static	int		DATA_LEN_OFFSET	=	4;
	public	static	int		MSG_OFFSET		=	8;
	public	class SGTMsgHeader
	{
		public	ushort	wCheckSum;  //校验码		=(wDataLen ^ 0xBBCC) & 0x88AA;
		public	ushort	wMark;      //包头标示	=PKG_MARK
		public	ushort	wDataLen;   //数据包长度	=SGTMsgHeader+SGSMsgHeader+Data
		public	byte	byFlags;	//			=0
		public	byte	byOptLen;	//			=0
		public	ushort  wMsgID; 	// 协议ID
		public	uint	dwTransID; 	// 传输ID	=0
		public	SGTMsgHeader()
		{
			wCheckSum	=	0;
			wMark		=	PKG_MARK;
			wDataLen	=	0;
			byFlags		=	0;
			byOptLen	=	0;
			wMsgID		=	0;
			dwTransID	=	0;
		}
		public	void	Encode(CNetData data)
		{
			data.Add(wCheckSum);
			data.Add(wMark);
			data.Add(wDataLen);
			data.Add(byFlags);
			data.Add(byOptLen);
			data.Add(wMsgID);
			data.Add(dwTransID);
		}
		public	void	Decode(CNetData data)
		{
			data.Del(ref wCheckSum);
			data.Del(ref wMark);
			data.Del(ref wDataLen);
			data.Del(ref byFlags);
			data.Del(ref byOptLen);
			data.Del(ref wMsgID);
			data.Del(ref dwTransID);
		}
	}
	*/
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

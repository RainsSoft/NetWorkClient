using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace NetIOCPClient.NetWork
{

    public class PacketHelper
    {
        unsafe static byte[] GetBytes(byte* ptr, int count) {
            byte[] ret = new byte[count];

            for (int i = 0; i < count; i++) {
                ret[i] = ptr[i];
            }

            return ret;
        }
        unsafe public static byte[] GetBytes(bool value) {
            return GetBytes((byte*)&value, 1);
        }

        unsafe public static byte[] GetBytes(char value) {
            return GetBytes((byte*)&value, 2);
        }

        unsafe public static byte[] GetBytes(short value) {
            return GetBytes((byte*)&value, 2);
        }

        unsafe public static byte[] GetBytes(int value) {
            return GetBytes((byte*)&value, 4);
        }

        unsafe public static byte[] GetBytes(long value) {
            return GetBytes((byte*)&value, 8);
        }

        [CLSCompliant(false)]
        unsafe public static byte[] GetBytes(ushort value) {
            return GetBytes((byte*)&value, 2);
        }

        [CLSCompliant(false)]
        unsafe public static byte[] GetBytes(uint value) {
            return GetBytes((byte*)&value, 4);
        }

        [CLSCompliant(false)]
        unsafe public static byte[] GetBytes(ulong value) {
            return GetBytes((byte*)&value, 8);
        }

        unsafe public static byte[] GetBytes(float value) {
            return GetBytes((byte*)&value, 4);
        }
        unsafe public static byte[] GetBytes(double value) {
            return BitConverter.GetBytes(value);
            //if (SwappedWordsInDouble) {
            //    byte[] data = new byte[8];
            //    byte* p = (byte*)&value;
            //    data[0] = p[4];
            //    data[1] = p[5];
            //    data[2] = p[6];
            //    data[3] = p[7];
            //    data[4] = p[0];
            //    data[5] = p[1];
            //    data[6] = p[2];
            //    data[7] = p[3];
            //    return data;
            //}
            //else {
            //    return GetBytes((byte*)&value, 8);
            //}
        }
        public class Writer
        {
            public static void WriteInt32(System.IO.Stream stream, int value) {
                //m_Buffer[0] = (byte)(value >> 24);
                //m_Buffer[1] = (byte)(value >> 16);
                //m_Buffer[2] = (byte)(value >>  8);
                //m_Buffer[3] = (byte) value;

                //m_Stream.Write( m_Buffer, 0, 4 );
                stream.WriteByte((byte)(value));
                stream.WriteByte((byte)(value >> 8));
                stream.WriteByte((byte)(value >> 16));
                stream.WriteByte((byte)(value >> 24));
            }
            public unsafe static void WriteInt32(byte* buf, ref int offset, int value) {
                *((int*)(buf + offset)) = value;
                offset += 4;
                return;
            }
            unsafe static void _writeBytes(BufferSegment buf, int offset, byte* ptr, int count) {
                for (int i = 0; i < count; i++) {
                    buf[offset + i] = ptr[i];
                }
            }
            public unsafe static void WriteInt32(BufferSegment buf, ref int offset, int value) {
                _writeBytes(buf, offset, (byte*)&value, 4);
                offset += 4;
            }
            public static unsafe void WriteLong64(Stream stream, long value) {
                stream.Write(BitConverter.GetBytes(value), 0, 8);
            }
            public static unsafe void WriteLong64(byte* buf, ref int offset, long value) {

                *((long*)(buf + offset)) = value;
                offset += 8;
                return;

            }
            public static unsafe void WriteLong64(BufferSegment buf, ref int offset, long value) {

                _writeBytes(buf, offset, (byte*)&value, 8);
                offset += 8;
                return;

            }
            public static void WriteUShort(System.IO.Stream stream, ushort value) {
                stream.WriteByte((byte)(value));
                stream.WriteByte((byte)(value >> 8));
            }
            public static unsafe void WriteUShort(byte* buf, ref int offset, ushort value) {
                *(buf + offset) = (byte)(value);
                offset++;
                *(buf + offset) = (byte)(value >> 8);
                offset++;
            }
            public static unsafe void WriteUShort(BufferSegment buf, ref int offset, ushort value) {
                _writeBytes(buf, offset, (byte*)&value, 2);
                offset += 2;
            }
            public unsafe static void WriteFloat(System.IO.Stream stream, float f) {
                int v = (*((int*)&f));

                byte a = ((byte)(v));
                byte b = ((byte)(v >> 8));
                byte c = ((byte)(v >> 16));
                byte d = ((byte)(v >> 24));

                stream.WriteByte(a);
                stream.WriteByte(b);
                stream.WriteByte(c);
                stream.WriteByte(d);

            }
            public unsafe static void WriteBool(byte* buf, ref int offset, bool value) {
                *(buf + offset) = (value ? (byte)1 : (byte)0);
                offset++;
            }
            public unsafe static void WriteBool(BufferSegment buf, ref int offset, bool value) {
                byte b = value ? (byte)1 : (byte)0;
                _writeBytes(buf, offset, (byte*)&b, 1);
                offset++;
            }


            public unsafe static void WriteByte(byte* buf, ref int offset, byte value) {
                *(buf + offset) = value;
                offset++;
            }
            public unsafe static void WriteByte(BufferSegment buf, ref int offset, byte value) {
                _writeBytes(buf, offset, (byte*)&value, 1);
                offset++;
            }
            public unsafe static void WriteBytes(byte* buf, ref int offset, byte[] value, int size) {
                if (value == null) {
                    throw new ArgumentNullException("value");
                }
                Marshal.Copy(value, 0, (IntPtr)(void*)(buf + offset), size);
                offset += size;
            }
            public unsafe static void WriteBytes(BufferSegment buf, ref int offset, byte[] value, int size) {
                if (value == null) {
                    throw new ArgumentNullException("value");
                }
                if (buf.Offset + size > buf.Length) {
                    throw new ArgumentOutOfRangeException("value is out len");
                }
                //Marshal.Copy(value, 0, (IntPtr)(void*)(buf + offset), size);
                buf.CopyStartFromBytes(offset, value, 0, size);
                offset += size;
            }
            public unsafe static void WriteBytes(byte* buf, ref int offset, byte[] value, int dataoffset, int size) {
                if (value == null) {
                    throw new ArgumentNullException("value");
                }
                Marshal.Copy(value, dataoffset, (IntPtr)(void*)(buf + offset), size);
                offset += size;
            }
            public unsafe static void WriteBytes(BufferSegment buf, ref int offset, byte[] value, int dataoffset, int size) {
                if (value == null) {
                    throw new ArgumentNullException("value");
                }
                //Marshal.Copy(value, dataoffset, (IntPtr)(void*)(buf + offset), size);
                buf.CopyStartFromBytes(offset, value, dataoffset, size);
                offset += size;
            }
            public unsafe static void WriteBytes(System.IO.Stream stream, byte[] buf) {
                stream.Write(buf, 0, buf.Length);
            }
            public unsafe static void WriteBytes2(byte* buf, ref int offset, byte[] value, int dataoffset, int size) {
                if (value == null) {
                    throw new ArgumentNullException("value");
                }
                WriteUShort(buf, ref offset, (ushort)size);
                Marshal.Copy(value, dataoffset, (IntPtr)(void*)(buf + offset), size);
                offset += size;
            }
            public unsafe static void WriteBytes2(BufferSegment buf, ref int offset, byte[] value, int dataoffset, int size) {
                if (value == null) {
                    throw new ArgumentNullException("value");
                }
                WriteUShort(buf, ref offset, (ushort)size);
                //Marshal.Copy(value, dataoffset, (IntPtr)(void*)(buf + offset), size);
                buf.CopyStartFromBytes(offset, value, dataoffset, size);
                offset += size;
            }
            /// <summary>
            /// 一个字节长度写进去了
            /// </summary>
            /// <param name="buf"></param>
            /// <param name="offset"></param>
            /// <param name="value"></param>
            public unsafe static void WriteBytes1(byte* buf, ref int offset, byte[] value) {
                byte len = (byte)value.Length;
                WriteByte(buf, ref offset, len);
                Marshal.Copy(value, 0, (IntPtr)(void*)(buf + offset), len);
                offset += len;
            }
            /// <summary>
            /// 一个字节长度写进去了
            /// </summary>
            /// <param name="buf"></param>
            /// <param name="offset"></param>
            /// <param name="value"></param>
            public unsafe static void WriteBytes1(BufferSegment buf, ref int offset, byte[] value) {
                byte len = (byte)value.Length;
                WriteByte(buf, ref offset, len);
                //Marshal.Copy(value, 0, (IntPtr)(void*)(buf + offset), len);
                buf.CopyStartFromBytes(offset, value, 0, value.Length);
                offset += len;
            }
            /// <summary>
            /// 2个字节长度 写进去了
            /// </summary>
            /// <param name="buf"></param>
            /// <param name="offset"></param>
            /// <param name="value"></param>
            public unsafe static void WriteBytes2(byte* buf, ref int offset, byte[] value) {
                ushort len = (ushort)value.Length;
                WriteUShort(buf, ref offset, len);
                Marshal.Copy(value, 0, (IntPtr)(void*)(buf + offset), len);
                offset += len;
            }
            public unsafe static void WriteBytes2(BufferSegment buf, ref int offset, byte[] value) {
                ushort len = (ushort)value.Length;
                WriteUShort(buf, ref offset, len);
                //Marshal.Copy(value, 0, (IntPtr)(void*)(buf + offset), len);
                buf.CopyStartFromBytes(offset, value, 0, value.Length);
                offset += len;
            }

            /// <summary>
            /// 4个字节长度 写进去了
            /// </summary>
            /// <param name="buf"></param>
            /// <param name="offset"></param>
            /// <param name="value"></param>
            public unsafe static void WriteBytes4(byte* buf, ref int offset, byte[] value) {
                int len = value.Length;
                WriteInt32(buf, ref offset, len);
                //for (int i = 0; i < value.Length; i++) {
                //    WriteByte(buf, ref offset, value[i]);
                //}
                Marshal.Copy(value, 0, (IntPtr)(void*)(buf + offset), len);
                offset += len;
            }
            public unsafe static void WriteBytes4(BufferSegment buf, ref int offset, byte[] value) {
                int len = value.Length;
                WriteInt32(buf, ref offset, len);
                //for (int i = 0; i < value.Length; i++) {
                //    WriteByte(buf, ref offset, value[i]);
                //}
                //Marshal.Copy(value, 0, (IntPtr)(void*)(buf + offset), len);
                buf.CopyStartFromBytes(offset, value, 0, value.Length);
                offset += len;
            }

            /// <summary>
            /// 写字符串.最大64K
            /// </summary>
            /// <param name="buf"></param>
            /// <param name="offset"></param>
            /// <param name="value"></param>
            /// <param name="encoding"></param>
            public unsafe static void WriteString(byte* buf, ref int offset, string value, Encoding encoding) {
                if (string.IsNullOrEmpty(value)) {
                    throw new ArgumentNullException("value");

                    return;
                }
                int len = 0;
                fixed (char* str = value) {
                    len = encoding.GetBytes(str, value.Length, buf, 65536);
                }
                offset += len;
            }
            /// <summary>
            /// 适用于写小于255长度的字符串,内部不包含长度信息
            /// </summary>
            /// <param name="buf"></param>
            /// <param name="offset"></param>
            /// <param name="value"></param>
            /// <param name="encoding"></param>
            [Obsolete()]
            public unsafe static void WriteString(BufferSegment buf, ref int offset, string value, Encoding encoding) {
                if (string.IsNullOrEmpty(value)) {
                    throw new ArgumentNullException("value");

                    return;
                }
                //byte len = 0;
                byte[] buf2 = encoding.GetBytes(value);
                buf.CopyStartFromBytes(offset, buf2, 0, buf2.Length);
                offset += buf2.Length;
            }
            /// <summary>
            /// 把长度1 字节写进了
            /// </summary>
            /// <param name="buf"></param>
            /// <param name="offset"></param>
            /// <param name="value"></param>
            /// <param name="encoding"></param>
            public unsafe static void WriteString1(byte* buf, ref int offset, string value, Encoding encoding) {
                if (string.IsNullOrEmpty(value)) {
                    //throw new ArgumentNullException("value");
                    WriteByte(buf, ref offset, (byte)0);
                    return;
                }
                byte len = 0;
                int start = offset;
                offset += 1;
                fixed (char* str = value) {
                    len = (byte)encoding.GetBytes(str, value.Length, buf + offset, 256);
                }
                //ushort len = (ushort)strBuf.Length;
                WriteByte(buf, ref start, len);
                //Marshal.Copy(strBuf, 0, (IntPtr)(void*)(buf + offset), len);
                offset += len;

                //byte[] strBuf = encoding.GetBytes(value);
                //System.Diagnostics.Debug.Assert(strBuf.Length < 256);
                //byte len = (byte)strBuf.Length;
                //WriteByte(buf, ref offset, len);
                //Marshal.Copy(strBuf, 0, (IntPtr)(void*)(buf + offset), len);
                //offset += len;

                //fixed (byte* ptr = strBuf) {
                //    byte* temp = ptr;
                //    for (int i = 0; i < len; i++) {
                //        *(buf + offset) = *temp;
                //        offset++;
                //        temp++;
                //    }

                //}
            }
            /// <summary>
            ///  把长度1 字节写进了,字符串长度不能超255
            /// </summary>
            /// <param name="buf"></param>
            /// <param name="offset"></param>
            /// <param name="value"></param>
            /// <param name="encoding"></param>
            public unsafe static void WriteString1(BufferSegment buf, ref int offset, string value, Encoding encoding) {
                if (string.IsNullOrEmpty(value)) {
                    //throw new ArgumentNullException("value");
                    WriteByte(buf, ref offset, (byte)0);
                    return;
                }
                byte len = 0;
                int start = offset;
                offset += 1;
                //fixed (char* str = value) {
                //    len = (byte)encoding.GetBytes(str, value.Length, buf + offset, 256);
                //}
                byte[] buf2 = encoding.GetBytes(value);
                len = (byte)buf2.Length;
                buf.CopyStartFromBytes(offset, buf2, 0, buf2.Length);
                //ushort len = (ushort)strBuf.Length;
                WriteByte(buf, ref start, len);
                //Marshal.Copy(strBuf, 0, (IntPtr)(void*)(buf + offset), len);
                offset += len;


            }

            /// <summary>
            /// 把长度2 字节写进了
            /// </summary>
            /// <param name="buf"></param>
            /// <param name="offset"></param>
            /// <param name="value"></param>
            /// <param name="encoding"></param>
            public unsafe static void WriteString2(byte* buf, ref int offset, string value, Encoding encoding) {
                if (string.IsNullOrEmpty(value)) {
                    //throw new ArgumentNullException("value");
                    WriteUShort(buf, ref offset, (ushort)0);
                    return;
                }

                ushort len = 0;
                int start = offset;
                offset += sizeof(ushort);
                fixed (char* str = value) {
                    len = (ushort)encoding.GetBytes(str, value.Length, buf + offset, 65535);
                }
                //ushort len = (ushort)strBuf.Length;
                WriteUShort(buf, ref start, len);
                //Marshal.Copy(strBuf, 0, (IntPtr)(void*)(buf + offset), len);
                offset += len;

            }
            public unsafe static void WriteString2(BufferSegment buf, ref int offset, string value, Encoding encoding) {
                if (string.IsNullOrEmpty(value)) {
                    //throw new ArgumentNullException("value");
                    WriteUShort(buf, ref offset, (ushort)0);
                    return;
                }

                ushort len = 0;
                int start = offset;
                offset += 2;
                //fixed (char* str = value) {
                //    len = (ushort)encoding.GetBytes(str, value.Length, buf + offset, 65535);
                //}
                byte[] buf2 = encoding.GetBytes(value);
                len = (ushort)buf2.Length;
                buf.CopyStartFromBytes(offset, buf2, 0, buf2.Length);
                //ushort len = (ushort)strBuf.Length;
                WriteUShort(buf, ref start, len);
                //Marshal.Copy(strBuf, 0, (IntPtr)(void*)(buf + offset), len);
                offset += len;

            }

            public unsafe static void WriteFloat(byte* buf, ref int offset, float f) {
                int v = (*((int*)&f));
                *((int*)(buf + offset)) = v;
                offset += 4;
                return;
            }
            public unsafe static void WriteFloat(BufferSegment buf, ref int offset, float f) {
                _writeBytes(buf, offset, (byte*)&f, 4);
                offset += 4;
                return;
            }

        }
        unsafe static void PutBytes(byte* dst, byte[] src, int start_index, int count) {
            if (src == null)
                throw new ArgumentNullException("value");

            if (start_index < 0 || (start_index > src.Length - 1))
                throw new ArgumentOutOfRangeException("startIndex", "Index was"
                    + " out of range. Must be non-negative and less than the"
                    + " size of the collection.");

            // avoid integer overflow (with large pos/neg start_index values)
            if (src.Length - count < start_index)
                throw new ArgumentException("Destination array is not long"
                    + " enough to copy all the items in the collection."
                    + " Check array index and length.");

            for (int i = 0; i < count; i++)
                dst[i] = src[i + start_index];
        }

        unsafe public static char ToChar(byte[] value, int startIndex) {
            char ret;

            PutBytes((byte*)&ret, value, startIndex, 2);

            return ret;
        }

        unsafe public static short ToInt16(byte[] value, int startIndex) {
            short ret;

            PutBytes((byte*)&ret, value, startIndex, 2);

            return ret;
        }

        unsafe public static int ToInt32(byte[] value, int startIndex) {
            int ret;

            PutBytes((byte*)&ret, value, startIndex, 4);

            return ret;
        }

        unsafe public static long ToInt64(byte[] value, int startIndex) {
            long ret;

            PutBytes((byte*)&ret, value, startIndex, 8);

            return ret;
        }

        [CLSCompliant(false)]
        unsafe public static ushort ToUInt16(byte[] value, int startIndex) {
            ushort ret;

            PutBytes((byte*)&ret, value, startIndex, 2);

            return ret;
        }

        [CLSCompliant(false)]
        unsafe public static uint ToUInt32(byte[] value, int startIndex) {
            uint ret;

            PutBytes((byte*)&ret, value, startIndex, 4);

            return ret;
        }

        [CLSCompliant(false)]
        unsafe public static ulong ToUInt64(byte[] value, int startIndex) {
            ulong ret;

            PutBytes((byte*)&ret, value, startIndex, 8);

            return ret;
        }

        unsafe public static float ToSingle(byte[] value, int startIndex) {
            float ret;

            PutBytes((byte*)&ret, value, startIndex, 4);

            return ret;
        }
        unsafe public static double ToDouble(byte[] value, int startIndex) {
            return BitConverter.ToDouble(value, startIndex);
        }
        public class Reader
        {
            unsafe static void _readBytes() {

            }
            public static string ReadString(byte[] buf, int len, ref int offset, Encoding encoding) {
                string s = encoding.GetString(buf, offset, len);
                offset += len;
                return s;
            }
            /// <summary>
            /// 从buf中读取字符串.注意,len参数不是字符串的字符个数,而是字符串经过encoding编码后的字节长度.
            /// </summary>
            /// <param name="buf"></param>
            /// <param name="offset"></param>
            /// <param name="len"></param>
            /// <param name="encoding"></param>
            /// <returns></returns>
            public static unsafe string ReadString(byte* buf, ref int offset, int len, Encoding encoding) {
                //byte[] temp = new byte[len];
                //Marshal.Copy((IntPtr)(void*)(buf + offset), temp, 0, len);
                //offset += len;

                //string s = encoding.GetString(temp);
                //return s;


                int length = encoding.GetCharCount(buf + offset, len);
                if (length == 0) {
                    return string.Empty;
                }
                string str = new string(' ', length);
                fixed (char* chRef = str) {
                    encoding.GetChars(buf + offset, len, chRef, length);
                }
                offset += len;
                return str;

            }
            /// <summary>
            /// 从buf中读取字符串.注意,len参数不是字符串的字符个数,而是字符串经过encoding编码后的字节长度.
            /// </summary>
            /// <param name="buf"></param>
            /// <param name="offset"></param>
            /// <param name="len"></param>
            /// <param name="encoding"></param>
            /// <returns></returns>
            [Obsolete()]
            public static unsafe string ReadString(BufferSegment buf, ref int offset, int len, Encoding encoding) {
                //              
                if (encoding.GetCharCount(buf.Buffer.Array, buf.Offset + offset, len) == 0) {
                    return string.Empty;
                }
                // string str = new string(' ', length);
                //fixed (char* chRef = str) {
                char[] chars = encoding.GetChars(buf.Buffer.Array, buf.Offset + offset, len);
                //}
                string str = new string(chars);
                offset += len;
                return str;

            }
            //public static string ReadString(Stream stream, int len, Encoding encoding) {
            //    byte[] buf = new byte[len];
            //    stream.Read(buf, 0, len);
            //    return encoding.GetString(buf);
            //}
            /// <summary>
            /// 把 1位 的长度也读进来了
            /// </summary>
            /// <param name="stream"></param>
            /// <param name="len"></param>
            /// <param name="encoding"></param>
            /// <returns></returns>
            public static unsafe string ReadString1(byte* buf, ref int offset, Encoding encoding) {
                int len = ReadByte(buf, ref offset);
                //byte[] temp = new byte[len];
                ////fixed (byte* bb = temp) {
                ////    byte* cc = bb;
                ////    for (int i = 0; i < len; i++) {
                ////        *cc = *(buf + offset);
                ////        cc++;
                ////        offset++;
                ////    }
                ////}
                //Marshal.Copy((IntPtr)(void*)(buf + offset), temp, 0, len);
                //offset += len;
                //string s = encoding.GetString(temp);
                //return s;

                if (len == 0) {
                    return string.Empty;
                }
                int length = encoding.GetCharCount(buf + offset, len);
                if (length == 0) {
                    return string.Empty;
                }
                string str = new string(' ', length);
                fixed (char* chRef = str) {
                    encoding.GetChars(buf + offset, len, chRef, length);
                }
                offset += len;
                return str;
            }
            public static unsafe string ReadString1(BufferSegment buf, ref int offset, Encoding encoding) {
                int len = ReadByte(buf, ref offset);
                if (len == 0) {
                    return string.Empty;
                }
                if (encoding.GetCharCount(buf.Buffer.Array, buf.Offset + offset, len) == 0) {
                    return string.Empty;
                }
                string str = new string(encoding.GetChars(buf.Buffer.Array, buf.Offset + offset, len));
                offset += len;
                return str;
            }
            /// <summary>
            /// 把 2位 的长度也读进来了
            /// </summary>
            /// <param name="stream"></param>
            /// <param name="len"></param>
            /// <param name="encoding"></param>
            /// <returns></returns>
            public static unsafe string ReadString2(byte* buf, ref int offset, Encoding encoding) {
                int len = ReadUShort(buf, ref offset);
                //byte[] temp = new byte[len];
                //Marshal.Copy((IntPtr)(void*)(buf + offset), temp, 0, len);
                //offset += len;
                //string s = encoding.GetString(temp);
                //return s;
                if (len == 0) {
                    return string.Empty;
                }
                int length = encoding.GetCharCount(buf + offset, len);
                if (length == 0) {
                    return string.Empty;
                }
                string str = new string(' ', length);
                fixed (char* chRef = str) {
                    encoding.GetChars(buf + offset, len, chRef, length);
                }
                offset += len;
                return str;
            }
            public static unsafe string ReadString2(BufferSegment buf, ref int offset, Encoding encoding) {
                int len = ReadUShort(buf, ref offset);

                if (len == 0) {
                    return string.Empty;
                }
                if (encoding.GetCharCount(buf.Buffer.Array, buf.Offset + offset, len) == 0) {
                    return string.Empty;
                }
                string str = new string(encoding.GetChars(buf.Buffer.Array, buf.Offset + offset, len));
                offset += len;
                return str;
            }
            //public static string ReadString(Network.PacketReader reader, Encoding encoding) {
            //    return reader.ReadString();
            //}
            public static unsafe byte ReadByte(byte* buf, ref int offset) {
                byte b = *(buf + offset);
                offset++;
                return b;
            }
            public static unsafe byte ReadByte(BufferSegment buf, ref int offset) {
                byte b = buf[offset];
                offset++;
                return b;
            }

            public static unsafe byte PeekByte(byte* buf, int offset) {
                byte b = *(buf + offset);
                return b;
            }
            public static unsafe byte PeekByte(BufferSegment buf, int offset) {
                byte b = buf[offset];
                return b;
            }

            public static unsafe bool ReadBool(byte* buf, ref int offset) {
                byte b = *(buf + offset);
                offset++;
                return (b == 1);
            }
            public static unsafe bool ReadBool(BufferSegment buf, ref int offset) {
                byte b = buf[offset];
                offset++;
                return (b == 1);
            }
            /// <summary>
            /// 读取byte[]buf 中 offset位置的byte值
            /// </summary>
            /// <param name="buf">整个数组</param>
            /// <param name="offset">偏移位置</param>
            /// <returns></returns>
            public static byte ReadByte(byte[] buf, ref int offset) {
                byte ret = buf[offset];
                offset++;
                return ret;
            }
            /// <summary>
            ///  读取byte[]buf 中 offset位置为起点长度为len的的byte[]值
            /// </summary>
            /// <param name="buf">整个数组</param>
            /// <param name="offset">偏移位置</param>
            /// <param name="len">长度</param>
            /// <returns></returns>
            public static byte[] ReadBytes(byte[] buf, ref int offset, int len) {
                byte[] ret = new byte[len];
                Buffer.BlockCopy(buf, 0, ret, 0, len);
                offset += len;
                return ret;
            }
            /// <summary>
            ///  读取byte[]buf 中 offset位置为起点长度为len的的byte[]值
            /// </summary>
            /// <param name="buf">整个数组</param>
            /// <param name="offset">偏移位置</param>
            /// <param name="len">长度</param>
            /// <returns></returns>
            public static unsafe byte[] ReadBytes(byte* buf, ref int offset, int len) {
                byte[] ret = new byte[len];
                Marshal.Copy((IntPtr)(void*)(buf + offset), ret, 0, len);
                offset += len;
                return ret;
            }
            public static unsafe byte[] ReadBytes(BufferSegment buf, ref int offset, int len) {
                byte[] ret = new byte[len];
                //Marshal.Copy((IntPtr)(void*)(buf + offset), ret, 0, len);
                buf.CopyToBytes(offset, ret, 0, len);
                offset += len;
                return ret;
            }
            /// <summary>
            /// 把1个字节长度也读进来了
            /// </summary>
            /// <param name="buf"></param>
            /// <param name="offset"></param>
            /// <param name="len"></param>
            /// <returns></returns>
            public static unsafe byte[] ReadBytes1(byte* buf, ref int offset) {
                int len = ReadByte(buf, ref offset);
                byte[] ret = new byte[len];
                if (len <= 8) {
                    for (int i = 0; i < len; i++) {
                        ret[i] = ReadByte(buf, ref offset);
                    }
                }
                else {
                    Marshal.Copy((IntPtr)(void*)(buf + offset), ret, 0, len);

                }
                offset += len;
                return ret;
            }
            public static unsafe byte[] ReadBytes1(BufferSegment buf, ref int offset) {
                int len = ReadByte(buf, ref offset);
                byte[] ret = new byte[len];
                buf.CopyToBytes(0, ret, 0, len);
                offset += len;
                return ret;
            }
            /// <summary>
            /// 把2个字节长度也读进来了
            /// </summary>
            /// <param name="buf"></param>
            /// <param name="offset"></param>
            /// <param name="len"></param>
            /// <returns></returns>
            public static unsafe byte[] ReadBytes2(byte* buf, ref int offset) {
                int len = ReadUShort(buf, ref offset);
                byte[] ret = new byte[len];
                Marshal.Copy((IntPtr)(void*)(buf + offset), ret, 0, len);
                offset += len;
                return ret;
            }
            public static unsafe byte[] ReadBytes2(BufferSegment buf, ref int offset) {
                int len = ReadUShort(buf, ref offset);
                byte[] ret = new byte[len];
                //Marshal.Copy((IntPtr)(void*)(buf + offset), ret, 0, len);
                buf.CopyToBytes(offset, ret, 0, len);
                offset += len;
                return ret;
            }
            /// <summary>
            /// 把4个字节长度也读进来了
            /// </summary>
            /// <param name="buf"></param>
            /// <param name="offset"></param>
            /// <param name="len"></param>
            /// <returns></returns>
            public static unsafe byte[] ReadBytes4(byte* buf, ref int offset) {
                int len = ReadInt32(buf, ref offset);
                byte[] ret = new byte[len];
                Marshal.Copy((IntPtr)(void*)(buf + offset), ret, 0, len);
                offset += len;
                return ret;
            }
            public static unsafe byte[] ReadBytes4(BufferSegment buf, ref int offset) {
                int len = ReadInt32(buf, ref offset);
                byte[] ret = new byte[len];
                // Marshal.Copy((IntPtr)(void*)(buf + offset), ret, 0, len);
                buf.CopyToBytes(offset, ret, 0, len);
                offset += len;
                return ret;
            }

            public static byte ReadByte(Stream stream) {
                return (byte)stream.ReadByte();
            }
            //public static byte ReadByte(Network.PacketReader reader) {
            //    return reader.ReadByte();
            //}
            public static unsafe ushort ReadUShort(byte* buf, ref int offset) {
                byte a = *(buf + offset);
                offset++;
                byte b = *(buf + offset);
                offset++;
                return (ushort)((ushort)((ushort)b << 8) + (ushort)a);
            }
            public static unsafe ushort ReadUShort(BufferSegment buf, ref int offset) {
                ushort ret = ToUInt16(buf.Buffer.Array, buf.Offset + offset);
                offset += 2;
                return ret;
            }
            public static unsafe ushort PeekUShort(byte* buf, int offset) {
                byte a = *(buf + offset);
                offset++;
                byte b = *(buf + offset);
                offset++;
                return (ushort)((ushort)((ushort)b << 8) + (ushort)a);
            }
            public static unsafe ushort PeekUShort(BufferSegment buf, int offset) {
                ushort ret = ToUInt16(buf.Buffer.Array, buf.Offset + offset);
                return ret;
            }
            public static ushort ReadUShort(System.IO.Stream stream) {
                int a = stream.ReadByte();
                int b = stream.ReadByte();

                return (ushort)((ushort)(b << 8) + (ushort)a);
            }
            public static ushort ReadUShort(byte[] buf, ref int offset) {
                byte a = buf[offset++];
                byte b = buf[offset++];

                return (ushort)((ushort)((ushort)b << 8) + (ushort)a);
            }
            //public static ushort ReadUShort(Network.PacketReader reader) {
            //    return reader.ReadUInt16();
            //}
            public unsafe static float ReadFloat(byte* buf, ref int offset) {
                //int a = (int)*(buf + offset);
                //offset++;
                //int b = (int)*(buf + offset);
                //offset++;
                //int c = (int)*(buf + offset);
                //offset++;
                //int d = (int)*(buf + offset);
                //offset++;

                //int ret = (a << 24) + (b << 16) + (c << 8) + d;

                int ret = *(((int*)(buf + offset)));
                offset += 4;
                return *(((float*)&ret));
            }
            public unsafe static float ReadFloat(BufferSegment buf, ref int offset) {

                float ret = ToSingle(buf.Buffer.Array, buf.Offset + offset);
                offset += 4;
                return ret;
            }
            public unsafe static float ReadFloat(System.IO.Stream stream) {
                int a = stream.ReadByte();
                int b = stream.ReadByte();
                int c = stream.ReadByte();
                int d = stream.ReadByte();

                int ret = (d << 24) + (c << 16) + (b << 8) + a;
                return *(((float*)&ret));
            }
            public unsafe static float ReadFloat(byte[] buf, ref int offset) {
                int a = (int)buf[offset++];
                int b = (int)buf[offset++];
                int c = (int)buf[offset++];
                int d = (int)buf[offset++];

                //offset += 4;

                int ret = (d << 24) + (c << 16) + (b << 8) + a;
                return *(((float*)&ret));
            }
            //public static float ReadFloat(Network.PacketReader reader) {
            //    return reader.ReadSingle();
            //}
            public unsafe static int ReadInt32(byte* buf, ref int offset) {

                int ret = *(((int*)(buf + offset)));
                offset += 4;
                return ret;
            }
            public static int ReadInt32(BufferSegment buf, ref int offset) {
                int ret = ToInt32(buf.Buffer.Array, buf.Offset + offset);
                offset += 4;
                return ret;
            }
            public unsafe static long ReadLong64(byte* buf, ref int offset) {

                //if (((buf+offset) % 8) == 0) {
                long ret = *(((long*)(buf + offset)));
                offset += 8;
                return ret;
            }
            public unsafe static long ReadLong64(BufferSegment buf, ref int offset) {

                //if (((buf+offset) % 8) == 0) {
                long ret = ToInt64(buf.Buffer.Array, buf.Offset + offset);// *(((long*)(buf + offset)));
                offset += 8;
                return ret;
            }
            public static int ReadInt32(System.IO.Stream stream) {
                int a = stream.ReadByte();
                int b = stream.ReadByte();
                int c = stream.ReadByte();
                int d = stream.ReadByte();

                return (d << 24) + (c << 16) + (b << 8) + a;
            }
            public static int ReadInt32(byte[] buf, ref int offset) {
                int a = (int)buf[offset++];
                int b = (int)buf[offset++];
                int c = (int)buf[offset++];
                int d = (int)buf[offset++];

                return (d << 24) + (c << 16) + (b << 8) + a;
            }

            //public static int ReadInt32(Network.PacketReader reader) {
            //    return reader.ReadInt32();
            //}
        }
    }

}
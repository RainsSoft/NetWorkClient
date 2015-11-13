

#region zh-CHS 包含名字空间 | en Include namespace
using System.IO;
using System.Text;
using System;
using System.Collections;
#endregion

namespace NetIOCPClient.Util
{
    /// <summary>
    /// 
    /// </summary>
    public static class Utility
    {

        /// <summary>
        /// Dumps the array to string form, using hexadecimal as the formatter
        /// </summary>
        /// <returns>hexadecimal representation of the data parsed</returns>
        public static string ToHex(ushort packetId, byte[] arr, int start, int count) {
            var hexDump = new StringBuilder();

            hexDump.Append('\n');
            hexDump.Append("{SERVER} " + string.Format("Packet: ({0}) {1} PacketSize = {2}\n" +
                                                       "|------------------------------------------------|----------------|\n" +
                                                       "|00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F |0123456789ABCDEF|\n" +
                                                       "|------------------------------------------------|----------------|\n",
                                                       "0x" + (packetId).ToString("X4"), packetId, count));

            int end = start + count;
            for (int i = start; i < end; i += 16) {
                var text = new StringBuilder();
                var hex = new StringBuilder();
                hex.Append("|");

                for (int j = 0; j < 16; j++) {
                    if (j + i < end) {
                        byte val = arr[j + i];
                        hex.Append(arr[j + i].ToString("X2"));
                        hex.Append(" ");
                        if (val >= 32 && val <= 127) {
                            text.Append((char)val);
                        }
                        else {
                            text.Append(".");
                        }
                    }
                    else {
                        hex.Append("   ");
                        text.Append(" ");
                    }
                }
                hex.Append("|");
                hex.Append(text + "|");
                hex.Append('\n');
                hexDump.Append(hex.ToString());
            }

            hexDump.Append("-------------------------------------------------------------------");

            return hexDump.ToString();
        }

        public static string FormatBytes(float num) {
            string numPrefix = "B";

            if (num >= 1024f) {
                num /= 1024.0f; numPrefix = "kb";
            }
            if (num >= 1024f) {
                num /= 1024.0f; numPrefix = "MB";
            }
            if (num >= 1024f) {
                num /= 1024.0f; numPrefix = "GB";
            }

            return string.Format("{0,6:f}{1}", num, numPrefix);
        }

        public static string FormatBytes(double num) {
            string numPrefix = "B";

            if (num >= 1024.0) {
                num /= 1024.0; numPrefix = "kb";
            }
            if (num >= 1024.0) {
                num /= 1024.0; numPrefix = "MB";
            }
            if (num >= 1024.0) {
                num /= 1024.0; numPrefix = "GB";
            }

            return string.Format("{0,6:f}{1}", num, numPrefix);
        }
        #region zh-CHS Text Format方法 | en Public Static Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Output"></param>
        /// <param name="streamInput"></param>
        /// <param name="iLength"></param>
        public static void FormatBuffer( TextWriter Output, System.IO.Stream streamInput, long iLength )
        {
            Output.WriteLine( "     | -- -- -- -- -- -- -- --  -- -- -- -- -- -- -- -- | ---------------- |" );
            Output.WriteLine( "     | 00 01 02 03 04 05 06 07  08 09 0A 0B 0C 0D 0E 0F | 0123456789ABCDEF |" );
            Output.WriteLine( "     | -- -- -- -- -- -- -- --  -- -- -- -- -- -- -- -- | ---------------- |" );

            long iByteIndex = 0;
            long iWhole = iLength >> 4;
            long iRem = iLength & 0xF;

            for ( long iIndex = 0; iIndex < iWhole; ++iIndex, iByteIndex += 16 )
            {
                StringBuilder strBytes = new StringBuilder( 49 );
                StringBuilder strChars = new StringBuilder( 16 );

                for ( int iIndex2 = 0; iIndex2 < 16; ++iIndex2 )
                {
                    int iByte = streamInput.ReadByte();

                    strBytes.Append( iByte.ToString( "X2" ) );

                    if ( iIndex2 != 7 )
                        strBytes.Append( ' ' );
                    else
                        strBytes.Append( "  " );

                    if ( iByte >= 0x20 && iByte < 0x80 )
                        strChars.Append( (char)iByte );
                    else
                        strChars.Append( '.' );
                }

                Output.Write( iByteIndex.ToString( "X4" ) );
                Output.Write( "   " );
                Output.Write( strBytes.ToString() );
                Output.Write( "  " );
                Output.WriteLine( strChars.ToString() );
            }

            if ( iRem != 0 )
            {
                StringBuilder strBytes = new StringBuilder( 49 );
                StringBuilder strChars = new StringBuilder( (int)iRem );

                for ( long iIndex2 = 0; iIndex2 < 16; ++iIndex2 )
                {
                    if ( iIndex2 < iRem )
                    {
                        long iByte = streamInput.ReadByte();

                        strBytes.Append( iByte.ToString( "X2" ) );

                        if ( iIndex2 != 7 )
                            strBytes.Append( ' ' );
                        else
                            strBytes.Append( "  " );

                        if ( iByte >= 0x20 && iByte < 0x80 )
                            strChars.Append( (char)iByte );
                        else
                            strChars.Append( '.' );
                    }
                    else
                        strBytes.Append( "   " );
                }

                if ( iRem <= 7 )
                    strBytes.Append( ' ' );

                Output.Write( iByteIndex.ToString( "X4" ) );
                Output.Write( "   " );
                Output.Write( strBytes.ToString() );
                Output.Write( "  " );
                Output.WriteLine( strChars.ToString() );
            }
        }
        #endregion

        #region zh-CHS 获取枚举的最大最小值 方法 | en Public Static Methods
        /// <summary>
        /// 获取枚举的最大值
        /// </summary>
        /// <typeparam name="EnumTypeT"></typeparam>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public static EnumTypeT GetEmunMaxValues<EnumTypeT>( Type enumType ) where EnumTypeT : IComparable<EnumTypeT>
        {
            EnumTypeT maxValue = default( EnumTypeT );

            Array enumArray = Enum.GetValues( enumType );
            foreach ( var item in enumArray )
            {
                if ( maxValue.CompareTo( (EnumTypeT)item ) < 0 )
                    maxValue = (EnumTypeT)item;
            }

            return maxValue;
        }


        /// <summary>
        /// 获取枚举的最小值
        /// </summary>
        /// <typeparam name="EnumTypeT"></typeparam>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public static EnumTypeT GetEmunMinValues<EnumTypeT>( Type enumType ) where EnumTypeT : IComparable<EnumTypeT>
        {
            EnumTypeT minValue = default( EnumTypeT );

            Array enumArray = Enum.GetValues( enumType );
            foreach ( var item in enumArray )
            {
                if ( minValue.CompareTo( (EnumTypeT)item ) > 0 )
                    minValue = (EnumTypeT)item;
            }

            return minValue;
        }
        #endregion
    }

    /// <summary>
    /// 不敏感的字符串大小写比较的类
    /// </summary>
    public static class InsensitiveStringComparer
    {
        #region zh-CHS 静态属性 | en Static Properties
        #region zh-CHS 私有静态成员变量 | en Private Static Member Variables
        /// <summary>
        /// 初始化不敏感的字符串大小写比较的接口
        /// </summary>
        private static IComparer s_Comparer = CaseInsensitiveComparer.Default;
        #endregion
        /// <summary>
        /// 返回不敏感的字符串大小写比较的接口
        /// </summary>
        public static IComparer Comparer {
            get { return s_Comparer; }
        }
        #endregion

        #region zh-CHS 静态方法 | en Static Method
        /// <summary>
        /// 不敏感的字符串大小写比较
        /// </summary>
        public static int Compare(string strStringA, string strStringB) {
            return s_Comparer.Compare(strStringA, strStringB);
        }

        /// <summary>
        /// 不敏感的字符串比较是否相同
        /// </summary>
        public static bool Equals(string strStringA, string strStringB) {
            if (strStringA == null && strStringB == null)
                return true;
            else if (strStringA == null || strStringB == null || strStringA.Length != strStringB.Length)
                return false;

            return (s_Comparer.Compare(strStringA, strStringB) == 0);
        }

        /// <summary>
        /// 不敏感的字符串比较第一个字符串前几个字符是否和第二个字符串的完全相同
        /// </summary>
        public static bool StartsWith(string strStringA, string strStringB) {
            if (strStringA == null || strStringB == null || strStringA.Length < strStringB.Length)
                return false;

            return (s_Comparer.Compare(strStringA.Substring(0, strStringB.Length), strStringB) == 0);
        }

        /// <summary>
        /// 不敏感的字符串比较第一个字符串后几个字符是否和第二个字符串的完全相同
        /// </summary>
        public static bool EndsWith(string strStringA, string strStringB) {
            if (strStringA == null || strStringB == null || strStringA.Length < strStringB.Length)
                return false;

            return (s_Comparer.Compare(strStringA.Substring(strStringA.Length - strStringB.Length), strStringB) == 0);
        }

        /// <summary>
        /// 不敏感的字符串比较第一个字符串是否完全包容第二个字符串
        /// </summary>
        public static bool Contains(string strStringA, string strStringB) {
            if (strStringA == null || strStringB == null || strStringA.Length < strStringB.Length)
                return false;

            strStringA = strStringA.ToLower();
            strStringB = strStringB.ToLower();

            return (strStringA.IndexOf(strStringB) >= 0);
        }
        #endregion
    }
}



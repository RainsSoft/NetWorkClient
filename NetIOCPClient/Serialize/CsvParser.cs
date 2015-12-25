using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace NetIOCPClient.Serialize
{
    /// <summary>
    ///     Determines how empty lines are interpreted when reading CSV files.
    ///     These values do not affect empty lines that occur within quoted fields
    ///     or empty lines that appear at the end of the input file.
    /// </summary>
    /// <remarks>
    /// code from http://www.blackbeltcoder.com/Articles/files/reading-and-writing-csv-files-in-c
    /// </remarks>
    public enum EmptyLineBehavior
    {
        /// <summary>
        /// Empty lines are interpreted as a line with zero columns.
        /// </summary>
        NoColumns,
        /// <summary>
        /// Empty lines are interpreted as a line with a single empty column.
        /// </summary>
        EmptyColumn,
        /// <summary>
        /// Empty lines are skipped over as though they did not exist.
        /// </summary>
        Ignore,
        /// <summary>
        /// An empty line is interpreted as the end of the input file.
        /// </summary>
        EndOfFile,
    }

       /// <summary>
    /// Common base class for CSV reader and writer classes.
    /// </summary>
    public abstract class CsvFileCommon
    {
        /// <summary>
        /// These are special characters in CSV files. If a column contains any
        /// of these characters, the entire column is wrapped in double quotes.
        /// </summary>
        protected char[] SpecialChars = new char[] { ',', '"', '\r', '\n' };

        // Indexes into SpecialChars for characters with specific meaning
        private const int DelimiterIndex = 0;
        private const int QuoteIndex = 1;

        /// <summary>
        /// Gets/sets the character used for column delimiters.
        /// </summary>
        public char Delimiter {
            get { return SpecialChars[DelimiterIndex]; }
            set { SpecialChars[DelimiterIndex] = value; }
        }

        /// <summary>
        /// Gets/sets the character used for column quotes.
        /// </summary>
        public char Quote {
            get { return SpecialChars[QuoteIndex]; }
            set { SpecialChars[QuoteIndex] = value; }
        }
    }

    /// <summary>
    /// Class for reading from comma-separated-value (CSV) files
    /// </summary>
    public class CsvFileReader : CsvFileCommon, IDisposable
    {
        // Private members
        private StreamReader Reader;
        MemoryStream Reader_MemoryStream;
        private string CurrLine;
        private int CurrPos;
        private EmptyLineBehavior EmptyLineBehavior;

        ///// <summary>
        ///// Initializes a new instance of the CsvFileReader class for the
        ///// specified stream.
        ///// </summary>
        ///// <param name="stream">The stream to read from</param>
        ///// <param name="emptyLineBehavior">Determines how empty lines are handled</param>
        //public CsvFileReader(StreamReader reader,
        //                     EmptyLineBehavior emptyLineBehavior ) {
        //    Reader = reader;
        //    EmptyLineBehavior = emptyLineBehavior;
        //}

        /// <summary>
        /// Initializes a new instance of the CsvFileReader class for the
        /// specified stream.
        /// </summary>
        /// <param name="stream">The stream to read from</param>
        /// <param name="emptyLineBehavior">Determines how empty lines are handled</param>
        public CsvFileReader(byte[] stream,
                         EmptyLineBehavior emptyLineBehavior ) {
            Reader_MemoryStream = new MemoryStream(stream);
            Reader = CsvFileReaderByAutoDetectEncoding.OpenStream(Reader_MemoryStream, Encoding.UTF8, Encoding.Default); //new StreamReader(ms,true);
            EmptyLineBehavior = emptyLineBehavior;
        }

        ///// <summary>
        ///// Initializes a new instance of the CsvFileReader class for the
        ///// specified file path.
        ///// </summary>
        ///// <param name="path">The name of the CSV file to read from</param>
        ///// <param name="emptyLineBehavior">Determines how empty lines are handled</param>
        //public CsvFileReader(string path,
        //                     EmptyLineBehavior emptyLineBehavior ) {
        //    Reader = new StreamReader(path);
        //    EmptyLineBehavior = emptyLineBehavior;
        //}

        //public static List<List<string>> ReadAll(string path, Encoding encoding) {
        //    using (var sr = new StreamReader(path, encoding)) {
        //        var cfr = new CsvFileReader(sr, EmptyLineBehavior.NoColumns);
        //        List<List<string>> dataGrid = new List<List<string>>();
        //        if (cfr.ReadAll(dataGrid)) return dataGrid;
        //    }
        //    return null;
        //}

        public bool ReadAll(List<List<string>> dataGrid) {
            // Verify required argument
            if (dataGrid == null) {
                throw new ArgumentNullException("dataGrid");
            }

            List<string> row = new List<string>();
            while (this.ReadRow(row)) {
                dataGrid.Add(new List<string>(row));
            }

            return true;
        }

        /// <summary>
        /// Reads a row of columns from the current CSV file. Returns false if no
        /// more data could be read because the end of the file was reached.
        /// </summary>
        /// <param name="columns">Collection to hold the columns read</param>
        public bool ReadRow(List<string> columns) {
            // Verify required argument
            if (columns == null)
                throw new ArgumentNullException("columns");

        ReadNextLine:
            // Read next line from the file
            CurrLine = Reader.ReadLine();
            CurrPos = 0;
            // Test for end of file
            if (CurrLine == null)
                return false;
            // Test for empty line
            if (CurrLine.Length == 0) {
                switch (EmptyLineBehavior) {
                    case EmptyLineBehavior.NoColumns:
                        columns.Clear();
                        return true;
                    case EmptyLineBehavior.Ignore:
                        goto ReadNextLine;
                    case EmptyLineBehavior.EndOfFile:
                        return false;
                }
            }

            // Parse line
            string column;
            int numColumns = 0;
            while (true) {
                // Read next column
                if (CurrPos < CurrLine.Length && CurrLine[CurrPos] == Quote)
                    column = ReadQuotedColumn();
                else
                    column = ReadUnquotedColumn();
                // Add column to list
                if (numColumns < columns.Count)
                    columns[numColumns] = column;
                else
                    columns.Add(column);
                numColumns++;
                // Break if we reached the end of the line
                if (CurrLine == null || CurrPos == CurrLine.Length)
                    break;
                // Otherwise skip delimiter
                Debug.Assert(CurrLine[CurrPos] == Delimiter);
                CurrPos++;
            }
            // Remove any unused columns from collection
            if (numColumns < columns.Count)
                columns.RemoveRange(numColumns, columns.Count - numColumns);
            // Indicate success
            return true;
        }

        /// <summary>
        /// Reads a quoted column by reading from the current line until a
        /// closing quote is found or the end of the file is reached. On return,
        /// the current position points to the delimiter or the end of the last
        /// line in the file. Note: CurrLine may be set to null on return.
        /// </summary>
        private string ReadQuotedColumn() {
            // Skip opening quote character
            Debug.Assert(CurrPos < CurrLine.Length && CurrLine[CurrPos] == Quote);
            CurrPos++;

            // Parse column
            StringBuilder builder = new StringBuilder();
            while (true) {
                while (CurrPos == CurrLine.Length) {
                    // End of line so attempt to read the next line
                    CurrLine = Reader.ReadLine();
                    CurrPos = 0;
                    // Done if we reached the end of the file
                    if (CurrLine == null)
                        return builder.ToString();
                    // Otherwise, treat as a multi-line field
                    builder.Append(Environment.NewLine);
                }

                // Test for quote character
                if (CurrLine[CurrPos] == Quote) {
                    // If two quotes, skip first and treat second as literal
                    int nextPos = (CurrPos + 1);
                    if (nextPos < CurrLine.Length && CurrLine[nextPos] == Quote)
                        CurrPos++;
                    else
                        break;  // Single quote ends quoted sequence
                }
                // Add current character to the column
                builder.Append(CurrLine[CurrPos++]);
            }

            if (CurrPos < CurrLine.Length) {
                // Consume closing quote
                Debug.Assert(CurrLine[CurrPos] == Quote);
                CurrPos++;
                // Append any additional characters appearing before next delimiter
                builder.Append(ReadUnquotedColumn());
            }
            // Return column value
            return builder.ToString();
        }

        /// <summary>
        /// Reads an unquoted column by reading from the current line until a
        /// delimiter is found or the end of the line is reached. On return, the
        /// current position points to the delimiter or the end of the current
        /// line.
        /// </summary>
        private string ReadUnquotedColumn() {
            int startPos = CurrPos;
            CurrPos = CurrLine.IndexOf(Delimiter, CurrPos);
            if (CurrPos == -1)
                CurrPos = CurrLine.Length;
            if (CurrPos > startPos)
                return CurrLine.Substring(startPos, CurrPos - startPos);
            return String.Empty;
        }

        // Propagate Dispose to StreamReader
        public void Dispose() {
            Reader.Dispose();
            Reader_MemoryStream.Dispose();
        }
    }
    public static class CsvFileReaderByAutoDetectEncoding {

        /// <summary>
        /// 通过给定的文件流，判断文件的编码类型
        /// </summary>
        /// <param name="fs">文件流</param>
        /// <returns>文件的编码类型</returns>
        public static System.Text.Encoding GetType(MemoryStream fs) {
            //byte[] Unicode = new byte[] { 0xFF, 0xFE, 0x41 };
            //byte[] UnicodeBIG = new byte[] { 0xFE, 0xFF, 0x00 };
            //byte[] UTF8 = new byte[] { 0xEF, 0xBB, 0xBF }; //带BOM
            Encoding reVal = Encoding.Default;

            BinaryReader r = new BinaryReader(fs, System.Text.Encoding.Default);
            int i;
            int.TryParse(fs.Length.ToString(), out i);
            byte[] ss = r.ReadBytes(i);
            if (IsUTF8Bytes(ss) || (ss[0] == 0xEF && ss[1] == 0xBB && ss[2] == 0xBF)) {
                reVal = Encoding.UTF8;
            }
            else if (ss[0] == 0xFE && ss[1] == 0xFF && ss[2] == 0x00) {
                reVal = Encoding.BigEndianUnicode;
            }
            else if (ss[0] == 0xFF && ss[1] == 0xFE && ss[2] == 0x41) {
                reVal = Encoding.Unicode;
            }
            r.Close();
            return reVal;

        }

        /// <summary>
        /// 判断是否是不带 BOM 的 UTF8 格式
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static bool IsUTF8Bytes(byte[] data) {
            int charByteCounter = 1; //计算当前正分析的字符应还有的字节数
            byte curByte; //当前分析的字节.
            for (int i = 0; i < data.Length; i++) {
                curByte = data[i];
                if (charByteCounter == 1) {
                    if (curByte >= 0x80) {
                        //判断当前
                        while (((curByte <<= 1) & 0x80) != 0) {
                            charByteCounter++;
                        }
                        //标记位首位若为非0 则至少以2个1开始 如:110XXXXX...........1111110X
                        if (charByteCounter == 1 || charByteCounter > 6) {
                            return false;
                        }
                    }
                }
                else {
                    //若是UTF-8 此时第一位必须为1
                    if ((curByte & 0xC0) != 0x80) {
                        return false;
                    }
                    charByteCounter--;
                }
            }
            if (charByteCounter > 1) {
                throw new Exception("非预期的byte格式");
            }
            return true;
        }
        public static bool IsUnicode(Encoding encoding) {
            int codepage = encoding.CodePage;
            // return true if codepage is any UTF codepage
            return codepage == 65001 || codepage == 65000 || codepage == 1200 || codepage == 1201;
        }

        //public static string ReadFileContent(string fileName, ref Encoding encoding, Encoding defaultEncoding) {
        //    using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
        //        using (StreamReader reader = OpenStream(fs, encoding, defaultEncoding)) {
        //            encoding = reader.CurrentEncoding;
        //            return reader.ReadToEnd();
        //        }
        //    }
        //}

        public static StreamReader OpenStream(MemoryStream fs, Encoding suggestedEncoding, Encoding defaultEncoding) {
            if (fs.Length > 3) {
                // the autodetection of StreamReader is not capable of detecting the difference
                // between ISO-8859-1 and UTF-8 without BOM.
                int firstByte = fs.ReadByte();
                int secondByte = fs.ReadByte();
                switch ((firstByte << 8) | secondByte) {
                    case 0x0000: // either UTF-32 Big Endian or a binary file; use StreamReader
                    case 0xfffe: // Unicode BOM (UTF-16 LE or UTF-32 LE)
                    case 0xfeff: // UTF-16 BE BOM
                    case 0xefbb: // start of UTF-8 BOM
                        // StreamReader autodetection works
                        fs.Position = 0;
                        return new StreamReader(fs);
                    default:
                        return AutoDetect(fs, (byte)firstByte, (byte)secondByte, defaultEncoding);
                }
            }
            else {
                if (suggestedEncoding != null) {
                    return new StreamReader(fs, suggestedEncoding);
                }
                else {
                    return new StreamReader(fs);
                }
            }
        }

        static StreamReader AutoDetect(MemoryStream fs, byte firstByte, byte secondByte, Encoding defaultEncoding) {
            int max = (int)Math.Min(fs.Length, 500000); // look at max. 500 KB
            const int ASCII = 0;
            const int Error = 1;
            const int UTF8 = 2;
            const int UTF8Sequence = 3;
            int state = ASCII;
            int sequenceLength = 0;
            byte b;
            for (int i = 0; i < max; i++) {
                if (i == 0) {
                    b = firstByte;
                }
                else if (i == 1) {
                    b = secondByte;
                }
                else {
                    b = (byte)fs.ReadByte();
                }
                if (b < 0x80) {
                    // normal ASCII character
                    if (state == UTF8Sequence) {
                        state = Error;
                        break;
                    }
                }
                else if (b < 0xc0) {
                    // 10xxxxxx : continues UTF8 byte sequence
                    if (state == UTF8Sequence) {
                        --sequenceLength;
                        if (sequenceLength < 0) {
                            state = Error;
                            break;
                        }
                        else if (sequenceLength == 0) {
                            state = UTF8;
                        }
                    }
                    else {
                        state = Error;
                        break;
                    }
                }
                else if (b >= 0xc2 && b < 0xf5) {
                    // beginning of byte sequence
                    if (state == UTF8 || state == ASCII) {
                        state = UTF8Sequence;
                        if (b < 0xe0) {
                            sequenceLength = 1; // one more byte following
                        }
                        else if (b < 0xf0) {
                            sequenceLength = 2; // two more bytes following
                        }
                        else {
                            sequenceLength = 3; // three more bytes following
                        }
                    }
                    else {
                        state = Error;
                        break;
                    }
                }
                else {
                    // 0xc0, 0xc1, 0xf5 to 0xff are invalid in UTF-8 (see RFC 3629)
                    state = Error;
                    break;
                }
            }
            fs.Position = 0;
            switch (state) {
                case ASCII:
                case Error:
                    // when the file seems to be ASCII or non-UTF8,
                    // we read it using the user-specified encoding so it is saved again
                    // using that encoding.
                    if (IsUnicode(defaultEncoding)) {
                        // the file is not Unicode, so don't read it using Unicode even if the
                        // user has choosen Unicode as the default encoding.

                        // If we don't do this, SD will end up always adding a Byte Order Mark
                        // to ASCII files.
                        defaultEncoding = Encoding.Default; // use system encoding instead
                    }
                    return new StreamReader(fs, defaultEncoding);
                default:
                    return new StreamReader(fs);
            }
        }
    }
    /// <summary>
    /// Class for writing to comma-separated-value (CSV) files.
    /// </summary>
    public class CsvFileWriter : CsvFileCommon, IDisposable
    {
        // Private members
        private StreamWriter Writer;
        private string OneQuote = null;
        private string TwoQuotes = null;
        private string QuotedFormat = null;

        /// <summary>
        /// Initializes a new instance of the CsvFileWriter class for the
        /// specified stream.
        /// </summary>
        /// <param name="stream">The stream to write to</param>
        public CsvFileWriter(StreamWriter writer) {
            Writer = writer;
        }

        /// <summary>
        /// Initializes a new instance of the CsvFileWriter class for the
        /// specified stream.
        /// </summary>
        /// <param name="stream">The stream to write to</param>
        public CsvFileWriter(Stream stream) {
            Writer = new StreamWriter(stream);
        }

        /// <summary>
        /// Initializes a new instance of the CsvFileWriter class for the
        /// specified file path.
        /// </summary>
        /// <param name="path">The name of the CSV file to write to</param>
        public CsvFileWriter(string path) {
            Writer = new StreamWriter(path);
        }

        public static void WriteAll(List<List<string>> dataGrid, string path, Encoding encoding) {
            using (var sw = new StreamWriter(path, false, encoding)) {
                var cfw = new CsvFileWriter(sw);
                foreach (var row in dataGrid) {
                    cfw.WriteRow(row);
                }
            }
        }

        public void WriteAll(List<List<string>> dataGrid) {
            foreach (List<string> row in dataGrid) {
                this.WriteRow(row);
            }
        }

        /// <summary>
        /// Writes a row of columns to the current CSV file.
        /// </summary>
        /// <param name="columns">The list of columns to write</param>
        public void WriteRow(List<string> columns) {
            // Verify required argument
            if (columns == null)
                throw new ArgumentNullException("columns");

            // Ensure we're using current quote character
            if (OneQuote == null || OneQuote[0] != Quote) {
                OneQuote = String.Format("{0}", Quote);
                TwoQuotes = String.Format("{0}{0}", Quote);
                QuotedFormat = String.Format("{0}{{0}}{0}", Quote);
            }

            // Write each column
            for (int i = 0; i < columns.Count; i++) {
                // Add delimiter if this isn't the first column
                if (i > 0)
                    Writer.Write(Delimiter);
                // Write this column
                if (columns[i].IndexOfAny(SpecialChars) == -1)
                    Writer.Write(columns[i]);
                else
                    Writer.Write(QuotedFormat, columns[i].Replace(OneQuote, TwoQuotes));
            }
            Writer.Write("\r\n");
        }

        // Propagate Dispose to StreamWriter
        public void Dispose() {
            Writer.Dispose();
        }

        internal void Flush() {
            Writer.Flush();
        }
    }
}
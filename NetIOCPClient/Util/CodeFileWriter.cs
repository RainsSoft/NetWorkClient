using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NetIOCPClient.Log;


namespace NetIOCPClient.Util.Code
{
    public class CodeFileWriter : IDisposable
    {
        //protected static Logger Logs;// = LogManager.GetCurrentClassLogger();

        public const string PublicVisibility = "public";
        public const string ReadonlyTag = "readonly";
        public const string StaticTag = "static";
        public const string VoidType = "void";
        public const string Class = "class";
        public const string Enum = "enum";

        private IndentTextWriter writer;
        private string namespce;
        private string className;
        private string typeNamePrefix, typeNameSuffix;
        private int bracketCount;
        private string m_fileName;
        private bool raisedException;
        private string backup;

        public CodeFileWriter(string fileName, string namespce, string className, string typeNamePrefix, string typeNameSuffix,
                              params string[] usedNamespaces) {
            if (File.Exists(fileName)) {
                using (System.IO.StreamReader sr = new StreamReader(fileName)) {
                    backup = sr.ReadToEnd();
                }
            }

            try {
                writer = new IndentTextWriter(new StreamWriter(m_fileName = fileName));
                this.namespce = namespce;
                this.className = className;
                this.typeNamePrefix = typeNamePrefix;
                this.typeNameSuffix = typeNameSuffix;

                WriteHeader(usedNamespaces);
            }
            catch (Exception e) {
                OnException(e);
            }
        }

        public CodeFileWriter(string fileName, string namespce, string className,
                              string[] usedNamespaces)
            : this(fileName, namespce, className, "class", "", usedNamespaces) {
        }

        /// <summary>
        /// Whether an Exception was raised during writing of the file.
        /// </summary>
        public bool RaisedException {
            get { return raisedException; }
        }

        /// <summary>
        /// The content of the file before (or null if there was none).
        /// </summary>
        public string Backup {
            get { return backup; }
        }

        public string Name {
            get { return className; }
        }

        public string Namespace {
            get { return namespce; }
        }

        public int IndentLevel {
            get { return writer.IndentLevel; }
            set { writer.IndentLevel = value; }
        }

        public IndentTextWriter BaseWriter {
            get { return writer; }
        }

        /// <summary>
        /// Executes the given action. 
        /// If an Exception is raised, the Exception handler will be called and the file will be reverted.
        /// </summary>
        /// <param name="action"></param>
        public void ExecuteSafely(Action action) {
            try {
                action();
            }
            catch (Exception ex) {
                OnException(ex);
            }
        }

        public void Revert() {
            try {
                Dispose();
            }
            finally {
                if (backup != null) {
                    //File.WriteAllText(m_fileName, backup);
                    using (StreamWriter sw = new StreamWriter(m_fileName, false)) {
                        sw.Write(backup);
                    }
                }
                else {
                    // File didn't exist before - Lets remove it.
                    File.Delete(m_fileName);
                }
            }
        }

        private void WriteHeader(string[] usedNamespaces) {
            foreach (var usedNamespace in usedNamespaces) {
                WriteLine("using " + usedNamespace + ";");
            }

            WriteLine();
            WriteLine("///");
            WriteLine("/// This file was automatically created, using WCell's CodeFileWriter");
            WriteLine("/// Date: " + DateTime.Now.Date.ToShortDateString());
            WriteLine("///");
            WriteLine();
            WriteLine("namespace " + namespce);
            OpenBracket();

            if (string.IsNullOrEmpty(typeNamePrefix)) {
                throw new Exception("No modifiers");
            }
            WriteLine("public " + typeNamePrefix + " " + className + " " + typeNameSuffix);
            OpenBracket();
        }

        #region Write Methods

        /// <summary>
        /// Public Default CTor
        /// </summary>
        /// <param name="bodyWriter"></param>
        public void WriteCTor(Action bodyWriter) {
            WriteCTor(PublicVisibility, "", bodyWriter);
        }

        /// <summary>
        /// Static CTor
        /// </summary>
        /// <param name="bodyWriter"></param>
        public void WriteStaticCTor(Action bodyWriter) {
            WriteCTor(StaticTag, "", bodyWriter);
        }

        public void WriteCTor(string args, Action bodyWriter) {
            WriteCTor(PublicVisibility, args, bodyWriter);
        }

        public void WriteCTor(string visibility, string args, Action bodyWriter) {
            WriteMethod(visibility, className, null, args, bodyWriter);
        }

        public void WriteDefaultCTor(string visibility, Action bodyWriter) {
            WriteMethod(visibility, className, null, "", bodyWriter);
        }

        private void WriteMethodHeader(string visibility, string returnType, string name, string args) {
            if (!string.IsNullOrEmpty(visibility)) {
                visibility += " ";
            }
            if (!string.IsNullOrEmpty(name)) {
                name = " " + name;
            }

            WriteLine("{0}{1}{2}({3})", visibility, returnType, name, args);
        }

        public void WritePublicMethod(string returnType, string name, string args, Action bodyWriter) {
            WriteMethod(PublicVisibility, returnType, name, args, bodyWriter);
        }

        public void WriteStaticMethod(string name, Action bodyWriter) {
            WriteMethod(PublicVisibility + " " + StaticTag, VoidType, name, "", bodyWriter);
        }

        public void WriteMethod(string name, Action bodyWriter) {
            WriteMethod(PublicVisibility, VoidType, name, "", bodyWriter);
        }

        public void WriteMethod(string returnType, string name, Action bodyWriter) {
            WriteMethod(PublicVisibility, returnType, name, "", bodyWriter);
        }

        public void WriteMethod(string visibility, string returnType, string name, Action bodyWriter) {
            WriteMethod(visibility, returnType, name, "", bodyWriter);
        }

        public void WriteMethod(string visibility, string returnType, string name, string args, Action bodyWriter) {
            WriteMethodHeader(visibility, returnType, name, args);
            OpenBracket();
            try {
                bodyWriter();
            }
            catch (Exception ex) {
                OnException(ex);
            }
            CloseBracket();
        }

        #endregion

        #region Write Properties

        private void WritePropHeader(string visibility, string returnType, string name) {
            if (!string.IsNullOrEmpty(visibility)) {
                visibility += " ";
            }
            else if (!string.IsNullOrEmpty(name)) {
                name = " " + name;
            }
            Write("{0}{1}{2}", visibility, returnType, name);
        }

        public void WriteAnonymousProperty(string returnType, string name, bool get, bool set) {
            WriteAnonymousProperty(PublicVisibility, returnType, name, get, set);
        }

        public void WriteAnonymousProperty(string visibility, string returnType, string name, bool get, bool set) {
            WritePropHeader(visibility, returnType, name);
            WriteLine();

            OpenBracket();

            if (get) {
                WriteLine("get;");
            }
            if (set) {
                WriteLine("set;");
            }

            CloseBracket();
        }

        #endregion

        #region Write Fields

        public void WriteField(string returnType, string name, bool isReadOnly, bool isStatic) {
            WriteField(PublicVisibility, returnType, name, isReadOnly, isStatic);
        }

        public void WriteField(string visibility, string returnType, string name, bool isReadOnly, bool isStatic) {
            if (isReadOnly) {
                visibility = string.Format("{0} {1}", visibility, ReadonlyTag);//new[] {visibility, ReadonlyTag}.ToString(" ");
            }
            if (isStatic) {
                visibility = string.Format("{0} {1}", visibility, StaticTag);//new[] {visibility, StaticTag}.ToString(" ");
            }
            WritePropHeader(visibility, returnType, name);
            WriteLine(";");
        }

        #endregion

        #region Write Statements

        public void Call(string methodName) {
            Call(methodName, "");
        }

        public void Call(string methodName, string args) {
            WriteLine(methodName + "(" + args + ");");
        }

        public void Assign(string key, string value) {
            WriteLine(key + "=" + value + ";");
        }

        public void Assign(string type, string varName, string value) {
            WriteLine(type + " " + varName + " = " + value + ";");
        }

        public void Assign(string type, string varName, string[] args) {
            WriteIndent(type + " " + varName + " = ");
            New(type, args);
        }

        public void Inc(string fieldOrProp) {
            WriteLine(fieldOrProp + "++;");
        }

        public void Dec(string fieldOrProp) {
            WriteLine(fieldOrProp + "--;");
        }

        public void PublicArray(string type, string name) {
            WriteLine(PublicVisibility + " " + type + "[] " + name + ";");
        }

        public void Array(string type, string name) {
            WriteLine(type + "[] " + name + ";");
        }

        public void Array(string type, string name, int length) {
            Assign(type + "[]", name, "new " + type + "[" + length + "]");
        }

        public void NewArray(string type, string seperator, Action valueWriter) {
            WriteIndent("new " + type + "[]");
            OpenBracket();
            try {
                valueWriter();
            }
            catch (Exception ex) {
                OnException(ex);
            }
            CloseBracket(seperator);
        }

        public void NewArray<T>(string type, IEnumerable<T> values) {
            WriteIndent("new " + type + "[]");
            OpenBracket();
            WriteLine(values.ToString(",\n" + writer.Indent));
            CloseBracket();
        }

        public void Array<T>(string type, string name, IEnumerable<T> values) {
            WriteIndent(type + "[] " + name + " = ");
            NewArray(type, values);
        }

        public void Array(string type, string name, Action valueWriter) {
            Array(type, name, 1, ";", valueWriter);
        }

        public void Array(string type, string name, int dimensions, string seperator, Action valueWriter) {
            var brackets = "";
            while (dimensions-- > 0) {
                brackets += "[]";
            }
            WriteIndent(type + brackets + " " + name + " = new " + type + brackets + " ");
            OpenBracket();
            try {
                valueWriter();
            }
            catch (Exception ex) {
                OnException(ex);
            }
            CloseBracket(seperator);
        }

        public void Array<T>(string type, string name, string seperator, T[][] arr) {
            Array(type, name, 2, seperator, () => {
                foreach (var subArr in arr) {
                    NewArray(type, subArr);
                }
            });
        }

        public void NewKomma(string type) {
            WriteLine("new " + type + "(),");
        }

        public void NewKomma(string type, IEnumerable<string> args) {
            WriteLine("new " + type + "(" + args.ToString(", ") + "),");
        }

        public void NewInit<K, V>(string type, IEnumerable<KeyValuePair<K, V>> args, string seperator) {
            WriteIndent("new " + type + " ");
            OpenBracket();
            Write(args.ToString(writer.Indent, ",\n"));
            WriteLine();
            CloseBracket(seperator);
        }

        public void New(string type, IEnumerable<string> args) {
            WriteLine("new " + type + "(" + args.ToString(", ") + ");");
        }

        #endregion

        #region Write Decoration

        public void WriteCommentLine(object comment) {
            WriteLine("// " + comment);
        }

        public void WriteXmlCommentLine(object comment) {
            WriteLine("/// " + comment);
        }

        public void WriteXmlCommentLine(object comment, params object[] args) {
            WriteLine("/// " + string.Format(comment.ToString(), args));
        }

        public void WriteSummary(string summary) {
            WriteLine("/// <summary>");
            WriteLine("/// " + summary);
            WriteLine("/// </summary>");
        }

        public void StartSummary() {
            WriteLine("/// <summary>");
        }

        public void EndSummary() {
            WriteLine("/// </summary>");
        }

        public void WriteMap(string mapName) {
            WriteLine("#region " + mapName);
        }

        public void WriteEndMap() {
            WriteLine("#endregion");
        }

        #endregion

        public void OpenBracket() {
            WriteLine("{");
            writer.IndentLevel++;
            bracketCount++;
        }

        public void CloseBracket(bool semicolon) {
            writer.IndentLevel--;
            WriteLine("}" + (semicolon ? ";" : ""));
            WriteLine();
            bracketCount--;
        }

        public void CloseBracket(string end) {
            writer.IndentLevel--;
            WriteLine("}" + end);
            bracketCount--;
        }

        public void CloseBracket() {
            CloseBracket(false);
        }

        /// <summary>
        /// Closes all remaining brackets
        /// </summary>
        public void Finish() {
            while (bracketCount > 0) {
                CloseBracket();
            }
        }

        public void WriteLine(string content) {
            try {
                writer.WriteLine(content);
            }
            catch (Exception e) {
                OnException(e);
            }
        }

        public void WriteLine(string content, params object[] args) {
            try {
                writer.WriteLine(string.Format(content, args));
            }
            catch (Exception e) {
                OnException(e);
            }
        }

        public void WriteLine() {
            try {
                writer.WriteLine();
            }
            catch (Exception e) {
                OnException(e);
            }
        }

        public void Write(string text, params object[] args) {
            Write(string.Format(text, args));
        }

        public void Write(string text) {
            try {
                writer.Write(text);
            }
            catch (Exception e) {
                OnException(e);
            }
        }

        public void WriteIndent(string text) {
            try {
                writer.Write(writer.Indent);
                writer.Write(text);
            }
            catch (Exception e) {
                OnException(e);
            }
        }

        public void OnException(Exception ex) {
            if (!raisedException) {
                raisedException = true;
                try {
                    //Logs.ErrorException(ex, false, "Failed to write code-file \"{0}\" - Reverting...", m_fileName);
                    Logs.Error(string.Format("Failed to write code-file \"{0}\" - Reverting...", m_fileName));
                    Revert();
                }
                catch (Exception ex2) {
                    throw new Exception("Failed to revert code-file \"" + m_fileName + "\" after Exception was risen!", ex2);
                }
            }
        }

        public void Dispose() {
            try {
                Finish();
            }
            finally {
                writer.Close();
            }
        }
    }
    static class IEnumerableToString
    {
        /// <summary>
        /// Returns the string representation of an IEnumerable (all elements, joined by comma)
        /// </summary>
        /// <param name="conj">The conjunction to be used between each elements of the collection</param>
        public static string ToString<T>(this IEnumerable<T> collection, string conj) {
            string vals;
            if (collection != null) {
                vals = string.Join(conj, ToStringArrT(collection));
            }
            else
                vals = "(null)";

            return vals;
        }

        /// <summary>
        /// Returns the string representation of an IEnumerable (all elements, joined by comma)
        /// </summary>
        /// <param name="conj">The conjunction to be used between each elements of the collection</param>
        public static string ToString<T>(this IEnumerable<T> collection, string conj, Func<T, object> converter) {
            string vals;
            if (collection != null) {
                vals = string.Join(conj, ToStringArrT(collection, converter));
            }
            else
                vals = "(null)";

            return vals;
        }
        /// <summary>
        /// Returns the string representation of an IEnumerable (all elements, joined by comma)
        /// </summary>
        /// <param name="conj">The conjunction to be used between each elements of the collection</param>
        public static string ToString(this IEnumerable collection, string conj) {
            string vals;
            if (collection != null) {
                vals = string.Join(conj, ToStringArr(collection));
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
            List<string> strArr = new List<string>();//new string[collection.Count()];
            var colEnum = collection.GetEnumerator();
            var i = 0;
            while (colEnum.MoveNext()) {
                var cur = colEnum.Current;
                if (!Equals(cur, default(T))) {
                    string strArr_i_ = (converter != null ? converter(cur) : cur).ToString();
                    strArr.Add(strArr_i_);
                }
            }
            return strArr.ToArray();
        }

        public static string[] ToJoinedStringArr<T>(IEnumerable<T> col, int partCount, string conj) {
            var strs = ToStringArrT(col);

            var list = new List<string>();
            var current = new List<string>(partCount);
            for (int index = 0, i = 0; index < strs.Length; i++, index++) {
                current.Add(strs[index]);
                if (i == partCount) {
                    i = 0;
                    list.Add(string.Join(conj, current.ToArray()));
                    current.Clear();
                }
            }
            if (current.Count > 0)
                list.Add(string.Join(conj, current.ToArray()));

            return list.ToArray();
        }

        public static string ToString<K, V>(this IEnumerable<KeyValuePair<K, V>> args, string indent, string seperator) {
            string s = "";
            var i = 0;
            int args_Count = 0;
            foreach (var arg in args) {
                args_Count++;
            }
            foreach (var arg in args) {
                i++;
                s += indent + arg.Key + " = " + arg.Value;

                if (i < args_Count) {
                    s += seperator;
                }
            }
            return s;
        }
    }
}
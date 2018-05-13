using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Security.Cryptography;

namespace TamPub1 {
    //////////////////一些通用函数//////////////////

    /// <summary>与Delphi字串类似的从1开始的字串</summary>
    public class DelphiString {
        /// <summary>兼容Delphi的从1开始的copy</summary>
        /// <param name="Value">需要拷贝的字串</param>
        /// <param name="StartPos">从1开始的起始位置</param>
        /// <param name="Len">需要拷贝的长度</param>
        /// <returns>子字符串</returns>
        public static string copy(string Value, int StartPos, int Len) {
            if (StartPos <= 0) StartPos = 1;
            if (Len > (Value.Length - StartPos + 1)) Len = Value.Length - StartPos + 1;
            if (Len <= 0) return "";
            return Value.Substring(StartPos - 1, Len);
        }

        /// <summary>兼容Delphi的从1开始的搜索</summary>
        /// <param name="SubString">需要搜索的字串</param>
        /// <param name="Value">被搜索的字串</param>
        /// <param name="StartPos">起始搜索位置（从1开始）</param>
        /// <returns>大于等于1标识搜索到了，否则返回0</returns>
        public static int pos(string SubString, string Value, int StartPos) {
            return (Value.IndexOf(SubString, StartPos - 1) + 1);
        }
        /// <summary>获取字元</summary>
        /// <param name="destString">需要分析的字符串 </param>
        /// <param name="position">位置指针</param>
        /// <param name="breakValueList">结束标记字符列表</param>
        /// <returns>token</returns>
        public static string getToken(string destString, ref int position, string breakValueList) {
            int i;
            string result = "";
            for (i = position - 1; i < destString.Length; i++) {
                if (pos(destString[i] + "", breakValueList, 1) > 0) break;
                result = result + destString[i];
            }
            position = i + 1;
            return result;
        }
        public static string getBlank(string destString, ref int position) {
            int i;
            string result = "";
            for (i = position - 1; i < destString.Length; i++) {
                if (Convert.ToInt16(destString[i]) > 0x20) break;
                result += destString[i];
            }
            position = i + 1;
            return result;
        }
        public static string getBlankAndToken(string destString, ref int position, string breakValueList) {
            string result = getBlank(destString, ref position);
            result += getToken(destString, ref position, breakValueList);
            return result;
        }
        /// <summary>
        /// 将字串转换为一种可显示的形式，比如回车字符以$0D的形式显示
        /// </summary>
        /// <param name="value">需要显示的字符串</param>
        public static string displayString(string value) {
            string result = "";
            int i;
            for (i = 0; i < value.Length; i++) {
                if (Convert.ToInt32(value[i]) < 16) {
                    result += "$0" + Convert.ToString(Convert.ToInt32(value[i]), 16).ToUpper();
                } else if (Convert.ToInt32(value[i]) < 20) {
                    result += "$" + Convert.ToString(Convert.ToInt32(value[i]), 16).ToUpper();
                } else result += value[i];
            }
            return result;
        }
    }

    /// <summary>计时函数</summary>
    public class TimeFunc {
        private static System.DateTime TimerValue = new System.DateTime();
        public static void TimerStart() {
            TimerValue = System.DateTime.Now;
        }
        /// <summary>返回自TimerStart()开始后经过的毫秒数</summary>
        public static int TimerEnd() {
            return System.Convert.ToInt32(System.DateTime.Now.Subtract(TimerValue).TotalMilliseconds);
        }
        /// <summary> 将秒值转换为各种显示形式 </summary>
        /// <param name="seconds">秒值</param>
        /// <param name="displayType">0: x天x小时x分钟x秒, 10: x天xx:xx:xx</param>
        public static string displaySeconds(int seconds, int displayType) {
            string result = "";
            if (displayType == 0) {
                if (seconds / 3600 / 24 > 0) {
                    result += (seconds / 3600 / 24) + "天";
                }
                if (seconds / 3600 > 0) {
                    result += ((seconds % (3600 * 24)) / 3600) + "小时";
                }
                if (seconds / 60 > 0) {
                    result += ((seconds % (3600)) / 60) + "分钟";
                }
                result += (seconds % (60)) + "秒";
            } else { //if ==10
                if (seconds / 3600 / 24 > 0) {
                    result += (seconds / 3600 / 24) + "天";
                }
                result += ((seconds % (3600 * 24)) / 3600) + ":";
                result += ((seconds % (3600)) / 60) + ":";
                result += (seconds % (60)) + "";
            }
            return result;
        }

        /// <summary>在value日期的基础上增加一个值，返回加好的值</summary>
        /// <param name="value">需要运算的日期时间</param>
        /// <param name="datetimeType">相加值的类型1年,2月,3日,4时,5分,6秒</param>
        /// <param name="datetimeValue">相加的值</param>
        /// <returns>加好的值</returns>
        public static DateTime increase(DateTime value, int datetimeType, int datetimeValue) {
            if (datetimeType == 1) {
                return value.AddYears(datetimeValue);
            } else if (datetimeType == 2) {
                return value.AddMonths(datetimeValue);
            } else if (datetimeType == 3) {
                return value.AddDays(datetimeValue);
            } else if (datetimeType == 4) {
                return value.AddHours(datetimeValue);
            } else if (datetimeType == 5) {
                return value.AddMinutes(datetimeValue);
            } else if (datetimeType == 6) {
                return value.AddSeconds(datetimeValue);
            } else {
                return value;
            }
        }

        /// <summary>月周期计时触发器</summary>
        public class SingleTimeTrigger {
            public string alias = ""; //别名仅起到标识作用，本类中没有使用
            public int triggerRangeSec;  //触发区域（秒）
            public int delayType = 0;
            public int delayValue;
            /// <summary>开启计时器</summary>
            public void setDelay(int delayType, int delayValue) {
                this.delayType = delayType;
                this.delayValue = delayValue;
                DateTime dtTemp = DateTime.Now;
                triggerTime = increase(dtTemp, delayType, delayValue);
            }
            /// <summary>在前面开启过计时器的前提下，重新开启计时器</summary>
            public void resetDelay() {
                if ((delayType < 1) || (delayType > 6)) return;
                setDelay(delayType, delayValue);
            }
            private bool lastBeforeTrigger;
            private int triggerCount;

            public SingleTimeTrigger() {
                triggerRangeSec = 3600;
                triggerCount = 0;
                triggerTime = new DateTime();
            }
            private DateTime ftriggerTime = DateTime.Now;
            //触发时间,设置起效
            public DateTime triggerTime {
                get {
                    return ftriggerTime;
                }
                set {
                    ftriggerTime = value;
                    lastBeforeTrigger = DateTime.Now < value;
                    triggerCount = 0;
                }
            }
            public bool isTriggered {
                get {
                    if (triggerCount > 0) {
                        triggerCount = 0;
                        return true;
                    } else {
                        return false;
                    }
                }
            }
            public bool timefragment() {
                bool currentBeforeTrigger;
                DateTime tTemp;
                currentBeforeTrigger = DateTime.Now < triggerTime;
                if ((!currentBeforeTrigger) && (lastBeforeTrigger)) {
                    tTemp = triggerTime;
                    tTemp = increase(tTemp, 6, triggerRangeSec);
                    if (tTemp > DateTime.Now) {
                        triggerCount++;
                    }
                }
                lastBeforeTrigger = currentBeforeTrigger;
                return false;
            }
        }
    }

    /// <summary>杂项函数</summary>
    public class Etc {
        //数值函数
        public static Int32 uint32ToInt32(UInt32 value) {
            Int32 result;
            unchecked {
                result = (Int32)value;
            }
            return result;
        }
        public static Boolean IsInteger(string Value) {
            if (Value.Trim().Length != Value.Length) return false;
            try { int.Parse(Value); } catch (Exception e) { if (e == null) e = null; return false; }
            return true;
        }
        /// <summary>生成一个SQL规范的字符串</summary>
        public static string sqlEncode(string SQLString) {
            return SQLString.Replace("'", "''");
        }
        /// <summary>饱和加算法</summary>
        /// <param name="value1">被加数</param>
        /// <param name="value2">加数</param>
        /// <param name="fullValue">饱和值</param>
        /// <returns>饱和加结果</returns>
        public static int fullAdd(int value1, int value2, int fullValue) {
            if (value1 + value2 > fullValue) return fullValue;
            return value1 + value2;
        }
        /// <summary>判断字符串是否符合手机号的标准</summary>
        /// <param name="mobile">手机号</param>
        /// <returns>是否是手机号</returns>
        public static bool isMobile(string mobile) {
            if (!isAllNumber(mobile)) return false;
            if (mobile[0] != '1') return false;
            if (mobile.Length != 11) return false;
            return true;
        }
        /// <summary>判断字符串是否只包含数字</summary>
        /// <param name="value">需要进行判断的字符串</param>
        /// <returns>是否全部是数字</returns>
        public static bool isAllNumber(string value) {
            int i;
            if (value == "") return false;
            for (i = 0; i < value.Length; i++) {
                if (value[i] < '0') return false;
                if (value[i] > '9') return false;
            }
            return true;
        }
        /// <summary>插值计算器，做动画的人都知道</summary>
        public static double getInterpolation(string name, double pos) {
            int i;
            if (name == "linear") {
                return pos;
            } else if (name == "slowDown") {
                return Math.Sin(pos * Math.PI / 2);
            } else if (name == "accUp") {
                return 1 - Math.Cos(pos * Math.PI / 2);
            } else if (name == "accSlow") {
                return (Math.Sin((pos * 2 - 1) * Math.PI / 2) + 1) / 2;
            } else if (name == "bonus") {
                List<double> a = new List<double>();
                List<double> b = new List<double>();
                a.Add(-0.5); b.Add(1);
                for (i = 0; i < 10; i++) {
                    a.Add(a[i] + b[i]);
                    b.Add(b[i] / 3);
                }
                for (i = 0; i < 10; i++) {
                    if (pos < a[i + 1]) {
                        return getInterpolation_f(pos, a[i], b[i]);
                    }
                }
                return 1;
            } else {
                return pos;
            }
        }
        private static double getInterpolation_f(double x, double a, double b) {
            var result = (x - a - b / 2);
            result = result * result * 2 / b;
            return result + 1 - b;
        }
        /// <summary>获取unix的timestamp</summary>
        public static long getUnixTimestamp(DateTime value) {
            return (value.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
        }
        /// <summary>把unixTimestamp值转换为datetime类型</summary>
        public static DateTime getDatetimeByUnixTimestamp(long value) {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return startTime.AddSeconds(value);
        }
    }

    public class StringData {
        public string Buff;
        public int sp;
        public StringData(string StringBuff) {
            Buff = StringBuff;
            sp = 1;
        }
        public StringData() {
            Buff = "";
            sp = 1;
        }
        public bool eof() {
            return sp > Buff.Length;
        }
        public void AppendInteger(int Value) {
            UInt32 iTemp = (UInt32)Value;
            Buff += (char)(((iTemp & 0xFF) << 8) | ((iTemp & 0xFF00) >> 8));
            Buff += (char)(((iTemp & 0xFF0000) >> 8) | ((iTemp & 0xFF000000) >> 24));
        }
        public int ReadInteger() {
            UInt32 iTemp, Result = 0;
            if (sp >= Buff.Length) return (0);
            iTemp = (UInt32)(Buff[sp - 1]); sp++;
            Result = ((iTemp & 0xFF) << 8) | ((iTemp & 0xFF00) >> 8);
            iTemp = (UInt32)(Buff[sp - 1]); sp++;
            Result = Result + ((iTemp & 0xFF) << 24) | ((iTemp & 0xFF00) << 8);
            return ((Int32)Result);
        }
        public void AppendString(string Value) {
            AppendInteger(Value.Length);
            Buff += Value;
        }
        public string ReadString() {
            int Len = ReadInteger();
            string Result = DelphiString.copy(Buff, sp, Len);
            sp += Len;
            return (Result);
        }
    }

    /// <summary>
    /// 不同于StringData,本类将数据结构存储为可见的字符串
    /// 2015/01/19 做了改造：以前的模式不符合普遍的传输协议，一个16bit数据应该将低位放在最前面
    /// </summary>
    public class StringData2 {
        public string buff = "";
        public int sp = 0;
        public bool eof() {
            return sp >= buff.Length;
        }
        public void appendByte(byte value) {
            buff += value.ToString("X2");
        }
        public void appendInt16(Int16 value) {
            appendWord(Convert.ToUInt16(value));
        }
        public void appendWord(UInt16 value) {
            appendByte(Convert.ToByte(value & 0xff));
            appendByte(Convert.ToByte(value >> 8));
        }
        public void appendInt32(Int32 value) {
            appendDWord(Convert.ToUInt32(value));
        }
        public void appendDWord(UInt32 value) {
            appendWord(Convert.ToUInt16(value & 0xffff));
            appendWord(Convert.ToUInt16(value >> 16));
        }
        public byte readByte() {
            if (sp >= buff.Length - 1) {
                sp = buff.Length;
                return 0;
            }
            byte result;
            try {
                result = Convert.ToByte(buff.Substring(sp, 2), 16);
            } catch {
                result = 0;
            }
            sp += 2;
            return result;
        }
        public Int16 readInt16() {
            return Convert.ToInt16(readWord());
        }
        public UInt16 readWord() {
            UInt16 result = Convert.ToUInt16(readByte());
            result = Convert.ToUInt16(result | (Convert.ToUInt16(readByte()) << 8));
            return result;
        }
        public Int32 readInt32() {
            return Convert.ToInt32(readDWord());
        }
        public UInt32 readDWord() {
            UInt32 result = Convert.ToUInt32(readWord());
            result = result | Convert.ToUInt32(readWord() << 16);
            return result;
        }
        public string readString() {
            int len = Math.Min(readInt32(), buff.Length - sp);
            string Result = buff.Substring(sp, len);
            sp += len;
            return (Result);
        }
        public string readString8() {
            int len = Math.Min(readByte(), buff.Length - sp);
            string Result = buff.Substring(sp, len);
            sp += len;
            return (Result);
        }
        public string readString16() {
            int len = Math.Min(readWord(), buff.Length - sp);
            string Result = buff.Substring(sp, len);
            sp += len;
            return (Result);
        }
        public void appendString(string value) {
            appendInt32(value.Length);
            buff += value;
        }
        public void appendString8(string value) {
            value = value.Substring(0, Math.Min(0xff, value.Length));
            appendByte(Convert.ToByte(value.Length));
            buff += value;
        }
        public void appendString16(string value) {
            value = value.Substring(0, Math.Min(0xffff, value.Length));
            appendWord(Convert.ToUInt16(value.Length));
            buff += value;
        }
    }

    /// <summary>
    /// 20160428 针对大量append情况下缓慢做了优化，进出两条线
    /// </summary>
    public class StringData3 {
        public StringBuilder buffAppend = new StringBuilder();
        public string buffRead = "";
        public int sp = 0;
        public bool eof() {
            return sp >= buffRead.Length;
        }
        public void clear() {
            sp = 0;
            buffAppend.Clear();
            buffRead = "";
        }
        public void appendByte(byte value) {
            buffAppend.Append(value.ToString("X2"));
        }
        public void appendInt16(Int16 value) {
            appendWord(Convert.ToUInt16(value));
        }
        public void appendWord(UInt16 value) {
            appendByte(Convert.ToByte(value & 0xff));
            appendByte(Convert.ToByte(value >> 8));
        }
        public void appendInt32(Int32 value) {
            appendDWord(Convert.ToUInt32(value));
        }
        public void appendDWord(UInt32 value) {
            appendWord(Convert.ToUInt16(value & 0xffff));
            appendWord(Convert.ToUInt16(value >> 16));
        }
        public byte readByte() {
            if (sp >= buffRead.Length - 1) {
                sp = buffRead.Length;
                return 0;
            }
            byte result;
            try {
                result = Convert.ToByte(buffRead.Substring(sp, 2), 16);
            } catch {
                result = 0;
            }
            sp += 2;
            return result;
        }
        public Int16 readInt16() {
            return Convert.ToInt16(readWord());
        }
        public UInt16 readWord() {
            UInt16 result = Convert.ToUInt16(readByte());
            result = Convert.ToUInt16(result | (Convert.ToUInt16(readByte()) << 8));
            return result;
        }
        public Int32 readInt32() {
            return Convert.ToInt32(readDWord());
        }
        public UInt32 readDWord() {
            UInt32 result = Convert.ToUInt32(readWord());
            result = result | Convert.ToUInt32(readWord() << 16);
            return result;
        }
        public string readString() {
            int len = Math.Min(readInt32(), buffRead.Length - sp);
            string Result = buffRead.Substring(sp, len);
            sp += len;
            return (Result);
        }
        public string readString8() {
            int len = Math.Min(readByte(), buffRead.Length - sp);
            string Result = buffRead.Substring(sp, len);
            sp += len;
            return (Result);
        }
        public string readString16() {
            int len = Math.Min(readWord(), buffRead.Length - sp);
            string Result = buffRead.Substring(sp, len);
            sp += len;
            return (Result);
        }
        public void appendString(string value) {
            appendInt32(value.Length);
            buffAppend.Append(value);
        }
        public void appendString8(string value) {
            value = value.Substring(0, Math.Min(0xff, value.Length));
            appendByte(Convert.ToByte(value.Length));
            buffAppend.Append(value);
        }
        public void appendString16(string value) {
            value = value.Substring(0, Math.Min(0xffff, value.Length));
            appendWord(Convert.ToUInt16(value.Length));
            buffAppend.Append(value);
        }
    }

    public class FileOperation {
        private System.IO.BinaryReader reader;
        private System.IO.BinaryWriter writer;
        private System.IO.FileStream fs = null;
        public string textCoding = System.Text.Encoding.Default.WebName; //2017-11新增默认编码形式，配合writeFixStringEx等
        public string filename = "";
        /// <summary>打开一个文件用来读取</summary>
        public bool openRead(string filename) {
            close();
            this.filename = filename;
            try {
                fs = new System.IO.FileStream(filename, System.IO.FileMode.Open);
            } catch {
                fs = null;
                return false;
            }
            reader = new System.IO.BinaryReader(fs, System.Text.Encoding.GetEncoding(textCoding));
            return true;
        }
        /// <summary>打开一个文件用来读取,需要提前指定filename</summary>
        public bool openRead() {
            return openRead(filename);
        }
        /// <summary>打开一个文件从末尾添加，如果文件不存在则新建</summary>
        public bool openAppend(string filename) {
            close();
            this.filename = filename;
            try {
                fs = new System.IO.FileStream(filename, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write, System.IO.FileShare.ReadWrite);
                writer = new System.IO.BinaryWriter(fs);
                fs.Seek(0, System.IO.SeekOrigin.End);
            } catch {
                fs = null;
                return false;
            }
            return true;
        }
        /// <summary>打开一个文件从末尾添加，如果文件不存在则新建,需要提前指定filename</summary>
        public bool openAppend() {
            return openAppend(filename);
        }
        /// <summary>强制开启一个新文件进行写操作, 如果原文件存在则删除,需要提前指定filename</summary>
        public bool openNew(string filename) {
            close();
            this.filename = filename;
            try {
                System.IO.File.Delete(filename);
            } catch {
                return false;
            }
            return openAppend(filename);
        }
        public bool openNew() {
            return openNew(filename);
        }
        /// <summary>当前指针的位置</summary>
        public Int64 pos {
            get { return fs.Position; }
        }

        //增加写入bool方法        (cfq)
        public bool writeBool(bool value) {
            if (fs == null) return false;
            try {
                writer.Write(value);
            } catch {
                return false;
            }
            return true;
        }

        public bool writeInt32(Int32 value) {
            if (fs == null) return false;
            try {
                writer.Write(value);
            } catch {
                return false;
            }
            return true;
        }


        public bool writeUInt16(UInt16 value) {
            if (fs == null) return false;
            try {
                writer.Write(value);
            } catch {
                return false;
            }
            return true;
        }
        public bool writeWFixedString(string value) {
            if (fs == null) return false;
            char[] buff = value.ToCharArray();
            try {
                writer.Write(buff, 0, buff.Length);
            } catch {
                return false;
            }
            return true;
        }
        public bool writeFixedStringEx(string value) {
            if (fs == null) return false;
            byte[] buff = System.Text.Encoding.GetEncoding(textCoding).GetBytes(value);
            try {
                writer.Write(buff, 0, buff.Length);
            } catch {
                return false;
            }
            return true;
        }

        /// <summary>写入文件BOM头(Byte Order Mark)</summary>
        public bool writeEncodingHeader(string encodingName = "") {
            if (encodingName.Length <= 0) encodingName = textCoding;
            encodingName = encodingName.ToLower();
            if (encodingName.Equals("utf-8")) {
                writer.Write(0xEF);
                writer.Write(0xBB);
                writer.Write(0xBF);
            } else if (encodingName.Equals("utf-16be")) {
                writer.Write(0xFE);
                writer.Write(0xFF);
            } else if (encodingName.Equals("unicode")) {
                writer.Write(0xFF);
                writer.Write(0xFE);
            }
            return true;
        }

        public bool writeWString(string value) {
            if (fs == null) return false;
            try {
                if (!writeInt32(value.Length)) return false;
                writeWFixedString(value);
            } catch {
                return false;
            }
            return true;
        }

        //新增加读取bool方法    (cfq)
        public bool readBool() {
            if (fs == null) return false;
            try {
                return reader.ReadBoolean();
            } catch {
                return false;
            }
        }

        public int readInteger() {
            if (fs == null) return 0;
            try {
                return reader.ReadInt32();
            } catch {
                return 0;
            }
        }

        public UInt16 readUInt16() {
            if (fs == null) return 0;
            try {
                //return reader.ReadUInt16();     2010-4-22修改为ReadChar()
                return reader.ReadChar();
            } catch {
                return 0;
            }
        }
        public string readWFixedString(int len) {
            if (fs == null) return "";
            char[] buff = new char[len];
            for (int i = 0; i < len; i++) {
                buff[i] = Convert.ToChar(readUInt16());
            }
            return (new string(buff, 0, buff.Length));
        }
        public string readFixedStringEx(int len) { //2017-11-14没测试过
            if (fs == null) return "";
            return reader.ReadChars(len).ToString();
        }

        ///---2010-4-27add
        public string readXString() {  //read unicode string
            if (fs == null) return "";
            return readXFixedString(readInteger());
        }
        public string readXFixedString(int len) {
            if (fs == null) return "";
            char[] buff = new char[len];
            for (int i = 0; i < len; i++) {
                buff[i] = Convert.ToChar(readXInt16());
            }
            return (new string(buff, 0, buff.Length));
        }
        public UInt16 readXInt16() {
            if (fs == null) return 0;
            try {
                return reader.ReadUInt16();
            } catch {
                return 0;
            }
        }
        ///-----addend


        public string readWString() {  //read unicode string
            if (fs == null) return "";
            return readWFixedString(readInteger());
        }
        public void close() {
            if (fs != null) fs.Close();
            fs = null;
        }
        //其他一些常用文件操作函数
        public static bool delete(string filename) {
            try {
                System.IO.File.Delete(filename);
            } catch {
                return false;
            }
            return true;
        }
        public static bool rename(string oldFilename, string newFilename) {
            try {
                System.IO.File.Move(oldFilename, newFilename);
            } catch {
                return false;
            }
            return true;
        }
        /// <summary>从全路径的文件名提取路径名，末尾包含"\"或者"/"</summary>
        /// <param name="filePathName">一个带路径的文件名</param>
        public static string extractFilePath(string filePathName) {
            int index = filePathName.LastIndexOf('\\');
            if (index < 0) {
                index = filePathName.LastIndexOf('/');
                if (index < 0) {
                    return filePathName;
                }
            }
            return filePathName.Substring(0, index + 1);
        }

        /// <summary>从全路径的文件名，提取单纯的文件名</summary>
        /// <param name="filePathName"></param>
        /// <returns></returns>
        public static string extractFileName(string filePathName) {
            int index = filePathName.LastIndexOf('\\');
            if (index < 0) {
                index = filePathName.LastIndexOf('/');
                if (index < 0) {
                    return filePathName;
                }
            }
            index += 2;
            return DelphiString.copy(filePathName, index, filePathName.Length);
        }

        /// <summary>修改文件扩展名,包含"."</summary>
        /// <param name="fileName">需要修改的文件名</param>
        /// <param name="newExt">新的扩展名，同样包含"."</param>
        /// <returns>修改好的东西</returns>
        public static string changeFileExt(string fileName, string newExt) {
            int index = fileName.LastIndexOf('.');
            if (index < 0) return fileName + newExt;
            index++;
            return DelphiString.copy(fileName, 1, index - 1) + newExt;
        }
        /// <summary>当前运行文件的路径+文件名</summary>
        public static string currentFilePathName {
            get {
                return System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            }
        }
        /// <summary>当前运行文件的文件名，不包含路径</summary>
        public static string currentFileName {
            get {
                return extractFileName(currentFilePathName);
            }
        }
        /// <summary>当前运行文件的路径</summary>
        public static string currentFilePath {
            get {
                return extractFilePath(currentFilePathName);
            }
        }
        public static bool exec(string exeNameParam, bool hide, int timeout) {
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            try {
                //指定调用的可执行文件
                proc.StartInfo.FileName = "cmd.exe";
                proc.StartInfo.Arguments = " /c " + "\"" + exeNameParam + "\"";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardInput = true;
                proc.StartInfo.CreateNoWindow = hide;
                proc.Start();
            } catch {
                if (!proc.HasExited) {
                    proc.Close();
                }
                return false;
            }
            DateTime dt = DateTime.Now;
            while (!proc.HasExited) {
                if (DateTime.Now.Subtract(dt).TotalMilliseconds >= timeout) {
                    proc.Close();
                    return false;
                }
            }
            return true;
        }
        /// <summary>2010-04-02 把整个文件载入到字符串，GBK功能目前没有做</summary>
        public static string loadToStringGBK(string filename) {
            System.IO.FileStream fp = null;
            try {
                fp = new System.IO.FileStream(filename, System.IO.FileMode.Open);
            } catch {
                return "";
            }
            System.IO.BinaryReader reader = new System.IO.BinaryReader(fp);

            StringBuilder result = new StringBuilder();
            while (true) {
                try {
                    result.Append(reader.ReadChar());
                } catch {
                    break;
                }
            }
            fp.Close();
            return result.ToString();
        }

        public static string loadToString(string filename, string encoding = "GB2312") {
            System.IO.FileStream fp = null;
            try {
                fp = new System.IO.FileStream(filename, System.IO.FileMode.Open);
            } catch {
                return "";
            }
            System.IO.BinaryReader reader = new System.IO.BinaryReader(fp, System.Text.Encoding.GetEncoding(encoding));
            StringBuilder result = new StringBuilder();
            while (true) {
                try {
                    result.Append(reader.ReadChar());
                } catch {
                    break;
                }
            }
            fp.Close();
            return result.ToString();
        }

        public static bool saveString(string filename, string data, string coding) {
            try {
                Encoding c = Encoding.GetEncoding(coding);
                if (c == null) return false;
                byte[] buff = c.GetBytes(data);
                if (System.IO.File.Exists(filename)) {
                    System.IO.File.Delete(filename);
                }
                System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Create);
                fs.Write(buff, 0, buff.Length);
                fs.Flush();
                fs.Close();
            } catch {
                return false;
            }
            return true;
        }

        /// <summary>获取一个可执行文件的详细版本号，以Vx.x.x.x的形式给出，如V1.0.0.3</summary>
        /// <param name="fileName">可执行文件的路径+文件名</param>
        /// <returns></returns>
        public static string getFileVersion(string fileName) {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.LoadFile(FileOperation.currentFilePathName);
            System.Reflection.AssemblyName assemblyName = assembly.GetName();
            return "V" + assemblyName.Version.ToString();
        }

        /// <summary>
        /// 拷贝整个目录
        /// </summary>
        public static void copyFolder(string from, string to) {
            from = formatPath(from);
            to = formatPath(to);
            if (!System.IO.Directory.Exists(to))
                System.IO.Directory.CreateDirectory(to);

            // 子文件夹
            foreach (string sub in System.IO.Directory.GetDirectories(from))
                copyFolder(sub + "\\", to + System.IO.Path.GetFileName(sub) + "\\");

            // 文件
            foreach (string file in System.IO.Directory.GetFiles(from))
                System.IO.File.Copy(file, to + System.IO.Path.GetFileName(file), true);
        }

        /// <summary>
        /// 删除整个目录，即使非空
        /// </summary>
        public static void deleteFolder(string deleteDirectory) {
            if (System.IO.Directory.Exists(deleteDirectory)) {
                foreach (string deleteFile in (System.IO.Directory.GetFileSystemEntries(deleteDirectory))) {
                    if (System.IO.File.Exists(deleteFile)) {
                        System.IO.File.Delete(deleteFile);
                    } else {
                        deleteFolder(deleteFile);
                    }
                }
                System.IO.Directory.Delete(deleteDirectory);
            }
        }
        /// <summary>
        /// 把一个路径转换为末尾以\结束的路径
        /// </summary>
        public static String formatPath(String path) {
            if (path == "") return path;
            if (path.Substring(path.Length - 1, 1) != "\\") {
                return path + "\\";
            } else {
                return path;
            }
        }

        public static String getFolderName(String path) {
            if (path == "") return "";
            if (path.Substring(path.Length - 1, 1) == "\\") {
                path = path.Substring(0, path.Length - 1);
            }
            String[] r = path.Split('\\');
            if (r.Length <= 0) return "";
            return r[r.Length - 1];
        }

        /// <summary>判断两个文件是否相同，判断依据是文件大小后最后修改时间</summary>
        public static bool isSame(string file1, string file2) {
            System.IO.FileInfo f1, f2;
            try {
                f1 = new System.IO.FileInfo(file1);
                f2 = new System.IO.FileInfo(file2);
                if ((f1.Length != f2.Length) || (f1.LastWriteTime != f2.LastWriteTime)) {
                    return false;
                } else {
                    return true;
                }
            } catch {
                return false;
            }
        }
    }

    /// <summary>2009-05-23 用于多线程访问某些公用数据的锁</summary>
    public class TamLocker {
        private Object LockObj = new Object();
        private Boolean Busy = false;
        /// <summary>毫秒单位的锁最大等待超时</summary>
        public int LockTimeout = 10000;
        public int waitInterval = 5;
        /// <summary>锁定</summary>
        /// <returns>返回false表示出现了超时解锁的现象</returns>
        public bool Lock() {
            bool result = true;
            lock (LockObj) {
                System.DateTime StartTick = System.DateTime.Now;
                while (Busy) {
                    if (LockTimeout > 0) {
                        if (System.DateTime.Now.Subtract(StartTick).TotalMilliseconds >= LockTimeout) {
                            Unlock();
                            result = false;
                            break;
                        }
                    }
                    System.Threading.Thread.Sleep(waitInterval);
                };
                Busy = true;
            }
            return result;
        }
        public void Unlock() {
            Busy = false;
        }
        /// <param name="lockTimeout">毫秒单位的锁最大等待超时,0表示不需要超时</param>
        /// <param name="waitInterval">等待时查询的时间间隔，单位毫秒，推荐使用5ms即1秒扫描200次</param>
        public TamLocker(int lockTimeout, int waitInterval) {
            this.LockTimeout = lockTimeout;
            this.waitInterval = waitInterval;
        }
        public TamLocker() {
        }
    }

    public class StringData2x {
        public string Buff;
        public int sp;
        public int InsertPos;
        public StringData2x(string StringBuff) {
            Buff = StringBuff;
            sp = 1;
            InsertPos = 1;
        }
        public StringData2x() {
            Buff = "";
            sp = 1;
        }
        public void AppendInteger(int Value) {
            UInt32 iTemp = (UInt32)Value;
            Buff += (char)(((iTemp & 0x0000000F) | 0x10) | ((((iTemp & 0x000000F0) >> 4) | 0x10) << 8));
            Buff += (char)((((iTemp & 0x00000F00) >> 8) | 0x10) | ((((iTemp & 0x0000F000) >> 12) | 0x10) << 8));
            Buff += (char)((((iTemp & 0x000F0000) >> 16) | 0x10) | ((((iTemp & 0x00F00000) >> 20) | 0x10) << 8));
            Buff += (char)((((iTemp & 0x0F000000) >> 24) | 0x10) | ((((iTemp & 0xF0000000) >> 28) | 0x10) << 8));
        }
        public void InsertInteger(int Value) {
            string sTemp = Buff;
            Buff = DelphiString.copy(sTemp, 1, InsertPos - 1);
            AppendInteger(Value);
            Buff += DelphiString.copy(sTemp, InsertPos, sTemp.Length);
            InsertPos += 4;
        }
        public int ReadInteger() {
            UInt32 Result, iTemp;
            if (sp >= Buff.Length - 2) return (0);
            iTemp = (UInt32)(Buff[sp - 1]); sp++;
            Result = (iTemp & 0x0F) | ((iTemp & 0x0F00) >> 4);
            iTemp = (UInt32)(Buff[sp - 1]); sp++;
            Result += ((iTemp & 0x0F) | ((iTemp & 0x0F00) >> 4)) << 8;
            iTemp = (UInt32)(Buff[sp - 1]); sp++;
            Result += ((iTemp & 0x0F) | ((iTemp & 0x0F00) >> 4)) << 16;
            iTemp = (UInt32)(Buff[sp - 1]); sp++;
            Result += ((iTemp & 0x0F) | ((iTemp & 0x0F00) >> 4)) << 24;
            return ((Int32)Result);
        }
        public void AppendString(string Value) {
            AppendInteger(Value.Length);
            Buff += Value;
        }
        public void InsertString(string Value) {
            string sTemp = Buff;
            Buff = DelphiString.copy(sTemp, 1, InsertPos - 1);
            AppendInteger(Value.Length);
            AppendString(Value);
            Buff += DelphiString.copy(sTemp, InsertPos, sTemp.Length);
            InsertPos += (4 + Value.Length);
        }
        public string ReadString() {
            int Len = ReadInteger();
            string Result = DelphiString.copy(Buff, sp, Len);
            sp += Len;
            return (Result);
        }
        public void MarkInsertPos() {
            InsertPos = Buff.Length + 1;
        }
        public void Pack() {
            int i;
            string Result = "";
            UInt32 iTemp;
            for (i = 0; i < Buff.Length; i++) {
                iTemp = (UInt32)Buff[i];
                iTemp = iTemp << 2;
                iTemp = ((iTemp & 0x000F0000) >> 16) | (iTemp & 0xFFFF);
                Result += (char)iTemp;
            }
            Buff = Result;
            iTemp = 0;
            for (i = 0; i < Buff.Length; i++) {
                iTemp = iTemp ^ ((UInt32)Buff[i]);
            }
            this.AppendInteger((int)iTemp);
        }
        public bool Unpack() {
            string Result = "";
            UInt32 iTemp = 0;
            int i;
            for (i = 0; i < Buff.Length - 4; i++) {
                iTemp = iTemp ^ ((UInt32)Buff[i]);
            }
            sp = Buff.Length - 3;
            if (ReadInteger() != ((int)iTemp)) return false;
            sp = 1;
            for (i = 0; i < Buff.Length - 4; i++) {
                iTemp = (UInt32)Buff[i];
                iTemp = ((iTemp & 0x03) << 16) | iTemp;
                iTemp = iTemp >> 2;
                Result += (char)iTemp;
            }
            Buff = Result;
            return true;
        }
        public void UrlEncode() {
            string Result = "", sTemp = "";
            for (int i = 0; i < Buff.Length; i++) {
                sTemp = string.Format("{0:X}", ((UInt32)Buff[i]));
                while (sTemp.Length < 4) sTemp = "0" + sTemp;
                Result += sTemp;
            }
            Buff = Result;
        }
        public Boolean UrlDecode() {
            string Result = "";
            if ((Buff.Length % 4) != 0) return false;
            for (int i = 0; i < Buff.Length; i += 4) {
                Result += (char)Convert.ToInt32(Buff.Substring(i, 4), 16);
            }
            Buff = Result;
            return true;
        }
    }

    public class StringData6b {
        public string Buff;
        public int sp;
        public int InsertPos;
        public StringData6b(string StringBuff) {
            Buff = StringBuff;
            sp = 1;
            InsertPos = 1;
        }
        public StringData6b() {
            Buff = "";
            sp = 1;
        }
        public void AppendInteger(int Value) {
            UInt32 iTemp = (UInt32)Value;
            Buff += (char)(iTemp & 0xFFFF);
            Buff += (char)((iTemp & 0xFFFF0000) >> 16);
        }
        public void AppendWord(int Value) {
            UInt32 iTemp = (UInt32)Value;
            Buff += (char)(iTemp & 0xFFFF);
        }
        public void InsertInteger(int Value) {
            string sTemp = Buff;
            Buff = DelphiString.copy(sTemp, 1, InsertPos - 1);
            AppendInteger(Value);
            Buff += DelphiString.copy(sTemp, InsertPos, sTemp.Length);
            InsertPos += 2;
        }
        public void InsertWord(int Value) {
            string sTemp = Buff;
            Buff = DelphiString.copy(sTemp, 1, InsertPos - 1);
            AppendWord(Value);
            Buff += DelphiString.copy(sTemp, InsertPos, sTemp.Length);
            InsertPos += 1;
        }
        public int ReadInteger() {
            UInt32 iTemp, Result = 0;
            if (sp >= Buff.Length) return (0);
            iTemp = (UInt32)(Buff[sp - 1]); sp++;
            Result = iTemp;
            iTemp = (UInt32)(Buff[sp - 1]); sp++;
            Result = Result | (iTemp << 16);
            return ((Int32)Result);
        }
        public int ReadWord() {
            UInt32 iTemp, Result = 0;
            if (sp > Buff.Length) return (0);
            iTemp = (UInt32)(Buff[sp - 1]); sp++;
            Result = iTemp;
            return ((Int32)Result);
        }
        public void AppendString(string Value) {
            AppendWord(Value.Length);
            Buff += Value;
        }
        public void AppendLongString(string Value) {
            AppendInteger(Value.Length);
            Buff += Value;
        }
        public void InsertString(string Value) {
            string sTemp = Buff;
            Buff = DelphiString.copy(sTemp, 1, InsertPos - 1);
            AppendString(Value);
            Buff += DelphiString.copy(sTemp, InsertPos, sTemp.Length);
            InsertPos += (1 + Value.Length);
        }
        public void InsertLongString(string Value) {
            string sTemp = Buff;
            Buff = DelphiString.copy(sTemp, 1, InsertPos - 1);
            AppendLongString(Value);
            Buff += DelphiString.copy(sTemp, InsertPos, sTemp.Length);
            InsertPos += (2 + Value.Length);
        }
        public string ReadString() {
            int Len = ReadWord();
            string Result = DelphiString.copy(Buff, sp, Len);
            sp += Len;
            return (Result);
        }
        public string ReadLongString() {
            int Len = ReadInteger();
            string Result = DelphiString.copy(Buff, sp, Len);
            sp += Len;
            return (Result);
        }
        public void MarkInsertPos() {
            InsertPos = Buff.Length + 1;
        }
        private void Pack() {
            int i;
            string Result = "";
            UInt32 iTemp;
            for (i = 0; i < Buff.Length; i++) {
                iTemp = (UInt32)Buff[i];
                iTemp = iTemp << 2;
                iTemp = ((iTemp & 0x000F0000) >> 16) | (iTemp & 0xFFFF);
                Result += (char)iTemp;
            }
            Buff = Result;
            iTemp = 0;
            for (i = 0; i < Buff.Length; i++) {
                iTemp = iTemp ^ ((UInt32)Buff[i]);
            }
            AppendInteger((int)iTemp);
        }
        private bool Unpack() {
            if (Buff.Length < 2) return false;
            string Result = "";
            UInt32 iTemp = 0;
            int i;
            for (i = 0; i < Buff.Length - 2; i++) {
                iTemp = iTemp ^ ((UInt32)Buff[i]);
            }
            sp = Buff.Length - 1;
            if (ReadInteger() != ((int)iTemp)) return false;
            sp = 1;
            for (i = 0; i < Buff.Length - 2; i++) {
                iTemp = (UInt32)Buff[i];
                iTemp = ((iTemp & 0x03) << 16) | iTemp;
                iTemp = iTemp >> 2;
                Result += (char)iTemp;
            }
            Buff = Result;
            return true;
        }
        private static string CodeTable = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ*#";
        public void UrlEncode(Boolean CheckCRC) {
            if (CheckCRC) Pack();
            string Result = "";
            int BytePos, BitPos;
            UInt32 DestCode;
            BytePos = 0; BitPos = 0;
            Buff += " ";
            while (true) {
                if (BitPos < 10) {
                    DestCode = (((UInt32)Buff[BytePos]) >> (10 - BitPos)) & 0x3F;
                } else if (BitPos == 10) {
                    DestCode = ((UInt32)Buff[BytePos]) & 0x3F;
                } else {
                    DestCode = (((UInt32)Buff[BytePos]) << (BitPos - 10)) & 0x3F;
                    DestCode = DestCode | (((UInt32)Buff[BytePos + 1]) >> (26 - BitPos));
                }
                Result += CodeTable[(Int32)DestCode];
                BitPos += 6;
                if (BitPos >= 16) {
                    BitPos -= 16;
                    BytePos++;
                    if (BytePos >= (Buff.Length - 1)) break;
                }
            }
            Buff = Result;
        }
        public Boolean UrlDecode(Boolean CheckCRC) {
            string Result = "";
            int BitPos, iTemp;
            UInt32 DestCode = 0, Code6 = 0;
            BitPos = 0;
            if (Buff == null) Buff = "";
            for (int i = 0; i < Buff.Length; i++) {
                iTemp = CodeTable.IndexOf(Buff[i]);
                if (iTemp < 0) return false;
                Code6 = (UInt32)iTemp;
                if (BitPos < 10) {
                    DestCode = DestCode | (Code6 << (10 - BitPos));
                } else if (BitPos == 10) {
                    DestCode = DestCode | Code6;
                    Result += (char)DestCode;
                    DestCode = 0;
                } else {
                    DestCode = DestCode | (Code6 >> (BitPos - 10));
                    Result += (char)DestCode;
                    DestCode = (Code6 << (26 - BitPos));
                }
                BitPos += 6;
                if (BitPos >= 16) BitPos -= 16;
            }
            Buff = Result;
            if (CheckCRC) return Unpack();
            return true;
        }
    }

    public class StringData6c {
        private System.Text.StringBuilder Buff = new System.Text.StringBuilder();
        private int sp = 0;
        private int InsertPos = 0;
        public StringData6c(string StringBuff) {
            Reset();
            Buff.Append(StringBuff);
        }
        public StringData6c() {
            Reset();
        }
        public void Clear() {
            Reset();
        }
        private void Reset() {
            sp = 0;
            InsertPos = 0;
            Buff.Length = 0;
        }
        public void SetBuff(string Data) {
            Reset();
            Buff.Append(Data);
        }
        public string GetBuff() {
            return Buff.ToString();
        }
        public string GetBuffUnicode() {
            System.Text.StringBuilder Result = new System.Text.StringBuilder();
            int i = 0, Code;
            while (i < Buff.Length) {
                Code = Convert.ToInt32(Buff[i]);
                i++;
                if (i < Buff.Length) {
                    Code = Code | (Convert.ToInt32(Buff[i]) << 8); i++;
                }
                Result.Append(Convert.ToChar(Code));
            }
            return Result.ToString();
        }
        public void AppendInteger(int Value) {
            UInt32 iTemp = (UInt32)Value;
            Buff.Append(Convert.ToChar(iTemp & 0xFF));
            Buff.Append(Convert.ToChar((iTemp & 0x0000FF00) >> 8));
            Buff.Append(Convert.ToChar((iTemp & 0x00FF0000) >> 16));
            Buff.Append(Convert.ToChar((iTemp & 0xFF000000) >> 24));
        }
        public void AppendWord(int Value) {
            UInt32 iTemp = (UInt32)Value;
            Buff.Append(Convert.ToChar(iTemp & 0xFF));
            Buff.Append(Convert.ToChar((iTemp & 0x0000FF00) >> 8));
        }
        public void AppendByte(int Value) {
            UInt32 iTemp = (UInt32)Value;
            Buff.Append(Convert.ToChar(iTemp & 0xFF));
        }
        public void InsertInteger(int Value) {
            UInt32 iTemp = (UInt32)Value;
            Buff.Insert(InsertPos, Convert.ToChar(iTemp & 0xFF)); InsertPos++;
            Buff.Insert(InsertPos, Convert.ToChar((iTemp & 0x0000FF00) >> 8)); InsertPos++;
            Buff.Insert(InsertPos, Convert.ToChar((iTemp & 0x00FF0000) >> 16)); InsertPos++;
            Buff.Insert(InsertPos, Convert.ToChar((iTemp & 0xFF000000) >> 24)); InsertPos++;
        }
        public void InsertWord(int Value) {
            UInt32 iTemp = (UInt32)Value;
            Buff.Insert(InsertPos, Convert.ToChar(iTemp & 0xFF)); InsertPos++;
            Buff.Insert(InsertPos, Convert.ToChar((iTemp & 0x0000FF00) >> 8)); InsertPos++;
        }
        public void InsertByte(int Value) {
            UInt32 iTemp = (UInt32)Value;
            Buff.Insert(InsertPos, Convert.ToChar(iTemp & 0xFF)); InsertPos++;
        }
        public int ReadInteger() {
            UInt32 Result;
            if (sp >= Buff.Length - 3) return 0;
            Result = Convert.ToUInt32(Buff[sp]); sp++;
            Result = Result | (Convert.ToUInt32(Buff[sp]) << 8); sp++;
            Result = Result | (Convert.ToUInt32(Buff[sp]) << 16); sp++;
            Result = Result | (Convert.ToUInt32(Buff[sp]) << 24); sp++;
            return ((Int32)Result);
        }
        public int ReadWord() {
            UInt32 Result;
            if (sp >= Buff.Length - 1) return 0;
            Result = Convert.ToUInt32(Buff[sp]); sp++;
            Result = Result | (Convert.ToUInt32(Buff[sp]) << 8); sp++;
            return ((Int32)Result);
        }
        public int ReadByte() {
            UInt32 Result;
            if (sp >= Buff.Length) return 0;
            Result = Convert.ToUInt32(Buff[sp]); sp++;
            return ((Int32)Result);
        }
        public void AppendString(string Value) {
            AppendWord(Value.Length);
            for (int i = 0; i < Value.Length; i++) {
                AppendWord(Convert.ToInt32(Value[i]));
            }
        }
        public void AppendShortString(string Value) {
            AppendByte(Value.Length);
            for (int i = 0; i < Value.Length; i++) {
                AppendWord(Convert.ToInt32(Value[i]));
            }
        }
        public void AppendLongString(string Value) {
            this.AppendInteger(Value.Length);
            for (int i = 0; i < Value.Length; i++) {
                AppendWord(Convert.ToInt32(Value[i]));
            }
        }
        public void InsertString(string Value) {
            this.InsertWord(Value.Length);
            for (int i = 0; i < Value.Length; i++) {
                InsertWord(Convert.ToInt32(Value[i]));
            }
        }
        public void InsertShortString(string Value) {
            this.InsertByte(Value.Length);
            for (int i = 0; i < Value.Length; i++) {
                InsertWord(Convert.ToInt32(Value[i]));
            }
        }
        public void InsertLongString(string Value) {
            this.InsertInteger(Value.Length);
            for (int i = 0; i < Value.Length; i++) {
                InsertWord(Convert.ToInt32(Value[i]));
            }
        }
        public string ReadString() {
            System.Text.StringBuilder Result = new System.Text.StringBuilder();
            Result.Length = ReadWord();
            for (int i = 0; i < Result.Length; i++) Result[i] = Convert.ToChar(ReadWord());
            return (Result.ToString());
        }
        public string ReadShortString() {
            System.Text.StringBuilder Result = new System.Text.StringBuilder();
            Result.Length = ReadByte();
            for (int i = 0; i < Result.Length; i++) Result[i] = Convert.ToChar(ReadWord());
            return (Result.ToString());
        }
        public string ReadLongString() {
            System.Text.StringBuilder Result = new System.Text.StringBuilder();
            Result.Length = ReadInteger();
            for (int i = 0; i < Result.Length; i++) Result[i] = Convert.ToChar(ReadWord());
            return (Result.ToString());
        }
        public void MarkInsertPos() {
            InsertPos = Buff.Length;
        }
        private void Pack() {
            UInt32 iTemp = 0;
            int i;
            for (i = 0; i < Buff.Length; i++) {
                iTemp = Convert.ToUInt32(Buff[i]) << 2;
                Buff[i] = Convert.ToChar(((iTemp & 0xFF00) >> 8) | (iTemp & 0xFF));
            }
            iTemp = 0;
            for (i = 0; i < Buff.Length; i++) {
                iTemp = iTemp ^ Convert.ToUInt32(Buff[i]);
            }
            AppendInteger(Convert.ToInt32(iTemp));
        }
        private bool Unpack() {
            UInt32 iTemp = 0;
            int i;
            if (Buff.Length < 4) return false;
            for (i = 0; i < Buff.Length - 4; i++) {
                iTemp = iTemp ^ Convert.ToUInt32(Buff[i]);
            }
            sp = Buff.Length - 4;
            if (ReadInteger() != iTemp) return false;
            sp = 0;
            Buff.Remove(Buff.Length - 4, 4);
            for (i = 0; i < Buff.Length; i++) {
                iTemp = Convert.ToUInt32(Buff[i]);
                iTemp = ((iTemp & 0x03) << 8) | iTemp;
                iTemp = iTemp >> 2;
                Buff[i] = Convert.ToChar(iTemp);
            }
            return true;
        }
        private static string CodeTable = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ*!";
        public void UrlEncode(Boolean CheckCRC) {
            if (CheckCRC) Pack();
            System.Text.StringBuilder Result = new System.Text.StringBuilder();
            Buff.Append(' ');
            int BytePos = 0, BitPos = 0, DestCode;
            while (true) {
                if (BitPos == 0) {
                    DestCode = Convert.ToInt32(Buff[BytePos]) & 0x3F;
                } else if (BitPos <= 2) {
                    DestCode = (Convert.ToInt32(Buff[BytePos]) >> BitPos) & 0x3F;
                } else {
                    DestCode = Convert.ToInt32(Buff[BytePos]) >> BitPos;
                    DestCode = DestCode | ((Convert.ToInt32(Buff[BytePos + 1]) << (8 - BitPos)) & 0x3F);
                }
                Result.Append(CodeTable[DestCode]);
                BitPos += 6;
                if (BitPos >= 8) {
                    BitPos -= 8;
                    BytePos++;
                    if (BytePos >= (Buff.Length - 1)) break;
                }
            }
            Buff = Result;
        }
        public Boolean UrlDecode(Boolean CheckCRC) {
            System.Text.StringBuilder Result = new System.Text.StringBuilder();
            int BitPos = 0, DestCode = 0, Code6 = 0;
            for (int i = 0; i < Buff.Length; i++) {
                Code6 = CodeTable.IndexOf(Buff[i]);
                if (Code6 < 0) return false;
                if (BitPos == 0) {
                    DestCode = DestCode | Code6;
                    BitPos += 6;
                } else if (BitPos <= 2) {
                    DestCode = DestCode | (Code6 << BitPos);
                    BitPos = (BitPos + 6) % 8;
                    if (BitPos == 0) {
                        Result.Append(Convert.ToChar(DestCode));
                        DestCode = 0;
                    }
                } else {
                    DestCode = DestCode | ((Code6 << BitPos) & 0xFF);
                    BitPos = BitPos - 2;
                    Result.Append(Convert.ToChar(DestCode));
                    DestCode = Code6 >> (6 - BitPos);
                }
            }
            Buff = Result;
            if (CheckCRC) return Unpack();
            return true;
        }
    }

    public class TamQueue_Item {
        public int IntValue1 = 0;
        public int IntValue2 = 0;
        public int IntValue3 = 0;
        public string StrValue1 = "";
        public string StrValue2 = "";
        public string StrValue3 = "";
        public void Assign(TamQueue_Item Value) {
            IntValue1 = Value.IntValue1;
            IntValue2 = Value.IntValue2;
            IntValue3 = Value.IntValue3;
            StrValue1 = Value.StrValue1;
            StrValue2 = Value.StrValue2;
            StrValue3 = Value.StrValue3;
        }
    }
    public class TamQueue {
        public TamQueue_Item[] Buff;
        private int FItemCount = 0;
        private int FSize = 0;
        private int FP = 0;
        private int RP = 0;
        public bool AutoDrop = false;
        public TamQueue_Item Item = new TamQueue_Item();
        public int Size {
            get { return FSize; }
            set {
                Buff = new TamQueue_Item[value];
                for (int i = 0; i < value; i++) Buff[i] = new TamQueue_Item();
                FP = 0; RP = 0; FItemCount = 0;
                FSize = value;
            }
        }
        public int ItemCount {
            get { return FItemCount; }
        }
        public bool Push() {
            if (!AutoDrop) {
                if (ItemCount >= (Size - 1)) return false;
            }
            if (ItemCount >= (Size - 1)) {
                RP = (RP + 1) % Size;
                FItemCount--;
            }
            Buff[FP].Assign(Item);
            FP = (FP + 1) % Size;
            FItemCount++;
            return true;
        }
        public bool Pop() {
            if (ItemCount <= 0) return false;
            Item.Assign(Buff[RP]);
            RP = (RP + 1) % Size;
            FItemCount--;
            return true;
        }
    }

    /// <summary>队列2版本，可以自定义数据结构</summary>
    public class TamQueue2<T> {
        public Item valuePop = null;
        public T valuePush;
        private System.Collections.ArrayList buff = new System.Collections.ArrayList();
        private int fp = 0;
        private int rp = 0;
        private int fCount = 0;
        public class Item {
            public T value;
            public Item(T value) {
                this.value = value;
            }
        }
        public TamQueue2(int size) {
            this.size = size;
        }
        public int size {
            set {
                if (buff.Count != value) {
                    fp = 0;
                    rp = 0;
                    fCount = 0;
                }
                while (buff.Count > value) buff.RemoveAt(buff.Count - 1);
                while (buff.Count < value) buff.Add(null);
                buff.TrimToSize();
            }
            get { return buff.Count; }
        }
        public int count {
            get { return fCount; }
        }
        public bool push(T value) {
            if (count + 1 >= size) return false;
            buff[fp] = new Item(value);
            fp = (fp + 1) % size;
            fCount++;
            return true;
        }
        public bool push() {
            return push(valuePush);
        }
        public Item pop() {
            if (count <= 0) return null;
            Item result = buff[rp] as Item;
            valuePop = result;
            rp = (rp + 1) % size;
            fCount--;
            return result;
        }
        public void clear() {
            while (pop() != null) { }
        }
    }

    /// <summary>堆栈算法</summary>
    public class TamStack<T> {
        private System.Collections.ArrayList buff = new System.Collections.ArrayList();
        private int fp = 0;
        /// <summary>堆栈溢出后是否清除栈底记录</summary>
        public bool overflowTrim = true;
        public class Item {
            public T value;
            public Item(T value) {
                this.value = value;
            }
        }
        public TamStack(int size, bool overflowTrim = true) {
            this.size = size;
            this.overflowTrim = overflowTrim;
        }
        public int size {
            get {
                return buff.Count;
            }
            set {
                if (buff.Count != value) {
                    fp = 0;
                }
                while (buff.Count > value) buff.RemoveAt(buff.Count - 1);
                while (buff.Count < value) buff.Add(null);
                buff.TrimToSize();
            }
        }
        /// <summary>栈元素数量</summary>
        public int count {
            get { return fp; }
        }
        public bool push(T value) {
            if (count >= size) {
                if (!overflowTrim) return false;
                buff.RemoveAt(0);
                buff.Add(null);
                fp--;
            }
            if ((fp < 0) || (fp >= size)) return false;
            buff[fp] = new Item(value);
            fp++;
            return true;
        }
        public Item pop() {
            if (count <= 0) return null;
            fp--;
            if ((fp < 0) || (fp >= size)) return null;
            return buff[fp] as Item;
        }
        public void clear() {
            while (pop() != null) { }
        }
        /// <summary>获取最后一个入栈的元素</summary>
        public Item getLastItem() {
            if (count <= 0) return null;
            int f = fp - 1;
            return buff[f] as Item;
        }
    }

    /// <summary>
    /// 改动将原有的current分开为valuePop和valuePush以便适应大多数情况下push和pop不在一条线程的情况
    /// </summary>
    public class Queue {
        public object valuePop = null;
        public object valuePush = null;
        private System.Collections.ArrayList buff = new System.Collections.ArrayList();
        private int fp = 0;
        private int rp = 0;
        private int fCount = 0;
        public int size {
            set {
                if (buff.Count != value) {
                    fp = 0;
                    rp = 0;
                    fCount = 0;
                }
                while (buff.Count > value) buff.RemoveAt(buff.Count - 1);
                while (buff.Count < value) buff.Add(null);
                buff.TrimToSize();
            }
            get { return buff.Count; }
        }
        public int count {
            get { return fCount; }
        }
        public bool push(object value) {
            if (count + 1 >= size) return false;
            buff[fp] = value;
            fp = (fp + 1) % size;
            fCount++;
            return true;
        }
        public bool push() {
            return push(valuePush);
        }
        public bool pop(ref object value) {
            if (count <= 0) return false;
            value = buff[rp];
            rp = (rp + 1) % size;
            fCount--;
            return true;
        }
        public bool popNone() {
            object noUse = null;
            return pop(ref noUse);
        }
        public bool pop() {
            return pop(ref valuePop);
        }
        public void clear() {
            while (pop()) { }
        }
    }

    public class StringQueue {
        string current = "";
        Queue queue = new Queue();
        public int size {
            set { queue.size = value; }
            get { return queue.size; }
        }
        public int count {
            get { return queue.count; }
        }
        public bool push() {
            return queue.push(current);
        }
        public bool pop() {
            bool result;
            result = queue.pop();
            if (result) current = queue.valuePop as string;
            return result;
        }
    }

    /// <summary>高速流缓冲</summary>
    public class StreamBuff {
        public byte[] buff = new byte[0];
        public byte[] arrayValuePush = null;
        public byte[] arrayValuePop = null;
        private int fp = 0;
        private int rp = 0;
        private int fCount = 0;
        private TamLocker locker1 = new TamLocker();  //没有对arrayValuePush和arrayValuePop保护，也就是说，不能同时多人入缓冲，或者多人同时出缓冲
        private TamLocker locker2 = new TamLocker();  //专锁size变化
        /// <summary>缓冲区大小</summary>
        int size {
            get { return buff.Length; }
            set {
                locker2.Lock();
                int i;
                if (value > size) {
                    if (fp >= rp) {
                        byte[] newBuff = new byte[value];
                        for (i = rp; i < fp; i++) newBuff[i] = buff[i];
                        buff = newBuff;
                    } else {
                        byte[] newBuff = new byte[value];
                        for (i = 0; i < fp; i++) newBuff[i] = buff[i];
                        for (i = rp; i < size; i++) newBuff[i + value - size] = buff[i];
                        rp = rp + value - size;
                        buff = newBuff;
                    }
                } else if (value < size) {
                    if (value <= count + 1) value = count + 1;
                    byte[] newBuff = new byte[value];
                    if (count <= 0) {
                        buff = newBuff;
                    } else {
                        popArray(count);
                        buff = newBuff;
                        fp = 0;
                        rp = 0;
                        newBuff = arrayValuePush;
                        arrayValuePush = arrayValuePop;
                        pushArray();
                        arrayValuePush = newBuff;
                    }
                }
                locker2.Unlock();
            }
        }
        /// <summary>实际数据的数量</summary>
        public int count {
            get { return fCount; }
        }
        public bool pushArray() {
            if (arrayValuePush == null) return false;
            return pushArray(arrayValuePush.Length);
        }
        public bool pushArray(int pushCount) {
            locker1.Lock();
            if (arrayValuePush == null) {
                locker1.Unlock();
                return false;
            }
            if (pushCount + count + 1 >= size) {
                size = count + pushCount + 1;
            }
            for (var i = 0; i < pushCount; i++) {
                buff[fp] = arrayValuePush[i];
                fp = (fp + 1) % size;
                fCount++;
            }
            locker1.Unlock();
            return true;
        }
        public bool popArray(int popCount) {
            locker1.Lock();
            if (count <= 0) {
                locker1.Unlock();
                return false;
            }
            if (popCount > count) popCount = count;
            arrayValuePop = new byte[popCount];
            for (var i = 0; i < popCount; i++) {
                arrayValuePop[i] = buff[rp];
                rp = (rp + 1) % size;
                fCount--;
            }
            locker1.Unlock();
            return true;
        }
        /// <summary>仅仅取数据用于检查，缓冲区中的数据并不减少</summary>
        /// <param name="viewCount">需要查看的字节数量</param>
        public bool viewArray(int viewCount) {
            locker1.Lock();
            if (count <= 0) {
                locker1.Unlock();
                return false;
            }
            if (viewCount > count) viewCount = count;
            arrayValuePop = new byte[viewCount];
            int tempRP = rp;
            for (var i = 0; i < viewCount; i++) {
                arrayValuePop[i] = buff[tempRP];
                tempRP = (tempRP + 1) % size;
            }
            locker1.Unlock();
            return true;
        }
        /// <summary>按照一定数量弹出数据并丢弃</summary>
        /// <param name="popCount">需要弹出的数量</param>
        /// <returns>实际弹出的数量</returns>
        public int popNone(int popCount) {
            locker1.Lock();
            if (count <= 0) {
                locker1.Unlock();
                return 0;
            }
            if (popCount > count) popCount = count;
            rp = (rp + popCount) % size;
            fCount -= popCount;
            locker1.Unlock();
            return popCount;
        }
        public void clear() {
            fp = 0;
            rp = 0;
            fCount = 0;
        }
        public void trim() {
            size = 0;
        }
    }

    /// <summary>内存日志，将程序，特别是底层程序的日志暂时保存在内存中，方便后续的高层程序处理</summary>
    public class MemoryLogItem {
        public int logType = 0;
        public string logString = "";
    }
    public class MemoryLog {
        public MemoryLogItem current = null;
        private Queue queue = new Queue();
        /// <summary> 是否不需要显示时间,一般情况下MemoryLog作为日志缓冲，在这种情况下即时记录时间可以保证后续程序取日志时时间更精确 </summary>
        public bool noTime = false;
        /// <summary> 时间日期的类型，参考本类的LogTypeXXXXXX </summary>
        public int datetimeType = 20;
        public static int LogTypeCommon = 10;
        public static int LogTypeWarning = 20;
        public static int LogTypeError = 30;
        public string Alias = "";
        public delegate void OnWriteLog(MemoryLog sender, int logType, string logString);
        public OnWriteLog onWriteLog = null;
        public MemoryLog() {
            queue.size = 300;
        }
        public int size {
            get { return queue.size; }
            set { queue.size = value; }
        }
        public int count {
            get { return queue.count; }
        }
        public void writeLogString(int logType, string logString) {
            if (noTime) {
                logString = Alias + logString;
                noTime = false;
            } else {
                if (datetimeType == 0) {
                    logString = Alias + logString;
                } else if (datetimeType == 20) {
                    logString = System.DateTime.Now.ToString("MM-dd HH:mm:ss") + " " + Alias + logString;
                } else if (datetimeType == 30) {
                    logString = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + Alias + logString;
                } else {
                    logString = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + Alias + logString;
                }
            }
            MemoryLogItem item = new MemoryLogItem();
            item.logType = logType;
            item.logString = logString;
            if (queue.count + 1 >= queue.size) queue.popNone();
            queue.push(item);
            if (onWriteLog != null) onWriteLog(this, logType, logString);
        }
        public void writeLogString(int logType, string logString, bool addTime) {
            noTime = !addTime;
            writeLogString(logType, logString);
        }
        public void writeCommon(string logString) {
            writeLogString(LogTypeCommon, logString);
        }
        public void writeCommon(string logString, bool addTime) {
            writeLogString(LogTypeCommon, logString, addTime);
        }
        public void writeWarning(string logString) {
            writeLogString(LogTypeWarning, logString);
        }
        public void writeWarning(string logString, bool addTime) {
            writeLogString(LogTypeWarning, logString, addTime);
        }
        public void writeError(string logString) {
            writeLogString(LogTypeError, logString);
        }
        public void writeError(string logString, bool addTime) {
            writeLogString(LogTypeError, logString, addTime);
        }
        public bool pop() {
            if (queue.count <= 0) return false;
            queue.pop();
            current = queue.valuePop as MemoryLogItem;
            return true;
        }
    }

    /// <summary>支持内存日志输出的基类，一般用于多线程后台服务类，本类的日志读写是线程安全的(2012/03/27)</summary>
    public class MemoryLogClass {
        private object logLocker = new object();
        /// <summary>本身自带logFile,如果高层程序需要使用高层的logFile则可以直接替换本变量</summary>
        public MemoryLog logFile = new MemoryLog();
        /// <summary>别名，用于显示在日志的头部</summary>
        public string alias = "";
        protected void writeLogCommon(string logString) {
            lock (logLocker) {
                if (logFile == null) return;
                writeLogString(TamPub1.MemoryLog.LogTypeCommon, logString);
            }
        }
        protected void writeLogWarning(string logString) {
            lock (logLocker) {
                if (logFile == null) return;
                writeLogString(TamPub1.MemoryLog.LogTypeWarning, logString);
            }
        }
        protected void writeLogError(string logString) {
            lock (logLocker) {
                if (logFile == null) return;
                writeLogString(TamPub1.MemoryLog.LogTypeError, logString);
            }
        }
        private void writeLogString(int logType, string logString) {
            if (logFile == null) return;
            logString = alias + logString;
            logFile.writeLogString(logType, logString);
        }
        public LogItem popLog() {
            lock (logLocker) {
                if (!logFile.pop()) {
                    return null;
                }
                LogItem result = new LogItem();
                result.logType = logFile.current.logType;
                result.logString = logFile.current.logString;
                return result;
            }
        }
        public class LogItem {
            public int logType;
            public string logString;
        }
    }

    /// <summary>磁盘日志，以磁盘文件的形式保存日志, 日志格式以****.all.txt的形式，如果一个文件满则改为***.all.bak.txt的形式</summary>
    public class DiskLog {
        private FileOperation logAll = new FileOperation();
        private FileOperation logWarning = new FileOperation();
        private FileOperation logError = new FileOperation();
        private string fFilename = "";
        public int maxFileSize = 20000000; //单个日志文件的最大大小(缺省20M)
        public bool addDatetime = false;
        public string alias = "";
        public DiskLog() {
            loadFilenameFromExe(FileOperation.currentFileName);
        }
        public string filename {
            get { return fFilename; }
            set {
                fFilename = value;
                logAll.filename = value + ".all.txt";
                logWarning.filename = value + ".wrn.txt";
                logError.filename = value + ".err.txt";
            }
        }
        /// <summary>从exe文件名获取日志文件名，如：exe为c:\abcd\ccc.exe, 则日志文件名为 c:\abcd\ccc, Warning文件名为c:\abcd\ccc.wrn.txt</summary>
        public void loadFilenameFromExe(string exeFilename) {
            filename = FileOperation.changeFileExt(exeFilename, "");
        }
        public string filenameAll { get { return logAll.filename; } }
        public string filenameWarning { get { return logWarning.filename; } }
        public string filenameError { get { return logError.filename; } }
        public bool writeLogString(int logType, string logString) {
            if (logType == MemoryLog.LogTypeWarning) {
                if (!logWarning.openAppend()) return false;
                if (!logWarning.writeWFixedString(logString + "\r\n")) return false;
                if (!closeAndCheck(logWarning)) return false;
            } else if (logType == MemoryLog.LogTypeError) {
                if (!logError.openAppend()) return false;
                if (!logError.writeWFixedString(logString + "\r\n")) return false;
                if (!closeAndCheck(logError)) return false;
            }
            if (logType == MemoryLog.LogTypeCommon) {
                logString = "  " + logString;
            } else if (logType == MemoryLog.LogTypeWarning) {
                logString = "W " + logString;
            } else {
                logString = "E " + logString;
            }
            if (!logAll.openAppend()) return false;
            if (!logAll.writeWFixedString(logString + "\r\n")) return false;
            if (!closeAndCheck(logAll)) return false;
            return true;
        }
        public bool writeLogString(int logType, string logString, bool addDatetime) {
            if (addDatetime) {
                return writeLogString(logType, System.DateTime.Now.ToString("MM-dd HH:mm:ss") + " " + logString);
            } else return writeLogString(logType, logString);
        }
        public bool writeCommon(string logString) {
            return writeCommon(logString, addDatetime);
        }
        public bool writeCommon(string logString, bool addDatetime) {
            if (addDatetime) {
                return writeLogString(MemoryLog.LogTypeCommon, System.DateTime.Now.ToString("MM-dd HH:mm:ss") + " " + alias + logString);
            } else {
                return writeLogString(MemoryLog.LogTypeCommon, alias + logString);
            }
        }
        public bool writeWarning(string logString) {
            return writeWarning(logString, addDatetime);
        }
        public bool writeWarning(string logString, bool addDatetime) {
            if (addDatetime) {
                return writeLogString(MemoryLog.LogTypeWarning, System.DateTime.Now.ToString("MM-dd HH:mm:ss") + " " + alias + logString);
            } else {
                return writeLogString(MemoryLog.LogTypeWarning, alias + logString);
            }
        }
        public bool writeError(string logString) {
            return writeError(logString, addDatetime);
        }
        public bool writeError(string logString, bool addDatetime) {
            if (addDatetime) {
                return writeLogString(MemoryLog.LogTypeError, System.DateTime.Now.ToString("MM-dd HH:mm:ss") + " " + alias + logString);
            } else {
                return writeLogString(MemoryLog.LogTypeError, alias + logString);
            }
        }
        private bool closeAndCheck(FileOperation checkFile) {
            if (checkFile.pos < maxFileSize) {
                checkFile.close();
                return true;
            }
            checkFile.close();
            string newFilename = checkFile.filename;
            newFilename = FileOperation.changeFileExt(newFilename, ".bak.txt");
            try {
                FileOperation.delete(newFilename);
                FileOperation.rename(checkFile.filename, newFilename);
            } catch {
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// IP服务端控件,封装好的socket控件,所有函数全部为无阻塞模式,高层程序只要通过事件就可以通信
    /// 所有的connection存放在IPServer自身, 如：IPServer abc; abc[2]则代表第3个连接
    /// </summary>
    public class IPServer {
        private System.Collections.ArrayList buff = new System.Collections.ArrayList();
        public Connection current = null;
        public MemoryLog log = new MemoryLog();
        public int port;
        public System.Net.Sockets.Socket socket = null;
        public delegate void OnReceive(IPServer sender, Connection connection);
        public delegate void OnAccept(IPServer sender, Connection connection);
        public delegate void OnClose(IPServer sender, Connection connection);
        public delegate void OnSendEnd(IPServer sender, Connection connection);
        /// <summary>服务器接收到了数据</summary>
        public OnReceive onReceive = null;
        /// <summary>服务器接受了一个连接</summary>
        public OnAccept onAccept = null;
        /// <summary>服务器关闭了一个连接（对方掉线或者双方的某一方断开连接）</summary>
        public OnClose onClose = null;
        /// <summary>服务器发送一条信息完成</summary>
        public OnSendEnd onSendEnd = null;
        private TamLocker locker1 = new TamLocker();  //用于锁定connection数量变化
        private TamLocker locker2 = new TamLocker();  //用于锁定sendBuff访问
        /// <summary>
        /// 每个连接到服务器的客户端都会有个对应的connection
        /// 其中的param1-4用于存放高层程序的数据
        /// </summary>
        public class Connection {
            public int index = 0;
            public string callerIP = "";
            public int callerPort = 0;
            public System.Net.Sockets.Socket socket = null;
            public TamPub1.StreamBuff recvBuff = new TamPub1.StreamBuff();
            public TamPub1.StreamBuff sendBuff = new TamPub1.StreamBuff();
            public int recvTempBuffSize = 1024 * 16;
            public int sendTempBuffSize = 10000;
            public Object param1 = null;
            public Object param2 = null;
            public Object param3 = null;
            public Object param4 = null;
            public Object param5 = null;
            public Object param6 = null;
            public Object param7 = null;
            public Object param8 = null;
            public TamUserData userData = new TamUserData();
            /// <summary>为了方便高层程序使用,用于接收数据的分包处理</summary>
            public StreamPacket streamRecv = new StreamPacket();
            public Connection() {
                recvBuff.arrayValuePush = new byte[recvTempBuffSize];
                sendBuff.arrayValuePop = new byte[0];
            }
        }
        public Connection this[int i] {
            get {
                if ((i < 0) || (i >= count)) return null;
                return (buff[i] as Connection);
            }
        }
        /// <summary>当前服务器的连接数量</summary>
        public int count { get { return buff.Count; } }
        /// <summary>初始化服务并开始监听</summary>
        public bool init() {
            if (socket != null) socket.Close();
            buff.Clear();
            try {

                socket = new System.Net.Sockets.Socket(
                    System.Net.Sockets.AddressFamily.InterNetwork,
                    System.Net.Sockets.SocketType.Stream,
                    System.Net.Sockets.ProtocolType.Tcp
                );
                socket.Bind(new System.Net.IPEndPoint(System.Net.IPAddress.Any, port));
                socket.Listen(100);
                socket.BeginAccept(new System.AsyncCallback(fOnAccept), 100);
            } catch (Exception e) {
                socket.Close();
                socket = null;
                log.writeWarning("开始监听端口失败,Port=" + port + ",Error=" + e.Message);//写上日志
                return false;
            }
            return true;
        }
        private void append(Connection connection) {
            locker1.Lock();
            buff.Add(connection);
            connection.index = count - 1;
            locker1.Unlock();
        }
        private bool delete(Connection connection) {
            locker1.Lock();
            int i, index;
            for (i = 0; i < count; i++) if (this[i] == connection) break;
            if (i >= count) {
                locker1.Unlock();
                return false;
            }
            index = i;
            buff.RemoveAt(index);
            for (i = index; i < count; i++) this[i].index--;
            locker1.Unlock();
            return true;
        }
        /// <summary>发送一个byte[]形式的数据</summary>
        /// <param name="connection">发送数据的连接通道</param>
        /// <param name="byteArray">需要发送的数据</param>
        /// <param name="bytesCount">发送数据的数量</param>
        public bool sendArray(Connection connection, byte[] byteArray, int bytesCount) {
            if (bytesCount > byteArray.Length) bytesCount = byteArray.Length;
            if (bytesCount <= 0) return false;
            //缓冲区处理
            locker2.Lock();
            connection.sendBuff.arrayValuePush = byteArray;
            connection.sendBuff.pushArray(bytesCount);
            if (connection.sendBuff.arrayValuePop.Length > 0) {
                //正在有数据发送，退出
                locker2.Unlock();
                return true;
            }
            connection.sendBuff.popArray(connection.sendTempBuffSize);
            locker2.Unlock();
            try {
                connection.socket.BeginSend(connection.sendBuff.arrayValuePop, 0, connection.sendBuff.arrayValuePop.Length, 0, new AsyncCallback(fOnSend), connection);
            } catch (Exception e) {
                log.writeWarning("sendArray.BeginSend() error, Error=" + e.Message);
                close(connection, true);
                return false;
            }
            return true;
        }
        /// <summary>发送一个byte[]形式的数据</summary>
        /// <param name="connection">发送数据的连接通道</param>
        /// <param name="byteArray">需要发送的数据</param>
        public bool sendArray(Connection connection, byte[] byteArray) {
            return sendArray(connection, byteArray, byteArray.Length);
        }
        /// <summary>关闭连接</summary>
        /// <param name="connection">连接通道</param>
        /// <param name="shutdownAtOnce">是否立即强制关闭</param>
        public void close(Connection connection, bool shutdownAtOnce) {
            if (onClose != null) onClose(this, connection);
            try {
                if (!shutdownAtOnce) connection.socket.Shutdown(System.Net.Sockets.SocketShutdown.Both);
                connection.socket.Close();
            } catch { }
            connection.socket = null;
            delete(connection);
        }
        private void fOnAccept(System.IAsyncResult aResult) {
            Connection connection = new Connection();
            try {
                connection.socket = socket.EndAccept(aResult);
            } catch (Exception e) {
                log.writeWarning("EndAccept() error, Error=" + e.Message);
                if (connection.socket != null) {
                    connection.socket.Close();
                }
                try {
                    socket.BeginAccept(new System.AsyncCallback(fOnAccept), 100);
                } catch (Exception e1) {
                    log.writeError("fOnAccept.BeginAccept() error, Error=" + e1.Message);
                }
                return;
            }
            connection.callerIP = (connection.socket.RemoteEndPoint as System.Net.IPEndPoint).Address.ToString();
            connection.callerPort = (connection.socket.RemoteEndPoint as System.Net.IPEndPoint).Port;
            append(connection);
            if (onAccept != null) onAccept(this, connection);
            try {
                connection.socket.BeginReceive(connection.recvBuff.arrayValuePush, 0, connection.recvTempBuffSize, 0, new AsyncCallback(fOnReceive), connection);
            } catch (Exception e) {
                log.writeWarning("fOnAccept.BeginReceive() error, Error=" + e.Message);
                close(connection, true);
            }
            try {
                socket.BeginAccept(new System.AsyncCallback(fOnAccept), 100);
            } catch (Exception e) {
                log.writeError("fOnAccept.BeginAccept() error, Error=" + e.Message);
                return;
            }
        }
        private void fOnReceive(System.IAsyncResult aResult) {
            Connection connection = aResult.AsyncState as Connection;
            if (connection.socket == null) return;
            int bytesRead = 0;
            try {
                bytesRead = connection.socket.EndReceive(aResult);
            } catch (System.Net.Sockets.SocketException e) {
                if (e.ErrorCode == 10054) { //远程主机强迫关闭了一个现有的连接。
                } else {
                    log.writeWarning("fOnReceive.EndReceive() error, ErrorCode=" + e.ErrorCode + ",Error=" + e.Message);
                }
                close(connection, true);
                return;
            } catch (Exception e) {
                log.writeWarning("fOnReceive.EndReceive() error, Error=" + e.Message);
                close(connection, true);
                return;
            }
            if (bytesRead > 0) {
                connection.recvBuff.pushArray(bytesRead);
            } else {
                //log.writeWarning("fOnReceive.EndReceive() error, BytesRead=" + bytesRead); //由于对方断连也会收到一个0字节的包，所以本语句注释掉
                close(connection, true);
                return;
            }
            try {
                connection.socket.BeginReceive(connection.recvBuff.arrayValuePush, 0, connection.recvTempBuffSize, 0, new AsyncCallback(fOnReceive), connection);
            } catch (Exception e) {
                log.writeWarning("fOnReceive.BeginReceive() error, Error=" + e.Message);
                close(connection, true);
                return;
            }
            if (onReceive != null) onReceive(this, connection);
        }
        private void fOnSend(System.IAsyncResult aResult) {
            Connection connection = aResult.AsyncState as Connection;
            int bytesSend = 0;
            try {
                bytesSend = connection.socket.EndSend(aResult);
            } catch (Exception e) {
                log.writeWarning("fOnSend.EndSend() error, Error=" + e.Message);
                close(connection, true);
                return;
            }
            locker2.Lock();
            if (bytesSend != connection.sendBuff.arrayValuePop.Length) {
                log.writeWarning("实际发送的字节数有问题,bytesSend=" + bytesSend + ",BuffLength=" + connection.sendBuff.arrayValuePop.Length);
            }
            if (!connection.sendBuff.popArray(connection.sendTempBuffSize)) {
                if (onSendEnd != null) {
                    onSendEnd(this, connection);
                }
                connection.sendBuff.arrayValuePop = new byte[0];
                locker2.Unlock();
                return;
            }
            locker2.Unlock();
            try {
                connection.socket.BeginSend(connection.sendBuff.arrayValuePop, 0, connection.sendBuff.arrayValuePop.Length, 0, new AsyncCallback(fOnSend), connection);
            } catch (Exception e) {
                log.writeWarning("fOnSend.BeginSend() error, Error=" + e.Message);
                close(connection, true);
                return;
            }
        }
    }

    /// <summary>IP客户端控件,封装好的socket控件,所有函数全部为无阻塞模式,高层程序只要通过事件就可以通信</summary>
    public class IPClient {
        /// <summary>服务器地址，可以是IP或者域名</summary>
        public string hostAddress = "127.0.0.1";
        /// <summary>服务器端口</summary>
        public int hostPort = 0;
        /// <summary>用户指定的本地端口号，0表示由系统在发起连接后随机分配</summary>
        public int localPort = 0;
        /// <summary>实际由系统分配的最终本地端口号</summary>
        public int localPortReal = 0;
        public System.Net.Sockets.Socket socket = null;
        public MemoryLog log = new MemoryLog();
        private bool fConnected = false;
        public delegate void OnConnect(IPClient sender);
        public delegate void OnClose(IPClient sender);
        public delegate void OnReceive(IPClient sender);
        /// <summary>连接成功</summary>
        public event OnConnect onConnect = null;
        /// <summary>连接断开</summary>
        public event OnClose onClose = null;
        /// <summary>接收到数据</summary>
        public event OnReceive onReceive = null;
        private TamLocker locker1 = new TamLocker();  //用于锁定connected状态
        private TamLocker locker2 = new TamLocker();  //用于锁定sendBuff访问
        /// <summary>接收缓冲</summary>
        public TamPub1.StreamBuff recvBuff = new TamPub1.StreamBuff();
        /// <summary>发送缓冲</summary>
        public TamPub1.StreamBuff sendBuff = new TamPub1.StreamBuff();
        public int recvTempBuffSize = 1024 * 16;
        public int sendTempBuffSize = 10000;
        /// <summary>提供给高层程序使用的指针，用于存放高层数据</summary>
        public Object param1 = null;
        /// <summary>提供给高层程序使用的指针，用于存放高层数据</summary>
        public Object param2 = null;
        /// <summary>提供给高层程序使用的指针，用于存放高层数据</summary>
        public Object param3 = null;
        /// <summary>提供给高层程序使用的指针，用于存放高层数据</summary>
        public Object param4 = null;
        public IPClient() {
            recvBuff.arrayValuePush = new byte[recvTempBuffSize];
            sendBuff.arrayValuePop = new byte[0];
        }
        /// <summary>客户端目前与服务器的连接状态</summary>
        public bool connected {
            get { return fConnected; }
        }
        private class Connection {
            public bool suspend = false;
        }
        private Connection connection = new Connection();
        /// <summary>向服务器发起一个连接请求(有超时)</summary>
        public bool connect() {
            if ((socket != null) && (connected)) close(true);
            connection = new Connection();
            fConnected = false;
            recvBuff.clear();
            sendBuff.clear();
            try {
                socket = null;
                socket = new System.Net.Sockets.Socket(
                    System.Net.Sockets.AddressFamily.InterNetwork,
                    System.Net.Sockets.SocketType.Stream,
                    System.Net.Sockets.ProtocolType.Tcp
                );
                socket.Bind(new System.Net.IPEndPoint(System.Net.IPAddress.Any, localPort));
                localPortReal = (socket.LocalEndPoint as System.Net.IPEndPoint).Port;
                System.Net.IPEndPoint destEndPoint;
                System.Net.IPHostEntry destIP;
                try {
                    destEndPoint = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(hostAddress), hostPort); //合法IP则跳过域名解析过程
                } catch {
                    destIP = System.Net.Dns.GetHostEntry(hostAddress);
                    destEndPoint = new System.Net.IPEndPoint(destIP.AddressList[0], hostPort);
                }
                socket.BeginConnect(destEndPoint, new AsyncCallback(fOnConnect), connection);
            } catch (Exception e) {
                if (socket != null) {
                    socket.Close();
                }
                socket = null;
                log.writeWarning("无法初始化连接请求connectReq() error,HostAddress=" + hostAddress + ",Port=" + hostPort + ",Error=" + e.Message);
                return false;
            }
            return true;
        }
        public bool close(bool shutdownAtOnce) {
            connection.suspend = true;
            if (onClose != null) onClose(this);
            locker1.Lock();
            if (!connected) {
                locker1.Unlock();
                return true;
            }
            try {
                fConnected = false;
                if (!shutdownAtOnce) socket.Shutdown(System.Net.Sockets.SocketShutdown.Both);
                socket.Close();
            } catch { }
            socket = null;
            locker1.Unlock();
            return true;
        }
        /// <summary>发送byte[]形式的数据</summary>
        /// <param name="byteArray">需要发送的数据</param>
        /// <param name="bytesCount">发送数据的数量</param>
        public bool sendArray(byte[] byteArray, int bytesCount) {
            if (bytesCount > byteArray.Length) bytesCount = byteArray.Length;
            if (bytesCount <= 0) return false;
            //缓冲区处理
            locker2.Lock();
            sendBuff.arrayValuePush = byteArray;
            sendBuff.pushArray(bytesCount);
            if (sendBuff.arrayValuePop.Length > 0) {
                //正在有数据发送，退出
                locker2.Unlock();
                return true;
            }
            sendBuff.popArray(sendTempBuffSize);
            locker2.Unlock();
            try {
                socket.BeginSend(sendBuff.arrayValuePop, 0, sendBuff.arrayValuePop.Length, 0, new AsyncCallback(fOnSend), connection);
            } catch (Exception e) {
                log.writeWarning("sendArray.BeginSend() error, Error=" + e.Message);
                close(true);
                return false;
            }
            return true;
        }
        private void fOnSend(System.IAsyncResult aResult) {
            int bytesSend = 0;
            try {
                bytesSend = socket.EndSend(aResult);
            } catch (Exception e) {
                log.writeWarning("fOnSend.EndSend() error, Error=" + e.Message);
                close(true);
                return;
            }
            locker2.Lock();
            if (bytesSend != sendBuff.arrayValuePop.Length) {
                log.writeWarning("实际发送的字节数有问题,bytesSend=" + bytesSend + ",BuffLength=" + sendBuff.arrayValuePop.Length);
            }
            if (!sendBuff.popArray(sendTempBuffSize)) {
                sendBuff.arrayValuePop = new byte[0];
                locker2.Unlock();
                return;
            }
            locker2.Unlock();
            try {
                socket.BeginSend(sendBuff.arrayValuePop, 0, sendBuff.arrayValuePop.Length, 0, new AsyncCallback(fOnSend), connection);
            } catch (Exception e) {
                log.writeWarning("fOnSend.BeginSend() error, Error=" + e.Message);
                close(true);
                return;
            }
        }
        private void fOnConnect(System.IAsyncResult aResult) {
            try {
                socket.EndConnect(aResult);
            } catch (Exception e) {
                //连接超时会出现这类问题
                if (onClose != null) onClose(this);
                socket.Close();
                socket = null;
                log.writeWarning("fOnConnect.EndConnect() error,HostAddress=" + hostAddress + ",Port=" + hostPort + ",Error=" + e.Message);
                return;
            }
            fConnected = true;
            if (onConnect != null) onConnect(this);
            try {
                socket.BeginReceive(recvBuff.arrayValuePush, 0, recvTempBuffSize, 0, new AsyncCallback(fOnReceive), connection);
            } catch (Exception e) {
                log.writeWarning("fOnConnect.BeginReceive() error, Error=" + e.Message);
                close(true);
                return;
            }
        }
        private void fOnReceive(System.IAsyncResult aResult) {
            Connection con = aResult.AsyncState as Connection;
            if (con.suspend) return;
            int bytesRead = 0;
            try {
                bytesRead = socket.EndReceive(aResult);
            } catch (System.Net.Sockets.SocketException e) {
                if (e.ErrorCode == 10054) { //远程主机强迫关闭了一个现有的连接。
                } else {
                    log.writeWarning("fOnReceive.EndReceive() error, ErrorCode=" + e.ErrorCode + ",Error=" + e.Message);
                }
                close(true);
                return;
            } catch (Exception e) {
                log.writeWarning("fOnReceive.EndReceive() error, Error=" + e.Message);
                close(true);
                return;
            }
            if (bytesRead > 0) {
                recvBuff.pushArray(bytesRead);
            } else {
                log.writeWarning("fOnReceive.EndReceive() error, BytesRead=" + bytesRead);
                close(true);
                return;
            }
            if (onReceive != null) onReceive(this);
            try {
                socket.BeginReceive(recvBuff.arrayValuePush, 0, recvTempBuffSize, 0, new AsyncCallback(fOnReceive), connection);
            } catch (Exception e) {
                log.writeWarning("fOnReceive.BeginReceive() error, Error=" + e.Message);
                close(true);
                return;
            }
        }
    }

    /// <summary>专用加密与解密</summary>
    public class Encrypt {
        private static byte[] chaos = new byte[256] {
            119,58,12,255,163,60,69,84,138,2,155,27,183,62,46,13,235,
            52,114,21,42,193,135,129,38,127,177,66,147,216,37,240,185,
            19,49,15,134,188,181,215,67,139,137,126,5,248,89,144,201,
            239,184,197,196,205,40,112,115,123,150,71,74,156,93,85,168,
            208,78,87,169,103,180,4,92,195,151,241,191,207,28,43,182,
            192,221,70,133,179,90,141,190,102,47,236,166,118,244,6,113,
            8,94,57,63,226,105,131,88,9,199,1,250,242,109,189,125,121,
            232,142,204,213,223,107,14,222,91,230,77,124,212,95,83,249,
            34,55,111,251,41,54,178,35,33,243,20,96,237,210,214,145,101,
            48,198,80,29,162,104,117,149,68,36,45,17,227,247,22,143,173,
            175,206,73,153,164,160,116,100,186,218,110,132,53,254,64,220,
            224,97,202,253,136,86,238,122,154,25,18,148,23,61,246,194,16,
            0,24,39,50,10,11,146,108,157,99,217,170,200,75,209,211,82,203,
            128,3,229,152,140,176,159,161,233,171,165,98,172,56,79,32,120,
            44,158,219,231,245,167,7,51,59,65,76,106,81,228,31,72,130,30,
            225,26,174,234,252,187
        };
        //single looped 8bit chaos code table
        private static byte[] chaosSL = new byte[256] {
            //0    1    2    3    4    5    6    7    8    9    A    B    C    D    E    F
            0xD4,0xBF,0x91,0xEB,0x4E,0x37,0xDD,0x51,0x7D,0x43,0x1B,0x6C,0x59,0xAB,0x9B,0xEF, // 0
            0xD8,0x71,0x3B,0x94,0xD2,0xBA,0x9D,0xE7,0xE5,0x1F,0x85,0x44,0x1A,0x73,0x22,0xB4, // 1
            0x96,0x14,0xC0,0x16,0xE0,0x50,0x87,0x5F,0x0F,0xA0,0xEE,0x17,0xCA,0x8C,0x02,0xC7, // 2
            0x75,0xAD,0x6E,0x09,0x01,0xA2,0xED,0x54,0x49,0x4F,0x31,0x2B,0xBB,0xA5,0x15,0x1D, // 3
            0x20,0xC1,0x28,0x61,0x6F,0xB8,0xD9,0x4A,0x89,0x08,0x2C,0x90,0x88,0xF2,0x97,0x2A, // 4
            0x03,0x56,0x34,0x83,0x69,0x05,0x72,0x63,0x30,0x5D,0xDE,0x3D,0xB0,0x79,0x0C,0xB7, // 5
            0x00,0x18,0xD3,0x35,0x66,0xFF,0x8B,0xB9,0x21,0x57,0x55,0x11,0x95,0x04,0x36,0x93, // 6
            0xB3,0xB5,0xEA,0xD1,0x2F,0xCD,0xF6,0xDA,0xE2,0x4B,0x53,0xFE,0xAF,0x4C,0x9C,0x78, // 7
            0xDF,0x58,0x8A,0x3F,0x3A,0xAE,0x42,0xE8,0x6B,0x5C,0x2D,0x81,0xCB,0x13,0x1C,0x38, // 8
            0xD6,0xA7,0xE6,0xBD,0x12,0x52,0x41,0x47,0x6A,0xD5,0x10,0xA8,0x2E,0xF0,0x07,0x7A, // 9
            0xC3,0x86,0xDC,0xB1,0x06,0xC2,0xAC,0xA3,0xC6,0x27,0xF1,0x5E,0x60,0xF4,0x7F,0xCF, // A
            0xE1,0xFD,0xEC,0xF3,0xC4,0x33,0xDB,0xA4,0xC5,0xE4,0x25,0x8D,0x4D,0x48,0x9E,0xC8, // B
            0xF9,0x26,0x82,0x24,0xF7,0x7B,0x7E,0x65,0xF8,0xD7,0x84,0x9A,0xFC,0x3E,0x1E,0xD0, // C
            0x74,0x64,0xA1,0xFA,0x23,0x7C,0x76,0x68,0x92,0x0D,0xC9,0x3C,0x8E,0x98,0xBE,0x19, // D
            0x0B,0x39,0xCC,0xA6,0xB2,0x77,0x32,0x5B,0x6D,0x40,0xFB,0x45,0x9F,0xE3,0x70,0x0E, // E
            0xF5,0x0A,0x46,0x67,0xCE,0x80,0x8F,0x99,0xE9,0xB6,0xBC,0xAA,0x5A,0xA9,0x29,0x62  // F
            //0    1    2    3    4    5    6    7    8    9    A    B    C    D    E    F
        };

        /// <summary>
        ///                       S1            S2             SN
        /// Key->(Mod)->(chaos)->(+)->(chaos)->(+)->(chaos)...(+)->(chaos)-> Low
        ///                       M1            M2             MN
        ///              S1            S2            SN
        /// Key->(Mod)->(+)->(chaos)->(+)->(chaos)...(+)->(chaos)-> High
        /// </summary>
        /// <param name="buff">需要加密的数据</param>
        /// <param name="key">密钥</param>
        /// <returns>根据数据和密钥计算出的校验值</returns>
        public static UInt16 wordEncode(byte[] buff, UInt32 key) {
            int i, len;
            byte cLow, cHigh;
            cHigh = Convert.ToByte(key % 0x100);
            cLow = chaos[cHigh];
            len = buff.Length;
            for (i = 0; i < len; i++) {
                cHigh = Convert.ToByte(cHigh ^ buff[i]);
                cHigh = chaos[cHigh];
                cLow = Convert.ToByte(cLow ^ buff[i]);
                buff[i] = cLow;
                cLow = chaos[cLow];
            }
            return Convert.ToUInt16(cHigh * 0x100 + cLow);
        }

        /// <summary>
        ///                       M1\            M2\             SN\
        /// Key->(Mod)->(chaos)->(+) ->(chaos)->(+) ->(chaos)...(+) ->(chaos)-> Low
        ///                       S1            S2               MN
        ///              S1            S2            SN
        /// Key->(Mod)->(+)->(chaos)->(+)->(chaos)...(+)->(chaos)-> High
        /// </summary>
        /// <param name="buff">需要解密的数据</param>
        /// <param name="key">密钥</param>
        /// <returns>根据数据和密钥计算出的校验值</returns>
        public static UInt16 wordDecode(byte[] buff, UInt32 key) {
            int i, len;
            byte cLow, cHigh, mn;
            cHigh = Convert.ToByte(key % 0x100);
            cLow = chaos[cHigh];
            len = buff.Length;
            for (i = 0; i < len; i++) {
                mn = buff[i];
                buff[i] = Convert.ToByte(cLow ^ mn);
                cLow = chaos[mn];
                cHigh = Convert.ToByte(cHigh ^ buff[i]);
                cHigh = chaos[cHigh];
            }
            return Convert.ToUInt16(cHigh * 0x100 + cLow);
        }
        /// <summary>duplex word encode: 双向加密</summary>
        /// <param name="buff">需要加密的数据</param>
        /// <param name="key">密钥</param>
        /// <returns>根据数据和密钥计算出的校验值</returns>
        public static UInt16 dwordEncode(byte[] buff, UInt32 key) {
            return dwordEncode(buff, buff.Length, key);
        }
        public static UInt16 dwordEncode(byte[] buff, int length, UInt32 key) {
            int i, len;
            byte cLow, cHigh;
            cHigh = 0x00;
            cHigh = Convert.ToByte(cHigh ^ Convert.ToByte((key & 0xff000000) >> 24));
            cHigh = Convert.ToByte(cHigh ^ Convert.ToByte((key & 0x00ff0000) >> 16));
            cHigh = Convert.ToByte(cHigh ^ Convert.ToByte((key & 0x0000ff00) >> 8));
            cHigh = Convert.ToByte(cHigh ^ Convert.ToByte(key & 0x000000ff));
            cLow = chaosSL[cHigh];
            len = length;
            for (i = 0; i < len; i++) {
                cHigh = Convert.ToByte(cHigh ^ buff[i]);
                cHigh = chaosSL[cHigh];
                cLow = Convert.ToByte(cLow ^ buff[i]);
                buff[i] = cLow;
                cLow = chaosSL[cLow];
            }
            cHigh = 0x00;
            cHigh = Convert.ToByte(cHigh ^ Convert.ToByte((key & 0xff000000) >> 24));
            cHigh = Convert.ToByte(cHigh ^ Convert.ToByte((key & 0x00ff0000) >> 16));
            cHigh = Convert.ToByte(cHigh ^ Convert.ToByte((key & 0x0000ff00) >> 8));
            cHigh = Convert.ToByte(cHigh ^ Convert.ToByte(key & 0x000000ff));
            cLow = chaosSL[cHigh];
            for (i = len - 1; i >= 0; i--) {
                cHigh = Convert.ToByte(cHigh ^ buff[i]);
                cHigh = chaosSL[cHigh];
                cLow = Convert.ToByte(cLow ^ buff[i]);
                buff[i] = cLow;
                cLow = chaosSL[cLow];
            }
            return Convert.ToUInt16(cHigh * 0x100 + cLow);
        }
        /// <summary>duplex word decode: 双向解密</summary>
        /// <param name="buff">需要解密的数据</param>
        /// <param name="key">密钥</param>
        /// <returns>根据数据和密钥计算出的校验值</returns>
        public static UInt16 dwordDecode(byte[] buff, UInt32 key) {
            int i, len;
            byte cLow, cHigh, mn;
            UInt16 result;
            cHigh = 0x00;
            cHigh = Convert.ToByte(cHigh ^ Convert.ToByte((key & 0xff000000) >> 24));
            cHigh = Convert.ToByte(cHigh ^ Convert.ToByte((key & 0x00ff0000) >> 16));
            cHigh = Convert.ToByte(cHigh ^ Convert.ToByte((key & 0x0000ff00) >> 8));
            cHigh = Convert.ToByte(cHigh ^ Convert.ToByte(key & 0x000000ff));
            cLow = chaosSL[cHigh];
            len = buff.Length;
            for (i = len - 1; i >= 0; i--) {
                mn = buff[i];
                buff[i] = Convert.ToByte(cLow ^ mn);
                cLow = chaosSL[mn];
                cHigh = Convert.ToByte(cHigh ^ buff[i]);
                cHigh = chaosSL[cHigh];
            }
            result = Convert.ToUInt16(cHigh * 0x100 + cLow);

            cHigh = 0x00;
            cHigh = Convert.ToByte(cHigh ^ Convert.ToByte((key & 0xff000000) >> 24));
            cHigh = Convert.ToByte(cHigh ^ Convert.ToByte((key & 0x00ff0000) >> 16));
            cHigh = Convert.ToByte(cHigh ^ Convert.ToByte((key & 0x0000ff00) >> 8));
            cHigh = Convert.ToByte(cHigh ^ Convert.ToByte(key & 0x000000ff));
            cLow = chaosSL[cHigh];
            for (i = 0; i < len; i++) {
                mn = buff[i];
                buff[i] = Convert.ToByte(cLow ^ mn);
                cLow = chaosSL[mn];
                cHigh = Convert.ToByte(cHigh ^ buff[i]);
                cHigh = chaosSL[cHigh];
            }
            return result;
        }
    }

    /// <summary>byte[] 形式容器,可以放入整数、字符串等等, sp起始值为0</summary>
    public class ByteArrayContainer {
        public static byte readByte(byte[] buff, ref int sp) {
            if ((sp < 0) || (sp > buff.Length - 1)) return 0;
            sp++;
            return buff[sp - 1];
        }
        public static UInt16 readUInt16(byte[] buff, ref int sp) {
            UInt16 result = readByte(buff, ref sp);
            result = Convert.ToUInt16(result | (readByte(buff, ref sp) << 8));
            return result;
        }
        public static UInt32 readUInt32(byte[] buff, ref int sp) {
            UInt32 result = readUInt16(buff, ref sp);
            result = Convert.ToUInt32(result | (Convert.ToUInt32(readUInt16(buff, ref sp)) << 16));
            return result;
        }
        public static Int32 readInt32(byte[] buff, ref int sp) {
            unchecked {
                Int32 result = (Int32)readUInt32(buff, ref sp);
                return result;
            }
        }
        private static string readStringLen(Int32 len, byte[] buff, ref int sp) {
            if (len <= 0) return "";
            char[] tempBuff = new char[len];
            for (int i = 0; i < len; i++) {
                tempBuff[i] = Convert.ToChar(readUInt16(buff, ref sp));
            }
            string result = new string(tempBuff, 0, len);
            return result;
        }
        public static string readString(byte[] buff, ref int sp) {
            Int32 len = Convert.ToInt32(readByte(buff, ref sp));
            return readStringLen(len, buff, ref sp);
        }
        public static string readLongString(byte[] buff, ref int sp) {
            Int32 len = Convert.ToInt32(readUInt16(buff, ref sp));
            return readStringLen(len, buff, ref sp);
        }
        public static string readHugeString(byte[] buff, ref int sp) {
            Int32 len = readInt32(buff, ref sp);
            return readStringLen(len, buff, ref sp);
        }
        public static string readFixedString(int stringLength, byte[] buff, ref int sp) {
            return readStringLen(stringLength, buff, ref sp);
        }
        public static byte[] readBytes(int len, byte[] buff, ref int sp) {
            byte[] result = new byte[len];
            int i;
            for (i = 0; i < len; i++) {
                if (sp >= buff.Length) {
                    result[i] = 0;
                } else {
                    result[i] = buff[sp];
                    sp++;
                }
            }
            return result;
        }
        public static double readDouble(byte[] buff, ref int sp) {
            if (sp + 8 > buff.Length) return 0;
            sp += 8;
            return System.BitConverter.ToDouble(buff, sp - 8);
        }
        public static string readAscIIString(byte[] buff, ref int sp) {
            Int32 len = readByte(buff, ref sp);
            if (len <= 0) return "";
            char[] tempBuff = new char[len];
            for (int i = 0; i < len; i++) {
                tempBuff[i] = Convert.ToChar(readByte(buff, ref sp));
            }
            string result = new string(tempBuff, 0, len);
            return result;
        }
        private static int expandBuff(ref byte[] buff, int expandSize) {
            int realExpandSize = Convert.ToInt32(buff.Length * 0.1);
            if (realExpandSize < expandSize) realExpandSize = expandSize;
            if (realExpandSize < 50) realExpandSize = 50;
            byte[] newBuff = new byte[buff.Length + realExpandSize];
            for (int i = 0; i < buff.Length; i++) newBuff[i] = buff[i];
            buff = newBuff;
            return realExpandSize;
        }
        //新增函数会自动增加buff的大小，为了效率考虑，buff大小会比实际数据的大小要大一些，以免下次增加数据时重新分配内存
        //此时sp存放buff内数据的实际数量，buff.Length为buff的大小
        public static void appendByte(byte value, ref byte[] buff, ref int sp) {
            if (sp >= buff.Length) expandBuff(ref buff, 1);
            buff[sp] = value;
            sp++;
        }
        public static void appendUInt16(UInt16 value, ref byte[] buff, ref int sp) {
            appendByte(Convert.ToByte(value & 0xFF), ref buff, ref sp);
            appendByte(Convert.ToByte((value >> 8) & 0xFF), ref buff, ref sp);
        }
        public static void appendUInt32(UInt32 value, ref byte[] buff, ref int sp) {
            appendUInt16(Convert.ToUInt16(value & 0xFFFF), ref buff, ref sp);
            appendUInt16(Convert.ToUInt16((value >> 16) & 0xFFFF), ref buff, ref sp);
        }
        public static void appendInt32(Int32 value, ref byte[] buff, ref int sp) {
            unchecked {
                appendUInt32((UInt32)value, ref buff, ref sp);
            }
        }
        public static void appendString(string value, ref byte[] buff, ref int sp) {
            if (value.Length > 0xFF) value = value.Substring(0, 0xFF);
            appendByte(Convert.ToByte(value.Length), ref buff, ref sp);
            appendFixedString(value, ref buff, ref sp);
        }
        public static void appendLongString(string value, ref byte[] buff, ref int sp) {
            if (value.Length > 0xFFFF) value = value.Substring(0, 0xFFFF);
            appendUInt16(Convert.ToUInt16(value.Length), ref buff, ref sp);
            appendFixedString(value, ref buff, ref sp);
        }
        public static void appendHugeString(string value, ref byte[] buff, ref int sp) {
            appendInt32(value.Length, ref buff, ref sp);
            appendFixedString(value, ref buff, ref sp);
        }
        public static void appendFixedString(string value, ref byte[] buff, ref int sp) {
            int i;
            for (i = 0; i < value.Length; i++) {
                appendUInt16(Convert.ToUInt16(value[i]), ref buff, ref sp);
            }
        }
        public static void appendDouble(double value, ref byte[] buff, ref int sp) {
            byte[] doubleBuff = System.BitConverter.GetBytes(value);
            int i;
            for (i = 0; i < doubleBuff.Length; i++) {
                appendByte(doubleBuff[i], ref buff, ref sp);
            }
        }
        public static void appendAscIIString(string value, ref byte[] buff, ref int sp) {
            if (value.Length > 0xFF) value = value.Substring(0, 0xFF);
            appendByte(Convert.ToByte(value.Length), ref buff, ref sp);
            int i;
            for (i = 0; i < value.Length; i++) appendByte(Convert.ToByte(Convert.ToUInt16(value[i]) & 0xFF), ref buff, ref sp);
        }
        public static void appendBytes(byte[] value, ref byte[] buff, ref int sp) {
            int i;
            for (i = 0; i < value.Length; i++) {
                appendByte(value[i], ref buff, ref sp);
            }
        }
        //收缩buff至实际数据的大小
        public static void trimBuff(ref byte[] buff, int sp) {
            byte[] newBuff = new byte[sp];
            for (int i = 0; i < sp; i++) newBuff[i] = buff[i];
            buff = newBuff;
        }
    }

    /// <summary>
    /// 以类形式实现的byte[]形式封装数据结构，这种形式的封装在多线程情况下会出现问题，如果在多线程环境下应用次类请在外部自行加锁
    /// 本类采用高速设计，可以通过手动调整参数来适应以下几种情况：
    /// 1. 大量小数据: 每个数据都不大，但是非常多
    /// 2. 大量大数据: 每个数据都比较大，数量也比较多
    /// 3. 小量大数据: 每个数据都很大，但是数量不多
    /// 4. 小量小数据: 每个数据不大，数量也不多
    /// </summary>
    public class ByteArrayContainer2 {
        #region 辅助部分
        private byte[] buff = null;
        /// <summary>添加和读取的指针，如果要重复使用本类请调用reset()</summary>
        public int spWrite = 0;
        public int spRead = 0;

        /// <param name="initSize">初始预留的空间大小</param>
        public ByteArrayContainer2(int initSize) {
            buff = new byte[initSize];
        }
        public ByteArrayContainer2() {
            buff = new byte[64];
        }

        private void expandBuff() {
            int expandSize = buff.Length;
            if (expandSize < 128) expandSize = 128;
            byte[] newBuff = new byte[buff.Length + expandSize];
            for (int i = 0; i < buff.Length; i++) newBuff[i] = buff[i];
            buff = newBuff;
        }

        public void reset() {
            spWrite = 0;
            spRead = 0;
            spInsert = 0;
        }
        public void clear() {
            reset();
        }

        /// <summary>获取整个缓冲区</summary>
        public byte[] getBuff() {
            byte[] result = new byte[spWrite];
            int i;
            for (i = 0; i < spWrite; i++) {
                result[i] = buff[i];
            }
            return result;
        }

        public void setBuff(byte[] buff) {
            this.buff = buff;
            reset();
        }

        private static string CodeTable = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ*!";
        /// <summary>把缓冲区中的数据以可见英文字符的形式提取出来</summary>
        /// <returns>返回可见字符形式的缓冲区</returns>
        public string getBuffAsString() {
            System.Text.StringBuilder result = new System.Text.StringBuilder();
            appendByte(Convert.ToByte(' '));
            int BytePos = 0, BitPos = 0, DestCode;
            while (true) {
                if (BitPos == 0) {
                    DestCode = buff[BytePos] & 0x3F;
                } else if (BitPos <= 2) {
                    DestCode = (buff[BytePos] >> BitPos) & 0x3F;
                } else {
                    DestCode = buff[BytePos] >> BitPos;
                    DestCode = DestCode | ((buff[BytePos + 1] << (8 - BitPos)) & 0x3F);
                }
                result.Append(CodeTable[DestCode]);
                BitPos += 6;
                if (BitPos >= 8) {
                    BitPos -= 8;
                    BytePos++;
                    if (BytePos >= (spWrite - 1)) break;
                }
            }
            return result.ToString();
        }

        /// <summary>把可见字符表示的字串解码并形成缓冲区</summary>
        /// <param name="stringBuff">可见字符表示的字符串</param>
        /// <returns>正确解码返回true</returns>
        public bool setBuffAsString(string stringBuff) {
            System.Text.StringBuilder result = new System.Text.StringBuilder();
            reset();
            int BitPos = 0, DestCode = 0, Code6 = 0;
            for (int i = 0; i < stringBuff.Length; i++) {
                Code6 = CodeTable.IndexOf(stringBuff[i]);
                if (Code6 < 0) return false;
                if (BitPos == 0) {
                    DestCode = DestCode | Code6;
                    BitPos += 6;
                } else if (BitPos <= 2) {
                    DestCode = DestCode | (Code6 << BitPos);
                    BitPos = (BitPos + 6) % 8;
                    if (BitPos == 0) {
                        appendByte(Convert.ToByte(DestCode));
                        DestCode = 0;
                    }
                } else {
                    DestCode = DestCode | ((Code6 << BitPos) & 0xFF);
                    BitPos = BitPos - 2;
                    appendByte(Convert.ToByte(DestCode));
                    DestCode = Code6 >> (6 - BitPos);
                }
            }
            return true;
        }

        /// <summary>在写入状态下数据的实际长度</summary>
        public int length {
            get { return spWrite; }
        }
        #endregion

        #region read部分
        /// <summary>直接读取一个缓冲区，相当于流操作的模式</summary>
        /// <param name="readCount">读取最大的字节数量</param>
        /// <returns>返回的数组是由本函数自动分配的</returns>
        public byte[] readBuff(int readCount) {
            int count = readCount;
            if (spWrite - spRead > count) {
                count = spWrite - spRead;
            }
            byte[] result = new byte[count];
            int i = 0;
            while (i < count) {
                result[i] = buff[spRead];
                i++;
                spRead++;
            }
            return result;
        }
        /// <summary>直接读取一个缓冲区，本模式可以直接向高层程序分配的缓冲区中填数据，速度是最快的</summary>
        /// <param name="buff">由高层程序分配的缓冲区</param>
        /// <param name="startIndex">开始填充的位置 </param>
        /// <param name="maxCount">最大填充的字节数</param>
        /// <returns>返回实际填充的字节数，如果无数据则返回0</returns>
        public int readBuff(byte[] buff, int startIndex, int maxCount) {
            int result = maxCount;
            if (result > spWrite - spRead) result = spWrite - spRead;
            int i = startIndex;
            int readCount = 0;
            while (readCount < result) {
                buff[i] = this.buff[spRead];
                i++; spRead++; readCount++;
            }
            return result;
        }
        public byte readByte() {
            if ((spRead < 0) || (spRead > buff.Length - 1)) return 0;
            spRead++;
            return buff[spRead - 1];
        }
        public UInt16 readUInt16() {
            UInt16 result = readByte();
            result = Convert.ToUInt16(result | (readByte() << 8));
            return result;
        }
        public UInt16 readWord() {
            return readUInt16();
        }
        public UInt32 readUInt32() {
            UInt32 result = readUInt16();
            result = Convert.ToUInt32(result | (Convert.ToUInt32(readUInt16()) << 16));
            return result;
        }
        public Int32 readInt32() {
            unchecked {
                Int32 result = (Int32)readUInt32();
                return result;
            }
        }
        private string readStringLen(Int32 len) {
            if (len <= 0) return "";
            char[] tempBuff = new char[len];
            for (int i = 0; i < len; i++) {
                tempBuff[i] = Convert.ToChar(readUInt16());
            }
            string result = new string(tempBuff, 0, len);
            return result;
        }
        public string readString() {
            Int32 len = Convert.ToInt32(readByte());
            return readStringLen(len);
        }
        public string readLongString() {
            Int32 len = Convert.ToInt32(readUInt16());
            return readStringLen(len);
        }
        public string readHugeString() {
            Int32 len = readInt32();
            return readStringLen(len);
        }
        public string readFixedString(int stringLength) {
            return readStringLen(stringLength);
        }
        public double readDouble() {
            if (spRead + 8 > buff.Length) return 0;
            spRead += 8;
            return System.BitConverter.ToDouble(buff, spRead - 8);
        }
        public string readAscIIString() {
            Int32 len = readByte();
            if (len <= 0) return "";
            char[] tempBuff = new char[len];
            for (int i = 0; i < len; i++) {
                tempBuff[i] = Convert.ToChar(readByte());
            }
            string result = new string(tempBuff, 0, len);
            return result;
        }
        #endregion

        #region append部分
        public void appendByte(byte value) {
            if (spWrite >= buff.Length) expandBuff();
            buff[spWrite] = value;
            spWrite++;
            if (spWrite == 511) {
                Console.WriteLine(spWrite + "");
            }
        }
        /// <summary>向内部缓冲区直接写入一个数组</summary>
        public void appendBuff(byte[] buff) {
            int i;
            for (i = 0; i < buff.Length; i++) {
                appendByte(buff[i]);
            }
        }
        /// <summary>从数组中的某个位置取一段数组写入内部缓冲区</summary>
        /// <param name="buff">高层程序的缓冲区</param>
        /// <param name="startIndex">开始位置</param>
        /// <param name="count">长度</param>
        public void appendBuff(byte[] buff, int startIndex, int count) {
            int i;
            int endPos = startIndex + count;
            for (i = startIndex; i < endPos; i++) {
                appendByte(buff[i]);
            }
        }
        public void appendUInt16(UInt16 value) {
            appendByte(Convert.ToByte(value & 0xFF));
            appendByte(Convert.ToByte((value >> 8) & 0xFF));
        }
        public void appendWord(UInt16 value) {
            appendUInt16(value);
        }
        public void appendUInt32(UInt32 value) {
            appendUInt16(Convert.ToUInt16(value & 0xFFFF));
            appendUInt16(Convert.ToUInt16((value >> 16) & 0xFFFF));
        }
        public void appendInt32(Int32 value) {
            unchecked {
                appendUInt32((UInt32)value);
            }
        }
        public void appendInteger(Int32 value) {
            appendInt32(value);
        }
        public void appendString(string value) {
            if (value.Length > 0xFF) value = value.Substring(0, 0xFF);
            appendByte(Convert.ToByte(value.Length));
            appendFixedString(value);
        }
        public void appendLongString(string value) {
            if (value.Length > 0xFFFF) value = value.Substring(0, 0xFFFF);
            appendUInt16(Convert.ToUInt16(value.Length));
            appendFixedString(value);
        }
        public void appendHugeString(string value) {
            appendInt32(value.Length);
            appendFixedString(value);
        }
        public void appendFixedString(string value) {
            int i;
            for (i = 0; i < value.Length; i++) {
                appendUInt16(Convert.ToUInt16(value[i]));
            }
        }
        public void appendDouble(double value) {
            byte[] doubleBuff = System.BitConverter.GetBytes(value);
            int i;
            for (i = 0; i < doubleBuff.Length; i++) {
                appendByte(doubleBuff[i]);
            }
        }
        public void appendAscIIString(string value) {
            if (value.Length > 0xFF) value = value.Substring(0, 0xFF);
            appendByte(Convert.ToByte(value.Length));
            int i;
            for (i = 0; i < value.Length; i++) appendByte(Convert.ToByte(Convert.ToUInt16(value[i]) & 0xFF));
        }
        #endregion

        #region insert部分
        private int spInsert = 0;

        public int markInsertPos() {
            spInsert = spWrite;
            return spWrite;
        }
        public void markInsertPos(int index) {
            spInsert = index;
        }
        public void insertByte(byte value) {
            appendByte(value);
            int i;
            for (i = spWrite - 2; i >= spInsert; i--) {
                buff[i + 1] = buff[i];
            }
            buff[spInsert] = value;
            spInsert++;
        }
        public void insertUInt16(UInt16 value) {
            insertByte(Convert.ToByte(value & 0xFF));
            insertByte(Convert.ToByte((value >> 8) & 0xFF));
        }
        public void insertWord(UInt16 value) {
            insertUInt16(value);
        }
        public void insertUInt32(UInt32 value) {
            insertUInt16(Convert.ToUInt16(value & 0xFFFF));
            insertUInt16(Convert.ToUInt16((value >> 16) & 0xFFFF));
        }
        public void insertInt32(Int32 value) {
            unchecked {
                insertUInt32((UInt32)value);
            }
        }
        public void insertInteger(Int32 value) {
            insertInt32(value);
        }
        public void insertString(string value) {
            if (value.Length > 0xFF) value = value.Substring(0, 0xFF);
            insertByte(Convert.ToByte(value.Length));
            insertFixedString(value);
        }
        public void insertLongString(string value) {
            if (value.Length > 0xFFFF) value = value.Substring(0, 0xFFFF);
            insertUInt16(Convert.ToUInt16(value.Length));
            insertFixedString(value);
        }
        public void insertHugeString(string value) {
            insertInt32(value.Length);
            insertFixedString(value);
        }
        public void insertFixedString(string value) {
            int i;
            for (i = 0; i < value.Length; i++) {
                insertUInt16(Convert.ToUInt16(value[i]));
            }
        }
        public void insertDouble(double value) {
            byte[] doubleBuff = System.BitConverter.GetBytes(value);
            int i;
            for (i = 0; i < doubleBuff.Length; i++) {
                insertByte(doubleBuff[i]);
            }
        }
        public void insertAscIIString(string value) {
            if (value.Length > 0xFF) value = value.Substring(0, 0xFF);
            insertByte(Convert.ToByte(value.Length));
            int i;
            for (i = 0; i < value.Length; i++) insertByte(Convert.ToByte(Convert.ToUInt16(value[i]) & 0xFF));
        }
        #endregion
    }

    /// <summary>
    ///流数据打包和解包
    ///格式: header+length(加密数据长度)+CRC(2byte)+加密数据
    /// </summary>
    public class StreamPacket {
        private StreamBuff buff = new StreamBuff();
        public byte headerSmallPacket = 0x3A;  //255bytes
        public byte headerNormalPacket = 0x4B; //65535bytes
        public byte headerBigPacket = 0x5C;    //65536*65536-1bytes
        public UInt32 key = 0x74AB57A5;
        public byte[] packetReceive = new byte[0];
        public MemoryLog log = new MemoryLog();
        /// <summary>放入接收到的流数据</summary>
        /// <param name="stream">数据流</param>
        /// <param name="count">需要放入的数量</param>
        /// <returns>返回一般都成功，除非stream为空</returns>
        public bool pushStream(byte[] stream, int count) {
            buff.arrayValuePush = stream;
            return buff.pushArray(stream.Length);
        }
        public bool pushStream(byte[] stream) {
            return pushStream(stream, stream.Length);
        }
        /// <summary>
        /// 获取数据包，如果出现包失步等无法继续向下分析数据包的严重错误，则返回false，
        /// 返回true则需要判断packetReceive的长度来判断是否有新的包被解析出来
        /// </summary>
        public bool popPacket() {
            int nLost = 0;
            while (buff.viewArray(1)) {
                if (buff.arrayValuePop[0] == headerSmallPacket ||
                    buff.arrayValuePop[0] == headerNormalPacket ||
                    buff.arrayValuePop[0] == headerBigPacket) {
                    break;
                } else {
                    nLost++;
                    buff.popArray(1);
                }
            }
            if (nLost > 0) {
                log.writeError("包失步,丢失长度" + nLost);
            }
            if (!buff.viewArray(1)) {
                returnNull();
                return true;
            }

            int lenlen;
            if (buff.arrayValuePop[0] == headerSmallPacket) {
                lenlen = 1;
            } else if (buff.arrayValuePop[0] == headerNormalPacket) {
                lenlen = 2;
            } else if (buff.arrayValuePop[0] == headerBigPacket) {
                lenlen = 4;
            } else {
                return false;  //包失步,应该不会出现这种情况.
            }
            if (!buff.viewArray(3 + lenlen)) {
                returnNull();
                return true;
            }
            //读取包头
            int packetLen = 0;
            UInt16 packetCRC = 0;
            int sp = 1;
            switch (lenlen) {
                case 1: packetLen = ByteArrayContainer.readByte(buff.arrayValuePop, ref sp); break;
                case 2: packetLen = ByteArrayContainer.readUInt16(buff.arrayValuePop, ref sp); break;
                case 4: packetLen = ByteArrayContainer.readInt32(buff.arrayValuePop, ref sp); break;
            }
            packetCRC = ByteArrayContainer.readUInt16(buff.arrayValuePop, ref sp);
            if (buff.count < packetLen + 3 + lenlen) {
                returnNull();
                return true;
            }
            //读取包内容
            buff.popNone(lenlen + 3);
            buff.popArray(packetLen);
            //校验
            if (packetCRC != Encrypt.dwordDecode(buff.arrayValuePop, key)) return false;
            packetReceive = buff.arrayValuePop;
            return true;
        }
        public void encodePacket(ref byte[] buff) {
            encodePacket(ref buff, buff.Length);
        }
        public void encodePacket(ref byte[] buff, int length) {
            byte[] newBuff = null;
            int sp = 0, lenlen;
            if (length <= 0xFF) {
                lenlen = 1;
            } else if (length <= 0xFFFF) {
                lenlen = 2;
            } else {
                lenlen = 4;
            }
            newBuff = new byte[length + lenlen + 3];
            switch (lenlen) {
                case 1:
                    ByteArrayContainer.appendByte(headerSmallPacket, ref newBuff, ref sp);
                    ByteArrayContainer.appendByte(Convert.ToByte(length), ref newBuff, ref sp);
                    break;
                case 2:
                    ByteArrayContainer.appendByte(headerNormalPacket, ref newBuff, ref sp);
                    ByteArrayContainer.appendUInt16(Convert.ToUInt16(length), ref newBuff, ref sp);
                    break;
                case 4:
                    ByteArrayContainer.appendByte(headerBigPacket, ref newBuff, ref sp);
                    ByteArrayContainer.appendInt32(length, ref newBuff, ref sp);
                    break;
            }
            UInt16 packetCRC = Encrypt.dwordEncode(buff, length, key);
            ByteArrayContainer.appendUInt16(packetCRC, ref newBuff, ref sp);
            for (int i = 0; i < length; i++) {
                newBuff[i + lenlen + 3] = buff[i];
            }
            buff = newBuff;
        }
        private void returnNull() {
            if (packetReceive.Length > 0) packetReceive = new byte[0];
        }
        public void clear() {
            buff.clear();
            buff.trim();
            packetReceive = new byte[0];
        }

    }

    /// <summary>基于Xml格式的配置文件读写</summary>
    public class ConfigFileXml {
        public static string rootName = "root";
        private static bool checkFile(string filename) {
            if (System.IO.File.Exists(filename)) return true;
            try {
                System.Xml.XmlTextWriter writer = new System.Xml.XmlTextWriter(filename, null);
                writer.Formatting = System.Xml.Formatting.Indented;
                writer.WriteStartDocument(true);
                writer.WriteStartElement(rootName);
                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Flush();
                writer.Close();
            } catch {
                return false;
            }
            return true;
        }
        /// <summary>检测并创建Key</summary>
        /// <returns>-1:error; 0:success; 1:newkey</returns>
        public static int checkKey(string filename, string key) {
            System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.Load(filename);
            System.Xml.XmlNode node = null;
            System.Xml.XmlNode lastNode = xmlDoc.SelectSingleNode(rootName);
            if (lastNode == null) return -1;
            System.Xml.XmlElement newNode = null;
            int sp = 0, sp1, result = 0;
            string nodeName = "";
            while (true) {
                if (sp >= key.Length) break;
                sp1 = key.IndexOf('/', sp);
                if (sp1 < 0) {
                    sp1 = key.Length;
                }
                nodeName = key.Substring(sp, sp1 - sp);
                node = xmlDoc.SelectSingleNode(rootName + "/" + key.Substring(0, sp1));
                if (node == null) {
                    newNode = xmlDoc.CreateElement(nodeName);
                    lastNode.AppendChild(newNode);
                    node = newNode;
                    result = 1;
                }
                lastNode = node;
                sp = sp1 + 1;
            }
            xmlDoc.Save(filename);
            return result;
        }
        /// <summary>按照路径创建节点，如果节点已经存在则不创建</summary>
        /// <returns>-1:error; 0:success; 1:newkey</returns>
        public static System.Xml.XmlNode createPath(System.Xml.XmlDocument xmlDoc, string path) {
            System.Xml.XmlNode node = null, lastNode = null;
            path = StringFunc.deleteFrontChar(path, '/');
            path = StringFunc.deleteRearChar(path, '/');
            string[] p = path.Split('/');
            if (p.Length <= 0) return null;
            path = "";
            for (int i = 0; i < p.Length; i++) {
                path += "/" + p[i];
                node = xmlDoc.SelectSingleNode(path);
                if (node == null) {
                    node = xmlDoc.CreateElement(p[i]);
                    if (lastNode == null) {
                        xmlDoc.AppendChild(node);
                    } else {
                        lastNode.AppendChild(node);
                    }
                }
                lastNode = node;
            }
            return node;
        }
        public static bool writeString(string filename, string key, string value) {
            if (!checkFile(filename)) return false;
            if (checkKey(filename, key) < 0) return false;
            System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.Load(filename);
            System.Xml.XmlNode node = xmlDoc.SelectSingleNode(rootName + "/" + key);
            if (node == null) return false;
            node.InnerText = value;
            xmlDoc.Save(filename);
            return true;
        }
        public static bool writeString(string filename, string key, string subKey, string value) {
            if (!checkFile(filename)) return false;
            if (checkKey(filename, key) < 0) return false;
            System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.Load(filename);
            System.Xml.XmlNode node = xmlDoc.SelectSingleNode(rootName + "/" + key);
            if (node == null) return false;
            (node as System.Xml.XmlElement).SetAttribute(subKey, value);
            xmlDoc.Save(filename);
            return true;
        }
        public static bool writeInt32(string filename, string key, Int32 value) {
            return writeString(filename, key, Convert.ToString(value));
        }
        public static bool writeInt32(string filename, string key, string subKey, Int32 value) {
            return writeString(filename, key, subKey, Convert.ToString(value));
        }
        public static string readString(string filename, string key, string defaultValue) {
            if (!checkFile(filename)) return defaultValue;
            int ckResult = checkKey(filename, key);
            if (ckResult < 0) return defaultValue;
            System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.Load(filename);
            System.Xml.XmlNode node = xmlDoc.SelectSingleNode(rootName + "/" + key);
            if (node == null) return "";
            if (ckResult == 1) node.InnerText = defaultValue;
            string result = node.InnerText;
            xmlDoc.Save(filename);
            return result;
        }
        public static string readString(string filename, string key, string subKey, string defaultValue) {
            if (!checkFile(filename)) return defaultValue;
            if (checkKey(filename, key) < 0) return defaultValue;
            System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.Load(filename);
            System.Xml.XmlNode node = xmlDoc.SelectSingleNode(rootName + "/" + key);
            string result = readString(node, subKey, defaultValue);
            xmlDoc.Save(filename);
            return result;
        }
        public static Int32 readInt32(string filename, string key, Int32 defaultValue) {
            Int32 result;
            try {
                result = Convert.ToInt32(readString(filename, key, Convert.ToString(defaultValue)));
            } catch {
                result = 0;
            }
            return result;
        }
        public static Int32 readInt32(string filename, string key, string subKey, Int32 defaultValue) {
            Int32 result;
            try {
                result = Convert.ToInt32(readString(filename, key, subKey, Convert.ToString(defaultValue)));
            } catch {
                result = 0;
            }
            return result;
        }
        #region 根据XmlNode进行存取操作部分
        /// <summary>
        /// 自动创建项目（如果项目不存在的话）
        /// </summary>
        public static System.Xml.XmlNode create(System.Xml.XmlDocument doc, String key) {
            System.Xml.XmlNode node = null;
            System.Xml.XmlElement element = null;
            key = TamPub1.StringFunc.deleteFrontChar(key, '/');
            key = TamPub1.StringFunc.deleteRearChar(key, '/');
            string[] s = key.Split('/');
            string path;
            for (int i = 0; i < s.Length; i++) {
                path = "";
                for (int j = 0; j <= i; j++) {
                    path = path + "/" + s[j];
                }
                if (doc.SelectSingleNode(path) != null) {
                    node = doc.SelectSingleNode(path);
                    continue;
                }
                element = doc.CreateElement(s[i]);
                if (i == 0) {
                    node = doc.AppendChild(element);
                } else {
                    node = node.AppendChild(element);
                }
            }
            return node;
        }
        /// <summary>
        /// 在node下创建一个子节点，子节名字允许重复
        /// </summary>
        public static System.Xml.XmlNode create(System.Xml.XmlDocument doc, System.Xml.XmlNode node, String key) {
            System.Xml.XmlElement element = null;
            element = doc.CreateElement(key);
            return node.AppendChild(element);
        }
        public static string readString(System.Xml.XmlNode node, string defaultValue) {
            if (node == null) return "";
            string result;
            if (TamPub1.StringFunc.copy(node.InnerXml, 0, "<![CDATA[".Length).Equals("<![CDATA[")) {
                result = node.InnerText.Replace("&amp;", "&");
            } else {
                result = node.InnerText;
            }
            return result;
        }
        public static string readString(System.Xml.XmlNode node, string subKey, string defaultValue) {
            if (node == null) return "";
            if ((node as System.Xml.XmlElement).Attributes.GetNamedItem(subKey) == null) {
                (node as System.Xml.XmlElement).SetAttribute(subKey, defaultValue);
            }
            string result = (node as System.Xml.XmlElement).GetAttribute(subKey);
            return result;
        }
        public static bool writeString(System.Xml.XmlNode node, string value) {
            if (node == null) return false;
            node.InnerText = value;
            return true;
        }
        public static bool writeString(System.Xml.XmlNode node, string subKey, string value) {
            if (node == null) return false;
            (node as System.Xml.XmlElement).SetAttribute(subKey, value);
            return true;
        }
        public static Int32 readInt32(System.Xml.XmlNode node, Int32 defaultValue) {
            Int32 result;
            try {
                result = Convert.ToInt32(readString(node, Convert.ToString(defaultValue)));
            } catch {
                result = defaultValue;
            }
            return result;
        }
        public static Int32 readInt32(System.Xml.XmlNode node, string subKey, Int32 defaultValue) {
            Int32 result;
            try {
                result = Convert.ToInt32(readString(node, subKey, Convert.ToString(defaultValue)));
            } catch {
                result = defaultValue;
            }
            return result;
        }
        public static bool writeInt32(System.Xml.XmlNode node, Int32 value) {
            return writeString(node, Convert.ToString(value));
        }
        public static bool writeInt32(System.Xml.XmlNode node, string subKey, Int32 value) {
            return writeString(node, subKey, Convert.ToString(value));
        }
        public static Int32 readHex(System.Xml.XmlNode node, Int32 defaultValue) {
            Int32 result;
            try {
                result = Convert.ToInt32(readString(node, Convert.ToString(defaultValue)), 16);
            } catch {
                result = defaultValue;
            }
            return result;
        }
        public static Int32 readHex(System.Xml.XmlNode node, string subKey, Int32 defaultValue) {
            Int32 result;
            try {
                result = Convert.ToInt32(readString(node, subKey, Convert.ToString(defaultValue)), 16);
            } catch {
                result = defaultValue;
            }
            return result;
        }
        public static bool writeHex(System.Xml.XmlNode node, Int32 value) {
            return writeString(node, String.Format("{0:X}", value));
        }
        public static bool writeHex(System.Xml.XmlNode node, string subKey, Int32 value) {
            return writeString(node, subKey, String.Format("{0:X}", value));
        }
        public static void appendAttr(System.Xml.XmlDocument doc, System.Xml.XmlNode node, string key, string value) {
            System.Xml.XmlAttribute xmlAttr = doc.CreateAttribute(key);
            xmlAttr.Value = value;
            node.Attributes.Append(xmlAttr);
        }
        /// <summary>把一个普通字串转换为CData形式的数据</summary>
        public static string getCData(string value) {
            return "<![CDATA[" + value.Replace("]]>", "]]]]><![CDATA[>") + "]]>";
        }
        public static string getCDataText(string value) {
            return getCData(value.Replace("\r\n", "\n").Replace("\n", "\r\n"));
        }
        /// <summary>自动判断是否有产生CData形式数据的必要</summary>
        public static string getDDataTextAuto(string value) {
            value = value.Replace("&", "&amp;");
            if (value.IndexOf('<') >= 0) {
                return getCDataText(value);
            } else if (value.IndexOf('>') >= 0) {
                return getCDataText(value);
            } else {
                return value;
            }
        }
        #endregion
    }

    /// <summary>数据库连接类</summary>
    public class SqlServerConnection {
        public MemoryLog log = new MemoryLog();
        public DiskLog diskLog = null;
        public static string connectionString = "";
        public System.Data.SqlClient.SqlConnection sqlConnection = new System.Data.SqlClient.SqlConnection();
        public System.Data.SqlClient.SqlCommand sqlCommand = new System.Data.SqlClient.SqlCommand();
        public System.Data.SqlClient.SqlDataReader sqlResult;
        private int lastOperation = 0;
        //private TamLocker locker = new TamLocker();
        private object lockObj = new object();
        private bool Busy = false;
        private DateTime StartTime = DateTime.Now;
        private void Lock() {
            lock (lockObj) {
                while (true) {
                    //searching for free device
                    if (!Busy) {
                        Busy = true;
                        StartTime = DateTime.Now;
                        break;
                    } else if ((DateTime.Now - StartTime).Milliseconds >= 5000) {
                        if (lastOperation == 1) sqlResult.Close();
                        lastOperation = 0;
                        diskLog.writeError("SQL语句执行超时,SQL=" + sqlCommand.CommandText, true);
                        Busy = true;
                        StartTime = DateTime.Now;
                        break;
                    }
                    System.Threading.Thread.Sleep(100);
                }
            }
        }
        public SqlServerConnection() {
            if (diskLog == null) {
                diskLog = new DiskLog();
                diskLog.filename = "D:\\www\\Map\\LogFile"; //Application.StartupPath + "\\QueryServerConnection";
            }
        }
        public Boolean executeQuery(string sqlString) {
            return executeQuery(sqlString, false);
        }
        /// <summary>执行SQL语句</summary>
        /// <param name="sqlString">SQL语句</param>
        /// <param name="hasResult">本SQL语句是否有返回的结果集，如果有，在操作完结果集后需要执行close()</param>
        /// <returns>是否成功</returns>
        public Boolean executeQuery(string sqlString, Boolean hasResult) {
            //locker.Lock();
            Lock();
            lastOperation = 0;
            if (!checkConnnection()) return false;
            sqlCommand.CommandText = sqlString;
            try {
                if (hasResult) {
                    lastOperation = 1;
                    sqlResult = null;
                    sqlResult = sqlCommand.ExecuteReader();
                } else {
                    lastOperation = 2;
                    sqlCommand.ExecuteNonQuery();
                }
            } catch (Exception e) {
                diskLog.writeError("无法执行SQL语句,Error=" + e.Message + ",SQL=" + sqlString, true);
                close();
                sqlConnection.Close();
                return false;
            }
            if (!hasResult) close();
            return true;
        }
        public void close() {
            if (lastOperation == 1) {
                if (sqlResult != null) {
                    sqlResult.Close();
                    sqlResult = null;
                }
            }
            lastOperation = 0;
            //locker.Unlock();
            double t = System.DateTime.Now.Subtract(StartTime).TotalMilliseconds;
            if (t >= 30) {
                diskLog.writeError("sql执行时间过长,time=" + t + "ms" +
                    ", sql=" + sqlCommand.CommandText, true
                );
            } else {
                diskLog.writeCommon("sql执行完成,time=" + t + "ms" +
                    ", sql=" + sqlCommand.CommandText, true
                );
            }
            Busy = false;
        }
        public Boolean checkConnnection() {
            if (sqlConnection.State != System.Data.ConnectionState.Open) {
                sqlConnection.ConnectionString = connectionString;
                try {
                    sqlConnection.Open();
                } catch (Exception e) {
                    log.writeError("无法连接数据库,Error=" + e.Message);
                    close();
                    return false;
                }
            }
            if (lastOperation == 1) {
                sqlResult.Close();
                lastOperation = 0;
            }
            sqlCommand.Connection = sqlConnection;
            return true;
        }
        //以下函数为方便取结果用，里面会报异常，需要高层程序抓取
        public string getString(string fieldName) {
            if (isNull(fieldName)) return "";
            return sqlResult.GetString(sqlResult.GetOrdinal(fieldName));
        }
        public Int32 getInt32(string fieldName) {
            if (isNull(fieldName)) return 0;
            return sqlResult.GetInt32(sqlResult.GetOrdinal(fieldName));
        }
        public DateTime getDateTime(string fieldName) {
            return sqlResult.GetDateTime(sqlResult.GetOrdinal(fieldName));
        }
        public double getDouble(string fieldName) {
            if (isNull(fieldName)) return 0;
            return sqlResult.GetDouble(sqlResult.GetOrdinal(fieldName));
        }
        public bool isNull(string fieldName) {
            return sqlResult.IsDBNull(sqlResult.GetOrdinal(fieldName));
        }
    }

    /// <summary>堆排序算法,需要给buff，onCompare赋值才能使用</summary>
    public class HeapSort {
        public System.Collections.ArrayList buff = new System.Collections.ArrayList();
        public delegate bool OnCompare(object value1, object value2);
        /// <summary>如果value1>value2返回true,则结果返回递增序列</summary>
        public OnCompare onCompare = null;
        private object temp = null;
        private int heapSize = 0;
        /// <summary>开始排序过程</summary>
        public bool sort() {
            if (buff == null) return false;
            if (buff.Count <= 0) return false; ;
            if (onCompare == null) return false;
            buildHeap();
            popAll();
            return true;
        }
        private void buildHeap() {
            int p;
            heapSize = 0;
            while (heapSize < buff.Count) {
                p = heapSize;
                temp = buff[p];
                while (onCompare(temp, buff[p / 2]) && (p > 0)) {
                    buff[p] = buff[p / 2];
                    p = p / 2;
                }
                buff[p] = temp;
                heapSize++;
            }
        }
        private void heapfy(int index) {
            int lIndex, rIndex, swapIndex;
            lIndex = 2 * index;
            rIndex = lIndex + 1;
            swapIndex = index;
            if (lIndex < heapSize) {
                if (onCompare(buff[lIndex], buff[index])) swapIndex = lIndex;
            }
            if (rIndex < heapSize) {
                if (onCompare(buff[rIndex], buff[swapIndex])) swapIndex = rIndex;
            }
            if (index != swapIndex) {
                temp = buff[index];
                buff[index] = buff[swapIndex];
                buff[swapIndex] = temp;
                heapfy(swapIndex);
            }
        }
        private void popAll() {
            while (heapSize > 1) {
                temp = buff[0];
                buff[0] = buff[heapSize - 1];
                buff[heapSize - 1] = temp;
                heapSize--;
                heapfy(0);
            }
        }
    }

    /// <summary>二分法查找,必须指定buff和onCompare才能使用</summary>
    public class SpeedSearch {
        public System.Collections.ArrayList buff = null;
        public delegate int OnCompare(object value1, object value2);
        //value1>value2返回1则表示buff中元素按升序列排序,在这种情况下value1>value2返回1,value1<value2返回-1,value1=value2返回0
        public OnCompare onCompare = null;
        public int pos = 0;
        public bool search(object value) {
            pos = 0;
            if (buff == null) return false;
            if (buff.Count <= 0) {
                pos = 0;
                return false;
            }
            if (onCompare == null) return false;
            int loIndex, hiIndex, cIndex, compResult;
            loIndex = 0; hiIndex = buff.Count - 1;
            while (true) {
                if (loIndex > hiIndex) break;
                cIndex = (loIndex + hiIndex) / 2;
                compResult = onCompare(buff[cIndex], value);
                if (compResult > 0) {
                    hiIndex = cIndex - 1;
                } else if (compResult < 0) {
                    loIndex = cIndex + 1;
                } else {
                    pos = cIndex;
                    return true;
                }
            }
            pos = loIndex;
            return false;
        }
    }

    /// <summary>二分法查找V2版本,必须指定onCompare才能使用</summary>
    public class SpeedSearch<T> {
        public List<T> buff = new List<T>();
        public delegate int OnCompare(T value1, T value2);
        /// <summary>value1>value2返回1则表示buff中元素按升序列排序,在这种情况下value1>value2返回1,value1<value2返回-1,value1=value2返回0</summary>
        public OnCompare onCompare = null;
        public int pos = 0;
        /// <summary>如果没找到，（如果是升序）pos存放第一个大于此值的位置</summary>
        public bool search(T value) {
            pos = 0;
            if (buff == null) return false;
            if (buff.Count <= 0) {
                pos = 0;
                return false;
            }
            if (onCompare == null) return false;
            int loIndex, hiIndex, cIndex, compResult;
            loIndex = 0; hiIndex = buff.Count - 1;
            while (true) {
                if (loIndex > hiIndex) break;
                cIndex = (loIndex + hiIndex) / 2;
                compResult = onCompare(buff[cIndex], value);
                if (compResult > 0) {
                    hiIndex = cIndex - 1;
                } else if (compResult < 0) {
                    loIndex = cIndex + 1;
                } else {
                    pos = cIndex;
                    return true;
                }
            }
            pos = loIndex;
            return false;
        }
    }

    /// <summary>
    /// 古老的LCG(linear congruential generator)代表了最好的伪随机数产生器算法。主要原因是容易理解，容易实现，而且速度快。这种算法数学上基于X(n+1) = (a * X(n) + c) % m这样的公式，其中：
    /// 模m, m > 0
    /// 系数a, 0 < a < m
    /// 增量c, 0 <= c < m
    /// 原始值(种子) 0 <= X(0) < m
    /// 其中参数c, m, a比较敏感，或者说直接影响了伪随机数产生的质量。
    /// 一般而言，高LCG的m是2的指数次幂(一般2^32或者2^64)，因为这样取模操作截断最右的32或64位就可以了。多数编译器的库中使用了该理论实现其伪随机数发生器rand()。下面是部分编译器使用的各个参数值：
    /// Source                 m            a            c          rand() / Random(L)的种子位
    /// Numerical Recipes                  
    ///                        2^32         1664525      1013904223    
    /// Borland C/C++                      
    ///                        2^32         22695477     1          位30..16 in rand(), 30..0 in lrand()
    /// glibc (used by GCC)                
    ///                        2^32         1103515245   12345      位30..0
    /// ANSI C: Watcom, Digital Mars, CodeWarrior, IBM VisualAge C/C++
    ///                        2^32         1103515245   12345      位30..16
    /// Borland Delphi, Virtual Pascal
    ///                        2^32         134775813    1          位63..32 of (seed * L)
    /// Microsoft Visual/Quick C/C++
    ///                        2^32         214013       2531011    位30..16
    /// Apple CarbonLib
    ///                        2^31-1       16807        0          见Park–Miller随机数发生器
    /// </summary>
    public class TamRandom {
        public UInt32 seed;
        /// <summary> 可以指定seed, seed是4字节的整数 </summary>
        /// <param name="seed"></param>
        public TamRandom(UInt32 seed) {
            this.seed = seed;
            algorithm = ALGORITHM_AnsiC;
        }
        public TamRandom() {
            Random rand = new Random();
            seed = Convert.ToUInt32(rand.Next());
            algorithm = ALGORITHM_AnsiC;
        }

        public const int ALGORITHM_NumericalRecipes = 10;
        public const int ALGORITHM_BorlandCplusplus = 20;
        public const int ALGORITHM_Glibc = 30;
        public const int ALGORITHM_AnsiC = 40;
        public const int ALGORITHM_Delphi = 50;
        public const int ALGORITHM_VisualCplusplus = 60;
        public const int ALGORITHM_AppleCarbonLib = 70;
        private int falgorithm = 0;
        /// <summary> 随机数算法，默认为ansi C </summary>
        public int algorithm {
            set {
                switch (value) {
                    case ALGORITHM_NumericalRecipes: a = 1664525; c = 1013904223; break;
                    case ALGORITHM_BorlandCplusplus: a = 22695477; c = 1; break;
                    case ALGORITHM_Glibc: a = 1103515245; c = 12345; break;
                    case ALGORITHM_AnsiC: a = 1103515245; c = 12345; break;
                    case ALGORITHM_Delphi: a = 134775813; c = 1; break;
                    case ALGORITHM_VisualCplusplus: a = 214013; c = 2531011; break;
                    case ALGORITHM_AppleCarbonLib: a = 16807; c = 0; break;
                    default: a = 1103515245; c = 12345; break; //delphi
                }
                falgorithm = value;
            }
            get {
                return falgorithm;
            }
        }

        /// <summary> 产生一个正的随机整数 </summary>
        public int next() {
            nextSeed();
            return Convert.ToInt32(seed & 0x7FFFFFFF);
        }

        /// <summary> 产生一个UInt32的随机数 </summary>
        public UInt32 nextUInt32() {
            nextSeed();
            return seed;
        }

        /// <summary> 产生一个minValue到maxValue范围的随机数，随机数可能是minValue,但不可能是maxValue </summary>
        public int next(int minValue, int maxValue) {
            nextSeed();
            return Convert.ToInt32(((seed >> 16) % (maxValue - minValue)) + minValue);
        }

        /// <summary> 产生一个0到maxValue范围的随机数，随机数可能是0,但不可能是maxValue </summary>
        public int next(int maxValue) {
            return next(0, maxValue);
        }

        /// <summary> 产生一个UInt32的随机数 </summary>
        public double nextDouble() {
            return next() / Convert.ToDouble(0x80000000);
        }

        /** 根据value概率判断是否成功，比如value=30, 则本函数有30%的概率返回true, 本函数的精度为1万分之1 */
        public bool percent(double value) {
            return next(0, 10000) < Math.Round(value * 100);
        }

        private UInt32 a;
        private UInt32 c;
        private void nextSeed() {
            //Console.WriteLine(seed + "*" + a + "=" + ((Convert.ToUInt64(seed) * a)) + "," + ((Convert.ToUInt64(seed) * a) + c));
            seed = Convert.ToUInt32(((Convert.ToUInt64(seed) * a)) & 0xFFFFFF00);
            seed = seed + c;
            //Console.WriteLine("Seed=" + seed);
        }
    }


    /// <summary> 数值状态类，主要方便高层程序捕捉某个整数值的跳变状态 </summary>
    public class TamStatus {
        public TamStatus(float initValue) {
            queue = new Queue();
            this.initValue(initValue);
        }
        public TamStatus() {
            queue = new Queue();
            this.initValue(0);
        }

        /// <summary>上次状态改变的时间</summary>
        public DateTime lastChangeTime = DateTime.Now;
        private Queue queue = null;

        /// <summary>初始化当前值，本函数不会导致跳变状态的发生</summary>
        /// <param name="value">初始值</param>
        public void initValue(float value) {
            fvalue = value;
            queue.clear();
        }
        public float fvalue = -1;

        /// <summary>当前值</summary>
        public float value {
            get {
                return fvalue;
            }
            set {
                if (value != fvalue) {
                    queue.push(new TamStatusItem(fvalue, value));
                    lastChangeTime = DateTime.Now;
                }
                fvalue = value;
            }
        }
        /// <summary>上次状态改变到当前的毫秒差</summary>
        public int lastChangeMS {
            get {
                return Convert.ToInt32(DateTime.Now.Subtract(lastChangeTime).TotalMilliseconds);
            }
        }
        /// <summary>当前变化的情况,需要通过getChange获取</summary>
        public TamStatusItem change = null;
        /// <summary>获取变化的情况，如果没有变化change和返回值都为空</summary>
        /// <returns>变化结果</returns>
        public TamStatusItem getChange() {
            if (!queue.pop()) {
                change = null;
            } else {
                change = queue.valuePop as TamStatusItem;
            }
            return change;
        }
    }

    /// <summary>
    /// 状态跳变的历史记录
    /// </summary>
    public class TamStatusItem {
        public float oldValue;
        public float newValue;
        public TamStatusItem(float oldValue, float newValue) {
            this.oldValue = oldValue;
            this.newValue = newValue;
        }
    }

    /// <summary>
    /// 用以存贮用户数据，方便其他类使用，比如用户使用某个类以后需要向里面添加一些随类走的相关数据，这个类就可以开个userData的结构来放用户数据
    /// </summary>
    public class TamUserData {
        public TamUserData() {
        }
        public int intParam1 = 0;
        public int intParam2 = 0;
        public int intParam3 = 0;
        public int intParam4 = 0;
        public int intParam5 = 0;
        public int intParam6 = 0;
        public System.Collections.ArrayList intParam = new System.Collections.ArrayList();
        public string strParam1 = "";
        public string strParam2 = "";
        public string strParam3 = "";
        public string strParam4 = "";
        public string strParam5 = "";
        public string strParam6 = "";
        public System.Collections.ArrayList strParam = new System.Collections.ArrayList();
        public Object objParam1 = null;
        public Object objParam2 = null;
        public Object objParam3 = null;
        public Object objParam4 = null;
        public Object objParam5 = null;
        public Object objParam6 = null;
        public System.Collections.ArrayList objParam = new System.Collections.ArrayList();

        /// <summary>为了防止指针的相互引用而导致类无法被回收，dispose()在释放前必须被调用</summary>
        public void dispose() {
            objParam1 = null;
            objParam2 = null;
            objParam3 = null;
            objParam4 = null;
            objParam5 = null;
            objParam6 = null;
            objParam.Clear();
        }
    }

    /// <summary>兼容flash的Rectangle</summary>
    public class flashRectangle {
        public double x = 0;
        public double y = 0;
        public double width = 0;
        public double height = 0;
        public flashRectangle() {
        }
        public flashRectangle(double x, double y, double width, double height) {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }
        public double left { get { return x; } }
        public double top { get { return y; } }
        public double right { get { return x + width; } }
        public double bottom { get { return y + height; } }
        public bool contains(double x, double y) {
            if (x < this.x) return false;
            if (y < this.y) return false;
            if (x >= right) return false;
            if (y >= bottom) return false;
            return true;
        }
        public bool containsPoint(flashPoint point) {
            return contains(point.x, point.y);
        }
    }

    /// <summary>兼容flash的一条线段 </summary>
    public class flashLine {
        public double sx = 0;
        public double sy = 0;
        public double dx = 0;
        public double dy = 0;
        public flashLine() {
        }
        public flashLine(double sx, double sy, double dx, double dy) {
            this.sx = sx;
            this.sy = sy;
            this.dx = dx;
            this.dy = dy;
        }
        /// <summary>线段长度</summary>
        public double length {
            get {
                return Math.Sqrt((dx - sx) * (dx - sx) + (dy - sy) * (dy - sy));
            }
        }
        /// <summary>线段的角度, 单位度数</summary>
        public double angle {
            get {
                return Math.Atan2(dy - sy, dx - sx) * 180 / Math.PI;
            }
        }
        /// <summary>线段的角度, 单位弧度</summary>
        public double anglePI {
            get {
                return Math.Atan2(dy - sy, dx - sx);
            }
        }
        /// <summary>获取直线上percent位置的点，percent为0-1之间的浮点数 */</summary>
        public flashPoint getPoint(double percent) {
            return new flashPoint((dx - sx) * percent + sx, (dy - sy) * percent + sy);
        }
    }

    /// <summary>兼容flash的point结构</summary>
    public class flashPoint {
        public double x = 0;
        public double y = 0;
        public flashPoint() {
        }
        public flashPoint(double x, double y) {
            this.x = x;
            this.y = y;
        }
    }

    public class Net {
        /// <summary>http客户端</summary>
        public class HttpClient : MemoryLogClass {
            public delegate void OnReceive(HttpClient sender);
            private IPClient ipClient = new IPClient();
            public TamStatus status = new TamStatus();
            public int Status_Init = 10; //初始状态
            public int Status_Connecting = 20;  //正在连接服务器
            public int Status_Connected = 30;  //已经和服务器建立IP连接
            public int Status_WaitResponse = 40; //发送请求后等待回应
            public Config config = new Config();
            public class Config {
                public string serverIP;
                public int serverPort;
            }
            public string receiveBuff = "";
            public string receiveBuffEncoding = "UTF-8";
            public OnReceive onReceive = null;
            public HttpClient() {
                alias = "HttpClient> ";
                status.value = Status_Init;
            }
            public int localPort {
                get {
                    return ipClient.localPortReal;
                }
            }
            /// <summary>时间片函数</summary>
            public void timeFragment() {
                if (status.value == Status_Init) {
                    ipClient = new IPClient();
                    ipClient.onConnect += ipClientOnConnect;
                    ipClient.onReceive += ipClientOnReceive;
                    ipClient.hostAddress = config.serverIP;
                    ipClient.hostPort = config.serverPort;
                    ipClient.connect();
                    status.value = Status_Connecting;
                    writeLogCommon("正在连接服务器," + serverInfo);
                }
                if (status.value == Status_Connecting) {
                    if (status.lastChangeMS >= 3000) {
                        status.value = Status_Init;
                        writeLogWarning("连接服务器20秒超时," + serverInfo);
                    }
                }
                if (status.value == Status_Connected) {
                    if (!ipClient.connected) {
                        status.value = Status_Init;
                    }
                }
                if (status.value == Status_WaitResponse) {
                    if (status.lastChangeMS >= 20000) {
                        status.value = Status_Init;
                        writeLogWarning("等待回应20秒超时");
                    }
                }
            }
            //重置连接
            public void reset() {
                status.value = Status_Init;
                receiveBuff = "";
            }
            private void ipClientOnConnect(IPClient sender) {
                status.value = Status_Connected;
                writeLogCommon("建立连接成功," + serverInfo);
            }
            private void ipClientOnReceive(IPClient sender) {
                while (sender.recvBuff.popArray(1000)) {
                    //writeLogCommon(DelphiString.displayString(Encoding.GetEncoding("UTF-8").GetString(sender.recvBuff.arrayValuePop)));
                    receiveBuff += Encoding.GetEncoding(receiveBuffEncoding).GetString(sender.recvBuff.arrayValuePop);
                }
                if (onReceive != null) {
                    onReceive(this);
                }
            }
            public bool request(string url) {
                string packet = "";
                packet += "GET " + url + " HTTP/1.1\n";
                //packet += "Host:www.baidu.com\n";
                packet += "Accept-Language: zh-cn\n";
                packet += "Connection: Keep-Alive\n";
                packet += "Cache-Control: no-cache\n";
                packet += "User-Agent:Mozilla/4.0\n\n\n\n";
                byte[] buff = Encoding.GetEncoding("GBK").GetBytes(packet);
                ipClient.sendArray(buff, buff.Length);
                return true;
            }
            public bool sendPacket(byte[] packet) {
                return ipClient.sendArray(packet, packet.Length);
            }

            public string serverInfo {
                get {
                    return "server=" + config.serverIP + ":" + config.serverPort;
                }
            }
            public bool ready {
                get {
                    return status.value == Status_Connected;
                }
            }


        }
    }

    /// <summary>一些高级的字符串函数以及类</summary>
    public class StringFunc {
        /// <summary>以0为开始的取子字符串函数</summary>
        /// <param name="value">源字符串</param>
        /// <param name="start">开始的位置</param>
        /// <param name="len">取值长度</param>
        public static string copy(string value, int start, int len) {
            if (start < 0) {
                len += start;
                start = 0;
            }
            if (start + len > value.Length) {
                len -= start + len - value.Length;
            }
            if (len <= 0) {
                return "";
            } else {
                return value.Substring(start, len);
            }
        }
        /// <summary>
        /// 在字符串末尾加一个字符，如果这个字符已经存在则不加
        /// </summary>
        public static string addRearChar(string value, char ch) {
            if (value == null) return null;
            if (value.Length <= 0) {
                return "" + ch;
            }
            if (value.Substring(value.Length - 1, 1) == "" + ch) {
                return value;
            } else {
                return value + ch;
            }
        }
        /// <summary>
        /// 在字符串末尾删除一个字符，如果这个字符不存在则不删除任何字符
        /// </summary>
        public static string deleteRearChar(string value, char ch) {
            if (value == null) return null;
            if (value.Length <= 0) {
                return "";
            }
            if (value.Substring(value.Length - 1, 1) == "" + ch) {
                return value.Substring(0, value.Length - 1);
            } else {
                return value;
            }
        }
        /// <summary>
        /// 在字符串前端增加一个字符，如果这个字符已经存在则不增加
        /// </summary>
        public static string addFrontChar(string value, char ch) {
            if (value == null) return null;
            if (value.Length <= 0) {
                return "" + ch;
            }
            if (value.Substring(0, 1) == "" + ch) {
                return value;
            } else {
                return ch + value;
            }
        }
        /// <summary>
        /// 在字符串前端删除一个字符，如果这个字符不存在则不做任何操作
        /// </summary>
        public static string deleteFrontChar(string value, char ch) {
            if (value == null) return null;
            if (value.Length <= 0) {
                return "";
            }
            if (value.Substring(0, 1) == "" + ch) {
                return value.Substring(1, value.Length - 1);
            } else {
                return value;
            }
        }
        /// <summary>GBK编码形式的httpEncode，兼容delphi的httpEncode</summary>
        public static string httpEncode(string value) {
            int i;
            string result = "";
            byte[] buff = Encoding.GetEncoding("GBK").GetBytes(value);
            for (i = 0; i < buff.Length; i++) {
                if ((buff[i] >= Convert.ToInt32('a')) && (buff[i] <= Convert.ToInt32('z'))) {
                    result += Convert.ToChar(buff[i]);
                } else if ((buff[i] >= Convert.ToInt32('A')) && (buff[i] <= Convert.ToInt32('Z'))) {
                    result += Convert.ToChar(buff[i]);
                } else if ((buff[i] >= Convert.ToInt32('0')) && (buff[i] <= Convert.ToInt32('9'))) {
                    result += Convert.ToChar(buff[i]);
                } else if (buff[i] == Convert.ToByte(' ')) {
                    result += "+";
                } else {
                    result += "%" + buff[i].ToString("X2");
                }
            }
            return result;
        }
        /// <summary>获取字元(第一个字符序号为0)</summary>
        /// <param name="destString">需要分析的字符串 </param>
        /// <param name="position">位置指针，以0为开始</param>
        /// <param name="breakValueList">结束标记字符列表</param>
        /// <returns>token</returns>
        public static string getToken(string destString, ref int position, string breakValueList) {
            int i;
            string result = "";
            for (i = position; i < destString.Length; i++) {
                if (breakValueList.IndexOf(destString[i]) >= 0) break;
                result = result + destString[i];
            }
            position = i;
            return result;
        }
        /// <summary>判断ch是否是英文字母</summary>
        public static bool isLetter(char ch) {
            if ((ch >= 'a') && (ch <= 'z')) return true;
            if ((ch >= 'A') && (ch <= 'Z')) return true;
            return false;
        }
        public static bool isNumber(char ch) {
            if ((ch >= '0') && (ch <= '9')) return true;
            return false;
        }
        /// <summary>判断一个值是否是浮点数,不支持e的形式，用于替换缓慢的convert.toDouble判断形势</summary>
        public static bool isFloat(string value) {
            if (value.Length <= 0) return false;
            int dotCount = 0;
            for (int i=0; i<value.Length; i++) {
                if (i==0) {
                    if (value[i] == '+') continue;
                    if (value[i] == '-') continue;
                }
                if (isNumber(value[i])) continue;
                if (value[i] == '.') {
                    dotCount++;
                    continue;
                }
                return false;
            }
            if (dotCount > 1) {
                return false;
            } else return true;
        }
        /// <summary>GBK编码形式的httpDecode，兼容delphi的httpDecode</summary>
        public static string httpDecode(string value) {
            int i, len;
            byte[] buff1 = new byte[value.Length];
            byte[] buff2;
            i = 0; len = 0;
            while (i < value.Length) {
                if (value[i] == '%') {
                    i++;
                    try {
                        buff1[len] = Convert.ToByte(copy(value, i, 2), 16);
                    } catch {
                        buff1[len] = 0;
                    }
                    i += 2;
                } else if (value[i] == '+') {
                    buff1[len] = Convert.ToByte(' ');
                    i++;
                } else {
                    buff1[len] = Convert.ToByte(Convert.ToInt32(value[i]) & 0xFF);
                    i++;
                }
                len++;
            }
            buff2 = new byte[len];
            for (i = 0; i < len; i++) {
                buff2[i] = buff1[i];
            }
            return Encoding.GetEncoding("GBK").GetString(buff2);
        }

        /// <summary>MD5加密算法</summary>
        public static string md5Encrypt(string strPwd) {
            System.Security.Cryptography.MD5CryptoServiceProvider md5Hasher = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(strPwd));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++) {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }
        /// <summary>sha256加密算法，输出为小写字符串，输入编码采用系统默认编码形式</summary>
        public static string sha256(string value) {
            byte[] bytValue = Encoding.Default.GetBytes(value);
            try {
                SHA256 sha256 = new SHA256CryptoServiceProvider();
                byte[] retVal = sha256.ComputeHash(bytValue);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++) {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            } catch (Exception ex) {
                return "";
            }
        }
        public static string urlEncode(string str, string codePage) {
            StringBuilder sb = new StringBuilder();
            byte[] byStr;
            if (codePage == "") {
                byStr = System.Text.Encoding.Default.GetBytes(str);
            } else {
                byStr = System.Text.Encoding.GetEncoding(codePage).GetBytes(str);
            }
            for (int i = 0; i < byStr.Length; i++) {
                sb.Append(@"%" + Convert.ToString(byStr[i], 16));
            }
            return (sb.ToString());
        }

        /// <summary>采用二进制+ASCII码的形式显示一个二进制数据块</summary>
        public static string displayBin1(byte[] data) {
            StringBuilder result = new StringBuilder();
            StringBuilder rear = new StringBuilder();
            for (int i = 0; i < data.Length; i++) {
                if ((i > 0) && (i % 16 == 0)) {
                    result.Append("\r\n");
                }
                result.Append(data[i].ToString("X2") + " ");
                if ((data[i] < 0x20) || (data[i] > 0x7E)) {
                    rear.Append(".");
                } else {
                    rear.Append(Convert.ToChar(data[i]));
                }
                if (((i + 1) % 16 == 0) || (i == data.Length - 1)) {
                    result.Append(" " + rear.ToString());
                    rear.Clear();
                }
            }
            return result.ToString();
        }

        public class StringParamAnalyzerItem {
            public string name;
            public string value;
        }
        /// <summary>xxx=xxxx;xxx=xxx;形式的字符串处理器</summary>
        public class StringParamAnalyzer {
            private ArrayList buff = new ArrayList();
            public string paramSpace;
            public char separator;
            public string rawString;
            public bool caseSensitive;
            public bool autoTrimParamName;
            public bool autoTrimParamValue;
            public bool urlEncoding;
            public StringParamAnalyzer() {
                count = 0;
                rawString = "";
                separator = ';';
                paramSpace = "";
                caseSensitive = false;
                autoTrimParamName = true;
                autoTrimParamValue = true;
                urlEncoding = false;
            }
            public void analyze() {
                analyze(false, true);
            }
            public void analyze(bool merge, bool mergeSameName) {
                int len, sp;
                string paramName, paramValue;
                if (!merge) {
                    count = 0;
                }
                sp = 1;
                len = rawString.Length;
                if (len == 0) return;
                while (true) {
                    if (sp > len) break;
                    //analyze
                    paramName = DelphiString.getToken(rawString, ref sp, "=");
                    sp++;
                    paramValue = "";
                    while (true) {
                        if (sp > len) break;
                        if (rawString[sp - 1] == separator) {
                            sp++;
                            if (sp > len) break;
                            if (rawString[sp - 1] == separator) {
                                sp++;
                                paramValue = paramValue + separator;
                            } else break;
                        } else {
                            paramValue = paramValue + rawString[sp - 1];
                            sp++;
                        }
                    }
                    //add parameter
                    if (autoTrimParamName) paramName = paramName.Trim();
                    if (autoTrimParamValue) paramValue = paramValue.Trim();
                    if (merge) {
                        if (mergeSameName) {
                            if (!setValueByName(paramName, paramValue, true)) appendParam(paramName, paramValue);
                        } else {
                            appendParam(paramName, paramValue);
                        }
                    } else {
                        appendParam(paramName, paramValue);
                    }
                }
            }
            public void encode() {
                int i;
                string sTemp;
                rawString = "";
                for (i = 0; i < count; i++) {
                    if (urlEncoding) {
                        sTemp = httpEncode(this[i].value);
                    } else {
                        sTemp = encodeValue(this[i].value);
                    }
                    if (i == 0) {
                        rawString = rawString + this[i].name + "=" + sTemp + separator;
                    } else {
                        rawString = rawString + paramSpace + this[i].name + "=" + sTemp + separator;
                    }
                }
            }
            public string encodeValue(string value) {
                return value.Replace(separator + "", separator + separator + "");
            }
            public StringParamAnalyzerItem this[int index] {
                get {
                    if ((index < 0) || (index >= count)) {
                        return null;
                    } else {
                        return buff[index] as StringParamAnalyzerItem;
                    }
                }
            }
            public int count {
                get { return buff.Count; }
                set {
                    while (buff.Count < value) {
                        buff.Add(new StringParamAnalyzerItem());
                    }
                    while (buff.Count > value) {
                        buff.RemoveAt(buff.Count - 1);
                    }
                }
            }
            public int indexByName(string paramName) {
                return indexByName(paramName, 1);
            }
            public int indexByName(string paramName, int sameCount) {
                int i;
                if (!caseSensitive) paramName = paramName.ToUpper();
                for (i = 0; i < count; i++) {
                    if (caseSensitive) {
                        if (this[i].name == paramName) sameCount--;
                    } else {
                        if (this[i].name.ToUpper() == paramName) sameCount--;
                    }
                    if (sameCount <= 0) break;
                }
                if (i >= count) {
                    return -1;
                } else {
                    return i;
                }
            }
            public string valueByName(string paramName) {
                return valueByName(paramName, 1);
            }
            public string valueByName(string paramName, int sameCount) {
                int iTemp;
                iTemp = indexByName(paramName, sameCount);
                if (iTemp < 0) {
                    return "";
                } else {
                    return this[iTemp].value;
                }
            }
            public int intValueByName(string paramName) {
                return intValueByName(paramName, 1);
            }
            public int intValueByName(string paramName, int sameCount) {
                string sTemp;
                int result;
                sTemp = valueByName(paramName, sameCount);
                try {
                    result = Convert.ToInt32(sTemp);
                } catch {
                    result = 0;
                }
                return result;
            }
            public bool setValueByName(string paramName, string paramValue) {
                return setValueByName(paramName, paramValue, true);
            }
            public bool setValueByName(string paramName, string paramValue, bool autoAppend) {
                int paramIndex;
                paramIndex = indexByName(paramName);
                if ((!autoAppend) && (paramIndex < 0)) return false;
                if (paramIndex < 0) {
                    appendParam(paramName, paramValue);
                    return true;
                }
                this[paramIndex].value = paramValue;
                return true;
            }
            public void appendParam(string paramName, string paramValue) {
                StringParamAnalyzerItem item = new StringParamAnalyzerItem();
                item.name = paramName;
                item.value = paramValue;
                buff.Add(item);
            }
            public bool deleteItemByIndex(int index) {
                if ((index < 0) || (index >= count)) {
                    return false;
                } else {
                    buff.RemoveAt(index);
                    return true;
                }
            }
            public bool deleteItemByName(string paramName) {
                return deleteItemByName(paramName, 0x7FFFFFFF);
            }
            public bool deleteItemByName(string paramName, int deleteCount) {
                int iTemp;
                bool result = false;
                while (deleteCount > 0) {
                    iTemp = indexByName(paramName);
                    if (iTemp < 0) break;
                    deleteItemByIndex(iTemp);
                    deleteCount--;
                    result = true;
                }
                return result;
            }
            public bool combineSame() {
                int itemIndex, i;
                string sTemp;
                bool result = false;
                itemIndex = 0;
                while (true) {
                    if (itemIndex >= (count - 1)) break;
                    sTemp = this[itemIndex].name;
                    if (!caseSensitive) sTemp = sTemp.ToUpper();
                    i = itemIndex + 1;
                    while (true) {
                        if (i >= count) break;
                        if (!caseSensitive) {
                            if (this[i].name.ToUpper() != sTemp) {
                                i++;
                                continue;
                            }
                        } else {
                            if (this[i].name != sTemp) {
                                i++;
                                continue;
                            }
                        }
                        if (this[i].value != this[itemIndex].value) {
                            i++;
                            continue;
                        }
                        deleteItemByIndex(i);
                        result = true;
                    }
                    itemIndex++;
                }
                return result;
            }
        }
    }

    public class Tree<T> {
        public List<Tree<T>> children = new List<Tree<T>>();
        public Tree<T> parent = null;
        public T value;
        public Tree<T> current;
        public Tree<T> getFirst() {
            current = this;
            return current;
        }
        /// <summary>以非递归形式实现的找下一个，当前节点存放在current中，仅凭current即可定位下一个节点位置</summary>
        public Tree<T> getNext() {
            if (current == null) return null;
            //找最左边的子节点，找不到向下继续
            if (current.children.Count > 0) {
                current = current.children[0];
                return current;
            }
            //是否是自身
            if (current == this) {
                current = null;
                return null;
            }
            //找右邻的兄弟节点，找不到则返回父级继续找，如果父级等于自身则结束。
            Tree<T> node;
            while (true) {
                node = getNextBrother(current);
                if (node != null) {
                    current = node;
                    return current;
                }
                if (current.parent == null) {
                    current = null;
                    return null;
                }
                current = current.parent;
                if (current == this) {
                    //遍历结束
                    current = null;
                    return null;
                }
            }
        }
        /// <summary>获取下一个同级的节点</summary>
        public Tree<T> getNextBrother(Tree<T> node) {
            if (node.parent == null) return null;
            for (int i = 0; i < node.parent.children.Count; i++) {
                if (node.parent.children[i] == node) {
                    if (i + 1 >= node.parent.children.Count) {
                        return null;
                    } else {
                        return node.parent.children[i + 1];
                    }
                }
            }
            return null;
        }
        public Tree<T> add(T value) {
            return insert(children.Count, value);
        }
        public Tree<T> add(Tree<T> node) {
            return insert(children.Count, node);
        }
        public Tree<T> insert(int index, Tree<T> node) {
            children.Insert(index, node);
            node.parent = this;
            return node;
        }
        public Tree<T> insert(int index, T value) {
            Tree<T> item = new Tree<T>();
            item.value = value;
            insert(index, item);
            return item;
        }
        public Tree<T> addFirst(T value) {
            return insert(0, value);
        }
        public bool remove(Tree<T> node) {
            if (node.parent == null) {
                return false;
            }
            return node.parent.children.Remove(node);
        }
        public void clear() {
            children.Clear();
            current = null;
        }
        /// <summary>获取元素的数量（包含根节点）</summary>
        /// <returns></returns>
        public int getCount() {
            int result = 0;
            while (true) {
                if (result == 0) {
                    if (getFirst() == null) break;
                } else {
                    if (getNext() == null) break;
                }
                result++;
            }
            return result;
        }
    }
}

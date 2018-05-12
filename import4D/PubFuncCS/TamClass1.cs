using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TamClass1 {
    /// <summary>属性列表</summary>
    public class PropertyList {
        public List<PropertyListItem> items = new List<PropertyListItem>();
        /// <summary>不存在则自动增加</summary>
        public PropertyListItem getItem(string name) {
            int insertPos = 0;
            int index = getItemIndex(name, ref insertPos);
            if (index >= 0) {
                return items[index];
            }
            PropertyListItem item = new PropertyListItem();
            item.name = name;
            items.Insert(insertPos, item);
            return item;
        }
        /// <summary>仅搜索</summary>
        public PropertyListItem findItem(string name) {
            int insertPos = 0;
            int index = getItemIndex(name, ref insertPos);
            if (index < 0) {
                return null;
            } else {
                return items[index];
            }
        }
        /// <summary>获取元素的位置，没找到返回-1，并推荐insertPos插入位置</summary>
        private int getItemIndex(string name, ref int insertPos) {
            if (items.Count <= 0) {
                insertPos = 0;
                return -1;
            }
            int fp, rp, p = 0, r = 0;
            fp = items.Count;
            rp = 0;
            while (fp > rp) {
                p = (fp + rp) / 2;
                r = name.CompareTo(items[p].name);
                if (r > 0) {
                    rp = p + 1;
                } else if (r < 0) {
                    fp = p;
                } else {
                    return p;
                }
            }
            if (r > 0) {
                insertPos = p + 1;
            } else {
                insertPos = p;
            }
            return -1;
        }
        /// <summary>增加一个新的属性，如果属性存在则更新，不存在则新建</summary>
        public void addProperty(PropertyListItem item) {
            PropertyListItem current = getItem(item.name);
            current.assign(item);
        }
        public PropertyList clone() {
            PropertyList result = new PropertyList();
            result.assign(this);
            return result;
        }
        public void assign(PropertyList value) {
            items.Clear();
            for (int i = 0; i < value.items.Count; i++) {
                addProperty(value.items[i].clone());
            }
        }
        /// <summary>删除category相同的元素</summary>
        public void deleteCategory(string category) {
            int index = 0;
            while (index < items.Count) {
                if (items[index].category.Equals(category)) {
                    items.RemoveAt(index);
                } else {
                    index++;
                }
            }
        }
    }
    public class PropertyListItem {
        public string name = ""; //内部标识名字，不区分大小写
        public string displayName = ""; //显示名称
        public string category = ""; //分类
        public Variant value = new Variant();
        public Variant defaultValue = new Variant();
        public void assign(PropertyListItem value) {
            this.name = value.name;
            this.displayName = value.displayName;
            this.category = value.category;
            this.value.assign(value.value);
            this.defaultValue.assign(value.defaultValue);
        }
        public PropertyListItem clone() {
            PropertyListItem result = new PropertyListItem();
            result.assign(this);
            return result;
        }
    }
    /// <summary>多类型变量</summary>
    public class Variant {
        private string data = "";
        /// <summary>取值：string, int, double, color, enum</summary>
        public string defaultType = "string";
        public string[] enumData = null;
        public int asInt32 {
            get {
                try {
                    return Convert.ToInt32(data);
                } catch (Exception) {
                    return 0;
                }
            }
            set {
                data = value.ToString();
            }
        }
        public string asString {
            get {
                return data;
            }
            set {
                data = value;
            }
        }
        public double asDouble {
            get {
                return Convert.ToDouble(data);
            }
            set {
                data = value.ToString();
            }
        }
        public object asObject {
            get {
                if (defaultType == "string") {
                    return asString;
                } else if (defaultType == "int") {
                    return asInt32;
                } else if (defaultType == "double") {
                    return asDouble;
                } else if (defaultType == "color") {
                    return asColor;
                } else if (defaultType == "enum") {
                    return asString;
                } else {
                    return asString;
                }
            }
            set {
                if (defaultType == "string") {
                    asString = value as string;
                } else if (defaultType == "int") {
                    asInt32 = Convert.ToInt32(value);
                } else if (defaultType == "double") {
                    asDouble = Convert.ToDouble(value);
                } else if (defaultType == "color") {
                    asColor = (System.Drawing.Color)value;
                } else if (defaultType == "enum") {
                    asString = value as string;
                } else {
                    asString = value as string;
                }
            }
        }
        public System.Drawing.Color asColor {
            get {
                return System.Drawing.Color.FromArgb(
                    Convert.ToInt32(data.Substring(0, 2), 16),
                    Convert.ToInt32(data.Substring(2, 2), 16),
                    Convert.ToInt32(data.Substring(4, 2), 16));
            }
            set {
                data = value.R.ToString("X2") + value.G.ToString("X2") + value.B.ToString("X2");
            }
        }
        public void assign(Variant value) {
            this.data = value.data;
            this.defaultType = value.defaultType;
            if (value.enumData == null) {
                this.enumData = null;
            } else {
                this.enumData = new string[value.enumData.Count()];
                for (int i = 0; i < value.enumData.Count(); i++) {
                    this.enumData[i] = value.enumData[i];
                }
            }
        }
        public Variant clone() {
            Variant result = new Variant();
            result.assign(this);
            return result;
        }
    }

    /// <summary>日志系统基础类2015/01/17</summary>
    public abstract class TamLog {
        public string alias = "";
        public bool addDatetime = false;
        public bool addYear = false;
        public static int TYPE_COMMON = 10;
        public static int TYPE_WARNING = 20;
        public static int TYPE_ERROR = 30;
        public abstract bool writeLog(int logType, string logString);
        public bool writeCommon(string logString) {
            return writeLog(TYPE_COMMON, prepareLogString(logString));
        }
        public bool writeWarning(string logString) {
            return writeLog(TYPE_WARNING, prepareLogString(logString));
        }
        public bool writeError(string logString) {
            return writeLog(TYPE_ERROR, prepareLogString(logString));
        }
        private string prepareLogString(string logString) {
            string result = alias + logString;
            if (addDatetime) {
                if (addYear) {
                    result = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + result;
                } else {
                    result = System.DateTime.Now.ToString("MM-dd HH:mm:ss") + " " + result;
                }
            }
            return result;
        }
    }

    /// <summary>磁盘文件日志，附带线程锁，可以多线程同时操作</summary>
    public class TamDiskLog : TamLog {
        /// <summary>单个日志文件的最大大小(缺省20M)</summary>
        public int maxFileSize = 20000000;
        private TamPub1.FileOperation fileAll = new TamPub1.FileOperation();
        private TamPub1.FileOperation fileWarning = new TamPub1.FileOperation();
        private TamPub1.FileOperation fileError = new TamPub1.FileOperation();
        private string fFilename = "";
        public TamDiskLog() {
            filename = TamPub1.FileOperation.changeFileExt(TamPub1.FileOperation.currentFileName, "");
        }
        /// <summary>磁盘存储的文件名（带路径），不必要附带扩展名</summary>
        public string filename {
            get { return fFilename; }
            set {
                fFilename = value;
                fileAll.filename = value + ".all.txt";
                fileWarning.filename = value + ".wrn.txt";
                fileError.filename = value + ".err.txt";
            }
        }
        private object writeLogLock = new object();
        public override bool writeLog(int logType, string logString) {
            lock (writeLogLock) {
                if (logType == TYPE_WARNING) {
                    if (!fileWarning.openAppend()) return false;
                    if (!fileWarning.writeWFixedString(logString + "\r\n")) return false;
                    if (!closeAndCheck(fileWarning)) return false;
                } else if (logType == TYPE_ERROR) {
                    if (!fileError.openAppend()) return false;
                    if (!fileError.writeWFixedString(logString + "\r\n")) return false;
                    if (!closeAndCheck(fileError)) return false;
                }
                if (logType == TYPE_COMMON) {
                    logString = "  " + logString;
                } else if (logType == TYPE_WARNING) {
                    logString = "W " + logString;
                } else {
                    logString = "E " + logString;
                }
                if (!fileAll.openAppend()) return false;
                if (!fileAll.writeWFixedString(logString + "\r\n")) return false;
                if (!closeAndCheck(fileAll)) return false;
                return true;
            }
        }
        private bool closeAndCheck(TamPub1.FileOperation checkFile) {
            if (checkFile.pos < maxFileSize) {
                checkFile.close();
                return true;
            }
            checkFile.close();
            string newFilename = checkFile.filename;
            newFilename = TamPub1.FileOperation.changeFileExt(newFilename, ".bak.txt");
            try {
                TamPub1.FileOperation.delete(newFilename);
                TamPub1.FileOperation.rename(checkFile.filename, newFilename);
            } catch {
                return false;
            }
            return true;
        }
    }

}

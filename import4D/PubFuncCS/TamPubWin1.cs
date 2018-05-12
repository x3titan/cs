using System;
using System.Windows.Forms;
using System.Drawing;

namespace TamPubWin1 {
    /// <summary>
    /// 2009-7-15 开始的Windows应用程序改造新增的公共库，主要针对网络通信
    /// </summary>
    public class LogDisplay {
        //public TamPub1.DiskLog diskLog = new TamPub1.DiskLog();
        private TamPub1.StringQueue buff = new TamPub1.StringQueue();
        private System.Windows.Forms.RichTextBox fRichTextBox = null;
        private System.Windows.Forms.ListBox fListBox = null;
        private TamPub1.TamLocker locker = new TamPub1.TamLocker();
        public LogDisplay() {
            buff.size = 10000;
        }
        public System.Windows.Forms.RichTextBox richTextBox {
            set {
                if (value == null) return;
                fRichTextBox = value;
                fRichTextBox.BackColor = System.Drawing.Color.Black;
                fRichTextBox.Clear();
                //System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
            }
            get {
                return fRichTextBox;
            }
        }
        public System.Windows.Forms.ListBox listBox {
            set {
                if (value == null) return;
                fListBox = value;
                fListBox.BackColor = System.Drawing.Color.Black;
                fListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
                fListBox.Items.Clear();
                fListBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(listBox_DrawItem);
            }
            get {
                return fListBox;
            }
        }
        public class ListBoxItem {
            public string logString = "";
            public int LogType = TamPub1.MemoryLog.LogTypeCommon;
            public override string ToString() {
                return logString;
            }
        }
        private void listBox_DrawItem(object sender, System.Windows.Forms.DrawItemEventArgs e) {
            listBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            e.DrawBackground();
            System.Drawing.SolidBrush brush = null;
            //如果是RGB颜色，需要在最高位加FF，如: 0xFF102030
            if (e.Index == -1) { return; } // 如果listbox里面没有数据，点击会引发异常，如果指为-1，跳出
            if ((listBox.Items[e.Index] as ListBoxItem).LogType == TamPub1.MemoryLog.LogTypeCommon) {
                brush = new System.Drawing.SolidBrush(System.Drawing.Color.Aqua);
            } else if ((listBox.Items[e.Index] as ListBoxItem).LogType == TamPub1.MemoryLog.LogTypeWarning) {
                brush = new System.Drawing.SolidBrush(System.Drawing.Color.Yellow);
            } else if ((listBox.Items[e.Index] as ListBoxItem).LogType == TamPub1.MemoryLog.LogTypeError) {
                brush = new System.Drawing.SolidBrush(System.Drawing.Color.Red);
            }
            e.Graphics.DrawString(listBox.Items[e.Index].ToString(), e.Font, brush, e.Bounds, System.Drawing.StringFormat.GenericDefault);
            e.DrawFocusRectangle();
        }
        public void displayLog(string logString, int logType) {
            //diskLog.writeLogString(logType, logString);
            locker.LockTimeout = 1000;
            locker.Lock();
            if (richTextBox != null) {
                if (richTextBox.Lines.Length >= 500) {
                    richTextBox.Select(0, richTextBox.TextLength / 2);
                    richTextBox.SelectedText = "";
                }
                int startIndex = richTextBox.Text.Length;
                richTextBox.AppendText(logString + "\r\n");
                richTextBox.Select(startIndex, logString.Length + 2);
                if (logType == TamPub1.MemoryLog.LogTypeCommon) {
                    richTextBox.SelectionColor = System.Drawing.Color.Cyan;
                } else if (logType == TamPub1.MemoryLog.LogTypeWarning) {
                    richTextBox.SelectionColor = System.Drawing.Color.Yellow;
                } else if (logType == TamPub1.MemoryLog.LogTypeError) {
                    richTextBox.SelectionColor = System.Drawing.Color.Red;
                }
                richTextBox.Select(startIndex + logString.Length + 2, logString.Length + 2);
                richTextBox.ScrollToCaret();
            }
            if (listBox != null) {
                if (listBox.Items.Count >= 500) {
                    for (int i = 0; i < 100; i++) listBox.Items.RemoveAt(0);
                    listBox.SelectedIndex = 0;
                }
                logString = logString.Replace("\r\n", "\n");
                string[] lines = logString.Split('\n');
                for (int i=0; i<lines.Length; i++) {
                    if (lines[i].Length <= 0) continue;
                    ListBoxItem listItem = new ListBoxItem();
                    listItem.LogType = logType;
                    listItem.logString = lines[i];
                    ThreadListBox(listItem);
                }
            }
            locker.Unlock();
        }
        //同行显示功能
        private bool sameLineCurrent = false;
        private bool sameLineLast = false;
        public void sameLine() {
            sameLineCurrent = true;
        }
        private delegate void ChangeListBoxText(object listItem);
        private void ThreadListBox(object listItem) {
            if (this.listBox.InvokeRequired) { //等待异步
                ChangeListBoxText c = new ChangeListBoxText(ThreadListBox);
                this.listBox.Invoke(c, new object[] { listItem });//通过代理调用刷新方法
            } else {
                if (sameLineCurrent && sameLineLast && listBox.Items.Count > 0) {
                    listBox.Items[listBox.Items.Count - 1] = listItem;
                } else {
                    ListBoxItem li = listItem as ListBoxItem;
                    listBox.Items.Add(li);
                    listBox.SelectedIndex = listBox.Items.Count - 1;
                }
                sameLineLast = sameLineCurrent;
                sameLineCurrent = false;
            }
        }
        public void writeLogCommon(string logString) {
            displayLog(logString, TamPub1.MemoryLog.LogTypeCommon);
        }
        public void writeLogWarning(string logString) {
            displayLog(logString, TamPub1.MemoryLog.LogTypeWarning);
        }
        public void writeLogError(string logString) {
            displayLog(logString, TamPub1.MemoryLog.LogTypeError);
        }
        public void writeLogString(int logType, string logString) {
            if (logType == TamPub1.MemoryLog.LogTypeCommon) {
                writeLogCommon(logString);
            } else if (logType == TamPub1.MemoryLog.LogTypeWarning) {
                writeLogWarning(logString);
            } else {
                writeLogError(logString);
            }
        }
    }

    /// <summary>
    /// 可显示和写入磁盘
    /// </summary>
    public class LogFile {
        public LogDisplay logDisplay = new LogDisplay();
        public TamPub1.DiskLog logDisk = new TamPub1.DiskLog();
        /// <summary>
        /// 0: 不显示时间
        /// 10: 显示短时间格式（不包含年）
        /// 20: 显示长时间格式（包含年）
        /// </summary>
        public int datetimeType = 10;
        public System.Windows.Forms.RichTextBox richTextBox {
            set { logDisplay.richTextBox = value; }
            get { return logDisplay.richTextBox; }
        }
        public System.Windows.Forms.ListBox listBox {
            set { logDisplay.listBox = value; }
            get { return logDisplay.listBox; }
        }
        public string filename {
            set { logDisk.filename = value; }
            get { return logDisk.filename; }
        }
        public void writeLogCommon(string logString) {
            writeLogString(TamPub1.MemoryLog.LogTypeCommon, logString);
        }
        public void writeLogWarning(string logString) {
            writeLogString(TamPub1.MemoryLog.LogTypeWarning, logString);
        }
        public void writeLogError(string logString) {
            writeLogString(TamPub1.MemoryLog.LogTypeError, logString);
        }
        public void writeLogString(int logType, string logString) {
            if (datetimeType == 10) {
                logString = System.DateTime.Now.ToString("MM-dd HH:mm:ss") + " " + logString;
            } else if (datetimeType == 20) {
                logString = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + logString;
            }
            logDisplay.writeLogString(logType, logString);
            logDisk.writeLogString(logType, logString);
        }
        public void getFromMemoryLog(TamPub1.MemoryLogClass memoryLog) {
            TamPub1.MemoryLogClass.LogItem logItem;
            while (true) {
                logItem = memoryLog.popLog();
                if (logItem == null) return;
                writeLogString(logItem.logType, logItem.logString);
            }
        }
    }

    /// <summary>
    /// 支持日志输出的基类
    /// </summary>
    public class LogClass {
        public LogFile logFile = null;
        /// <summary>
        /// 别名，用于显示在日志的头部
        /// </summary>
        public string alias = "";
        public void writeLogCommon(string logString) {
            if (logFile == null) return;
            writeLogString(TamPub1.MemoryLog.LogTypeCommon, logString);
        }
        public void writeLogWarning(string logString) {
            if (logFile == null) return;
            writeLogString(TamPub1.MemoryLog.LogTypeWarning, logString);
        }
        public void writeLogError(string logString) {
            if (logFile == null) return;
            writeLogString(TamPub1.MemoryLog.LogTypeError, logString);
        }
        public void writeLogString(int logType, string logString) {
            if (logFile == null) return;
            logString = alias + logString;
            logFile.writeLogString(logType, logString);
        }
        public void attachMemoryLog(TamPub1.MemoryLog memoryLog) {
            memoryLog.onWriteLog = onWriteLogString;
        }
        private void onWriteLogString(TamPub1.MemoryLog sender, int logType, string logString) {
            while (sender.pop()) {
                writeLogString(sender.current.logType, sender.current.logString);
            }
        }
    }

    /// <summary>
    /// 支持内存日志输出的基类，一般用于多线程后台服务类
    /// </summary>
    public class MemoryLogClass {
        public TamPub1.MemoryLog logFile = null;
        /// <summary>
        /// 别名，用于显示在日志的头部
        /// </summary>
        public string alias = "";
        public void writeLogCommon(string logString) {
            if (logFile == null) return;
            writeLogString(TamPub1.MemoryLog.LogTypeCommon, logString);
        }
        public void writeLogWarning(string logString) {
            if (logFile == null) return;
            writeLogString(TamPub1.MemoryLog.LogTypeWarning, logString);
        }
        public void writeLogError(string logString) {
            if (logFile == null) return;
            writeLogString(TamPub1.MemoryLog.LogTypeError, logString);
        }
        public void writeLogString(int logType, string logString) {
            if (logFile == null) return;
            logString = alias + logString;
            logFile.writeLogString(logType, logString);
        }
    }

    public class Dialog {
        /// <summary>字符串输入框,返回空标识用户没有输入或者选择了取消</summary>
        /// <param name="caption">窗口标题</param>
        /// <param name="title">窗口内的题头文字</param>
        /// <param name="defaultValue">缺省值</param>
        /// <returns>返回空标识用户没有输入或者选择了取消</returns>
        public static string inputBoxString(string caption, string title, string defaultValue) {
            Form form = new Form();
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.FormBorderStyle = FormBorderStyle.FixedSingle;
            form.Text = caption;

            Label label1 = new Label();
            label1.Text = title;
            label1.Left = 30;
            label1.Top = 20;
            label1.Parent = form;
            label1.AutoSize = true;

            TextBox textbox1 = new TextBox();
            textbox1.Left = 30;
            textbox1.Top = 45;
            textbox1.Parent = form;
            textbox1.Text = defaultValue;
            textbox1.SelectAll();

            Button button1 = new Button();
            button1.Left = 30;
            button1.Top = 80;
            button1.Parent = form;
            button1.Text = "确定";
            form.AcceptButton = button1;//回车响应
            button1.DialogResult = DialogResult.OK;

            Button button2 = new Button();
            button2.Left = 120;
            button2.Top = 80;
            button2.Parent = form;
            button2.Text = "取消";
            button2.DialogResult = DialogResult.Cancel;

            form.Width = label1.Width + 60;
            if (form.Width < 220) {
                form.Width = 220;
            }
            textbox1.Width = form.Width - 60;
            form.Height = 150;
            try {
                if (form.ShowDialog() == DialogResult.OK) {
                    return textbox1.Text;
                } else {
                    return null;
                }
            } finally {
                form.Dispose();
            }
        }
        public static bool inputBoxInteger(string caption, string title, ref int value) {
            string result = inputBoxString(caption, title, value + "");
            int iValue;
            if (result == null) return false;
            try {
                iValue = Convert.ToInt32(result);
            } catch {
                return false;
            }
            value = iValue;
            return true;
        }
        public static string inputBoxMemo(string caption, string title, string defaultValue) {
            Form form = new Form();
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.Width = 320;
            form.Height = 350;
            //form.FormBorderStyle = FormBorderStyle.Sizable;
            form.Text = caption;

            Label label1 = new Label();
            label1.Text = title;
            label1.Left = 10;
            label1.Top = 20;
            label1.Parent = form;
            label1.AutoSize = true;

            TextBox textbox1 = new TextBox();
            textbox1.Left = 10;
            textbox1.Top = 45;
            textbox1.Width = 280;
            textbox1.Height = 220;
            textbox1.Multiline = true;
            textbox1.ScrollBars = ScrollBars.Vertical;
            textbox1.Parent = form;
            textbox1.Text = defaultValue;
            textbox1.AcceptsTab = true;
            textbox1.Anchor = AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Top;
            textbox1.SelectAll();

            Button button1 = new Button();
            button1.Left = 130;
            button1.Top = 280;
            button1.Parent = form;
            button1.Text = "确定";
            button1.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            //form.AcceptButton = button1;//回车响应
            button1.DialogResult = DialogResult.OK;

            Button button2 = new Button();
            button2.Left = 220;
            button2.Top = 280;
            button2.Parent = form;
            button2.Text = "取消";
            button2.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            button2.DialogResult = DialogResult.Cancel;
            try {
                if (form.ShowDialog() == DialogResult.OK) {
                    return textbox1.Text;
                } else {
                    return null;
                }
            } finally {
                form.Dispose();
            }
        }

        public static System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
        public static int inputBoxList(string caption, string title, int defaultValue) {
            Form form = new Form();
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.Width = 220;
            form.Height = 150;
            form.FormBorderStyle = FormBorderStyle.FixedSingle;
            form.Text = caption;

            Label label1 = new Label();
            label1.Text = title;
            label1.Left = 10;
            label1.Top = 20;
            label1.Parent = form;
            label1.AutoSize = true;

            ComboBox combobox1 = new ComboBox();
            combobox1.Left = 30;
            combobox1.Top = 45;
            combobox1.Width = 160;
            combobox1.Parent = form;
            combobox1.DropDownStyle = ComboBoxStyle.DropDownList;
            for (int i = 0; i < list.Count; i++) {
                combobox1.Items.Add(list[i]);
            }
            if (defaultValue >= list.Count) defaultValue = list.Count - 1;
            if (defaultValue >= 0) combobox1.SelectedIndex = defaultValue;

            Button button1 = new Button();
            button1.Left = 30;
            button1.Top = 80;
            button1.Parent = form;
            button1.Text = "确定";
            form.AcceptButton = button1;//回车响应
            button1.DialogResult = DialogResult.OK;

            Button button2 = new Button();
            button2.Left = 120;
            button2.Top = 80;
            button2.Parent = form;
            button2.Text = "取消";
            button2.DialogResult = DialogResult.Cancel;
            try {
                if (form.ShowDialog() == DialogResult.OK) {
                    return combobox1.SelectedIndex;
                } else {
                    return -1;
                }
            } finally {
                form.Dispose();
            }
        }
        public static string inputBoxList(string caption, string title, string defaultValue) {
            int result;
            result = list.IndexOf(defaultValue);
            if (result <= 0) result = 0;
            result = inputBoxList(caption, title, result);
            if (result < 0) return null;
            return list[result];
        }

        private static Form progressForm = null;
        private static Label progressFormLabel1 = null;
        private static ProgressBar progressFormProgressBar1 = null;
        public static void showProgress(string caption, string title, float progress) {
            if (progress >= 100) {
                if (progressForm != null) {
                    progressForm.Close();
                    progressForm.Dispose();
                    progressForm = null;
                }
                return;
            }
            if (progressForm == null) {
                progressForm = new Form();
                progressForm.MinimizeBox = false;
                progressForm.MaximizeBox = false;
                progressForm.StartPosition = FormStartPosition.CenterScreen;
                progressForm.Width = 425;
                progressForm.Height = 100;
                progressForm.FormBorderStyle = FormBorderStyle.FixedSingle;
                progressForm.TopMost = true;

                progressFormLabel1 = new Label();
                progressFormLabel1.Left = 10;
                progressFormLabel1.Top = 15;
                progressFormLabel1.Parent = progressForm;
                progressFormLabel1.AutoSize = true;

                progressFormProgressBar1 = new ProgressBar();
                progressFormProgressBar1.Left = 10;
                progressFormProgressBar1.Top = 35;
                progressFormProgressBar1.Width = 400;
                progressFormProgressBar1.Height = 15;
                progressFormProgressBar1.Parent = progressForm;
                progressFormProgressBar1.Minimum = 0;
                progressFormProgressBar1.Maximum = 100;
            }
            progressForm.Show();
            progressForm.Text = caption;
            progressFormLabel1.Text = title;
            progressFormProgressBar1.Value = Convert.ToInt32(progress);

            Application.DoEvents();
        }
    }

    public class Etc {
        public static void saveDesktop(string xmlFilename, Form form) {
            if ((form.Width > 0) && (form.Height > 0) && (form.Left > 0) && (form.Top > 0)) {
                TamPub1.ConfigFileXml.writeInt32(xmlFilename, form.Name, "width", form.Width);
                TamPub1.ConfigFileXml.writeInt32(xmlFilename, form.Name, "height", form.Height);
                TamPub1.ConfigFileXml.writeInt32(xmlFilename, form.Name, "left", form.Left);
                TamPub1.ConfigFileXml.writeInt32(xmlFilename, form.Name, "top", form.Top);
            }
            if (form.WindowState == FormWindowState.Maximized) {
                TamPub1.ConfigFileXml.writeString(xmlFilename, form.Name, "windowState", "Maximized");
            } else if (form.WindowState == FormWindowState.Minimized) {
                TamPub1.ConfigFileXml.writeString(xmlFilename, form.Name, "windowState", "Minimized");
            } else if (form.WindowState == FormWindowState.Normal) {
                TamPub1.ConfigFileXml.writeString(xmlFilename, form.Name, "windowState", "Normal");
            }
        }
        public static void loadDesktop(string xmlFilename, Form form) {
            form.Width = TamPub1.ConfigFileXml.readInt32(xmlFilename, form.Name, "width", 600);
            form.Height = TamPub1.ConfigFileXml.readInt32(xmlFilename, form.Name, "height", 400);
            form.Left = TamPub1.ConfigFileXml.readInt32(xmlFilename, form.Name, "left", 100);
            form.Top = TamPub1.ConfigFileXml.readInt32(xmlFilename, form.Name, "top", 50);
            if (form.Left + form.Width <= 0) form.Left = 0;
            if (form.Top + form.Height <= 0) form.Top = 0;
            string windowState = TamPub1.ConfigFileXml.readString(xmlFilename, form.Name, "windowState", "Normal");
            if (windowState == "Normal") {
                form.WindowState = FormWindowState.Normal;
            } else if (windowState == "Minimized") {
                form.WindowState = FormWindowState.Minimized;
            } else if (windowState == "Maximized") {
                form.WindowState = FormWindowState.Maximized;
            }
        }
    }

    /// <summary>PropertyGrid控件使用的类</summary>
    public class PropertyManageCls : System.Collections.CollectionBase, System.ComponentModel.ICustomTypeDescriptor {
        public void Add(Property value) {
            int flag = -1;
            if (value != null) {
                if (base.List.Count > 0) {
                    System.Collections.Generic.List<Property> mList = new System.Collections.Generic.List<Property>();
                    for (int i = 0; i < base.List.Count; i++) {
                        Property p = base.List[i] as Property;
                        if (value.Name == p.Name) {
                            flag = i;
                        }
                        mList.Add(p);
                    }
                    if (flag == -1) {
                        mList.Add(value);
                    }
                    base.List.Clear();
                    foreach (Property p in mList) {
                        base.List.Add(p);
                    }
                } else {
                    base.List.Add(value);
                }
            }
        }
        public void Remove(Property value) {
            if (value != null && base.List.Count > 0)
                base.List.Remove(value);
        }
        public Property this[int index] {
            get {
                return (Property)base.List[index];
            }
            set {
                base.List[index] = (Property)value;
            }
        }
        public Property getByName(string name) {
            for (int i = 0; i < Count; i++) {
                if (this[i].Name == name) {
                    return this[i];
                }
            }
            return null;
        }
        #region ICustomTypeDescriptor 成员
        public System.ComponentModel.AttributeCollection GetAttributes() {
            return System.ComponentModel.TypeDescriptor.GetAttributes(this, true);
        }
        public string GetClassName() {
            return System.ComponentModel.TypeDescriptor.GetClassName(this, true);
        }
        public string GetComponentName() {
            return System.ComponentModel.TypeDescriptor.GetComponentName(this, true);
        }
        public System.ComponentModel.TypeConverter GetConverter() {
            return System.ComponentModel.TypeDescriptor.GetConverter(this, true);
        }
        public System.ComponentModel.EventDescriptor GetDefaultEvent() {
            return System.ComponentModel.TypeDescriptor.GetDefaultEvent(this, true);
        }
        public System.ComponentModel.PropertyDescriptor GetDefaultProperty() {
            return System.ComponentModel.TypeDescriptor.GetDefaultProperty(this, true);
        }
        public object GetEditor(Type editorBaseType) {
            return System.ComponentModel.TypeDescriptor.GetEditor(this, editorBaseType, true);
        }
        public System.ComponentModel.EventDescriptorCollection GetEvents(Attribute[] attributes) {
            return System.ComponentModel.TypeDescriptor.GetEvents(this, attributes, true);
        }
        public System.ComponentModel.EventDescriptorCollection GetEvents() {
            return System.ComponentModel.TypeDescriptor.GetEvents(this, true);
        }
        public System.ComponentModel.PropertyDescriptorCollection GetProperties(Attribute[] attributes) {
            System.ComponentModel.PropertyDescriptor[] newProps = new System.ComponentModel.PropertyDescriptor[this.Count];
            for (int i = 0; i < this.Count; i++) {
                Property prop = (Property)this[i];
                newProps[i] = new CustomPropertyDescriptor(ref prop, attributes);
            }
            return new System.ComponentModel.PropertyDescriptorCollection(newProps);
        }
        public System.ComponentModel.PropertyDescriptorCollection GetProperties() {
            return System.ComponentModel.TypeDescriptor.GetProperties(this, true);
        }
        public object GetPropertyOwner(System.ComponentModel.PropertyDescriptor pd) {
            return this;
        }
        #endregion
    }
    //属性类  
    public class Property {
        private string _name = string.Empty;
        private object _value = null;
        private bool _readonly = false;
        private bool _visible = true;
        private string _category = string.Empty;
        System.ComponentModel.TypeConverter _converter = null;
        object _editor = null;
        private string _displayname = string.Empty;
        private string _description = "";
        public Property(string sName, object sValue) {
            this._name = sName;
            this._value = sValue;
        }
        public Property(string sName, object sValue, bool sReadonly, bool sVisible) {
            this._name = sName;
            this._value = sValue;
            this._readonly = sReadonly;
            this._visible = sVisible;
        }
        public string Name { //获得属性名  
            get {
                return _name;
            }
            set {
                _name = value;
            }
        }
        public string DisplayName {  //属性显示名称  
            get {
                return _displayname;
            }
            set {
                _displayname = value;
            }
        }
        public string Description { //我加的，支持对属性的说明
            get {
                return _description;
            }
            set {
                _description = value;
            }
        }
        public System.ComponentModel.TypeConverter Converter { //类型转换器，我们在制作下拉列表时需要用到  
            get {
                return _converter;
            }
            set {
                _converter = value;
            }
        }
        public string Category {  //属性所属类别  
            get {
                return _category;
            }
            set {
                _category = value;
            }
        }
        public object Value {  //属性值  
            get {
                return _value;
            }
            set {
                _value = value;
            }
        }
        public bool ReadOnly { //是否为只读属性  
            get {
                return _readonly;
            }
            set {
                _readonly = value;
            }
        }
        public bool Visible { //是否可见  
            get {
                return _visible;
            }
            set {
                _visible = value;
            }
        }
        public virtual object Editor { //属性编辑器  
            get {
                return _editor;
            }
            set {
                _editor = value;
            }
        }
    }
    public class CustomPropertyDescriptor : System.ComponentModel.PropertyDescriptor {
        Property m_Property;
        public CustomPropertyDescriptor(ref Property myProperty, Attribute[] attrs)
            : base(myProperty.Name, attrs) {
            m_Property = myProperty;
        }
        #region PropertyDescriptor 重写方法
        public override bool CanResetValue(object component) {
            return false;
        }
        public override Type ComponentType {
            get {
                return null;
            }
        }
        public override object GetValue(object component) {
            return m_Property.Value;
        }
        public override string Description {
            get {
                return m_Property.Description;
            }
        }
        public override string Category {
            get {
                return m_Property.Category;
            }
        }
        public override string DisplayName {
            get {
                return m_Property.DisplayName != "" ? m_Property.DisplayName : m_Property.Name;
            }
        }
        public override bool IsReadOnly {
            get {
                return m_Property.ReadOnly;
            }
        }
        public override void ResetValue(object component) {
            //Have to implement  
        }
        public override bool ShouldSerializeValue(object component) {
            return false;
        }
        public override void SetValue(object component, object value) {
            m_Property.Value = value;
        }
        public override System.ComponentModel.TypeConverter Converter {
            get {
                return m_Property.Converter;
            }
        }
        public override Type PropertyType {
            get { return m_Property.Value.GetType(); }
        }
        public override object GetEditor(Type editorBaseType) {
            return m_Property.Editor == null ? base.GetEditor(editorBaseType) : m_Property.Editor;
        }
        #endregion
    }
    public class DropDownListConverter : System.ComponentModel.StringConverter {
        object[] m_Objects;
        bool canInput;
        public DropDownListConverter(object[] objects, bool canInput = false) {
            m_Objects = objects;
            this.canInput = canInput;
        }
        public override bool GetStandardValuesSupported(System.ComponentModel.ITypeDescriptorContext context) {
            return true;
        }
        public override bool GetStandardValuesExclusive(System.ComponentModel.ITypeDescriptorContext context) {
            return !canInput;
        }
        public override
        System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(System.ComponentModel.ITypeDescriptorContext context) {
            return new StandardValuesCollection(m_Objects);//我们可以直接在内部定义一个数组，但并不建议这样做，这样对于下拉框的灵活
            //性有很大影响  
        }
    }

    public class PropertyGridFileItem : System.Drawing.Design.UITypeEditor {
        public override System.Drawing.Design.UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context) {
            return System.Drawing.Design.UITypeEditorEditStyle.Modal;
        }
        public override object EditValue(System.ComponentModel.ITypeDescriptorContext context, System.IServiceProvider provider, object value) {
            System.Windows.Forms.Design.IWindowsFormsEditorService edSvc =
               (System.Windows.Forms.Design.IWindowsFormsEditorService)provider.GetService(typeof(System.Windows.Forms.Design.IWindowsFormsEditorService));
            if (edSvc != null) {
                // 可以打开任何特定的对话框  
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.AddExtension = false;
                if (dialog.ShowDialog().Equals(DialogResult.OK)) {
                    return dialog.FileName;
                }
            }
            return value;
        }
    }

    /// <summary>颜色处理</summary>
    public class TamColor {
        /// <summary>HSI色彩，取值范围0-1</summary>
        public class HSI {
            public double Hue = 0;
            public double Saturation = 0;
            public double Intensity = 0;
            public HSI() { }
            public HSI(double hue, double saturation, double intensity) {
                this.Hue = hue;
                this.Saturation = saturation;
                this.Intensity = intensity;
            }
        }
        public static HSI rgb2Hsi(Color rgb) {
            double x, y, z;
            double r = rgb.R, g = rgb.G, b = rgb.B;
            double h, s, i;
            //rgb -> xyz
            x = r * Math.Sqrt(2.0 / 3.0) - (g + b) / Math.Sqrt(6);
            y = (g - b) * Math.Sqrt(2) / 2;
            z = (r + g + b) / Math.Sqrt(3);
            //xyz -> hsi
            h = Math.Atan2(y, x);
            s = Math.Sqrt(x * x + y * y);
            i = z;
            //hsi归一化
            h = h / (Math.PI * 2);
            s = s / (0xff * Math.Sqrt(2.0 / 3.0));
            i = i / Math.Sqrt(0xff * 0xff + 0xff * 0xff + 0xff * 0xff);
            return (new HSI(h, s, i));
        }
        public static Color hsi2Rgb(HSI hsi) {
            double x, y, z;
            double r, g, b;
            double h = hsi.Hue, s = hsi.Saturation, i = hsi.Intensity;
            //hsi反归一化
            h = h * 2 * Math.PI;
            s = s * (0xff * Math.Sqrt(2.0 / 3.0));
            i = i * Math.Sqrt(0xff * 0xff + 0xff * 0xff + 0xff * 0xff);
            //hsi -> xyz
            x = s * Math.Cos(h);
            y = s * Math.Sin(h);
            z = i;
            //xyz -> rgb
            r = x * Math.Sqrt(2.0 / 3.0) + z / Math.Sqrt(3);
            g = -x / 2 + y * Math.Sqrt(3) / 2 + z / Math.Sqrt(3);
            b = -x / 2 - y * Math.Sqrt(3) / 2 + z / Math.Sqrt(3);
            //限制rgb超出范围
            r = Math.Max(0, Math.Min(0xff, r));
            g = Math.Max(0, Math.Min(0xff, g));
            b = Math.Max(0, Math.Min(0xff, b));
            return Color.FromArgb(0xff, Convert.ToByte(r), Convert.ToByte(g), Convert.ToByte(b));
        }
    }
}
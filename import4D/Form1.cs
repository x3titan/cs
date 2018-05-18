using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Odbc;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace import4D {
    public partial class Form1 : Form {
        private string odbcConnectionString;
        private OdbcConnection odbcConnection = new OdbcConnection();
        private TamPubWin1.LogFile log = new TamPubWin1.LogFile();
        private bool forceExit = false;
        private bool skip = false;

        public Form1() {
            InitializeComponent();
            log.listBox = listBox1;
        }

        private void button1_Click(object sender, EventArgs e) {
            DateTime startDay = new DateTime(2018, 4, 29);
            int dayCount = 4;
            log.writeLogWarning("====开始处理文件组，起始日期：" + startDay.ToString("yyyyMMdd") + "，处理天数：" + dayCount + "====");
            for (int i = 0; i < dayCount; i++) {
                string filename = "D:\\tam\\project\\nokiaBigData\\s1u_gen_konw_apn\\" +
                    startDay.ToString("yyyyMMdd") + "_known_apn_s1u_general.csv";
                startDay = startDay.AddDays(1);
                loadFileToDB(filename);
            }
            log.writeLogWarning("====文件组处理完成，共处理天数：" + dayCount + "====");
        }

        private void Form1_Load(object sender, EventArgs e) {
            string configFile = TamPub1.FileOperation.currentFilePath + "config.xml";
            TamPubWin1.Etc.loadDesktop(configFile, this);
            odbcConnectionString = TamPub1.ConfigFileXml.readString(configFile, "odbcConnectionString", "DSN=Peijia;UID=sa;PWD=zaq12wsx;");
            //textBox1.Text = TamPub1.ConfigFileXml.readString(configFile, "textBox1Text", "0");
            timer1.Enabled = true;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e) {
            string configFile = TamPub1.FileOperation.currentFilePath + "config.xml";
            TamPubWin1.Etc.saveDesktop(configFile, this);
            //TamPub1.ConfigFileXml.writeString(configFile, "textBox1Text", textBox1.Text);
        }

        private void timer1_Tick(object sender, EventArgs e) {

        }

        private int dimTimeOnCompare(string value1, string value2) {
            return value1.CompareTo(value2);
        }

        private bool loadFileToDB(string filename) {
            log.writeLogWarning("====开始处理文件，预计处理时间1小时：" + filename);

            System.IO.FileStream fs = null;
            System.IO.BinaryReader reader;
            //===============================
            //0:xDR_ID 一个会话生成一个xDR ID。>> 无用 >> ok
            //1:IMSI 用户IMSI（TBCD编码） >> 15位 >> ok
            //2:MSISDN 用户号码（TBCD编码）>> 手机号,13-??位 >> ok
            //3:Cell_ID UE所在小区的ECI >> 整数？>> 需要增加一个跳变次数
            //4:APN APN >> 需要入库至手机号表 >> ok
            //5:App_Type_Code 业务类型编码，参见附录D XDR类型编码定义 >> 要个编码表 >> 还没想好怎么弄???
            //6:Procedure_Start_Time TCP/ UDP流开始时间，UTC时间 - 毫秒 >> 需要解码时间
            //7:Procedure_End_Time TCP/ UDP流结束时间，UTC时间 - 毫秒 >> 需要解码时间
            //8:App_Type 应用大类—集团18种应用大类，参见《数据流量业务大类分类》 >> 要个编码表 >> 还没想好怎么弄???
            //9:App_Sub_type 应用小类 >> 要个编码规则 >> 还没想好怎么弄???
            //
            //10:USER_IPv4 终端用户的IPv4地址 >> 7-15长度
            //11:User_Port 用户的四层端口号 >> 谁是服务器，反向连接怎么表示？
            //12:L4_protocal L4协议类型：0：TCP；1：UDP >> 1位长度
            //13:App_Server_IP_IPv4  访问服务器的IPv4地址 >> 7-15位长度
            //14:App_Server_Port 访问的服务器的端口 >> 谁是服务器，反向连接怎么表示？
            //15:UL_Data 上行流量；单位：字节 >> 整数
            //16:DL_Data 下行流量；单位：字节 >> 整数
            //17:UL_IP_Packet    上行IP包数 >> 整数
            //18:DL_IP_Packet    下行IP包数 >> 整数
            //19:TCP_Try_Cnt TCP建链尝试次数，一次TCP流多次SYN的数值; 非TCP传输时，此字段填0 >> 整数
            //20:TCP_Link_flag   TCP连接状态指示；0：成功；1：失败 >> 255代表什么？
            //21:Talk_if_end 会话是否结束标志；1：结束 （用户中断）2：未结束   3：超时  4: 拆分会话的第一话单   5：结束 （服务端中断） >> 整数
            //===============================
            //                   0              1               2       3       4                             5       6           7           8  9   10             11    12 13              14   15   16 17 18 19 20  21                  
            //00008c10045a0e95c498,460041238803470,861064723883470,141986486,cmiotfz.sh.mnc004.mcc460.gprs,  100,1524588437627,1524588437683,21,8921,10.4.38.181,   0,    1, 114.114.114.114,0,   84,  84,1, 1, 0, 255,3
            //00008810545a3772ea0e,460027172840010,8613484491206,  242843531,temp.sn.mnc002.mcc460.gprs,     100,1524598898676,1524598913716,21,143, 100.6.132.46,  47680,0, 223.202.195.96, 443, 300, 0, 5, 0, 5, 1,  2
            //00008810545a3772ea00,460040096750197,861064851419087,7527307,  cmiotbyda.sn.mnc004.mcc460.gprs,100,1524598898656,1524598898695,8, 7549,10.246.119.29, 53661,0, 192.168.0.250,  5000,52,  52,1, 1, 0, 1,  2
            //00008810745a37722dd5,460008072539994,8613758069490,  28010507, OTA.ZJ.mnc000.mcc460.gprs,      100,1524598898731,1524598906076,13,40,  104.13.201.144,49839,0, 106.11.248.1,   80,  120, 0, 2, 0, 2, 1,  2
            //00008210545a67994584,460002605393612,8613927041963,  141269025,BOSSTJFT.GD.mnc000.mcc460.gprs, 100,1524611225191,1524611225191,12,1,   10.144.76.65,  52318,0, 123.151.10.164, 443, 1215,0, 1, 0, 0, 1,  2

            //char[] buff = reader.ReadChars(10000);
            //log.writeLogCommon(new string(buff));

            //大方向：读取入库至apnDay180

            //5. 获取文件名包含的主要日期
            string currentDayString;
            currentDayString = TamPub1.FileOperation.extractFileName(filename).Substring(0, 8);
            log.writeLogCommon("====5.根据文件名获取主要日期子串为：" + currentDayString);

            //10. 初始化内存手机表
            log.writeLogCommon("====10.初始化内存手机表====");
            PhoneNumber phoneNumberList = new PhoneNumber();
            phoneNumberList.init();

            //20. 扫描文件获取手机号列表
            log.writeLogCommon("====20.扫描文件获取手机号列表====");
            if (!System.IO.File.Exists(filename)) {
                log.writeLogWarning("文件不存在, filename=" + filename);
                return false;
            }
            try {
                fs = new System.IO.FileStream(filename, System.IO.FileMode.Open);
            } catch (Exception ex) {
                log.writeLogWarning("无法读取文件,reason=" + ex.Message + ", filename=" + filename);
                return false;
            }
            reader = new System.IO.BinaryReader(fs, System.Text.Encoding.GetEncoding("GBK"));
            string buff = "";
            string oneLine;
            int lineStartPos, sp;
            int buffSize = 10000;
            int batchCount = 0, linesCount = 0;
            Int64 bytesRead = 0;
            while (true) {
                if (skip) break;
                Application.DoEvents();
                if (forceExit) break;
                if (batchCount % 1000 == 0) {
                    log.logDisplay.sameLine();
                    log.writeLogCommon("批次:" + batchCount.ToString("N0") +
                        " / 处理字符:" + bytesRead / 1000000 + "M" +
                        " / 读取记录:" + linesCount.ToString("N0") +
                        " / 手机号:" + phoneNumberList.items.Count.ToString("N0")
                        );
                }

                //read one batch
                string fBuff = new string(reader.ReadChars(buffSize));
                bytesRead += fBuff.Length;
                if (fBuff.Length <= 0) break; //eof
                buff += fBuff.Replace("\r\n", "\n").Replace('\r', '\n');
                lineStartPos = 0;
                batchCount++;
                //if (batchCount > 50) break;
                while (true) {
                    //read line
                    sp = buff.IndexOf('\n', lineStartPos);
                    if (sp < 0) break;
                    oneLine = buff.Substring(lineStartPos, sp - lineStartPos);
                    lineStartPos = sp + 1;
                    linesCount++;
                    //decode line
                    string[] fields = oneLine.Split(',');
                    if (fields.Length != 22) {
                        log.writeLogWarning("非法记录行：" + oneLine);
                        continue;
                    }
                    PhoneNumber.Item phoneNumberItem = phoneNumberList.newItem();
                    phoneNumberItem.imsi = fields[1];
                    phoneNumberItem.phoneNumber = fields[2];
                    phoneNumberList.push(phoneNumberItem);
                }
                buff = buff.Substring(lineStartPos);
            }
            fs.Close();

            //30. 根据手机号列表创建apnDay180框架
            log.writeLogCommon("====30.根据手机号列表创建apnDay180框架====");
            StringBuilder sql = new StringBuilder();
            for (int i = 0; i < phoneNumberList.items.Count; i++) {
                if (skip) break;
                sql.Append(
                    "exec [dbo].[createImsi] " +
                    "'" + phoneNumberList.items[i].imsi + "'" +
                    ", '" + phoneNumberList.items[i].phoneNumber + "';\r\n"
                    );
                if ((i % 10 != 0) && (i < phoneNumberList.items.Count - 1)) continue;
                Application.DoEvents();
                if (forceExit) break;
                log.logDisplay.sameLine();
                log.writeLogCommon(
                    "初始化手机号:" + (i + 1).ToString("N0") + "/" + phoneNumberList.items.Count.ToString("N0")
                    );
                if (!execSqlN(sql.ToString())) return false;
                sql.Clear();
            }

            //35. 获取当前日期序列号
            int dateSn = 0;
            log.writeLogCommon("====35.获取当前日期序列号==== 由于未上线，此步骤跳过，人工指定日期序列号为：" + dateSn);

            //37. 读取并更新日期维度表，由于目前没有上线，直接产生此表
            log.writeLogCommon("====37.读取并更新日期维度表，由于目前没有上线，直接产生此表====");
            TamPub1.SpeedSearch<string> dimTime = new TamPub1.SpeedSearch<string>();
            DateTime dimTimePos = new DateTime(2018, 4, 23);
            for (int i = 0; i < 30 * 3; i++) {
                dimTime.buff.Add(dimTimePos.ToString("yyyyMMdd"));
                dimTimePos = dimTimePos.AddDays(1);
            }
            dimTime.onCompare = dimTimeOnCompare;

            //40. 扫描每行产生update
            /*
            if (!System.IO.File.Exists(filename)) {
                log.writeLogWarning("文件不存在, filename=" + filename);
                return false;
            }
            try {
                fs = new System.IO.FileStream(filename, System.IO.FileMode.Open);
            } catch (Exception ex) {
                log.writeLogWarning("无法读取文件,reason=" + ex.Message + ", filename=" + filename);
                return false;
            }
            reader = new System.IO.BinaryReader(fs, System.Text.Encoding.GetEncoding("GBK"));
            batchCount = 0;
            bytesRead = 0;
            linesCount = 0;
            sql.Clear();
            int updateCount = 0;
            while (true) {
                Application.DoEvents();
                if (forceExit) break;
                log.logDisplay.sameLine();
                log.writeLogCommon("批次:" + batchCount.ToString("N0") +
                    " / 处理字符:" + bytesRead / 1000000 + "M" +
                    " / 读取记录:" + linesCount.ToString("N0")
                    );

                //read one batch
                string fBuff = new string(reader.ReadChars(buffSize));
                bytesRead += fBuff.Length;
                if (fBuff.Length <= 0) break; //eof
                buff += fBuff.Replace("\r\n", "\n").Replace('\r', '\n');
                lineStartPos = 0;
                batchCount++;
                //if (batchCount > 50) break;
                while (true) {
                    //read line
                    sp = buff.IndexOf('\n', lineStartPos);
                    if (sp < 0) break;
                    oneLine = buff.Substring(lineStartPos, sp - lineStartPos);
                    lineStartPos = sp + 1;
                    linesCount++;
                    //decode line
                    string[] fields = oneLine.Split(',');
                    if (fields.Length != 22) {
                        log.writeLogWarning("非法记录行：" + oneLine);
                        continue;
                    }
                    //解码开始时间(忽略结束时间)
                    DateTime startTime = decodeDatetime(fields[6]);

                    //根据时间计算出时间维度坐标
                    if (!dimTime.search(startTime.ToString("yyyyMMdd"))) {
                        log.writeLogWarning("检测到超出维度范围的时间标记：" + oneLine);
                        continue;
                    }
                    int dimTimeValue = dimTime.pos;

                    //根据时间计算出时段维度坐标
                    int dimDayHourValue = Convert.ToInt32(startTime.Hour) / 3; //目前分割为3小时一个时段

                    //产生更新apn字段语句
                    sql.Append(
                        "update imsiInfo set apn = '" + fields[4] +
                        "' where imsi = '" + fields[1] + "';\r\n"
                        );

                    //产生更新day180表语句
                    //======================
                    //12:L4_protocal L4协议类型：0：TCP；1：UDP >> 1位长度
                    //15:UL_Data 上行流量；单位：字节 >> 整数
                    //16:DL_Data 下行流量；单位：字节 >> 整数
                    //17:UL_IP_Packet    上行IP包数 >> 整数
                    //18:DL_IP_Packet    下行IP包数 >> 整数
                    //19:TCP_Try_Cnt TCP建链尝试次数，一次TCP流多次SYN的数值; 非TCP传输时，此字段填0 >> 整数
                    //20:TCP_Link_flag   TCP连接状态指示；0：成功；1：失败 >> 255代表什么？
                    if (fields[12].Equals("0")) { //tcp
                        sql.Append(
                            "update apnDay180 set " +
                            " tcpUploadBytes = tcpUploadBytes + " + fields[15] +
                            ",tcpDownloadBytes = tcpDownloadBytes + " + fields[16] +
                            ",tcpUploadPackets = tcpUploadPackets + " + fields[17] +
                            ",tcpDownloadPackets = tcpDownloadPackets + " + fields[18] +
                            ",tcpRetryCount = tcpRetryCount + " + fields[19] +
                            " where" +
                            " imsi = '" + fields[1] + "' " +
                            " and dayIndex = " + dimTimeValue +
                            " and dayHour = " + dimDayHourValue +
                            ";\r\n"
                            );
                    } else if (fields[12].Equals("1")) { //udp
                        sql.Append(
                            "update apnDay180 set " +
                            " udpUploadBytes = udpUploadBytes + " + fields[15] +
                            ",udpDownloadBytes = udpDownloadBytes + " + fields[16] +
                            ",udpUploadPackets = udpUploadPackets + " + fields[17] +
                            ",udpDownloadPackets = udpDownloadPackets + " + fields[18] +
                            " where" +
                            " imsi = '" + fields[1] + "' " +
                            " and dayIndex = " + dimTimeValue +
                            " and dayHour = " + dimDayHourValue +
                            ";\r\n"
                            );
                    } else {
                        log.writeLogWarning("检测到未知类型的包结构：" + oneLine);
                        continue;
                    }
                    updateCount++;
                    //ip地址问题, 数据方向问题，数据类型问题

                    //择机执行sql
                    if (updateCount >= 500) {
                        execSqlN(sql.ToString());
                        updateCount = 0;
                        sql.Clear();
                    }

                }
                buff = buff.Substring(lineStartPos);
            }
            //执行尾部的sql
            if (updateCount > 0) {
                execSqlN(sql.ToString());
                updateCount = 0;
                sql.Clear();
            }
            fs.Close();
            */

            log.writeLogCommon("====40.扫描文件数据进入高速缓存====");
            if (!System.IO.File.Exists(filename)) {
                log.writeLogWarning("文件不存在, filename=" + filename);
                return false;
            }
            try {
                fs = new System.IO.FileStream(filename, System.IO.FileMode.Open);
            } catch (Exception ex) {
                log.writeLogWarning("无法读取文件,reason=" + ex.Message + ", filename=" + filename);
                return false;
            }
            reader = new System.IO.BinaryReader(fs, System.Text.Encoding.GetEncoding("GBK"));
            batchCount = 0;
            bytesRead = 0;
            linesCount = 0;
            sql.Clear();
            int updateCount = 0;
            int outbound1 = 0;
            int outbound2 = 0;
            while (true) {
                Application.DoEvents();
                if (forceExit) break;
                //if (phoneNumberList.items.Count > 3000) break;
                if (batchCount % 1000 == 0) {
                    log.logDisplay.sameLine();
                    log.writeLogCommon("批次:" + batchCount.ToString("N0") +
                        " / 处理字符:" + bytesRead / 1000000 + "M" +
                        " / 读取记录:" + linesCount.ToString("N0") +
                        " / 手机号:" + phoneNumberList.items.Count.ToString("N0") +
                        " / 时间维度越界次数：" + outbound1 + "/" + outbound2
                        );
                }

                //read one batch
                string fBuff = new string(reader.ReadChars(buffSize));
                bytesRead += fBuff.Length;
                if (fBuff.Length <= 0) break; //eof
                buff += fBuff.Replace("\r\n", "\n").Replace('\r', '\n');
                lineStartPos = 0;
                batchCount++;
                //if (batchCount > 50) break;
                while (true) {
                    //read line
                    sp = buff.IndexOf('\n', lineStartPos);
                    if (sp < 0) break;
                    oneLine = buff.Substring(lineStartPos, sp - lineStartPos);
                    lineStartPos = sp + 1;
                    linesCount++;
                    //decode line
                    string[] fields = oneLine.Split(',');
                    if (fields.Length != 22) {
                        log.writeLogWarning("非法记录行：" + oneLine);
                        continue;
                    }
                    //解码开始时间(忽略结束时间)
                    DateTime startTime = decodeDatetime(fields[6]);
                    startTime = startTime.AddMinutes(2); //增加x分钟的偏移校正
                    if (startTime.ToString("yyyyMMdd").CompareTo(currentDayString) < 0) {
                        startTime = DateTime.ParseExact(currentDayString, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                        outbound1++;
                    } else if (startTime.ToString("yyyyMMdd").CompareTo(currentDayString) > 0) {
                        startTime = DateTime.ParseExact(currentDayString + "235959", "yyyyMMddHHmmss", System.Globalization.CultureInfo.CurrentCulture);
                        outbound2++;
                    }

                    //根据时间计算出时段维度坐标
                    int dimDayHourValue = Convert.ToInt32(startTime.Hour) / 3; //目前分割为3小时一个时段

                    //抓取数据
                    //======================
                    //12:L4_protocal L4协议类型：0：TCP；1：UDP >> 1位长度
                    //15:UL_Data 上行流量；单位：字节 >> 整数
                    //16:DL_Data 下行流量；单位：字节 >> 整数
                    //17:UL_IP_Packet    上行IP包数 >> 整数
                    //18:DL_IP_Packet    下行IP包数 >> 整数
                    //19:TCP_Try_Cnt TCP建链尝试次数，一次TCP流多次SYN的数值; 非TCP传输时，此字段填0 >> 整数
                    //20:TCP_Link_flag   TCP连接状态指示；0：成功；1：失败 >> 255代表什么？
                    PhoneNumber.Item item = phoneNumberList.newItem();
                    item.imsi = fields[1];
                    item.phoneNumber = fields[2];
                    item.apn = fields[4];
                    if (fields[12].Equals("0")) { //tcp
                        item.dayHour[dimDayHourValue].tcpUploadBytes = Convert.ToInt32(fields[15]);
                        item.dayHour[dimDayHourValue].tcpDownloadBytes = Convert.ToInt32(fields[16]);
                        item.dayHour[dimDayHourValue].tcpUploadPackets = Convert.ToInt32(fields[17]);
                        item.dayHour[dimDayHourValue].tcpDownloadPackets = Convert.ToInt32(fields[18]);
                        item.dayHour[dimDayHourValue].tcpRetryCount = Convert.ToInt32(fields[19]);
                    } else if (fields[12].Equals("1")) { //udp
                        item.dayHour[dimDayHourValue].udpUploadBytes = Convert.ToInt32(fields[15]);
                        item.dayHour[dimDayHourValue].udpDownloadBytes = Convert.ToInt32(fields[16]);
                        item.dayHour[dimDayHourValue].udpUploadPackets = Convert.ToInt32(fields[17]);
                        item.dayHour[dimDayHourValue].udpDownloadPackets = Convert.ToInt32(fields[18]);
                    } else {
                        log.writeLogWarning("检测到未知类型的包结构：" + oneLine);
                        continue;
                    }

                    //增加或者查找手机号码
                    phoneNumberList.update(item);
                    updateCount++;

                    //ip地址问题, 数据方向问题，数据类型问题
                }
                buff = buff.Substring(lineStartPos);
            }
            fs.Close();

            //45.计算数据库时间维度偏移量
            if (!dimTime.search(currentDayString)) {
                log.writeLogWarning("文件提取时间超出范围：" + currentDayString);
                return false;
            }
            int dimTimeOffset = dimTime.pos;
            //dimTimeOffset--;
            //if (dimTimeOffset < 0) dimTimeOffset = daySize - 1;
            log.writeLogCommon("====45.计算数据库时间维度偏移量:" + dimTimeOffset + "====");

            //50. 高速缓存数据入库
            log.writeLogCommon("====50.高速缓存数据入库====");
            sql.Clear();
            for (int i = 0; i < phoneNumberList.items.Count; i++) {
                Application.DoEvents();
                if (forceExit) break;
                if ((i % 500 == 0) || (i == phoneNumberList.items.Count - 1)) {
                    log.logDisplay.sameLine();
                    log.writeLogCommon(
                        "正在入库高速缓存：" + (i + 1).ToString("N0") +
                        " / " + phoneNumberList.items.Count.ToString("N0")
                        );
                    if (sql.Length > 0) {
                        execSqlN(sql.ToString());
                        sql.Clear();
                    }
                }
                //产生更新手机表的语句
                sql.Append(
                    "update imsiInfo set " +
                    " phoneNumber='" + phoneNumberList.items[i].phoneNumber + "'" +
                    ",apn='" + phoneNumberList.items[i].apn + "'" +
                    " where imsi='" + phoneNumberList.items[i].imsi + "';\r\n"
                    );

                //产生更新day180表的语句
                for (int hourIndex = 0; hourIndex < 24 / 3; hourIndex++) {
                    sql.Append(
                        "update apnDay180 set " +
                        " tcpUploadBytes=" + phoneNumberList.items[i].dayHour[hourIndex].tcpUploadBytes +
                        ",tcpDownloadBytes=" + phoneNumberList.items[i].dayHour[hourIndex].tcpDownloadBytes +
                        ",udpUploadBytes=" + phoneNumberList.items[i].dayHour[hourIndex].udpUploadBytes +
                        ",udpDownloadBytes=" + phoneNumberList.items[i].dayHour[hourIndex].udpDownloadBytes +
                        ",tcpUploadPackets=" + phoneNumberList.items[i].dayHour[hourIndex].tcpUploadPackets +
                        ",tcpDownloadPackets=" + phoneNumberList.items[i].dayHour[hourIndex].tcpDownloadPackets +
                        ",udpUploadPackets=" + phoneNumberList.items[i].dayHour[hourIndex].udpUploadPackets +
                        ",udpDownloadPackets=" + phoneNumberList.items[i].dayHour[hourIndex].udpDownloadPackets +
                        ",tcpRetryCount=" + phoneNumberList.items[i].dayHour[hourIndex].tcpRetryCount +
                        " where imsi='" + phoneNumberList.items[i].imsi + "'" +
                        " and dayIndex=" + dimTimeOffset +
                        " and dayHour=" + hourIndex +
                        ";\r\n"
                        );
                }
            }



            //60. 更新当前日期序列号

            //100. 完成

            log.writeLogWarning("第一步处理(day180)完成");
            return true;
        }

        /// <summary>执行一个无返回值的sql语句</summary>
        private bool execSqlN(string sql) {
            OdbcCommand cmd = new OdbcCommand();
            cmd.CommandTimeout = 300;
            cmd.Connection = odbcConnection;
            cmd.CommandText = sql;
            try {
                cmd.ExecuteNonQuery();
            } catch (Exception ee) {
                log.writeLogWarning("数据库执行错误，准备执行重置操作。errorString=" + ee.Message + ",sql=" + sql);
                odbcConnection.Close();
                return false;
            }
            return true;
        }

        private void button2_Click(object sender, EventArgs e) {
            forceExit = !forceExit;
            log.writeLogCommon("设置强制退出模式为：" + forceExit.ToString());
        }

        private void button3_Click(object sender, EventArgs e) {
            log.writeLogWarning("尝试连接数据库");
            try {
                odbcConnection.ConnectionString = odbcConnectionString;
                odbcConnection.Open();
            } catch (Exception ee) {
                log.writeLogWarning("数据库连接失败,errorString=" + ee.Message);
                timer1.Enabled = true;
                return;
            }
            log.writeLogWarning("数据库连接成功");
        }

        private void button4_Click(object sender, EventArgs e) {
            skip = !skip;
            log.writeLogCommon("设置skip模式为：" + skip.ToString());
        }

        /// <summary>解码并转换为北京时间</summary>
        private DateTime decodeDatetime(string value) {
            long a;
            DateTime result = new DateTime(1970, 1, 1);
            try {
                a = Convert.ToInt64(value);
            } catch {
                return result;
            }
            return result.AddMilliseconds(a + 8 * 3600 * 1000); //东8区
        }

        private void button5_Click(object sender, EventArgs e) {

        }
    }
}

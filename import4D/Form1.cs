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

        public Form1() {
            InitializeComponent();
            log.listBox = listBox1;
        }

        private void button1_Click(object sender, EventArgs e) {

            testLoad();
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

        private bool testLoad() {
            string filename = "D:\\tam\\project\\nokiaBigData\\s1u_gen_konw_apn\\20180425_known_apn_s1u_general.csv";
            System.IO.FileStream fs = null;
            System.IO.BinaryReader reader;

            //===============================
            //0:xDR_ID 一个会话生成一个xDR ID。>> 无用
            //1:IMSI 用户IMSI（TBCD编码） >> 15位
            //2:MSISDN 用户号码（TBCD编码）>> 手机号,13-??位
            //3:Cell_ID UE所在小区的ECI >> 整数？
            //4:APN APN >> ??
            //5:App_Type_Code 业务类型编码，参见附录D XDR类型编码定义 >> 要个编码表
            //6:Procedure_Start_Time TCP/ UDP流开始时间，UTC时间 - 毫秒 >> 需要解码时间
            //7:Procedure_End_Time TCP/ UDP流结束时间，UTC时间 - 毫秒 >> 需要解码时间
            //8:App_Type 应用大类—集团18种应用大类，参见《数据流量业务大类分类》 >> 要个编码表
            //9:App_Sub_type 应用小类 >> 要个编码规则
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
            int lineStartPos, sp, sp1;
            int buffSize = 10000;
            int batchCount = 0, linesCount = 0;
            Int64 bytesRead = 0;
            while (true) {
                //test
                if (batchCount > 5000) break;

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
                    PhoneNumber.Item phoneNumberItem = new PhoneNumber.Item();
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
                    "初始化手机号:" + i.ToString("N0") + "/" + phoneNumberList.items.Count.ToString("N0")
                    );
                if (!execSqlN(sql.ToString())) return false;
                sql.Clear();
            }


            //35. 获取当前日期序列号

            //40. 扫描每行产生update

            //50. 更新当前日期序列号

            //100. 完成


            return true;
        }

        /// <summary>执行一个无返回值的sql语句</summary>
        private bool execSqlN(string sql) {
            OdbcCommand cmd = new OdbcCommand();
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
            forceExit = true;
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
    }
}

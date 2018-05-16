using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace import4D {
    public class PhoneNumber {
        TamPub1.SpeedSearch<Item> speedSearch = new TamPub1.SpeedSearch<Item>();
        public struct Item {
            public string imsi;
            public string phoneNumber;
            public string apn;
            public Day[] day;
        }
        public struct Day {
            public DayHour[] dayHour;
        }
        public struct DayHour {
            public int tcpUploadBytes;
            public int tcpDownloadBytes;
            public int udpUploadBytes;
            public int udpDownloadBytes;
            public int tcpUploadPackets;
            public int tcpDownloadPackets;
            public int udpUploadPackets;
            public int udpDownloadPackets;
            public int tcpRetryCount;
        }

        /// <summary>使用之前必须初始化</summary>
        public void init() {
            speedSearch.buff.Clear();
            speedSearch.onCompare = OnCompare;
        }

        public List<Item> items {
            get {
                return speedSearch.buff;
            }
        }

        public Item newItem() {
            Item item;
            item.imsi = "";
            item.phoneNumber = "";
            item.apn = "";
            item.day = new Day[2];
            for (int i = 0; i < item.day.Length; i++) {
                item.day[i].dayHour = new DayHour[24 / 3];
                for (int j = 0; j < item.day[i].dayHour.Length; j++) {
                    item.day[i].dayHour[j].tcpDownloadBytes = 0;
                    item.day[i].dayHour[j].tcpDownloadPackets = 0;
                    item.day[i].dayHour[j].tcpRetryCount = 0;
                    item.day[i].dayHour[j].tcpUploadBytes = 0;
                    item.day[i].dayHour[j].tcpUploadPackets = 0;
                    item.day[i].dayHour[j].udpDownloadBytes = 0;
                    item.day[i].dayHour[j].udpDownloadPackets = 0;
                    item.day[i].dayHour[j].udpUploadBytes = 0;
                    item.day[i].dayHour[j].udpUploadPackets = 0;
                }
            }
            return item;
        }

        /// <summary>增加一个手机号码</summary>
        public void push(Item item) {
            if (speedSearch.search(item)) return;
            speedSearch.buff.Insert(speedSearch.pos, item);
        }

        /// <summary>更新数据，没有则新建</summary>
        public void update(Item item) {
            if (speedSearch.search(item)) {
                Item temp = speedSearch.buff[speedSearch.pos];
                temp.apn = item.apn;
                if (temp.phoneNumber.Length <= 0) temp.phoneNumber = item.phoneNumber;
                for (int i = 0; i < 2; i++) {
                    for (int j = 0; j < 24 / 3; j++) {
                        temp.day[i].dayHour[j].tcpDownloadBytes += item.day[i].dayHour[j].tcpDownloadBytes;
                        temp.day[i].dayHour[j].tcpDownloadPackets += item.day[i].dayHour[j].tcpDownloadPackets;
                        temp.day[i].dayHour[j].tcpRetryCount += item.day[i].dayHour[j].tcpRetryCount;
                        temp.day[i].dayHour[j].tcpUploadBytes += item.day[i].dayHour[j].tcpUploadBytes;
                        temp.day[i].dayHour[j].tcpUploadPackets += item.day[i].dayHour[j].tcpUploadPackets;
                        temp.day[i].dayHour[j].udpDownloadBytes += item.day[i].dayHour[j].udpDownloadBytes;
                        temp.day[i].dayHour[j].udpDownloadPackets += item.day[i].dayHour[j].udpDownloadPackets;
                        temp.day[i].dayHour[j].udpUploadBytes += item.day[i].dayHour[j].udpUploadBytes;
                        temp.day[i].dayHour[j].udpUploadPackets += item.day[i].dayHour[j].udpUploadPackets;
                    }
                }
                speedSearch.buff[speedSearch.pos] = temp;
            } else {
                speedSearch.buff.Insert(speedSearch.pos, item);
            }
        }

        private int OnCompare(Item value1, Item value2) {
            return value1.imsi.CompareTo(value2.imsi);
        }
    }
}

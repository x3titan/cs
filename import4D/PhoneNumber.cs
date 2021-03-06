﻿using System;
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
            item.dayHour = new DayHour[24 / 3];
            for (int j = 0; j < item.dayHour.Length; j++) {
                item.dayHour[j].tcpDownloadBytes = 0;
                item.dayHour[j].tcpDownloadPackets = 0;
                item.dayHour[j].tcpRetryCount = 0;
                item.dayHour[j].tcpUploadBytes = 0;
                item.dayHour[j].tcpUploadPackets = 0;
                item.dayHour[j].udpDownloadBytes = 0;
                item.dayHour[j].udpDownloadPackets = 0;
                item.dayHour[j].udpUploadBytes = 0;
                item.dayHour[j].udpUploadPackets = 0;
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
                for (int j = 0; j < 24 / 3; j++) {
                    temp.dayHour[j].tcpDownloadBytes += item.dayHour[j].tcpDownloadBytes;
                    temp.dayHour[j].tcpDownloadPackets += item.dayHour[j].tcpDownloadPackets;
                    temp.dayHour[j].tcpRetryCount += item.dayHour[j].tcpRetryCount;
                    temp.dayHour[j].tcpUploadBytes += item.dayHour[j].tcpUploadBytes;
                    temp.dayHour[j].tcpUploadPackets += item.dayHour[j].tcpUploadPackets;
                    temp.dayHour[j].udpDownloadBytes += item.dayHour[j].udpDownloadBytes;
                    temp.dayHour[j].udpDownloadPackets += item.dayHour[j].udpDownloadPackets;
                    temp.dayHour[j].udpUploadBytes += item.dayHour[j].udpUploadBytes;
                    temp.dayHour[j].udpUploadPackets += item.dayHour[j].udpUploadPackets;
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

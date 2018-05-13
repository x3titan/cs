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

        /// <summary>增加一个手机号码</summary>
        public void push(Item item) {
            if (speedSearch.search(item)) return;
            speedSearch.buff.Insert(speedSearch.pos, item);
        }

        private int OnCompare(Item value1, Item value2) {
            return value1.imsi.CompareTo(value2.imsi);
        }

    }
}

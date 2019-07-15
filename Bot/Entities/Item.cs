using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Entities
{
    class Item
    {
        public Item(string name, long price)
        {
            this.name = name;
            this.price = price;
        }
        public string name { get; set;  }
        public long price { get; set; }
    }
}

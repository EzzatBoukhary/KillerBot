/* using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Entities
{
    public class Item
    {
        public Item(string name, long price, string description)
        {
            this.name = name;
            this.price = price;
            this.description = description;
            Duration = new TimeSpan(long.MaxValue);
        }

        public Item(string name, long price, string description, TimeSpan duration)
        {
            this.name = name;
            this.price = price;
            this.description = description;
            Duration = duration;
        }

        public Item(Item other)
        {
            this.name = other.name;
            this.price = other.price;
            this.description = description;
        }

        public string name { get; private set;  }
        public long price { get; private set; }
        public string description { get; private set; }
        public TimeSpan Duration { get; private set; }
    }

    public class UserItem : Item
    {
        public DateTime Date { get; set; }

        public UserItem(string name, long price, string description, DateTime date) : base(name, price, description)
        {
            this.Date = date;
        }

        [JsonConstructor]
        public UserItem(string name, long price, DateTime date, string description, TimeSpan duration) : base(name, price, description, duration)
        {
            this.Date = date;
        }

        public UserItem(Item item) : base(item)
        {
            this.Date = DateTime.UtcNow;
        }
    }
} */

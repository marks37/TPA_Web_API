using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TPA_Web_API.Models
{
    public class Initiative
    {
        public int ID { get; set; }
        public string TPB_ID { get; set; }
        public string Year { get; set; }
        public string Month { get; set; }
        public string Initiative_Type { get; set; }
        public string Initiative_Sub_Type { get; set; }
        public string Category { get; set; }
        public string Division { get; set; }
        public string Brand { get; set; }
        public string Initiative_Title { get; set; }
        public Nullable<System.DateTime> Start_Date { get; set; }
        public Nullable<System.DateTime> End_Date { get; set; }
        public Nullable<System.DateTime> Sell_in_date { get; set; }
    }
}
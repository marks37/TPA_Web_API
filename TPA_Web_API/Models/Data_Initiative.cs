//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TPA_Web_API.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Data_Initiative
    {
        public int ID { get; set; }
        public string TPB_ID { get; set; }
        public string Year { get; set; }
        public string Month { get; set; }
        public string Channel { get; set; }
        public string Agency { get; set; }
        public string Initiative_Type { get; set; }
        public string Initiative_Sub_Type { get; set; }
        public string Category { get; set; }
        public string Division { get; set; }
        public string Brand { get; set; }
        public string Initiative_Title { get; set; }
        public string Materials { get; set; }
        public Nullable<System.DateTime> Start_Date { get; set; }
        public Nullable<System.DateTime> End_Date { get; set; }
        public string Account { get; set; }
        public Nullable<System.DateTime> Sell_in_date { get; set; }
        public Nullable<int> Allocation { get; set; }
        public System.DateTime Created_At { get; set; }
        public string Created_By { get; set; }
        public Nullable<System.DateTime> Modified_At { get; set; }
        public string Modified_By { get; set; }
    }
}

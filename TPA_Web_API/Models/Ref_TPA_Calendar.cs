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
    
    public partial class Ref_TPA_Calendar
    {
        public int tpa_calendar_id { get; set; }
        public string year_period { get; set; }
        public string month_name { get; set; }
        public string month_alias { get; set; }
        public Nullable<int> month_number { get; set; }
        public Nullable<int> week_number { get; set; }
        public Nullable<System.DateTime> week_start { get; set; }
        public Nullable<System.DateTime> week_end { get; set; }
        public Nullable<System.DateTime> cutoff_date { get; set; }
        public Nullable<System.DateTime> created_at { get; set; }
        public string created_by { get; set; }
        public Nullable<System.DateTime> modified_at { get; set; }
        public string modified_by { get; set; }
        public Nullable<System.DateTime> deleted_at { get; set; }
        public string deleted_by { get; set; }
    }
}

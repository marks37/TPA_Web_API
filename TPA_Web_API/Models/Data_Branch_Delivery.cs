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
    
    public partial class Data_Branch_Delivery
    {
        public int branch_delivery_id { get; set; }
        public Nullable<int> branch_allocation_id { get; set; }
        public Nullable<System.DateTime> branch_delivery_date { get; set; }
        public Nullable<int> branch_delivery_status_id { get; set; }
        public string branch_delivery_status { get; set; }
        public string remarks { get; set; }
        public Nullable<System.DateTime> created_at { get; set; }
        public string created_by { get; set; }
        public Nullable<System.DateTime> modified_at { get; set; }
        public string modified_by { get; set; }
        public string source { get; set; }
    
        public virtual Data_Branch_Allocation Data_Branch_Allocation { get; set; }
    }
}
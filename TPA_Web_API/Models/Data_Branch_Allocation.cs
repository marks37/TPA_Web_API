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
    
    public partial class Data_Branch_Allocation
    {
        public int ID { get; set; }
        public string TPB_ID { get; set; }
        public string Account { get; set; }
        public Nullable<int> Branch_ID { get; set; }
        public Nullable<int> Branch_Allocation { get; set; }
        public string Remarks { get; set; }
        public string Materials { get; set; }
        public Nullable<System.DateTime> Created_At { get; set; }
        public string Created_By { get; set; }
    }
}

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
    
    public partial class Ref_Implem_Issue
    {
        public int implem_issue_id { get; set; }
        public Nullable<int> implem_status_id { get; set; }
        public string category { get; set; }
        public string issue { get; set; }
        public Nullable<System.DateTime> created_at { get; set; }
        public string created_by { get; set; }
        public Nullable<System.DateTime> modified_at { get; set; }
        public string modified_by { get; set; }
    
        public virtual Ref_Implem_Status Ref_Implem_Status { get; set; }
    }
}

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
    
    public partial class Data_Initiative_Branch_File
    {
        public int initiative_branch_file_id { get; set; }
        public string file_name { get; set; }
        public string file_type { get; set; }
        public string file_category { get; set; }
        public string remarks { get; set; }
        public Nullable<System.DateTime> uploaded_at { get; set; }
        public string uploaded_by { get; set; }
        public Nullable<System.DateTime> modified_at { get; set; }
        public string modified_by { get; set; }
        public Nullable<System.DateTime> deleted_at { get; set; }
        public string deleted_by { get; set; }
        public Nullable<int> initiative_branch_line_id { get; set; }
    
        public virtual Data_Initiative_Branch_Line Data_Initiative_Branch_Line { get; set; }
    }
}

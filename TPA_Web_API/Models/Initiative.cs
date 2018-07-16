using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace TPA_Web_API.Models
{
    public class Mobile_Branch_Initiative
    {
        public int BranchID { get; set; }
        public List<Mobile_Initiative> InitiativeList { get; set; }
    }

    public class Mobile_Initiative
    {
        public string TPB_ID { get; set; }
        public string Year { get; set; }
        public string Month { get; set; }
        public string InitiativeType { get; set; }
        public string InitiativeSubType { get; set; }
        public string Category { get; set; }
        public string Division { get; set; }
        public string Brand { get; set; }
        public string InitiativeTitle { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public Nullable<System.DateTime> SellInDate { get; set; }
        public List<Mobile_Material> MaterialList { get; set; }

        public Nullable<int> WeekNo { get; set; }
        public Nullable<int> MonthNo { get; set; }
    }

    [DataContract]
    public class Mobile_Material
    {
        [DataMember]
        public string Material { get; set; }

        [DataMember]
        public Nullable<int> TpaDeliveryStatusId { get; set; }

        [DataMember]
        public string TpaDeliveryStatus { get; set; }

        [DataMember]
        public Nullable<int> BranchDeliveryStatusId { get; set; }
        [DataMember]
        public string BranchDeliveryStatus { get; set; }
        [DataMember]
        public Nullable<int> BranchAllocation { get; set; }
        [DataMember]
        public Nullable<int> RunningActualAllocation { get; set; }
        [DataMember]
        public Nullable<int> ImplemStatusId { get; set; }
        [DataMember]
        public string ImplemStatus { get; set; }
        //public Nullable<int> ImplemIssueId { get; set; }
        //public string ImplemIssue { get; set; }
        [DataMember]
        public string BundledPercent { get; set; }
        [DataMember]
        public Nullable<int> TotalOfftake { get; set; }
        [DataMember]
        public string OfftakePercent { get; set; }
        [DataMember]
        public Nullable<int> EndingInventory { get; set; }
        [DataMember]
        public string OfftakeAnalysis { get; set; }
        [IgnoreDataMember]
        public DateTime StartDate { get; set; }
        [IgnoreDataMember]
        public DateTime EndDate { get; set; }
        [DataMember]
        public int MaterialType { get; set; }
    }

    public class Mobile_Installation
    {
        public Nullable<int> BranchAllocationId { get; set; }
        public Nullable<int> Bundled { get; set; }
    }

    public class Mobile_Offtake
    {
        public Nullable<int> BranchAllocationId { get; set; }
        public Nullable<int> Offtake { get; set; }
    }
}
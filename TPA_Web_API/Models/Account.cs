using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TPA_Web_API.Models
{
    public class Mobile_Channel
    {
        public string Name { get; set; }
        public List<Mobile_Account> AccountList { get; set; }
    }

    public class Mobile_Account
    {
        public string Name { get; set; }
        public List<Mobile_Branch> BranchList { get; set; }
    }

    public class Mobile_Branch
    {
        public int BranchID { get; set; }
        public string BranchName { get; set; }
        public string BranchAddress { get; set; }
        public string TeamLead { get; set; }
        public string CDS { get; set; }
        public int InitiativeCount { get; set; }
        public List<Mobile_Initiative> InitiativeList { get; set; }
    }
}
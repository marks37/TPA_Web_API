using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TPA_Web_API.Models
{
    public class Mobile
    {
    }

    public class SyncModel
    {
        public int UserID;
        public string Username;
        public DateTime CreatedAt;
        public DateTime SyncAt;

        public String TPB_ID;
        public String Material;
        public int BranchID;

        public int BranchDeliveryID;
        public string BranchDeliveryStatus;

        public int ActualBundled;
        public int BundledPercent;
        public int ImplemIssueID;
        public string ImplemIssue;

        public int ActualOfftake;
        public string OfftakePercent;
        public string OfftakeAnalysis;

        public int WeekNo;
        public int MonthNo;
    }

    public class UserProfile
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Position { get; set; }
        public string Contact { get; set; }
        public string Address { get; set; }
        public Nullable<int> UserId { get; set; }
        public string Username { get; set; }
    }

    public class UserLogin
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

}
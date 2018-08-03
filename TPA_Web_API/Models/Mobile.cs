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

    //public class UserProfile
    //{
    //    public string Firstname { get; set; }
    //    public string Lastname { get; set; }
    //    public string Position { get; set; }
    //    public string Contact { get; set; }
    //    public string Address { get; set; }
    //    public Nullable<int> UserId { get; set; }
    //    public string Username { get; set; }
    //}

    #region Initiatives
    public class Initiatives
    {
        public int InitiativeBranchId { get; set; }
        public string TpbId { get; set; }
        public string SfaInternalId { get; set; }
        public string Material { get; set; }
        public int? TpaDeliveryStatusId { get; set; }
        public string TpaDeliveryStatus { get; set; }
        public string DateDeliveredToTpa { get; set; }
        public int? BranchDeliveryStatusId { get; set; }
        public string BranchDeliveryStatus { get; set; }
        public int? BranchDeliveredQty { get; set; }
        public string DateDeliveredToBranches { get; set; }
        public int? BranchAllocation { get; set; }
        public int? TotalBundled { get; set; }
        public int? ActualBundled { get; set; }
        public int? ImplemStatusId { get; set; }
        public string ImplemStatus { get; set; }
        public int? ImplemIssueId { get; set; }
        public string ImplemIssue { get; set; }
        public string PercentBundled { get; set; }
        public int? TotalOfftake { get; set; }
        public int? ActualOfftake { get; set; }
        public string PercentOfftake { get; set; }
        public string DateCompleted { get; set; }
        public int? EndingInventory { get; set; }
        public string OfftakeAnalysis { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public int? MonthNo { get; set; }
        public int? WeekNo { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedAt { get; set; }
        public string SyncAt { get; set; }
        public List<Files> Files { get; set; }
    }
    #endregion

    public class InitiativeSyncModel
    {
        public List<Initiatives> Initiatives { get; set; }
        public string Username { get; set; }
    }

    #region Images
    public class Images
    {
        public string BranchId;
        public string InitiativeId { get; set; }
        public string Material { get; set; }
        public string AttachmentId{ get; set; }
        public string AttachmentFileName { get; set; }
        public string Image { get; set; }
        //public string WeekNo;
        //public string MonthNo;
        //public string CapturedBy;
        //public DateTime CapturedDate;
    }
    #endregion

    #region Images
    public class Files
    {
        public string BranchId;
        public string InitiativeId { get; set; }
        public string Material { get; set; }
        public string AttachmentId { get; set; }
        public string AttachmentFileName { get; set; }
        //1-Image,2-Document
        public int AttachmentType { get; set; }
        public string Attachment { get; set; }
        public string WeekNo;
        public string MonthNo;
        public string CapturedBy;
        public string CapturedDate;
        public string SyncAt;
    }
    #endregion
    
    public class FileSyncModel
    {
        public List<Files> Files { get; set; }
        public string Username { get; set; }
    }

    public class UserLogin
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    #region User
    public class UserProfile
    {
        public Nullable<int> UserId { get; set; }
        public Nullable<int> UserProfileId { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Position { get; set; }
    }
    #endregion

    #region Issues
    public class Status
    {
        public int StatusId { get; set; }
        public string StatusDesc { get; set; }
        public List<Category> Category { get; set; }
    }

    public class Category
    {
        public string CategoryDesc { get; set; }
        public List<Issue> Issues { get; set; }
    }

    public class Issue
    {
        public int IssueId { get; set; }
        public string IssueDesc { get; set; }
    }
    #endregion
}
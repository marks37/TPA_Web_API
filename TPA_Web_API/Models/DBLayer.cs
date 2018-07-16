using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace TPA_Web_API.Models
{
    public class DBLayer
    {
        private TPA_ImplemEntities db = new TPA_ImplemEntities();
        private static int saltLengthLimit = 256;

        public UserProfile LoginUser(string username, string password)
        {
            UserProfile userProfile = new UserProfile();

            string OldHASHValue = string.Empty;
            byte[] SALT = new byte[saltLengthLimit];

            try
            {
                using (db = new TPA_ImplemEntities())
                {
                    var user = db.Ref_User.Where(s => s.Username == username.Trim()).FirstOrDefault();
                    if (user != null)
                    {
                        OldHASHValue = user.HASH;
                        SALT = user.SALT;
                    }

                    bool isLogin = CompareHashValue(password, username, OldHASHValue, SALT);

                    if (isLogin)
                    {
                        userProfile = (from p in db.Ref_User_Profile
                                       where p.User_ID == user.UserID
                                       select new UserProfile
                                       {
                                        Address = p.address,
                                        Contact = p.contact,
                                        Firstname = p.firstname,
                                        Lastname = p.lastname,
                                        Position = p.position,
                                        UserId = p.User_ID,
                                        Username = user.Username
                                       }).FirstOrDefault();
                    }
                }
            }
            catch
            {
                throw;
            }

            return userProfile;
        }

        public static bool CompareHashValue(string password, string username, string OldHASHValue, byte[] SALT)
        {
            try
            {
                string expectedHashString = Get_HASH_SHA512(password, username, SALT);

                return (OldHASHValue == expectedHashString);
            }
            catch
            {
                return false;
            }
        }

        #region --> Generate SALT Key

        private static byte[] Get_SALT()
        {
            return Get_SALT(saltLengthLimit);
        }

        private static byte[] Get_SALT(int maximumSaltLength)
        {
            var salt = new byte[maximumSaltLength];

            //Require NameSpace: using System.Security.Cryptography;
            using (var random = new RNGCryptoServiceProvider())
            {
                random.GetNonZeroBytes(salt);
            }

            return salt;
        }

        #endregion

        //CODE: Generate HASH Using SHA512
        public static string Get_HASH_SHA512(string password, string username, byte[] salt)
        {
            try
            {
                //required NameSpace: using System.Text;
                //Plain Text in Byte
                byte[] plainTextBytes = Encoding.UTF8.GetBytes(password + username);

                //Plain Text + SALT Key in Byte
                byte[] plainTextWithSaltBytes = new byte[plainTextBytes.Length + salt.Length];

                for (int i = 0; i < plainTextBytes.Length; i++)
                {
                    plainTextWithSaltBytes[i] = plainTextBytes[i];
                }

                for (int i = 0; i < salt.Length; i++)
                {
                    plainTextWithSaltBytes[plainTextBytes.Length + i] = salt[i];
                }

                HashAlgorithm hash = new SHA512Managed();
                byte[] hashBytes = hash.ComputeHash(plainTextWithSaltBytes);
                byte[] hashWithSaltBytes = new byte[hashBytes.Length + salt.Length];

                for (int i = 0; i < hashBytes.Length; i++)
                {
                    hashWithSaltBytes[i] = hashBytes[i];
                }

                for (int i = 0; i < salt.Length; i++)
                {
                    hashWithSaltBytes[hashBytes.Length + i] = salt[i];
                }

                return Convert.ToBase64String(hashWithSaltBytes);
            }
            catch
            {
                return string.Empty;
            }
        }


        public List<Ref_Branch_APD> GetAssignedBranchesWithInitiatives(int userID)
        {
            var branchesWithInitiatives = (from dba in db.Data_Branch_Allocation
                                           group dba by dba.branch_id into grp
                                           select grp.Key).ToList();

            //var assignedBranches = (from ba in db.Ref_Branch_Assignment
            //                        where ba.User_ID == userID
            //                        join b_apd in db.Ref_Branch_APD
            //                        on ba.Branch_ID equals b_apd.ID
            //                        join dba in db.Data_Branch_Allocation
            //                        on ba.Branch_ID equals dba.Branch_ID into grps
            //                        from x in grps.DefaultIfEmpty()
            //                        where x != null
            //                        select b_apd;

            var assignedBranches = (from b_apd in db.Ref_Branch_APD
                                    join ba in db.Ref_Branch_Assignment
                                    on b_apd.branch_apd_id equals ba.branch_id
                                    where ba.user_id == userID
                                    select b_apd).ToList();

            var filtered = (from a in assignedBranches
                            join b in branchesWithInitiatives
                            on a.branch_apd_id equals b.Value
                            select a);

            return filtered.ToList();
        }

        public List<Data_Branch_Allocation> GetData_Branch_AllocationsByUserID(int userID)
        {
            var data_branch_allocation = (from ba in db.Ref_Branch_Assignment
                                          join dba in db.Data_Branch_Allocation
                                          on ba.branch_id equals dba.branch_id
                                          where ba.user_id == userID
                                          select dba);
            return data_branch_allocation.ToList();
        }

        public List<string> GetAssignedChannels(int userID)
        {
            var assignedBranches = GetAssignedBranchesWithInitiatives(userID);
            var channelList = (from g in assignedBranches
                               group g by g.channel into channels
                               select channels.Key);
            return channelList.ToList();
        }

        public List<string> GetAssignedAccounts(int userID, string channel)
        {
            var assignedBranches = GetAssignedBranchesWithInitiatives(userID);
            var accountList = (from g in assignedBranches
                               where g.channel == channel
                               group g by new { g.account, g.channel } into grp
                               select grp.Key.account);
            return accountList.ToList();
        }

        public List<Data_Branch_Allocation> GetData_Branch_Allocations(int branchID, string TPB_ID)
        {
            var materials = (from ba in db.Data_Branch_Allocation
                             where (ba.branch_id == branchID) && (ba.tpb_id == TPB_ID)
                             select ba);
            return materials.ToList();
        }

        public List<Mobile_Channel> GetAssignedMobile_Channels(int userID)
        {
            List<string> assignedChannels = GetAssignedChannels(userID);
            List<Mobile_Channel> mobile_Channels = new List<Mobile_Channel>();
            foreach (var item in assignedChannels)
            {
                Mobile_Channel mobile_Channel = new Mobile_Channel()
                {
                    Name = item,
                    AccountList = GetAssignedMobile_Accounts(userID, item)
                };
                mobile_Channels.Add(mobile_Channel);
            }
            return mobile_Channels;
        }

        public List<Mobile_Account> GetAssignedMobile_Accounts(int userID, string channel)
        {
            List<Mobile_Account> mobile_Accounts = new List<Mobile_Account>();
            List<string> assignedAccounts = GetAssignedAccounts(userID, channel);
            foreach (var item in assignedAccounts)
            {
                Mobile_Account mobile_Account = new Mobile_Account()
                {
                    Name = item,
                    BranchList = GetAssignedMobile_Branches(userID, item)
                };
                mobile_Accounts.Add(mobile_Account);
            }
            return mobile_Accounts;
        }

        public List<Mobile_Branch> GetAssignedMobile_Branches(int userID, string account)
        {
            var assignedBranches = GetAssignedBranchesWithInitiatives(userID);
            List<Mobile_Branch> mobile_Branches = new List<Mobile_Branch>();
            foreach (var item in assignedBranches)
            {
                Mobile_Branch mobile_Branch = new Mobile_Branch()
                {
                    BranchID = item.branch_apd_id,
                    BranchAddress = item.address,
                    BranchName = item.branch_name,
                    CDS = item.cds,
                    TeamLead = item.team_lead,
                    InitiativeList = GetMobile_Initiatives(item.branch_apd_id)
                };
                mobile_Branches.Add(mobile_Branch);
            }


            //var branchList = (from g in assignedBranches
            //                  where g.Account == account
            //                  select new Mobile_Branch
            //                  {
            //                      BranchID = g.ID,
            //                      BranchAddress = g.Address,
            //                      BranchName = g.Branch_Name,
            //                      CDS = g.CDS,
            //                      TeamLead = g.TeamLead
            //                  });
            return mobile_Branches;
        }

        //public Mobile_Initiative GetMobile_Initiative(string userID)
        //{
        //    Initiative result = new Initiative();
        //    result = (from p in db.Data_Initiative
        //              where p.TPB_ID == id
        //              group p by new { p.TPB_ID, p.Year, p.Month, p.Initiative_Type, p.Initiative_Sub_Type, p.Category, p.Division, p.Brand, p.Initiative_Title, p.Start_Date, p.End_Date, p.Sell_in_date } into groups
        //              select new Initiative
        //              {
        //                  TPB_ID = groups.Key.TPB_ID,
        //                  Year = groups.Key.Year,
        //                  Month = groups.Key.Month,
        //                  Initiative_Type = groups.Key.Initiative_Type,
        //                  Initiative_Sub_Type = groups.Key.Initiative_Sub_Type,
        //                  Category = groups.Key.Category,
        //                  Division = groups.Key.Division,
        //                  Brand = groups.Key.Brand,
        //                  Initiative_Title = groups.Key.Initiative_Title,
        //                  Start_Date = groups.Key.Start_Date,
        //                  End_Date = groups.Key.End_Date,
        //                  Sell_in_date = groups.Key.Sell_in_date
        //              }
        //               //select groups.OrderByDescending(p => p.Created_At).FirstOrDefault()
        //               ).FirstOrDefault();

        //    return result;
        //}

        public List<string> GetAccountListByChannelByUserID(string channel, int userID)
        {
            List<string> accountGroupList = new List<string>();

            //var list = (from assignment in db.Ref_Branch_Assignment
            //            join branch in Ref_Branch_APD
            //            on assignment.Branch_ID equals branch.)

            //var list = (from a in db.Ref_Branch_Assignment
            //            where a.User_ID == userID
            //            join b in db.Ref_Branch_APD
            //            on a.Branch_ID equals b.ID into ab
            //            from x in ab.DefaultIfEmpty()
            //            where (x==null) && (x.Channel==channel)
            //            group x by new {x.AccountGroup} into groups
            //            select groups.Key.AccountGroup).ToList();


            //var accountGroupList = (from p in db.data
            //                        where p.Branch_ID == branchID
            //                        group p by new { p.TPB_ID, p.Branch_ID } into groups
            //                        select groups.Key.TPB_ID).ToList();



            return accountGroupList;
        }

        public Mobile_Branch_Initiative GetMobile_Branch_Initiative(int branchID)
        {
            var mobile_initiatives = (from ba in db.Data_Branch_Allocation
                                      where ba.branch_id == branchID
                                      join i in db.Data_Initiative
                                      on new { ba.tpb_id, ba.account } equals new { i.tpb_id, i.account } into grps
                                      from p in grps
                                      group p by new
                                      { p.tpb_id, p.year, p.month, p.initiative_type, p.initiative_sub_type, p.category, p.division, p.brand, p.initiative_title, p.start_date, p.end_date, p.sell_in_date } into grouped
                                      select new Mobile_Initiative
                                      {
                                          TPB_ID = grouped.Key.tpb_id,
                                          Year = grouped.Key.year,
                                          Month = grouped.Key.month,
                                          InitiativeType = grouped.Key.initiative_type,
                                          InitiativeSubType = grouped.Key.initiative_sub_type,
                                          Category = grouped.Key.category,
                                          Division = grouped.Key.division,
                                          Brand = grouped.Key.brand,
                                          InitiativeTitle = grouped.Key.initiative_title,
                                          StartDate = grouped.Key.start_date,
                                          EndDate = grouped.Key.end_date,
                                          SellInDate = grouped.Key.sell_in_date
                                      });

            Mobile_Branch_Initiative mobile_Branch_Initiative = new Mobile_Branch_Initiative()
            {
                BranchID = branchID,
                InitiativeList = mobile_initiatives.ToList()
            };
            return mobile_Branch_Initiative;
        }

        public List<Mobile_Initiative> GetMobile_Initiatives(int branchID)
        {
            var mobile_initiatives = (from ba in db.Data_Branch_Allocation
                                      where ba.branch_id == branchID
                                      join i in db.Data_Initiative
                                      on new { ba.tpb_id, ba.account } equals new { i.tpb_id, i.account } into grps
                                      from p in grps
                                      group p by new
                                      { p.tpb_id, p.year, p.month, p.initiative_type, p.initiative_sub_type, p.category, p.division, p.brand, p.initiative_title, p.start_date, p.end_date, p.sell_in_date } into grouped
                                      select new Mobile_Initiative
                                      {
                                          TPB_ID = grouped.Key.tpb_id,
                                          Year = grouped.Key.year,
                                          Month = grouped.Key.month,
                                          InitiativeType = grouped.Key.initiative_type,
                                          InitiativeSubType = grouped.Key.initiative_sub_type,
                                          Category = grouped.Key.category,
                                          Division = grouped.Key.division,
                                          Brand = grouped.Key.brand,
                                          InitiativeTitle = grouped.Key.initiative_title,
                                          StartDate = grouped.Key.start_date,
                                          EndDate = grouped.Key.end_date,
                                          SellInDate = grouped.Key.sell_in_date
                                      }).ToList();

            foreach (var item in mobile_initiatives)
            {
                item.MaterialList = GetMobile_Materials(branchID, item.TPB_ID);
                item.WeekNo = GetTPA_Calendar(GetCurrentTime()).week_number;
                item.MonthNo = GetTPA_Calendar(GetCurrentTime()).month_number;
            }

            return mobile_initiatives;
        }

        public List<Mobile_Material> GetMobile_Materials(int branchID, string TPB_ID)
        {
            var materials = GetData_Branch_Allocations(branchID, TPB_ID);
            List<Mobile_Material> mobile_Materials = new List<Mobile_Material>();
            foreach (var item in materials)
            {
                var tpa_delivery = GetData_TPA_Delivery(TPB_ID, item.account, item.materials);
                var branch_delivery = GetData_Branch_Delivery(item.branch_allocation_id);
                var bundling = GetData_Installation(item.branch_allocation_id);
                var issue = GetData_Installation_Issue(item.branch_allocation_id);
                var offtake = GetData_Offtake(item.branch_allocation_id);

                var initiative = (from i in db.Data_Initiative
                                   where (i.tpb_id == TPB_ID) && (i.materials==item.materials)
                                   group i by new { i.tpb_id, i.materials} into grps
                                   select grps.OrderByDescending(p => p.created_at).FirstOrDefault() into tmp
                                   select new {
                                       tmp.tpb_id,
                                       tmp.materials,
                                       tmp.start_date,
                                       tmp.end_date
                                   }).FirstOrDefault();
                
                Mobile_Material material = new Mobile_Material();
                material.Material = item.materials;
                material.ImplemStatusId = item.implem_status_id;
                material.ImplemStatus = item.implem_status;
                material.OfftakeAnalysis = item.offtake_analysis;

                if (tpa_delivery != null)
                {
                    material.TpaDeliveryStatusId = tpa_delivery.tpa_delivery_status_id;
                    material.TpaDeliveryStatus = tpa_delivery.tpa_delivery_status;
                }

                if (branch_delivery != null)
                {
                    material.BranchDeliveryStatusId = branch_delivery.branch_delivery_status_id;
                    material.BranchDeliveryStatus = branch_delivery.branch_delivery_status;
                }

                if (bundling != null)
                {
                    material.BranchAllocation = item.branch_allocation;
                    material.RunningActualAllocation = bundling.Bundled;
                    material.BundledPercent = (((double)bundling.Bundled / (double)item.branch_allocation) * 100).ToString()+'%';
                }

                //if (issue != null)
                //{
                //    material.ImplemIssueId = issue.installation_issue_id;
                //    material.ImplemIssue = issue.issue;
                //}5

                if (offtake != null)
                {
                    material.TotalOfftake = offtake.Offtake;
                    material.OfftakePercent = (((double)offtake.Offtake / (double)bundling.Bundled) * 100).ToString() + "%";
                    material.EndingInventory = bundling.Bundled - offtake.Offtake;
                }
                
                if(branch_delivery == null)
                {
                    material.MaterialType = 1;
                }
                else
                {
                    switch (branch_delivery.branch_delivery_status_id)
                    {
                        case 1:
                            {
                                material.MaterialType = 1;
                                break;
                            }
                            case 2:
                            {
                                if ((item.date_completed == null)&&(initiative.start_date<=DbFunctions.TruncateTime(GetCurrentTime()))&& (initiative.end_date>= DbFunctions.TruncateTime(GetCurrentTime())))
                                {
                                    material.MaterialType = 2;
                                }
                                else if((item.date_completed!=null))
                                {
                                    material.MaterialType = 3;
                                }
                                break;
                            }

                        default:
                            {
                                break;
                            }

                    }
                }
                
                mobile_Materials.Add(material);
            }
            return mobile_Materials;
        }

        public Data_TPA_Delivery GetData_TPA_Delivery(string tpb_id, string account, string material)
        {
            Data_TPA_Delivery delivery = new Data_TPA_Delivery();

            delivery = (from d in db.Data_TPA_Delivery
                        where (d.tpb_id == tpb_id) && (d.account == account) && (d.materials == material)
                        select d).FirstOrDefault();
            return delivery;
        }

        public Data_Branch_Delivery GetData_Branch_Delivery(int branchAllocationID)
        {
            var delivery = (from d in db.Data_Branch_Delivery
                            where d.branch_allocation_id == branchAllocationID
                            select d);
            return delivery.FirstOrDefault();
        }

        public Mobile_Installation GetData_Installation(int branchAllocationID)
        {
            var bundling = (from d in db.Data_Installation
                            where d.branch_allocation_id == branchAllocationID
                            group d by d.branch_allocation_id into grp
                            select new Mobile_Installation
                            {
                                BranchAllocationId = grp.Key.Value,
                                Bundled = grp.Sum(a => a.bundled)
                            });
            return bundling.FirstOrDefault();
        }

        public Data_Installation_Issue GetData_Installation_Issue(int branchAllocationID)
        {
            var issue = (from i in db.Data_Installation_Issue
                         where i.branch_allocation_id == branchAllocationID
                         orderby i.installation_issue_id descending
                         select i);
            return issue.FirstOrDefault();
        }

        public Mobile_Offtake GetData_Offtake(int branchAllocationID)
        {
            var offtake = (from o in db.Data_Offtake
                           where o.branch_allocation_id == branchAllocationID
                           group o by o.branch_allocation_id into grp
                           select new Mobile_Offtake
                           {
                               BranchAllocationId = grp.Key.Value,
                               Offtake = grp.Sum(a => a.offtake)
                           });
            return offtake.FirstOrDefault();
        }

        public Ref_TPA_Calendar GetTPA_Calendar(DateTime date)
        {
            var calendar = (from c in db.Ref_TPA_Calendar
                            where (DbFunctions.TruncateTime(c.week_start) <= DbFunctions.TruncateTime(date)) && (DbFunctions.TruncateTime(c.week_end) >= DbFunctions.TruncateTime(date))
                            select c).FirstOrDefault();
            return calendar;
        }

        public DateTime GetCurrentTime()
        {
            DateTime serverTime = DateTime.Now;
            DateTime _localTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(serverTime, TimeZoneInfo.Local.Id, "Singapore Standard Time");
            return _localTime;
        }

    }
}
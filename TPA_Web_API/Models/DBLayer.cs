using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace TPA_Web_API.Models
{
    public class DBLayer
    {
        private WCPIADBEntities db = new WCPIADBEntities();
        private static int saltLengthLimit = 256;
        string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["TPADBConnectionString"].ConnectionString;

        public UserProfile LoginUser(string username, string password)
        {
            UserProfile userProfile = new UserProfile();

            string OldHASHValue = string.Empty;
            byte[] SALT = new byte[saltLengthLimit];

            try
            {
                using (db = new WCPIADBEntities())
                {
                    var user = db.Ref_User.Where(s => s.username == username.Trim()).FirstOrDefault();
                    if (user != null)
                    {
                        OldHASHValue = user.hash;
                        SALT = user.salt;
                    }

                    bool isLogin = CompareHashValue(password, username, OldHASHValue, SALT);

                    if (isLogin)
                    {
                        userProfile = (from p in db.Ref_User_Profile
                                       where p.user_profile_id == user.user_id
                                       select new UserProfile
                                       {
                                           //Address = p.address,
                                           //Contact = p.contact,
                                           //Firstname = p.firstname,
                                           //Lastname = p.lastname,
                                           Position = p.position,
                                           UserId = p.user_profile_id,
                                           Username = user.username
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

        public List<Data_Initiative> GetDistinctData_Initiatives()
        {
            var initiatives = (from i in db.Data_Initiative
                               group i by new { i.tpb_id, i.year, i.month, i.channel, i.agency, i.initiative_type, i.initiative_sub_type, i.category, i.division, i.brand, i.initiative_title, i.materials, i.start_date, i.end_date, i.account, i.sell_in_date, i.account_allocation } into grp
                               select grp
                               .OrderByDescending(p => p.created_at)
                               .FirstOrDefault());
            return initiatives.ToList();
        }

        //public List<Mobile_Material> GetMobile_Materials()
        //{
        //    var materials = (from ib in db.Data_Initiative_Branch
        //                     select new Mobile_Material
        //                     {
        //                         BranchAllocation = ib.branch_allocation,
        //                     }).ToList();
        //    return materials;
        //}

        public List<Data_Initiative_Branch> GetData_Initiative_Branches()
        {
            var data_initiative_branches = (from ib in db.Data_Initiative_Branch
                                            select ib).ToList();
            return data_initiative_branches;
        }

        public List<Ref_Branch_APD> GetAssignedBranchesWithInitiatives(int userID)
        {
            var branchesWithInitiatives = (from dba in db.Data_Initiative_Branch
                                           where dba.branch_allocation > 0
                                           join id in db.Data_Initiative_Delivery
                                           on new { dba.account, dba.materials, dba.tpb_id } equals new { id.account, id.materials, id.tpb_id }
                                           where id.tpa_delivery_status_id != 3
                                           group dba by dba.sfa_internal_id into grp
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
                                    on b_apd.branch_apd_id equals ba.branch_apd_id
                                    where ba.user_id == userID
                                    select b_apd).ToList();

            //var filtered = (from a in assignedBranches
            //                join b in branchesWithInitiatives   
            //                on a.sfa_id equals b.Value
            //                select a);


            var filtered = (from a in assignedBranches
                            where branchesWithInitiatives.Contains(a.sfa_id)
                            select a);

            return filtered.ToList();
        }

        public List<Data_Initiative_Branch> GetData_Branch_AllocationsByUserID(int userID)
        {
            var data_branch_allocation = (from ba in db.Ref_Branch_Assignment
                                          join dba in db.Data_Initiative_Branch
                                          on ba.branch_apd_id equals dba.branch_apd_id
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
                               group g by new { g.ul_account, g.channel } into grp
                               select grp.Key.ul_account);
            return accountList.ToList();
        }

        public List<Data_Initiative_Branch> GetData_Branch_Allocations(int branchID, string TPB_ID)
        {
            var materials = (from ba in db.Data_Initiative_Branch
                             where (ba.branch_apd_id == branchID) && (ba.tpb_id == TPB_ID)
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
            var assignedBranches = GetAssignedBranchesWithInitiatives(userID).Where(p => p.ul_account == account);
            List<Mobile_Branch> mobile_Branches = new List<Mobile_Branch>();
            foreach (var item in assignedBranches)
            {
                Mobile_Branch mobile_Branch = new Mobile_Branch()
                {
                    BranchID = item.sfa_id,
                    BranchAddress = item.address,
                    BranchName = item.branch_name,
                    CDS = item.cds,
                    TeamLead = item.team_lead,
                    InitiativeList = GetMobile_Initiatives(item.sfa_id),
                };
                mobile_Branch.InitiativeCount = mobile_Branch.InitiativeList.Count();
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
            var mobile_initiatives = (from ba in db.Data_Initiative_Branch
                                      where ba.branch_apd_id == branchID
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

        public List<Mobile_Initiative> GetMobile_Initiatives(string branchID)
        {
            var mobile_initiatives = (from ba in db.Data_Initiative_Branch
                                      where ba.sfa_internal_id == branchID && ba.branch_allocation > 0
                                      //join id in db.Data_Initiative_Delivery
                                      //on new { ba.account, ba.materials, ba.tpb_id } equals new { id.account, id.materials, id.tpb_id }
                                      //where id.tpa_delivery_status_id != 3
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

        public List<Mobile_Material> GetMobile_Materials(string branchID, string TPB_ID)
        {
            //var materials = GetData_Branch_Allocations(branchID, TPB_ID);

            var materials = (from ib in db.Data_Initiative_Branch
                             where ib.sfa_internal_id == branchID && ib.tpb_id == TPB_ID
                             select ib).ToList();

            var initiatives = GetDistinctData_Initiatives();

            var initiativeDeliveries = (from i in initiatives
                                        join id in db.Data_Initiative_Delivery
                                        on new { i.tpb_id, i.materials, i.account } equals new { id.tpb_id, id.materials, id.account } into tmp
                                        from t in tmp
                                        select new
                                        { i, t }).ToList();

            List<Mobile_Material> mobile_Materials = new List<Mobile_Material>();
            foreach (var item in materials)
            {
                Mobile_Material material = new Mobile_Material()
                {
                    Material = item.materials,
                    TpaDeliveryStatus = (from id in initiativeDeliveries
                                         where id.i.materials == item.materials
                                         select id.t.tpa_delivery_status).FirstOrDefault(),
                    TpaDeliveryStatusId = (from id in initiativeDeliveries
                                           where id.i.materials == item.materials
                                           select id.t.tpa_delivery_status_id).FirstOrDefault(),
                    DateDeliveredToTPA = (from id in initiativeDeliveries
                                          where id.i.materials == item.materials
                                          select id.t.tpa_delivery_date).FirstOrDefault().ToString(),
                    BranchDeliveryStatus = item.branch_delivery_status,
                    BranchDeliveryStatusId = item.branch_delivery_status_id,
                    DateDeliveredToBranches = item.branch_delivery_date.ToString(),
                    BranchAllocation = item.branch_allocation,
                    //RunningActualAllocation = (from ibl in db.Data_Initiative_Branch_Line
                    //                           where ibl.initiative_branch_id == item.initiative_branch_id
                    //                           group ibl by ibl.initiative_branch_id into grp
                    //                           select grp.Sum(p => p.actual_bundled)).FirstOrDefault(),
                    RunningActualAllocation = item.total_bundled == null ? 0 : item.total_bundled,
                    Bundled = 0,
                    ImplemStatus = item.implem_status,
                    ImplemStatusId = item.implem_status_id,
                    BundledPercent = ((item.total_bundled == null ? 0 : (double)item.total_bundled / item.branch_allocation == null ? 0 : (double)item.branch_allocation) * 100).ToString() + '%',
                    //BundledPercent = item.percent_bundled==null?0:item.percent_bundled,
                    TotalOfftake = item.total_offtake == null ? 0 : (int)item.total_offtake,
                    OfftakeBundled = 0,
                    OfftakePercent = ((item.total_offtake == null ? 0 : (double)item.total_offtake / item.total_bundled == null ? 0 : (double)item.total_bundled) * 100).ToString() + "%",
                    //OfftakePercent = ((item.total_offtake == null ? 0 : (double)item.total_offtake / item.total_bundled == null ? 0 : (double)item.total_bundled) * 100).ToString() + "%",
                    EndingInventory = item.ending_inventory == null ? 0 : (int)item.ending_inventory,
                    OfftakeAnalysis = item.offtake_analysis==null?"Slow Moving":item.offtake_analysis,
                    DateCompleted = item.date_of_completion.ToString()
                };



                #region MyRegion
                //var tpa_delivery = GetData_TPA_Delivery(TPB_ID, item.account, item.materials);
                //var branch_delivery = GetData_Branch_Delivery(item.branch_allocation_id);
                //var bundling = GetData_Installation(item.branch_allocation_id);
                //var issue = GetData_Installation_Issue(item.branch_allocation_id);
                //var offtake = GetData_Offtake(item.branch_allocation_id);

                //var initiative = (from i in db.Data_Initiative
                //                   where (i.tpb_id == TPB_ID) && (i.materials==item.materials)
                //                   group i by new { i.tpb_id, i.materials} into grps
                //                   select grps.OrderByDescending(p => p.created_at).FirstOrDefault() into tmp
                //                   select new {
                //                       tmp.tpb_id,
                //                       tmp.materials,
                //                       tmp.start_date,
                //                       tmp.end_date
                //                   }).FirstOrDefault();

                //Mobile_Material material = new Mobile_Material();
                //material.Material = item.materials;
                //material.ImplemStatusId = item.implem_status_id;
                //material.ImplemStatus = item.implem_status;
                //material.OfftakeAnalysis = item.offtake_analysis;

                //if (tpa_delivery != null)
                //{
                //    material.TpaDeliveryStatusId = tpa_delivery.tpa_delivery_status_id;
                //    material.TpaDeliveryStatus = tpa_delivery.tpa_delivery_status;
                //}

                //if (branch_delivery != null)
                //{
                //    material.BranchDeliveryStatusId = branch_delivery.branch_delivery_status_id;
                //    material.BranchDeliveryStatus = branch_delivery.branch_delivery_status;
                //}

                //if (bundling != null)
                //{
                //    material.BranchAllocation = item.branch_allocation;
                //    material.RunningActualAllocation = bundling.Bundled;
                //    material.BundledPercent = (((double)bundling.Bundled / (double)item.branch_allocation) * 100).ToString()+'%';
                //}

                //if (issue != null)
                //{
                //    material.ImplemIssueId = issue.installation_issue_id;
                //    material.ImplemIssue = issue.issue;
                //}5

                //if (offtake != null)
                //{
                //    material.TotalOfftake = offtake.Offtake;
                //    material.OfftakePercent = (((double)offtake.Offtake / (double)bundling.Bundled) * 100).ToString() + "%";
                //    material.EndingInventory = bundling.Bundled - offtake.Offtake;
                //}

                //if(branch_delivery == null)
                //{
                //    material.MaterialType = 1;
                //}
                //else
                //{
                //    switch (branch_delivery.branch_delivery_status_id)
                //    {
                //        case 1:
                //            {
                //                material.MaterialType = 1;
                //                break;
                //            }
                //            case 2:
                //            {
                //                if ((item.date_completed == null)&&(initiative.start_date<=DbFunctions.TruncateTime(GetCurrentTime()))&& (initiative.end_date>= DbFunctions.TruncateTime(GetCurrentTime())))
                //                {
                //                    material.MaterialType = 2;
                //                }
                //                else if((item.date_completed!=null))
                //                {
                //                    material.MaterialType = 3;
                //                }
                //                break;
                //            }

                //        default:
                //            {
                //                break;
                //            }

                //    }
                //} 
                #endregion

                mobile_Materials.Add(material);
            }
            return mobile_Materials;
        }

        public Data_Initiative_Delivery GetData_TPA_Delivery(string tpb_id, string account, string material)
        {
            Data_Initiative_Delivery delivery = new Data_Initiative_Delivery();

            delivery = (from d in db.Data_Initiative_Delivery
                        where (d.tpb_id == tpb_id) && (d.account == account) && (d.materials == material)
                        select d).FirstOrDefault();
            return delivery;
        }

        //public Data_Initiative_Branch GetData_Branch_Delivery(int branchAllocationID)
        //{
        //    var delivery = (from d in db.Data_Initiative_Branch
        //                    where d.branch_allocation_id == branchAllocationID
        //                    select d);
        //    return delivery.FirstOrDefault();
        //}

        //public Mobile_Installation GetData_Installation(int branchAllocationID)
        //{
        //    var bundling = (from d in db.Data_Installation
        //                    where d.branch_allocation_id == branchAllocationID
        //                    group d by d.branch_allocation_id into grp
        //                    select new Mobile_Installation
        //                    {
        //                        BranchAllocationId = grp.Key.Value,
        //                        Bundled = grp.Sum(a => a.bundled)
        //                    });
        //    return bundling.FirstOrDefault();
        //}

        //public Data_Installation_Issue GetData_Installation_Issue(int branchAllocationID)
        //{
        //    var issue = (from i in db.Data_Installation_Issue
        //                 where i.branch_allocation_id == branchAllocationID
        //                 orderby i.installation_issue_id descending
        //                 select i);
        //    return issue.FirstOrDefault();
        //}

        //public Mobile_Offtake GetData_Offtake(int branchAllocationID)
        //{
        //    var offtake = (from o in db.Data_Offtake
        //                   where o.branch_allocation_id == branchAllocationID
        //                   group o by o.branch_allocation_id into grp
        //                   select new Mobile_Offtake
        //                   {
        //                       BranchAllocationId = grp.Key.Value,
        //                       Offtake = grp.Sum(a => a.offtake)
        //                   });
        //    return offtake.FirstOrDefault();
        //}

        public Ref_TPA_Calendar GetTPA_Calendar(DateTime date)
        {
            var calendar = (from c in db.Ref_TPA_Calendar
                            where (DbFunctions.TruncateTime(c.week_start) <= DbFunctions.TruncateTime(date)) && (DbFunctions.TruncateTime(c.week_end) >= DbFunctions.TruncateTime(date))
                            //where (DbFunctions.TruncateTime(c.week_start) <= date.Date) && (DbFunctions.TruncateTime(c.week_end) >= date.Date)
                            select c).FirstOrDefault();
            return calendar;
        }

        public DateTime GetCurrentTime()
        {
            DateTime serverTime = DateTime.Now;
            DateTime _localTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(serverTime, TimeZoneInfo.Local.Id, "Singapore Standard Time");
            return _localTime;
        }

        public List<Initiatives> PostInitiatives(List<Initiatives> initiatives)
        {
            #region Define DataTable and Columns
            DataTable dt = new DataTable();
            DataColumn colTpbId = new DataColumn("tpb_id");
            DataColumn colSfaInternalId = new DataColumn("sfa_internal_id");
            DataColumn colMaterial = new DataColumn("material");
            //DataColumn colTpaDeliveryStatusId = new DataColumn("tpa_delivery_status_id");
            //DataColumn colTpaDeliveryStatus = new DataColumn("tpa_delivery_status");
            DataColumn colTpaDeliveryDate = new DataColumn("tpa_delivery_date");
            DataColumn colBranchDeliveryStatusId = new DataColumn("branch_delivery_status_id");
            DataColumn colBranchDeliveryStatus = new DataColumn("branch_delivery_status");
            DataColumn colBranchDeliveredQty = new DataColumn("branch_delivery_qty");
            DataColumn colBranchDeliveryDate = new DataColumn("branch_delivery_date");
            //DataColumn colBranchAllocation = new DataColumn("branch_allocation");
            DataColumn colTotalBundled = new DataColumn("total_bundled");
            DataColumn colActualBundled = new DataColumn("actual_bundled");
            DataColumn colImplemStatusId = new DataColumn("implem_status_id");
            DataColumn colImplemStatus = new DataColumn("implem_status");
            DataColumn colImplemIssueId = new DataColumn("implem_issue_id");
            DataColumn colImplemIssue = new DataColumn("implem_issue");
            DataColumn colPercentBundled = new DataColumn("percent_bundled");
            DataColumn colTotalOfftake = new DataColumn("total_offtake");
            DataColumn colActualOfftake = new DataColumn("actual_offtake");
            DataColumn colPercentOfftake = new DataColumn("percent_offtake");
            DataColumn colEndingInventory = new DataColumn("ending_inventory");
            DataColumn colOfftakeAnalysis = new DataColumn("offtake_analysis");
            //DataColumn colStartDate = new DataColumn("start_date");
            //DataColumn colEndDate = new DataColumn("end_date");
            DataColumn colDateOfCompletion = new DataColumn("date_of_completion");
            DataColumn colMonthNo = new DataColumn("month_no");
            DataColumn colWeekNo = new DataColumn("week_no");
            DataColumn colCreatedAt = new DataColumn("created_at");
            DataColumn colCreatedBy = new DataColumn("created_by");
            DataColumn colSyncAt = new DataColumn("sync_at");

            //Add Columns
            dt.Columns.Add(colTpbId);
            dt.Columns.Add(colSfaInternalId);
            dt.Columns.Add(colMaterial);
            //dt.Columns.Add(colTpaDeliveryStatusId);
            //dt.Columns.Add(colTpaDeliveryStatus);
            dt.Columns.Add(colTpaDeliveryDate);
            dt.Columns.Add(colBranchDeliveryStatusId);
            dt.Columns.Add(colBranchDeliveryStatus);
            dt.Columns.Add(colBranchDeliveredQty);
            dt.Columns.Add(colBranchDeliveryDate);
            //dt.Columns.Add(colBranchAllocation);
            dt.Columns.Add(colTotalBundled);
            dt.Columns.Add(colActualBundled);
            dt.Columns.Add(colImplemStatusId);
            dt.Columns.Add(colImplemStatus);
            dt.Columns.Add(colImplemIssueId);
            dt.Columns.Add(colImplemIssue);
            dt.Columns.Add(colPercentBundled);
            dt.Columns.Add(colTotalOfftake);
            dt.Columns.Add(colActualOfftake);
            dt.Columns.Add(colPercentOfftake);
            dt.Columns.Add(colEndingInventory);
            dt.Columns.Add(colOfftakeAnalysis);
            //dt.Columns.Add(colStartDate);
            //dt.Columns.Add(colEndDate);
            dt.Columns.Add(colDateOfCompletion);
            dt.Columns.Add(colMonthNo);
            dt.Columns.Add(colWeekNo);
            dt.Columns.Add(colCreatedAt);
            dt.Columns.Add(colCreatedBy);
            dt.Columns.Add(colSyncAt);
            #endregion 

            using (var connection = new SqlConnection(connectionString))
            {
                //using (var command = new SqlCommand(query, connection))
                //{
                foreach (Initiatives i in initiatives)
                {
                    DataRow row = dt.NewRow();
                    row[colTpbId] = i.TpbId;
                    row[colSfaInternalId] = i.SfaInternalId;
                    row[colMaterial] = i.Material;
                    //row[colTpaDeliveryStatusId] = i.TpaDeliveryStatusId;
                    //row[colTpaDeliveryStatus] = i.TpaDeliveryStatus;
                    row[colTpaDeliveryDate] = i.DateDeliveredToTpa;
                    row[colBranchDeliveryStatusId] = i.BranchDeliveryStatusId;
                    row[colBranchDeliveryStatus] = i.BranchDeliveryStatus;
                    row[colBranchDeliveredQty] = i.BranchDeliveredQty;
                    row[colBranchDeliveryDate] = i.DateDeliveredToBranches;
                    //row[colBranchAllocation] = i.BranchAllocation;
                    row[colTotalBundled] = i.TotalBundled;
                    row[colActualBundled] = i.ActualBundled;
                    row[colImplemStatusId] = i.ImplemStatusId;
                    row[colImplemStatus] = i.ImplemStatus;
                    row[colImplemIssueId] = i.ImplemIssueId;
                    row[colImplemIssue] = i.ImplemIssue;
                    row[colPercentBundled] = i.PercentBundled;
                    row[colTotalOfftake] = i.TotalOfftake;
                    row[colActualOfftake] = i.ActualOfftake;
                    row[colPercentOfftake] = i.PercentOfftake;
                    row[colEndingInventory] = i.EndingInventory;
                    row[colOfftakeAnalysis] = i.OfftakeAnalysis;
                    //row[colStartDate] = i.StartDate;
                    //row[colEndDate] = i.EndDate;
                    row[colDateOfCompletion] = i.DateCompleted;
                    row[colMonthNo] = i.MonthNo;
                    row[colWeekNo] = i.WeekNo;
                    row[colCreatedAt] = i.CreatedAt;
                    row[colCreatedBy] = i.CreatedBy;
                    row[colSyncAt] = i.SyncAt;
                    dt.Rows.Add(row);
                    if (i.Files != null)
                    {
                        if (i.Files.Count() > 0)
                        {
                            PostFiles(i.Files);
                        }
                    }
                    else
                    {
                        Error_Insert("No files found", "PostInitiatives/PostFiles");
                    }
                }

                // Create the SqlBulkCopy object. 
                using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(connectionString))
                {
                    //Set the database table name
                    sqlBulkCopy.DestinationTableName = "dbo.Source_Data_Mobile";

                    //[OPTIONAL]: Map the DataTable columns with that of the database table
                    // Guarantee that columns are mapped correctly by defining the column mappings for the order.
                    sqlBulkCopy.ColumnMappings.Add("tpb_id", "tpb_id");
                    sqlBulkCopy.ColumnMappings.Add("sfa_internal_id", "sfa_internal_id");
                    sqlBulkCopy.ColumnMappings.Add("material", "material");
                    //sqlBulkCopy.ColumnMappings.Add("tpa_delivery_status_id", "tpa_delivery_status_id");
                    //sqlBulkCopy.ColumnMappings.Add("tpa_delivery_status", "tpa_delivery_status");
                    sqlBulkCopy.ColumnMappings.Add("tpa_delivery_date", "tpa_delivery_date");
                    sqlBulkCopy.ColumnMappings.Add("branch_delivery_status_id", "branch_delivery_status_id");
                    sqlBulkCopy.ColumnMappings.Add("branch_delivery_status", "branch_delivery_status");
                    sqlBulkCopy.ColumnMappings.Add("branch_delivery_qty", "branch_delivery_qty");
                    sqlBulkCopy.ColumnMappings.Add("branch_delivery_date", "branch_delivery_date");
                    //sqlBulkCopy.ColumnMappings.Add("branch_allocation", "branch_allocation");
                    sqlBulkCopy.ColumnMappings.Add("total_bundled", "total_bundled");
                    sqlBulkCopy.ColumnMappings.Add("actual_bundled", "actual_bundled");
                    sqlBulkCopy.ColumnMappings.Add("implem_status_id", "implem_status_id");
                    sqlBulkCopy.ColumnMappings.Add("implem_status", "implem_status");
                    sqlBulkCopy.ColumnMappings.Add("implem_issue_id", "implem_issue_id");
                    sqlBulkCopy.ColumnMappings.Add("implem_issue", "implem_issue");
                    sqlBulkCopy.ColumnMappings.Add("percent_bundled", "percent_bundled");
                    sqlBulkCopy.ColumnMappings.Add("total_offtake", "total_offtake");
                    sqlBulkCopy.ColumnMappings.Add("actual_offtake", "actual_offtake");
                    sqlBulkCopy.ColumnMappings.Add("percent_offtake", "percent_offtake");
                    sqlBulkCopy.ColumnMappings.Add("ending_inventory", "ending_inventory");
                    sqlBulkCopy.ColumnMappings.Add("offtake_analysis", "offtake_analysis");
                    //sqlBulkCopy.ColumnMappings.Add("start_date", "start_date");
                    //sqlBulkCopy.ColumnMappings.Add("end_date", "end_date");
                    sqlBulkCopy.ColumnMappings.Add("date_of_completion", "date_of_completion");
                    sqlBulkCopy.ColumnMappings.Add("month_no", "month_no");
                    sqlBulkCopy.ColumnMappings.Add("week_no", "week_no");
                    sqlBulkCopy.ColumnMappings.Add("created_at", "created_at");
                    sqlBulkCopy.ColumnMappings.Add("created_by", "created_by");
                    sqlBulkCopy.ColumnMappings.Add("sync_at", "sync_at");
                    connection.Open();
                    sqlBulkCopy.WriteToServer(dt);
                    connection.Close();
                }

                #region Parameters
                //command.Parameters.AddWithValue("@material", initiatives.Material == String.Empty ? SqlString.Null : initiatives.Material);
                //command.Parameters.AddWithValue("@tpa_delivery_status_id", initiatives.TpaDeliveryStatusId.HasValue ? initiatives.TpaDeliveryStatusId.Value : SqlInt32.Null);
                //command.Parameters.AddWithValue("@tpa_delivery_status", initiatives.TpaDeliveryStatus == String.Empty ? SqlString.Null : initiatives.TpaDeliveryStatus);
                //command.Parameters.AddWithValue("@branch_delivery_status_id", initiatives.BranchDeliveryStatusId.HasValue ? initiatives.BranchDeliveryStatusId.Value : SqlInt32.Null);
                //command.Parameters.AddWithValue("@branch_delivery_status", initiatives.BranchDeliveryStatus == String.Empty ? SqlString.Null : initiatives.BranchDeliveryStatus);
                //command.Parameters.AddWithValue("@branch_delivered_qty", initiatives.BranchDeliveredQty.HasValue ? initiatives.BranchDeliveredQty.Value : SqlInt32.Null);
                //command.Parameters.AddWithValue("@branch_allocation", initiatives.BranchAllocation.HasValue ? initiatives.BranchAllocation.Value : SqlInt32.Null);
                //command.Parameters.AddWithValue("@total_bundled", initiatives.TotalBundled.HasValue ? initiatives.TotalBundled.Value : SqlInt32.Null);
                //command.Parameters.AddWithValue("@actual_bundled", initiatives.ActualBundled.HasValue ? initiatives.ActualBundled.Value : SqlInt32.Null);
                //command.Parameters.AddWithValue("@implem_status_id", initiatives.ImplemStatusId.HasValue ? initiatives.ImplemStatusId.Value : SqlInt32.Null);
                //command.Parameters.AddWithValue("@implem_status", initiatives.ImplemStatus == String.Empty ? SqlString.Null : initiatives.ImplemStatus);
                //command.Parameters.AddWithValue("@percent_bundled", initiatives.PercentBundled == String.Empty ? SqlString.Null : initiatives.PercentBundled);
                //command.Parameters.AddWithValue("@total_offtake", initiatives.TotalOfftake.HasValue ? initiatives.TotalOfftake.Value : SqlInt32.Null);
                //command.Parameters.AddWithValue("@actual_offtake", initiatives.ActualOfftake.HasValue ? initiatives.ActualOfftake.Value : SqlInt32.Null);
                //command.Parameters.AddWithValue("@percent_offtake", initiatives.PercentOfftake== String.Empty ? SqlString.Null : initiatives.PercentOfftake);
                //command.Parameters.AddWithValue("@ending_inventory", initiatives.EndingInventory.HasValue ? initiatives.EndingInventory.Value : SqlInt32.Null);
                //command.Parameters.AddWithValue("@offtake_analysis", initiatives.OfftakeAnalysis == String.Empty ? SqlString.Null : initiatives.OfftakeAnalysis);
                //command.Parameters.AddWithValue("@start_date", initiatives.StartDate == String.Empty ? SqlString.Null : initiatives.StartDate);
                //command.Parameters.AddWithValue("@end_date", initiatives.EndDate == String.Empty ? SqlString.Null : initiatives.EndDate);
                //command.Parameters.AddWithValue("@month_no", initiatives.MonthNo.HasValue ? initiatives.MonthNo.Value : SqlInt32.Null);
                //command.Parameters.AddWithValue("@week_no", initiatives.WeekNo.HasValue ? initiatives.WeekNo.Value : SqlInt32.Null);
                #endregion

                //connection.Open();
                //command.ExecuteNonQuery();
                //connection.Close();
                //}

                using (var command = new SqlCommand("sp_BranchInitAndLine_UpdateInsert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    connection.Open();
                    object o = command.ExecuteScalar();
                    connection.Close();
                }


            }

            //var source_mobile = (db.Source_Data_Mobile.Where(p => p.pulled_at == null && p.created_by == username));
            //var NewDataInitiativeBranchLine = (from sdm in db.Source_Data_Mobile
            //                                   where sdm.pulled_at == null && sdm.created_by == username
            //                                   join dib in db.Data_Initiative_Branch
            //                                   on new { sdm.tpb_id, materials = sdm.material, sdm.sfa_internal_id } equals new { dib.tpb_id, dib.materials, dib.sfa_internal_id }
            //                                   )



            return initiatives;
        }

        public List<Category> GetCategory(int status_id)
        {
            List<Category> category = new List<Category>();

            DataTable dt = GetData(string.Format(@"SELECT DISTINCT [category] FROM [dbo].[Ref_Implem_Issue] a
                                                    LEFT JOIN [dbo].[Ref_Implem_Status] b ON a.implem_status_id = b.implem_status_id
                                                    WHERE a.[implem_status_id] = '{0}'", status_id));

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                category.Add(new Category
                {
                    CategoryDesc = Convert.ToString(dt.Rows[i]["category"]),
                    Issues = GetIssues(status_id, Convert.ToString(dt.Rows[i]["category"])),
                });
            }

            return category;
        }

        public List<Issue> GetIssues(int status_id, string category)
        {
            List<Issue> issues = new List<Issue>();

            DataTable dt = GetData(string.Format("SELECT [implem_issue_id], [issue] FROM [dbo].[Ref_Implem_Issue] WHERE [implem_status_id] = '{0}' AND [category] = '{1}'", status_id, category));

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                issues.Add(new Issue
                {
                    IssueId = Convert.ToInt32(dt.Rows[i]["implem_issue_id"]),
                    IssueDesc = Convert.ToString(dt.Rows[i]["issue"])
                });
            }

            return issues;
        }

        private DataTable GetData(string query)
        {
            SqlCommand cmd = new SqlCommand(query);

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlDataAdapter sda = new SqlDataAdapter())
                {
                    cmd.Connection = con;
                    sda.SelectCommand = cmd;
                    using (DataTable dt = new DataTable())
                    {
                        sda.Fill(dt);
                        return dt;
                    }
                }
            }
        }

        public Status GetStatus(int status_id)
        {
            string query = string.Format(@"SELECT [implem_status_id], [implem_status_val] FROM [dbo].[Ref_Implem_Status] WHERE [implem_status_id] = @status_id");

            Status status = new Status();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@status_id", status_id);

                    if (connection.State == ConnectionState.Closed)
                        connection.Open();

                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        status.StatusId = reader.GetInt32(0);
                        status.StatusDesc = (reader.IsDBNull(1) ? null : reader.GetString(1));
                        status.Category = GetCategory(reader.GetInt32(0));
                    }

                    connection.Close();
                }
            }

            return status;
        }

        public Images GetImage()
        {
            string query = string.Format(@"SELECT TOP 1 [initiative_id]
      ,[material]
      ,[attachment_id]
      ,[attachment_file_name]
      ,[image]
  FROM [dbo].[Source_Data_Mobile_Attrib_1_Old]");

            Images image = new Images();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand(query, connection))
                {
                    if (connection.State == ConnectionState.Closed)
                        connection.Open();

                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        image.InitiativeId = reader.IsDBNull(0) ? null : reader.GetString(0);
                        image.Material = reader.IsDBNull(1) ? null : reader.GetString(1);
                        image.AttachmentId = reader.IsDBNull(2) ? null : reader.GetString(2);
                        image.AttachmentFileName = reader.IsDBNull(3) ? null : reader.GetString(3);
                        image.Image = Convert.ToBase64String((byte[])reader[4]);
                    }

                    connection.Close();
                }
            }
            return image;
        }

        public UserProfile GetByUsername(string username, string password)
        {
            string oldHASHValue = string.Empty;
            byte[] salt = new byte[saltLengthLimit];

            string query = string.Format(@"SELECT b.[user_id], a.[user_profile_id],[username],[firstname],[lastname],[position],[hash],[salt]
                                    FROM [dbo].[Ref_User_Profile] a
                                    LEFT JOIN [dbo].[Ref_User] b ON a.user_profile_id = b.user_profile_id
                                    WHERE [username] = @username");

            UserProfile userProfile = new UserProfile();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username);

                    if (connection.State == ConnectionState.Closed)
                        connection.Open();

                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        userProfile.UserId = reader.GetInt32(0);
                        userProfile.UserProfileId = reader.GetInt32(1);
                        userProfile.Username = (reader.IsDBNull(2) ? null : reader.GetString(2));
                        userProfile.FirstName = (reader.IsDBNull(3) ? null : reader.GetString(3));
                        userProfile.LastName = (reader.IsDBNull(4) ? null : reader.GetString(4));
                        userProfile.Position = (reader.IsDBNull(5) ? null : reader.GetString(5));
                        oldHASHValue = (reader.IsDBNull(6) ? null : reader.GetString(6));
                        salt = (reader.IsDBNull(7) ? null : (byte[])reader[7]);
                    }

                    bool isLogin = CompareHashValue(password, username, oldHASHValue, salt);

                    if (!isLogin)
                    {
                        userProfile.UserId = null;
                        userProfile.UserProfileId = null;
                        userProfile.Username = null;
                        userProfile.FirstName = null;
                        userProfile.LastName = null;
                        userProfile.Position = null;
                    }

                    connection.Close();
                }
            }

            return userProfile;
        }

        public List<Images> PostImages(List<Images> images)
        {
            #region Define DataTable and Columns
            DataTable dt = new DataTable();
            DataColumn colInitiativeId = new DataColumn("initiative_id");
            DataColumn colMaterial = new DataColumn("material");
            DataColumn colAttachmentId = new DataColumn("attachment_id");
            DataColumn colAttachmentFileName = new DataColumn("attachment_file_name");
            DataColumn colImage = new DataColumn("image", typeof(Byte[]));

            //Add Columns
            dt.Columns.Add(colInitiativeId);
            dt.Columns.Add(colMaterial);
            dt.Columns.Add(colAttachmentId);
            dt.Columns.Add(colAttachmentFileName);
            dt.Columns.Add(colImage);
            #endregion 

            using (var connection = new SqlConnection(connectionString))
            {
                //using (var command = new SqlCommand(query, connection))
                //{
                foreach (Images i in images)
                {
                    DataRow row = dt.NewRow();
                    row[colInitiativeId] = i.InitiativeId;
                    row[colMaterial] = i.Material;
                    row[colAttachmentId] = i.AttachmentId;
                    row[colAttachmentFileName] = i.AttachmentFileName;
                    row[colImage] = System.Convert.FromBase64String(i.Image);
                    dt.Rows.Add(row);
                }

                // Create the SqlBulkCopy object. 
                using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(connectionString))
                {
                    //Set the database table name
                    sqlBulkCopy.DestinationTableName = "dbo.Source_Data_Mobile_Attrib_1";

                    //[OPTIONAL]: Map the DataTable columns with that of the database table
                    // Guarantee that columns are mapped correctly by defining the column mappings for the order.
                    sqlBulkCopy.ColumnMappings.Add("initiative_id", "initiative_id");
                    sqlBulkCopy.ColumnMappings.Add("material", "material");
                    sqlBulkCopy.ColumnMappings.Add("attachment_id", "attachment_id");
                    sqlBulkCopy.ColumnMappings.Add("attachment_file_name", "attachment_file_name");
                    sqlBulkCopy.ColumnMappings.Add("image", "image");
                    connection.Open();
                    sqlBulkCopy.WriteToServer(dt);
                    connection.Close();
                }
            }

            return images;
        }

        public List<Files> PostFiles(List<Files> files)
        {
            DataTable dt = new DataTable();
            DataColumn colInitiativeId = new DataColumn("initiative_id");
            DataColumn colMaterial = new DataColumn("material");
            DataColumn colAttachmentId = new DataColumn("attachment_id");
            DataColumn colAttachmentFileName = new DataColumn("attachment_file_name");
            DataColumn colBranchId = new DataColumn("branch_id");
            DataColumn colAttachmentType = new DataColumn("attachment_type");
            DataColumn colAttachment = new DataColumn("attachment", typeof(byte[]));
            DataColumn colWeekNo = new DataColumn("week_no");
            DataColumn colMonthNo = new DataColumn("month_no");
            DataColumn colCapturedBy = new DataColumn("captured_by");
            DataColumn colCapturedDate = new DataColumn("captured_date");
            DataColumn colSyncAt = new DataColumn("sync_at");

            dt.Columns.Add(colInitiativeId);
            dt.Columns.Add(colMaterial);
            dt.Columns.Add(colAttachmentId);
            dt.Columns.Add(colAttachmentFileName);
            dt.Columns.Add(colBranchId);
            dt.Columns.Add(colAttachmentType);
            dt.Columns.Add(colAttachment);
            dt.Columns.Add(colWeekNo);
            dt.Columns.Add(colMonthNo);
            dt.Columns.Add(colCapturedBy);
            dt.Columns.Add(colCapturedDate);
            dt.Columns.Add(colSyncAt);

            using (var connection = new SqlConnection(connectionString))
            {
                //using (var command = new SqlCommand(query, connection))
                //{
                foreach (Files i in files)
                {
                    DataRow row = dt.NewRow();
                    row[colInitiativeId] = i.InitiativeId;
                    row[colMaterial] = i.Material;
                    row[colAttachmentId] = i.AttachmentId;
                    row[colAttachmentFileName] = i.AttachmentFileName;
                    row[colBranchId] = i.BranchId;
                    row[colAttachmentType] = i.AttachmentType;
                    row[colAttachment] = System.Convert.FromBase64String(i.Attachment);
                    row[colWeekNo] = i.WeekNo;
                    row[colMonthNo] = i.MonthNo;
                    row[colCapturedBy] = i.CapturedBy;
                    row[colCapturedDate] = i.CapturedDate;
                    row[colSyncAt] = i.SyncAt;
                    dt.Rows.Add(row);
                }

                // Create the SqlBulkCopy object. 
                using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(connectionString))
                {
                    //Set the database table name
                    sqlBulkCopy.DestinationTableName = "dbo.Source_Data_Mobile_Attrib_1";

                    //[OPTIONAL]: Map the DataTable columns with that of the database table
                    // Guarantee that columns are mapped correctly by defining the column mappings for the order.
                    sqlBulkCopy.ColumnMappings.Add("initiative_id", "initiative_id");
                    sqlBulkCopy.ColumnMappings.Add("material", "material");
                    sqlBulkCopy.ColumnMappings.Add("attachment_id", "attachment_id");
                    sqlBulkCopy.ColumnMappings.Add("attachment_file_name", "attachment_file_name");
                    sqlBulkCopy.ColumnMappings.Add("branch_id", "branch_id");
                    sqlBulkCopy.ColumnMappings.Add("attachment_type", "attachment_type");
                    sqlBulkCopy.ColumnMappings.Add("attachment", "attachment");
                    sqlBulkCopy.ColumnMappings.Add("week_no", "week_no");
                    sqlBulkCopy.ColumnMappings.Add("month_no", "month_no");
                    sqlBulkCopy.ColumnMappings.Add("captured_by", "captured_by");
                    sqlBulkCopy.ColumnMappings.Add("captured_date", "captured_date");
                    sqlBulkCopy.ColumnMappings.Add("sync_at", "sync_at");

                    connection.Open();
                    sqlBulkCopy.WriteToServer(dt);
                    connection.Close();
                }
            }
            return files;
        }


        public string Error_Insert(string strError, string strSource)
        {
            SqlConnection sqlConn = new SqlConnection();
            SqlCommand sqlCmd = new SqlCommand();

            sqlConn.ConnectionString = connectionString;

            sqlCmd.CommandText = "INSERT INTO dbo.Data_Error (Message, DateCreated, Source) VALUES (@Message, @DateCreated, @Source)";
            sqlCmd.CommandType = CommandType.Text;
            sqlCmd.Connection = sqlConn;

            sqlCmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@Message", SqlDbType.VarChar, 999));
            sqlCmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@DateCreated", SqlDbType.DateTime, 999));
            sqlCmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@Source", SqlDbType.VarChar, 999));

            sqlCmd.Parameters["@Message"].Value = strError;
            sqlCmd.Parameters["@DateCreated"].Value = GetCurrentTime();
            sqlCmd.Parameters["@Source"].Value = strSource;

            try
            {
                sqlConn.Open();
                sqlCmd.ExecuteNonQuery();
            }
            catch (Exception objExp)
            {
                throw objExp;
            }
            finally
            {
                if (sqlConn != null && sqlConn.State != System.Data.ConnectionState.Closed)
                {
                    sqlConn.Close();
                }
            }
            return "success";
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TPA_Web_API.Models
{
    public class DBLayer
    {
        private TPA_ImplemEntities db = new TPA_ImplemEntities();
        private static int saltLengthLimit = 256;

        public UserProfile loginUser(string username, string password)
        {
            UserProfile userProfile = new UserProfile();

            string OldHASHValue = string.Empty;
            byte[] SALT = new byte[saltLengthLimit];

            //try
            //{
            //    using ()
            //}



            return userProfile;
        }

        public Initiative getInitiativeByID(string id)
        {
            Initiative result = new Initiative();
            result = (from p in db.Data_Initiative
                      where p.TPB_ID == id
                      group p by new { p.TPB_ID, p.Year, p.Month, p.Initiative_Type, p.Initiative_Sub_Type, p.Category, p.Division, p.Brand, p.Initiative_Title, p.Start_Date, p.End_Date, p.Sell_in_date } into groups
                      select new Initiative
                      {
                          TPB_ID = groups.Key.TPB_ID,
                          Year = groups.Key.Year,
                          Month = groups.Key.Month,
                          Initiative_Type = groups.Key.Initiative_Type,
                          Initiative_Sub_Type = groups.Key.Initiative_Sub_Type,
                          Category = groups.Key.Category,
                          Division = groups.Key.Division,
                          Brand = groups.Key.Brand,
                          Initiative_Title = groups.Key.Initiative_Title,
                          Start_Date = groups.Key.Start_Date,
                          End_Date = groups.Key.End_Date,
                          Sell_in_date = groups.Key.Sell_in_date
                      }
                       //select groups.OrderByDescending(p => p.Created_At).FirstOrDefault()
                       ).FirstOrDefault();

            return result;
        }

        public List<string> getAccountListByChannelByUserID(string channel, int userID)
        {
            List<string> accountGroupList = new List<string>();

            //var list = (from assignment in db.Ref_Branch_Assignment
            //            join branch in Ref_Branch_APD
            //            on assignment.Branch_ID equals branch.)

            var list = (from a in db.Ref_Branch_Assignment
                        where a.User_ID == userID
                        join b in db.Ref_Branch_APD
                        on a.Branch_ID equals b.ID into ab
                        from x in ab.DefaultIfEmpty()
                        where (x==null) && (x.Channel==channel)
                        group x by new {x.AccountGroup} into groups
                        select groups.Key.AccountGroup).ToList();


            //var accountGroupList = (from p in db.data
            //                        where p.Branch_ID == branchID
            //                        group p by new { p.TPB_ID, p.Branch_ID } into groups
            //                        select groups.Key.TPB_ID).ToList();



            return list;
        }

    }
}
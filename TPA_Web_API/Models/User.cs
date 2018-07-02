using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TPA_Web_API.Models
{
    public class User
    {
    }

    public class UserProfile
    {
        public int ID { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Position { get; set; }
        public string Contact_Number { get; set; }
        public string Address { get; set; }
        public int User_ID { get; set; }
        public string Username { get; set; }
    }
}
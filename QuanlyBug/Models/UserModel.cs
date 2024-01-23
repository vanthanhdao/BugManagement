using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuanlyBug.Models
{
    public class UserModel
    {
        public int UserID { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }
        public string Status { get; set; }
    }
}
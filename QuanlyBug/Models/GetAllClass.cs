using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuanlyBug.Models
{
    public class GetAllClass
    {
        public int BugID { get; set; }
        public string Title { get; set; }

        public string Decription { get; set; }

        public string Priority { get; set; }

        public string Status { get; set; }

        public string CreateAt { get; set; }
        public string UpdateAt { get; set; }
        public int ProjectID { get; set; }
        public int? ProjectID_PMB { get; set; }

        public string Name { get; set; }
        public string DateCreate { get; set; }
        public string PeopleCreate { get; set; }
        public string EmailPeoCreate { get; set; }
        public int UserID { get; set; }

        public string Username { get; set; }
        public string Email { get; set; }

        public string Password { get; set; }
        public string ResetPass { get; set; }
        public int ProjectMembersID { get; set; }
        public string Role { get; set; }
    }
}
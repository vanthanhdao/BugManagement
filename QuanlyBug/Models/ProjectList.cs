using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuanlyBug.Models
{
    public class ProjectList
    {
        public int ProjectID { get; set; }
        public string Name { get; set; }
        public string Decription { get; set; }
        public string EmailPeoCreate { get; set; }
        public string DateCreate { get; set; }
        public string PeopleCreate { get; set; }
        public int UserID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public int FunctionID { get; set; }
        public string Title { get; set; }
        public string DateCreated { get; set; }
        public string DescriptionFunc { get; set; }
        public string EmailUser { get; set; }
        public string Status { get; set; }
        public string StatusUser { get; set; }
        public string EmailCreater { get; set; }
        public int BugID { get; set; }
        public string TitleBug { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; }
        public string StatusBug { get; set; }
        public string CreatedAt { get; set; }
        public string Severity { get; set; }
        public string Url { get; set; }
        public string Input { get; set; }
        public string Reproduce { get; set; }
        public string Expected { get; set; }
        public string Actual { get; set; }
        public string Env { get; set; }
        public int countFunctions { get; set; }
        public int countBugs { get; set; }
        public int countFunctionsNew { get; set; }
        public int countBugsNew { get; set; }
        public int countProject { get; set; }
        public string deadline { get; set; }
        public string nameUserChose{ get; set; }
    }
}

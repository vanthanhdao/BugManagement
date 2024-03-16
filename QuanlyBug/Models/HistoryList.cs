using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuanlyBug.Models
{
    public class HistoryList
    {
        public int ID_History { get; set; }
        public Nullable<int> ProjectMembersID { get; set; }
        public string Name_Project { get; set; }
        public string Description_History { get; set; }
        public string Time { get; set; }
        public string Activity { get; set; }
        public Nullable<int> ID_User { get; set; }
        public string nameUser { get; set; }
    }
}
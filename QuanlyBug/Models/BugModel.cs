using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuanlyBug.Models
{
    public class BugModel
    {
        public int BugID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public string CreatedAt { get; set; }
        public int FunctionID { get; set; }
        public string Severity { get; set; }
        public string Url { get; set; }
        public string Input { get; set; }
        public string Reproduce { get; set; }
        public string Expected { get; set; }
        public string Actual { get; set; }
        public string Env { get; set; }
    } 

}
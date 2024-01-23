using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuanlyBug.Models
{
    public class ProjectModel
    {
        public int ProjectID { get; set; }

        public string Name { get; set; }

        public string Decription { get; set; }
        public string EmailPeoCreate { get; set; }
    }
}
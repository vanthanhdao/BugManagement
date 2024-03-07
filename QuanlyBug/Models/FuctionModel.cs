using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuanlyBug.Models
{
    public class FuctionModel
    {
        public int FunctionID { get; set; }
        public int ProjecctID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string EmailCreater { get; set; }
        public string DateCreated { get; set;}
        public string EmailUser { get; set;}
        public string Status { get; set;}
    }
}
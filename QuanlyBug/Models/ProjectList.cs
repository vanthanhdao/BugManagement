﻿using System;
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

        }
    }

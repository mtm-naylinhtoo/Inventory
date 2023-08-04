using Inventory.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inventory.ViewModels
{
    public class CityDetailViewModel
    {
        public City City { get; set; }
        public string StateName { get; set; }
        public string CreatedUserName { get; set; }
        public string UpdatedUserName { get; set; }
        public string DeletedUserName { get; set; }
    }
}
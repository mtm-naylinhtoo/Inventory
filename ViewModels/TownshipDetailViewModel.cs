using Inventory.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inventory.ViewModels
{
    public class TownshipDetailViewModel
    {
        public Township Township { get; set; }
        public string StateName { get; set; }
        public string CreatedUserName { get; set; }
        public string UpdatedUserName { get; set; }
        public string DeletedUserName { get; set; }
    }
}
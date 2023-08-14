using Inventory.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inventory.ViewModels
{
    public class StockBalanceDetailViewModel
    {
        public StockBalance StockBalance { get; set; }
        public string CreatedUserName { get; set; }
        public string UpdatedUserName { get; set; }
    }
}
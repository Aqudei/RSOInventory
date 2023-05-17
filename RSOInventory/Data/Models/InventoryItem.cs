﻿using RSOInventory.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSOInventory.Data.Models
{
    internal class InventoryItem : EntityBase
    {
        public int ParentId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? DatePurchased { get; set; }
        public string SerialNumber { get; set; }
        public string PinNumber { get; set; }
        public string Location { get; set; }
        public string Condition { get; set; }
        public bool FoundInStation { get; set; }

    }
}
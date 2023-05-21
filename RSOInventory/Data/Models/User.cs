using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSOInventory.Data.Models
{
    internal class User : EntityBase
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Unit { get; set; }

        public override string ToString()
        {
            return $"{LastName}, {FirstName}";
        }
    }
}

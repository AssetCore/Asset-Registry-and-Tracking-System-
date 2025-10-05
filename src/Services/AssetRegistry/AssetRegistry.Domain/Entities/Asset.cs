using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetRegistry.Domain.Entities
{
    public class Asset
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = "";     
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
        public string Location { get; set; } = "";
        public DateTime PurchasedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
    }
}

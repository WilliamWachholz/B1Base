using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B1Base.Model
{
    public class DiscountGroupModel
    {
        public string ItemCode { get; set; }
        public int ItemGroup { get; set; }
        public double Discount { get; set; }
        public bool Delete { get; set; }
        public bool Insert { get; set; }

        public DiscountGroupModel()
        {
            ItemCode = string.Empty;
            ItemGroup = 0;
            Discount = 0;
            Insert = true;
            Delete = false;
        }
    }
}

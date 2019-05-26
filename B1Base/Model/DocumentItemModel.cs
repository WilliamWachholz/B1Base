using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B1Base.Model
{
    public class DocumentItemModel
    {
        public int DocEntry { get; set; }
        public string ItemCode { get; set; }
        public double Quantity { get; set; }
        public double Price { get; set; }
        public double Discount { get; set; }

        public Dictionary<string, dynamic> UserFields { get; set; }

        public DocumentItemModel()
        {
            DocEntry = 0;
            ItemCode = string.Empty;
            Quantity = 0;
            Price = 0;
            Discount = 0;

            UserFields = new Dictionary<string, dynamic>();
        }
    }
}

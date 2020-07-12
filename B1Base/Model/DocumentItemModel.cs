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
        public string AcctCode { get; set; }
        public string Dscription { get; set; }
        public double Quantity { get; set; }
        public double Price { get; set; }
        public double Discount { get; set; }
        public double Total { get; set; }
        public string TaxCode { get; set; }

        public int LineNum { get; set; }

        public string Text { get; set; }
        public int Usage { get; set; }

        public Dictionary<string, dynamic> UserFields { get; set; }

        public DocumentItemModel()
        {
            DocEntry = 0;
            ItemCode = string.Empty;
            AcctCode = string.Empty;
            Dscription = string.Empty;
            Quantity = 0;
            Price = 0;
            Discount = 0;
            Total = 0;
            TaxCode = string.Empty;
            Usage = 0;
            Text = string.Empty;
            LineNum = 0;
            UserFields = new Dictionary<string, dynamic>();
        }
    }
}

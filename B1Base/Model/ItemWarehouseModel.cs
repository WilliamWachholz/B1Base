using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B1Base.Model
{
    public class ItemWarehouseModel
    {
        public string ItemCode { get; set; }
        public string WhsCode { get; set; }        
        public string WhsName { get; set; }
        public bool Locked { get; set; }
    }
}

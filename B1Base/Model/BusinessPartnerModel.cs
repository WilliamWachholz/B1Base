using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B1Base.Model
{
    public class BusinessPartnerModel
    {
        public string CardCode { get; set; }
        public int SlpCode { get; set; }
        public string AgentCode { get; set; }

        public Dictionary<string, dynamic> UserFields { get; set; }

        public BusinessPartnerModel()
        {
            CardCode = string.Empty;
            SlpCode = 0;
            AgentCode = string.Empty;

            UserFields = new Dictionary<string, dynamic>();
        }
    }
}

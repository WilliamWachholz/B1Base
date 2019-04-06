using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B1Base.DAO
{
    class CompanyDAO
    {
        public bool MultiBranch
        {
            get
            {
                return Controller.ConnectionController.Instance.ExecuteSqlForObject<string>("GetBranchIsMulti") == "Y";
            }
        }
    }
}

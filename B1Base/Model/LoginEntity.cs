using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B1Base.Model
{
    class LoginEntity
    {
        public string CompanyDB { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string Language { get; set; }

        public LoginEntity()
        {
            //https://hanab1:50000/b1s/v1/UserLanguages
            Language = "19"; //pt
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B1Base.Model
{
    public class JournalLineModel
    {
        public int BplId { get; set; }
        public string Account { get; set; }
        public string ContraAct { get; set; }              
        public string ShortName { get; set; }
        public double Credit { get; set; }
        public double Debit { get; set; }

        public JournalLineModel()
        {
            BplId = 0;
            Account = string.Empty;
            ShortName = string.Empty;
            ContraAct = string.Empty;
            Credit = 0;
            Debit = 0;
        }
    }
}

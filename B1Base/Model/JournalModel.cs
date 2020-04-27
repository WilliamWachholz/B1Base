using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B1Base.Model
{
    public class JournalModel
    {
        public int TransId { get; set; }

        public string Ref1 { get; set; }

        public string Ref2 { get; set; }

        public string Memo { get; set; }

        public DateTime RefDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime TaxDate { get; set; }       
        public Dictionary<string, dynamic> UserFields { get; set; }

        public List<Model.JournalLineModel> JournalLineList { get; set; }

        public JournalModel()
        {
            TransId = 0;
            Ref1 = string.Empty;
            Ref2 = string.Empty;
            Memo = string.Empty;
            RefDate = DateTime.Now;
            DueDate = DateTime.Now;
            TaxDate = DateTime.Now;
            UserFields = new Dictionary<string, dynamic>();

            JournalLineList = new List<JournalLineModel>();
        }
    }
}

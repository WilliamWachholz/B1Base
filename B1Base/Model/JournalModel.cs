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
        public DateTime RefDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime TaxDate { get; set; }       
        public Dictionary<string, dynamic> UserFields { get; set; }

        public List<Model.JournalLineModel> JournalLineList { get; set; }

        public JournalModel()
        {
            TransId = 0;
            RefDate = DateTime.Now;
            DueDate = DateTime.Now;
            TaxDate = DateTime.Now;
            UserFields = new Dictionary<string, dynamic>();

            JournalLineList = new List<JournalLineModel>();
        }
    }
}

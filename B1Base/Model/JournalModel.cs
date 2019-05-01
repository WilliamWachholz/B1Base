using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B1Base.Model
{
    class JournalModel
    {
        public int TransId { get; set; }
        public DateTime RefDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime TaxDate { get; set; }

        public List<Model.JournalLineModel> JournalLineList { get; set; }

        public JournalModel()
        {
            TransId = 0;
            RefDate = DateTime.Now;
            DueDate = DateTime.Now;
            TaxDate = DateTime.Now;

            JournalLineList = new List<JournalLineModel>();
        }
    }
}

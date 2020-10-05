using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B1Base.Model
{
    public class DocumentModel
    {
        public int DocEntry { get; set; }
        public int BPLId { get; set; }
        public string CardCode { get; set; }
        public DateTime DocDate { get; set; }
        public DateTime TaxDate { get; set; }
        public DateTime DueDate { get; set; }
        public string DocCur { get; set; }
        public double DocRate { get; set; }
        public EnumDocType DocType { get; set; }
        public int SlpCode { get; set; }        
        public EnumObjType ObjType { get; set; }
        public string Comments { get; set; }

        public int Serial { get; set; }

        public int Model { get; set; }

        public List<Model.DocumentItemModel> DocumentItemList { get; set; }
        
        public Dictionary<string, dynamic> UserFields { get; set; }

        public DocumentModel()
        {
            DocEntry = 0;
            BPLId = 0;
            CardCode = string.Empty;
            DocDate = DateTime.Now;
            TaxDate = DateTime.Now;
            DueDate = DateTime.Now;
            DocRate = 0;
            DocType = EnumDocType.Item;
            SlpCode = 0;
            ObjType = EnumObjType.None;
            Comments = string.Empty;
            Serial = 0;
            Model = 0;

            DocumentItemList = new List<DocumentItemModel>();           

            UserFields = new Dictionary<string, dynamic>();
        }
    }
}

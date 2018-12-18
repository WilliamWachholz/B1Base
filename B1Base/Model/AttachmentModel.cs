using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B1Base.Model
{
    public class AttachmentModel
    {
        public int AbsEntry { get; set; }
        public int Line { get; set; }
        public string Path { get; set; }
        public bool Delete { get; set; }
        public bool Insert { get; set; }
    }
}

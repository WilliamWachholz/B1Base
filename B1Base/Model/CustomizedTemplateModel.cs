using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B1Base.Model
{
    public class CustomizedTemplateModel
    {
        public int Top { get; set; }
        public int Left { get; set; }
        public bool Editable { get; set; }
        public bool Visible { get; set; }

        public CustomizedTemplateModel()
        {
            Top = 0;
            Left = 0;
            Editable = false;
            Visible = false;
        }
    }
}

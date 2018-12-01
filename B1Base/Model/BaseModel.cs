using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B1Base.Model
{
    public class BaseModel
    {
        public int Code { get; set; }

        public bool Changed { get; set; }

        public class Size : Attribute
        {
            public Size(int value)
            {
                Value = value;
            }

            public int Value { get; private set; }
        }

        public class SpecificType : Attribute
        {
            public SpecificType(SpecificTypeEnum value)
            {
                Value = value;
            }

            public SpecificTypeEnum Value { get; private set; }

            public enum SpecificTypeEnum
            {
                Quantity,
                Percent,
                Price,
                Rate,
                Measurement,
                Phone,
                Time,
                Memo,
                Image
            }
        }

        
        //DELTA?
    }
}

using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace B1Base.Model
{
    public class BaseModel : IFromRecordSet
    {
        [ObjectField]
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

        public virtual void FromRecordSet(Recordset recordSet)
        {
            
        }

        public virtual void FromDBReader(OdbcDataReader dbReader)
        {

        }

        public class NonDB : Attribute
        {

        }

        public class ObjectField : Attribute
        {

        }
    }
}

﻿using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Odbc;

namespace B1Base.Model
{
    public interface IFromRecordSet
    {
        void FromRecordSet(Recordset recordSet);

        void FromDBReader(OdbcDataReader dbReader);
    }

}

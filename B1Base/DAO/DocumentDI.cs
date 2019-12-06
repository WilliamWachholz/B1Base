using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using SAPbobsCOM;

namespace B1Base.DAO
{
    public class DocumentDI
    {
        Documents _businessObject = null;

        bool _newObject = false;

        public void SetCardCode(string value)
        {
            _businessObject.CardCode = value == null ? "" : value;
        }

        public void SetDocDate(DateTime value)
        {
            _businessObject.DocDate = value;
        }

        public void SetTaxDate(DateTime value)
        {
            _businessObject.TaxDate = value;
        }

        public void SetDocDueDate(DateTime value)
        {
            _businessObject.DocDueDate = value;
        }

        public void SetDocCur(string value)
        {
            _businessObject.DocCurrency = value == null ? "" : value;
        }

        public void SetComments(string value)
        {
            _businessObject.Comments = value == null ? "" : value;
        }

        public void SetUserField(string key, dynamic value)
        {
            _businessObject.UserFields.Fields.Item(key).Value = value;
        }

        public void SetItemCode(string value, int line = -1)
        {
            if (line == -1)
            {
                _businessObject.Lines.Add();

                line = _businessObject.Address.Count() - 1;

                _businessObject.Lines.SetCurrentLine(line);
            }
            else
            {
                for (int i = _businessObject.Address.Count(); i <= line; i++)
                    _businessObject.Lines.Add();
            }

            _businessObject.Lines.SetCurrentLine(line);

            _businessObject.Lines.ItemCode = value == null ? "" : value; ;
        }

        public void SetItemQuantity(double value, int line = -1)
        {
            if (line == -1)
            {
                _businessObject.Lines.Add();

                line = _businessObject.Address.Count() - 1;

                _businessObject.Lines.SetCurrentLine(line);
            }
            else
            {
                for (int i = _businessObject.Address.Count(); i <= line; i++)
                    _businessObject.Lines.Add();
            }

            _businessObject.Lines.SetCurrentLine(line);

            _businessObject.Lines.Quantity = value;
        }

        public void SetItemPrice(double value, int line = -1)
        {
            if (line == -1)
            {
                _businessObject.Lines.Add();

                line = _businessObject.Address.Count() - 1;

                _businessObject.Lines.SetCurrentLine(line);
            }
            else
            {
                for (int i = _businessObject.Address.Count(); i <= line; i++)
                    _businessObject.Lines.Add();
            }

            _businessObject.Lines.SetCurrentLine(line);

            _businessObject.Lines.UnitPrice = value;
        }

        public void SetItemDiscPrcnt(double value, int line = -1)
        {
            if (line == -1)
            {
                _businessObject.Lines.Add();

                line = _businessObject.Address.Count() - 1;

                _businessObject.Lines.SetCurrentLine(line);
            }
            else
            {
                for (int i = _businessObject.Address.Count(); i <= line; i++)
                    _businessObject.Lines.Add();
            }

            _businessObject.Lines.SetCurrentLine(line);

            _businessObject.Lines.DiscountPercent = value;
        }

            public void Save()
        {
            _businessObject.UserFields.Fields.Item("U_DIUpdate").Value = "Y";
        }
    }
}

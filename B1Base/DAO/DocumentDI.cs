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

        public void InitializeObject(int docEntry, Model.EnumObjType objType)
        {
            _businessObject = GetDIObject(objType);

            if (!_businessObject.GetByKey(docEntry))
            {
                _newObject = true;
            }
        }
        public void FinalizeObject()
        {
            Marshal.ReleaseComObject(_businessObject);

            _businessObject = null;

            GC.Collect();
        }

        public void SetBplId(int value)
        {
            _businessObject.BPL_IDAssignedToInvoice = value;
        }

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

        public void SetDocRate(double value)
        {
            _businessObject.DocRate = value;
        }

        public void SetTrnspCode(int value)
        {
            _businessObject.TransportationCode = value;
        }

        public void SetTrackNo(string value)
        {
            _businessObject.TrackingNumber = value == null ? "" : value;
        }

        public void SetGroupNum(int value)
        {
            _businessObject.GroupNumber = value;
        }
        public void SetPeyMethod(string value)
        {
            _businessObject.PaymentMethod = value == null ? "" : value;
        }

        public void SetComments(string value)
        {
            _businessObject.Comments = value == null ? "" : value;
        }

        public void SetUserField(string key, dynamic value)
        {
            _businessObject.UserFields.Fields.Item(key).Value = value;
        }

        public int SetItemCode(string value, int line = -1)
        {
            if (line == -1)
            {
                if (_businessObject.Lines.ItemCode != string.Empty)
                    _businessObject.Lines.Add();

                line = _businessObject.Lines.Count - 1;
            }

            _businessObject.Lines.SetCurrentLine(line);

            _businessObject.Lines.ItemCode = value == null ? "" : value;

            return line;
        }

        public void SetItemQuantity(double value, int line)
        {
            _businessObject.Lines.SetCurrentLine(line);

            _businessObject.Lines.Quantity = value;
        }

        public void SetItemPrice(double value, int line)
        {
            _businessObject.Lines.SetCurrentLine(line);

            _businessObject.Lines.UnitPrice = value;
        }

        public void SetItemDiscPrcnt(double value, int line)
        {
            _businessObject.Lines.SetCurrentLine(line);

            _businessObject.Lines.DiscountPercent = value;
        }

        public void SetItemUsage(string value, int line)
        {
            _businessObject.Lines.SetCurrentLine(line);

            _businessObject.Lines.Usage = value == null ? "" : value;
        }
        
        public void SetItemTaxCode(string value, int line)
        {
            _businessObject.Lines.SetCurrentLine(line);

            _businessObject.Lines.TaxCode = value == null ? "" : value;
        }

        public int SetExpenseCode(int value, int line = -1)
        {
            if (line == -1)
            {
                if (_businessObject.Expenses.ExpenseCode != 0)
                    _businessObject.Expenses.Add();

                line = _businessObject.Expenses.Count - 1;
            }

            _businessObject.Expenses.SetCurrentLine(line);

            _businessObject.Expenses.ExpenseCode = value;

            return line;
        }

        public void SetExpenseTotal(double value, int line)
        {            
            _businessObject.Expenses.SetCurrentLine(line);

            _businessObject.Expenses.LineTotal = value;
        }


        public void SetExpenseDistribuitionMethod(BoAdEpnsDistribMethods value, int line)
        {
            _businessObject.Expenses.SetCurrentLine(line);

            _businessObject.Expenses.DistributionMethod = value;
        }

        public void Save()
        {
            _businessObject.UserFields.Fields.Item("U_DIUpdate").Value = "Y";

            try
            {
                if (_newObject)
                {
                    _businessObject.Add();
                }
                else
                {
                    _businessObject.Update();
                }

                Controller.ConnectionController.Instance.VerifyBussinesObjectSuccess();
            }
            catch (Exception ex)
            {
                throw ex;
                //guarda em tabela de banco. Criar uma tabela no banco para cada addon para guardar erro select (sql) e objetos DI (xml)
            }

            //if (_newObject)
            //    _businessObject.GetByKey(Controller.ConnectionController.Instance.LastObjectCode);
        }

        private Documents GetDIObject(Model.EnumObjType objType)
        {
            switch (objType)
            {
                case Model.EnumObjType.Invoice:
                    return (Documents)Controller.ConnectionController.Instance.Company.GetBusinessObject(BoObjectTypes.oInvoices);
                case Model.EnumObjType.SalesOrder:
                    return (Documents)Controller.ConnectionController.Instance.Company.GetBusinessObject(BoObjectTypes.oOrders);
                case Model.EnumObjType.InvoiceReturn:
                    return (Documents)Controller.ConnectionController.Instance.Company.GetBusinessObject(BoObjectTypes.oCreditNotes);
                case Model.EnumObjType.PurchaseOrder:
                    return (Documents)Controller.ConnectionController.Instance.Company.GetBusinessObject(BoObjectTypes.oPurchaseOrders);
                case Model.EnumObjType.PurchaseInvoice:
                    return (Documents)Controller.ConnectionController.Instance.Company.GetBusinessObject(BoObjectTypes.oPurchaseInvoices);
                default:
                    return (Documents)Controller.ConnectionController.Instance.Company.GetBusinessObject(BoObjectTypes.oInvoices);
            }
        }


        public Documents BusinessObject
        {
            get
            {
                return _businessObject;
            }
        }
    }
}
